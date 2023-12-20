using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace StableDiffusion
{
    [ExecuteAlways, SelectionBase]
    public class SdRenderer : MonoBehaviour
    {
        public SdApi sdApi = new SdApi(new ApiRequestHandler("http://localhost:7860/"));

        public SdModelSelector modelSelector;
        public SdConfigComponent configComponent;
        public SdRenderTargetSo renderTarget;

        public UnityEvent OnStartGenerate;
        // public UnityEvent OnFinishGenerate;

        [HideInInspector] public Texture2D currentPreviewImage;
        [HideInInspector] public float currentProgress;

        public string addonPrompt = "high detail, photorealistic, focus, high-detail, 4k, 8k";
        public string addonNegativePrompt = "blurry, ugly, cgi, low quality";

        // on enable, get all the components we need
        private void OnEnable()
        {
            if (configComponent == null)
                configComponent = GetComponent<SdConfigComponent>();


            //get the current model from the API
            EditorCoroutineUtility.StartCoroutine(sdApi.GetModel(model =>
            {
                modelSelector.SetModel(model);
            }), this);

            // modelSelector.OnValueChanged += (newModelName) =>
            // {
            //     EditorCoroutineUtility.StartCoroutineOwnerless(sdApi.SetModel(newModelName, null));
            // };
        }

        private void OnDisable()
        {
            // modelSelector.OnValueChanged -= (newModelName) =>
            // {
            //     EditorCoroutineUtility.StartCoroutineOwnerless(sdApi.SetModel(newModelName, null));
            // };
        }

        internal void Generate()
        {
            var config = configComponent.GetOutputJObj();

            OnStartGenerate?.Invoke();

            //append addon prompt to ["prompt"] if it exists, short code version:
            config["prompt"] = config["prompt"]?.ToString() + (addonPrompt.Length > 0 ? ", " : "") + addonPrompt;
            config["negative_prompt"] = config["negative_prompt"]?.ToString() + (addonNegativePrompt.Length > 0 ? ", " : "") + addonNegativePrompt;

            bool isImg2Img = config["init_images"] != null;
            
            Debug.Log($"Generating " + (isImg2Img ? "img2img" : "txt2img") + "...");
            EditorCoroutineUtility.StartCoroutineOwnerless(sdApi.Generate(config, images =>
            {
                if (images.Count == 0)
                    return;

                if (renderTarget != null)
                {
                    var iList = new List<Texture2D>();
                    foreach (var image in images)
                    {
                        iList.Add(image as Texture2D);
                    }
                    renderTarget.SetImages(iList);
                }
                else
                {
                    var rawImage = GetComponent<RawImage>();
                    if (rawImage != null)
                    {
                        rawImage.texture = images[0];
                    }
                }

                // OnFinishGenerate?.Invoke();
            }, (progress, previewTexture) =>
            {
                currentProgress = progress.progress;

                if (previewTexture != null)
                {
                    currentPreviewImage = previewTexture;
                }
            }));
        }
    }

    internal static class HashUtils
    {
        public static string Sha256(string str)
        {
            //if its null, return null
            if (str == null)
                return null;

            var bytes = System.Text.Encoding.UTF8.GetBytes(str);
            var hash = new System.Security.Cryptography.SHA256Managed().ComputeHash(bytes);
            return System.BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }


    // custom inspector for SdRenderToRawImage with a "generate" button to call generate on the sdApi
    [CustomEditor(typeof(SdRenderer))]
    public class SdRendererEditor : Editor
    {
        private SdRenderer tar;
        private GUIStyle genstyle;

        private void OnEnable()
        {
            tar = (SdRenderer)target;
        }
        private float aspect;
        private float width = 256;
        public override void OnInspectorGUI()
        {
            //create a style for the generate button, bold green text
            if (genstyle == null)
            {
                genstyle = new GUIStyle(GUI.skin.button);
                genstyle.normal.textColor = Color.green;
                genstyle.fontStyle = FontStyle.Bold;
            }

            DrawDefaultInspector();

            //update models list button
            if (GUILayout.Button("Update Models"))
            {
                SdModelSelector.UpdateModels(tar.sdApi);
            }

            EditorGUILayout.Space(); 

            var isgen = tar.sdApi.IsGenerating;
            //big generate button, colored green using a style
            if (GUILayout.Button(!isgen ? "Generate" : "Interupt", genstyle))
            {
                if (tar.sdApi.IsGenerating)
                {
                    EditorCoroutineUtility.StartCoroutine(tar.sdApi.Interrupt( null ), tar);
                }
                else
                {
                    tar.Generate();
                }
            }
            if (tar.sdApi.IsGenerating)
            {
                // Calculate the percentage.
                var percent = tar.currentProgress;

                // Draw the progress bar.
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("Generation Progress");
                EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), percent, $"{percent * 100:0.00}%");
                EditorGUILayout.EndVertical();

                if (tar.currentPreviewImage != null)
                {
                    var rect = EditorGUILayout.GetControlRect(false, width / aspect);
                    //repaint the inspector
                    if (Event.current.type == EventType.Repaint){
                        width = rect.width;
                        aspect = tar.currentPreviewImage.width / (float)tar.currentPreviewImage.height;
                    }
                    EditorGUI.DrawPreviewTexture(rect, tar.currentPreviewImage);
                }
            }
        }
    }
}