using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Codice.Client.BaseCommands;
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
    public enum Samplers
    {
        Euler_A, Euler,
    }

    public class SD_Editor : EditorWindow
    {
        [SerializeField]               private string prompt     = "pixar character";
        [SerializeField]               private string negprompt  = "";
        [SerializeField, Range(1, 50)] private int    steps      = 25;
        [SerializeField, Range(1, 10)] private int    batchCount = 1;
        [SerializeField, Range(1, 10)] private int    batchSize  = 2;
        [SerializeField, Range(1, 30)] private float  cfgScale   = 10;
        [SerializeField, Range(0, 1)]  private float  denoise    = .5f;

        [SerializeField] private int seed = -1;

        [SerializeField]                private string message;
        [SerializeField, Range(0f, 1f)] private float  progress;

        private MaskEditor maskEditWindow => GetWindow<MaskEditor>();

        [SerializeField, RangeEx(128, 1024, 128)]
        private int width = 512;

        [SerializeField, RangeEx(128, 1024, 128)]
        private int height = 512;

        private VisualElement   outContainer;
        private List<Texture2D> outputImages;
        private Texture2D       mask;

        private                  bool          isRunning;
        private                  VisualElement previewElement;
        private                  Texture2D     previewTexture;
        [SerializeField] private List<string>  models = new();

        [SerializeField] private List<string> samplers = new()
        {
            "Euler a",
            "Euler",
            "LMS",
            "Heun",
            "DPM adaptive",
            "DDIM",
            "PLMS"
        };

        [SerializeField] private string selectedSampler = "Euler a";

        [MenuItem("SD/Window")]
        public static void LoadWindow()
        {
            SD_Editor window = CreateWindow<SD_Editor>();
            window.Show();
        }

        private void OnGUI()
        {
            // message = Event.current.mousePosition.ToString();
        }

        private void Update()
        {
            if (isRunning)
            {
                Repaint();
            }
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            var vta  = Resources.Load<VisualTreeAsset>("EditorUI");
            vta.CloneTree(root);
            root.Bind(new SerializedObject(this));

            previewTexture = new Texture2D(512, 512);
            previewElement = new VisualElement() { style = { backgroundImage = previewTexture, width = 512, height = 512 } };

            ApiUtils.GetModels(models);

            if (root.Q<DropdownField>("model_dropdown") is { } dropdownField)
            {
                dropdownField.choices = models;
                EditorCoroutineUtility.StartCoroutine(ApiUtils.GetCurModel(s => dropdownField.value = s), this);

                dropdownField.RegisterValueChangedCallback(evt =>
                {
                    ApiUtils.SetModel(evt.newValue);
                });
            }

            if (root.Q<DropdownField>("samplers_dropdown") is { } samplerField)
            {
                samplerField.choices = samplers;
                samplerField.value   = samplers[0];
            }

            if (root.Q<ScrollView>() is { } sv)
            {
                if (sv.Q<VisualElement>("unity-content-container") is { } outCon)
                {
                    outContainer = outCon;
                    outContainer.Add(previewElement);
                }
            }

            if (root.Q<Button>("maskeditor_btn") is { } maskButton)
            {
                maskButton.clicked += () =>
                {
                    CreateWindow<MaskEditor>();
                };
            }


            if (root.Q<Button>("generate_btn") is { } genButton)
            {
                genButton.clicked += () =>
                {
                    Debug.Log("generate: " + prompt);
                    previewElement.style.display = DisplayStyle.Flex;
                    EditorCoroutineUtility.StartCoroutine(Generate(), this);
                };
            }

            // var cancelButton = root.Q<Button>("Interupt_btn");
            // cancelButton.clicked += () =>
            // {
            //     EditorCoroutineUtility.StartCoroutine(ApiUtils.ApiRequest(2), this);
            //     isRunning = false;
            // };
        }

        private IEnumerator Generate()
        {
            Dictionary<string, object> postData = new()
            {
                { "enable_hr", false },
                { "denoising_strength", denoise },
                { "firstphase_width", 0 },
                { "firstphase_height", 0 },
                { "prompt", prompt },
                // { "styles", new List<string>() },
                { "seed", seed },
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
                { "sampler_index", selectedSampler }
            };

            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(postData, Formatting.Indented));

            var request = new UnityWebRequest("http://localhost:7860/sdapi/v1/txt2img", UnityWebRequest.kHttpVerbPOST);

            UploadHandlerRaw uH = new UploadHandlerRaw(data);
            DownloadHandler  dH = new DownloadHandlerBuffer();
            request.uploadHandler   = uH;
            request.downloadHandler = dH;

            request.useHttpContinue = false;
            request.SetRequestHeader("Content-Type", "application/json");

            isRunning = true;
            EditorCoroutineUtility.StartCoroutine(ProgressCheck(), this);
            yield return request.SendWebRequest();

            ClearOutputContainer();
            previewElement.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            isRunning                    = false;
            progress                     = 1;

            if (request.result == UnityWebRequest.Result.Success)
            {
                // Debug.Log(dH.text);
                var reqJson     = JObject.Parse(dH.text);
                var imagesToken = reqJson["images"];
                var c           = imagesToken!.Count();
                for (int i = 0; i < c; i++)
                {
                    var t2d      = new Texture2D(width, height);
                    var imageStr = imagesToken[i]!.ToString();

                    var imgData = Convert.FromBase64String(imageStr);
                    t2d.LoadImage(imgData);

                    var newImg = new VisualElement()
                    {
                        style =
                        {
                            backgroundImage = t2d,
                            width           = 256,
                            height          = 256
                        }
                    };
                    newImg.RegisterCallback<MouseDownEvent>(evt =>
                    {
                        maskEditWindow.SetPreview(t2d);
                        maskEditWindow.SetPreview(t2d);
                    });
                    outContainer.Add(newImg);
                }
            }
        }

        private void ClearOutputContainer()
        {
            for (int i = outContainer.childCount - 1; i >= 1; i--)
            {
                outContainer.RemoveAt(i);
            }
        }

        private IEnumerator ProgressCheck()
        {
            while (isRunning)
            {
                yield return new EditorWaitForSeconds(.125f);

                yield return ApiUtils.ApiRequest(4, null, (s, JObject) =>
                {
                    var match = Regex.Match(s, @";width:-?([\d\.]+)%;");
                    if (float.TryParse(match.Groups[1].ToString(), out float val))
                    {
                        progress = val / 100;
                    }
                });

                yield return ApiUtils.ApiRequest(3, null, (s, jo) =>
                {
                    var imgString = jo["data"]?[2]?.ToString();
                    if (imgString.Length > 500 && imgString.Substring(22, imgString.Length - 22) is { } imgData)
                    {
                        previewTexture.LoadImage(Convert.FromBase64String(imgData));
                    }
                });
            }
        }
    }
}