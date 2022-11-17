using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
    public class StableDiffusionEditor : EditorWindow
    {
        private VisualElement outContainer;
        private List<SdImage> outputImages = new();
        private Texture2D     mask;

        private StableDiffusionViewport viewport = null;

        private bool isRunning;

        [SerializeField]                private string      message;
        [SerializeField, Range(0f, 1f)] private float       progress;
        [SerializeField]                private string      selectedSampler = "Euler a";
        [SerializeField]                public  RequestData requestData;

        [SerializeField] private List<string> models = new();

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

        [SerializeField] private VisualTreeAsset SDEditorUiAsset;
        [SerializeField] private VisualTreeAsset PreviewImageUiAsset;
        [SerializeField] private StyleSheet      style;

        [MenuItem("SD/Window")]
        public static void LoadWindow()
        {
            GetWindow<StableDiffusionEditor>();
        }

        private void CreateGUI()
        {
            if (HasOpenInstances<StableDiffusionViewport>())
            {
                viewport = GetWindow<StableDiffusionViewport>();
            }

            var root = rootVisualElement;
            SDEditorUiAsset.CloneTree(root);
            root.Bind(new(this));
            root.styleSheets.Add(style);

            outContainer = root.Q<VisualElement>("out_container");

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


            if (root.Q<Button>("maskeditor_btn") is { } maskButton)
            {
                maskButton.clicked += () =>
                {
                    viewport = GetWindow<StableDiffusionViewport>();
                };
            }

            if (root.Q<Button>("generate_btn") is { } genButton)
            {
                genButton.clicked += () =>
                {
                    EditorCoroutineUtility.StartCoroutine(Generate(), this);
                };
            }


            // if (root.Q<Button>("test-button") is { } testbtn)
            // {
            //     testbtn.clicked += () =>
            //     {
            //         AddPreviewImage();
            //     };
            // }

            // var cancelButton = root.Q<Button>("Interupt_btn");
            // cancelButton.clicked += () =>
            // {
            //     EditorCoroutineUtility.StartCoroutine(ApiUtils.ApiRequest(2), this);
            //     isRunning = false;
            // };
        }

        private void LayoutPreviews()
        {
            outContainer.Clear();

            var           colCount   = Mathf.FloorToInt(outContainer.contentRect.width / 256);
            VisualElement rowElement = null;
            for (int i = 0; i < outputImages.Count; i++)
            {
                if (i % colCount == 0)
                {
                    rowElement = new() { style = { flexDirection = FlexDirection.Row } };
                    outContainer.Add(rowElement);
                }

                var img = PreviewImageUiAsset.CloneTree()[0];

                var sdImage = outputImages[i];
                img.style.backgroundImage = sdImage.image;
                img.userData              = sdImage;

                img.EnableInClassList("img-icon--sel", false);
                img.RegisterCallback<MouseDownEvent>(evt =>
                {
                    SelectItem(img);

                    if (viewport != null)
                    {
                        viewport.SetImage(sdImage);
                    }
                });

                rowElement!.Add(img);
            }
        }

        private void SelectItem(VisualElement template)
        {
            foreach (var row in outContainer.Children())
            {
                foreach (var col in row.Children())
                {
                    col.EnableInClassList("img-icon--sel", col == template);
                }
            }
        }


        public IEnumerator Generate()
        {
            if (viewport != null)
            {
                if (viewport.sdImage.image != null)
                {
                    requestData.init_images.Clear();
                    requestData.init_images.Add(viewport.sdImage.image);
                }
            }

            isRunning = true;

            EditorCoroutineUtility.StartCoroutine(ProgressCheck(), this);


            yield return ApiUtils.Generate(requestData, generatedImages =>
            {
                VisualElement subcontainer = new() { style = { flexDirection = FlexDirection.Row } };

                outContainer.Add(subcontainer);
                outputImages.Clear();
                if (viewport != null && viewport.sdImage.image != null)
                {
                    outputImages.Add(viewport.sdImage);
                }

                foreach (var generatedImage in generatedImages)
                {
                    outputImages.Add(generatedImage);
                }

                LayoutPreviews();
            });

            isRunning = false;
            progress  = 1;
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
                //
                // if (editwindow != null && editwindow.genPreview)
                // {
                //     yield return ApiUtils.ApiRequest(3, null, (s, jo) =>
                //     {
                //         var imgString = jo["data"]?[2]?.ToString();
                //         if (imgString.Length > 500 && imgString.Substring(22, imgString.Length - 22) is { } imgData)
                //         {
                //             previewTexture.LoadImage(Convert.FromBase64String(imgData));
                //         }
                //     });
                // }
            }
        }
    }
}