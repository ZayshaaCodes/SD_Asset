using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
    public enum Samplers
    {
        Euler_A, Euler,
    }

    public class SD_Editor : EditorWindow
    {
        [SerializeField]                private string message;
        [SerializeField, Range(0f, 1f)] private float  progress;

        private MaskEditor maskEditWindow => GetWindow<MaskEditor>();

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

        [SerializeField] private string      selectedSampler = "Euler a";
        [SerializeField] private RequestData requestData     = new();

        [MenuItem("SD/Window")]
        public static void LoadWindow()
        {
            SD_Editor window = CreateWindow<SD_Editor>();
            window.Show();
        }

        // private void OnGUI()
        // {
        //     // message = Event.current.mousePosition.ToString();
        // }

        private void Update()
        {
            if (isRunning)
            {
                Repaint();
            }
        }

        public IEnumerator Generate()
        {
            isRunning = true;
            EditorCoroutineUtility.StartCoroutine(ProgressCheck(), this);

            previewElement.style.display = DisplayStyle.Flex;

            int w = Mathf.FloorToInt(outContainer.contentRect.width / 256);
            
            ClearOutputContainer();
            yield return ApiUtils.Generate(requestData, generatedImages =>
            {
                VisualElement subcontainer = new() { style = { flexDirection = FlexDirection.Row } };
                outContainer.Add(subcontainer);
                var           c            = w;
                foreach (var generatedImage in generatedImages)
                {
                    if (c-- <= 0)
                    {
                        c            = w;
                        subcontainer = new() { style = { flexDirection = FlexDirection.Row } };
                        outContainer.Add(subcontainer);
                    }

                    var newImg = new VisualElement()
                    {
                        style =
                        {
                            backgroundImage = generatedImage,
                            width           = 256,
                            height          = 256
                        }
                    };
                    newImg.RegisterCallback<MouseDownEvent>(evt =>
                    {
                        maskEditWindow.SetPreview(generatedImage);
                    });

                    subcontainer.Add(newImg);
                }
            });

            previewElement.style.display = DisplayStyle.None;
            isRunning                    = false;
            progress                     = 1;
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            var vta  = Resources.Load<VisualTreeAsset>("EditorUI");
            vta.CloneTree(root);
            root.Bind(new SerializedObject(this));

            previewTexture = new Texture2D(512, 512);
            previewElement = new VisualElement() { name = "preview", style = { backgroundColor = Color.yellow, backgroundImage = previewTexture, width = 512, height = 512 } };

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

            if (root.Q<ScrollView>("out_container") is { } sv)
            {
                if (sv.Q<VisualElement>("unity-content-container") is { } outCon)
                {
                    outCon.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, 0.2f));
                    outContainer                 = outCon;
                    outContainer.Add(previewElement);
                }
            }

            if (root.Q<Button>("maskeditor_btn") is { } maskButton)
            {
                maskButton.clicked += () =>
                {
                    maskEditWindow.Show();
                };
            }


            if (root.Q<Button>("generate_btn") is { } genButton)
            {
                genButton.clicked += () =>
                {
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
                        if (previewTexture == null)
                        {
                            Debug.Log("previewTexture is null");
                        }
                        else
                        {
                            previewTexture.LoadImage(Convert.FromBase64String(imgData));
                        }
                    }
                });
            }
        }
    }
}