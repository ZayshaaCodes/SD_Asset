using System;
using System.Collections;
using System.Collections.Generic;
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

        private ApiUtils.ApiConfig _config;

        private StableDiffusionViewport viewport = null;

        private bool isRunning;

        [SerializeField]                private string message;
        [SerializeField, Range(0f, 1f)] private float  progress;
        [SerializeField]                private string selectedSampler = "Euler a";
        [SerializeField]                private string genButtonText   = "Generate";

        [SerializeField] public RequestData requestData;

        [SerializeField] private List<string> models   = new();
        [SerializeField] private List<string> samplers = new();

        [SerializeField] private VisualTreeAsset sdEditorUiAsset;
        [SerializeField] private VisualTreeAsset previewImageUiAsset;
        [SerializeField] private StyleSheet      style;
        private                  ProgressBar     _progressBar;

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

            sdEditorUiAsset.CloneTree(rootVisualElement);
            rootVisualElement.Bind(new(this));
            rootVisualElement.styleSheets.Add(style);

            EditorCoroutineUtility.StartCoroutineOwnerless(InitializeGUI());
        }

        private IEnumerator InitializeGUI()
        {
            var root = rootVisualElement;
            outContainer = root.Q<VisualElement>("out_container");

            ApiUtils.GetModels(models);
            ApiUtils.GetSamplers(samplers);
            yield return ApiUtils.GetConfig(config =>
            {
                _config = config;
            });

            if (root.Q<DropdownField>("model_dropdown") is { } dropdownField)
            {
                dropdownField.choices = models;
                dropdownField.value   = _config.sd_model_checkpoint;

                dropdownField.RegisterValueChangedCallback(evt =>
                {
                    ApiUtils.SetModel(evt.newValue);
                });
            }

            if (root.Q<DropdownField>("samplers_dropdown") is { } samplerField)
            {
                samplerField.choices = samplers;
            }


            if (root.Q<Button>("maskeditor_btn") is { } maskButton)
            {
                maskButton.clicked += () =>
                {
                    viewport = GetWindow<StableDiffusionViewport>();
                };
            }

            _progressBar = root.Q<ProgressBar>("progress-bar");

            if (root.Q<Button>("generate_btn") is { } genButton)
            {
                genButton.clicked += () =>
                {
                    if (isRunning)
                        ApiUtils.Interrupt();
                    else
                        EditorCoroutineUtility.StartCoroutine(Generate(), this);
                };
            }
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

                var img = previewImageUiAsset.CloneTree()[0];

                var sdImage = outputImages[i];
                img.style.backgroundImage = sdImage.image;
                img.style.width           = sdImage.image.width / 2;
                img.style.height          = sdImage.image.height / 2;
                img.userData              = sdImage;

                img.EnableInClassList("img-icon--sel", false);
                var i1 = i;
                img.RegisterCallback<MouseDownEvent>(evt =>
                {
                    SelectItem(img);
                    if (evt.clickCount == 2)
                    {
                        Debug.Log($"double clicked {i1}");
                        GetWindow<ViewportTestingEditor>()?.SetBaseImage(sdImage.image);
                    }

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
            var paintWindow = GetWindow<ViewportTestingEditor>();
            if (paintWindow != null)
            {
                requestData.init_images.Clear();
                if (paintWindow.BuildOutputImage() is { } outImage)
                {
                    requestData.init_images.Add(outImage);
                }

                requestData.mask = paintWindow.BuildOutputMask();
            }

            isRunning     = true;
            genButtonText = "Interrupt";
            Repaint();

            Texture2D tempTexture             = null;
            if (viewport != null) tempTexture = viewport.TempShowTextureStart(requestData.height / (float)requestData.width);
            EditorCoroutineUtility.StartCoroutine(ProgressCheck(tempTexture), this);

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

            isRunning     = false;
            genButtonText = "Generate";
            progress      = 1;
        }

        private IEnumerator ProgressCheck(Texture2D previewTexture = null)
        {
            string currentImage = "";
            while (isRunning)
            {
                yield return new EditorWaitForSeconds(.5f);


                yield return ApiUtils.CheckProgress((progData) =>
                {
                    progress = progData.percent;

                    if (_progressBar != null)
                    {
                        _progressBar.title = progData.Info;
                    }

                    if (progData.image == currentImage) return;
                    currentImage = progData.image;
                    if (previewTexture != null)
                    {
                        previewTexture.LoadImage(Convert.FromBase64String(progData.image));
                    }
                });
            }

            yield return null;

            if (viewport != null) viewport.TempShowTextureEnd();
        }
    }
}