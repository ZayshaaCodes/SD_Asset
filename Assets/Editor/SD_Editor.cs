using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

namespace Editor
{
    public class SD_Editor : EditorWindow
    {
        [SerializeField]                          private string message;
        [SerializeField]                          private string prompt     = "pixar character";
        [SerializeField]                          private string negprompt  = "";
        [SerializeField, Range(1, 50)]            private int    steps      = 25;
        [SerializeField, Range(1, 10)]            private int    batchCount = 2;
        [SerializeField, Range(1, 30)]            private float  cfgScale   = 10;
        [SerializeField, RangeEx(128, 1024, 128)] private int  width      = 512;
        [SerializeField, RangeEx(128, 1024, 128)] private int  height     = 512;

        private VisualElement outContainer;

        [MenuItem("SD/Window")]
        public static void LoadWindow()
        {
            SD_Editor window = CreateWindow<SD_Editor>();
            window.Show();
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            var vta  = Resources.Load<VisualTreeAsset>("EditorUI");
            vta.CloneTree(root);
            root.Bind(new SerializedObject(this));

            var generateButton = root.Q<Button>("Generate_btn");

            var sv = root.Q<ScrollView>();
            outContainer = sv.Q<VisualElement>("unity-content-container");

            generateButton.clicked += () =>
            {
                Debug.Log("generate: " + prompt);
                EditorCoroutineUtility.StartCoroutine(test(), this);
            };
        }

        private IEnumerator test()
        {
            Dictionary<string, object> postData = new()
            {
                { "enable_hr", false },
                { "denoising_strength", 0 },
                { "firstphase_width", 0 },
                { "firstphase_height", 0 },
                { "prompt", prompt },
                { "styles", new List<string>() { "string" } },
                { "seed", -1 },
                { "subseed", -1 },
                { "subseed_strength", 0 },
                { "seed_resize_from_h", -1 },
                { "seed_resize_from_w", -1 },
                { "batch_size", batchCount },
                { "n_iter", 1 },
                { "steps", steps },
                { "cfg_scale", cfgScale },
                { "width", width },
                { "height", height },
                { "restore_faces", false },
                { "tiling", false },
                { "negative_prompt", negprompt },
                { "eta", 0 },
                { "s_churn", 0 },
                { "s_tmax", 0 },
                { "s_tmin", 0 },
                { "s_noise", 1 },
                { "sampler_index", "Euler A" }
            };

            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(postData, Formatting.Indented));

            var request = new UnityWebRequest("http://localhost:7860/sdapi/v1/txt2img", UnityWebRequest.kHttpVerbPOST);

            UploadHandlerRaw uH = new UploadHandlerRaw(data);
            DownloadHandler  dH = new DownloadHandlerBuffer();
            request.uploadHandler   = uH;
            request.downloadHandler = dH;

            request.useHttpContinue = false;
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            for (int i = outContainer.childCount - 1; i >= 0; i--)
            {
                outContainer.RemoveAt(i);
            }

            message = "result:" + request.result;
            if (request.result == UnityWebRequest.Result.Success)
            {
                var reqJson     = JObject.Parse(dH.text);
                var imagesToken = reqJson["images"];
                var c           = imagesToken!.Count();

                for (int i = 0; i < c; i++)
                {
                    var t2d      = new Texture2D(width, height);
                    var imageStr = imagesToken[i]!.ToString();

                    var imgData = Convert.FromBase64String(imageStr);
                    t2d.LoadImage(imgData);
                    Debug.Log(outContainer);
                    outContainer.Add(new VisualElement()
                    {
                        style =
                        {
                            backgroundImage = t2d,
                            width           = width/2,
                            height          = height/2
                        }
                    });
                }
            }
        }
    }
}