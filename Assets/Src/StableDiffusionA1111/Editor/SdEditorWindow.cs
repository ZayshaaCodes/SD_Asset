using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace StableDiffusion
{
    public class SdEditorWindow : EditorWindow
    {
        [SerializeField] private SdRenderTargetSo renderTarget;
        [SerializeField] private int currentImageIndex = 0;
        [SerializeField] private VisualTreeAsset UiAsset;
        [SerializeField, Range(.125f, 3)] private float zoom = 1;

        VisualElement _canvas;
        VisualElement _display;

        private bool _dragging;

        [MenuItem("Window/SdEditorWindow")]
        public static void ShowExample()
        {
            SdEditorWindow wnd = GetWindow<SdEditorWindow>();
            wnd.titleContent = new GUIContent("SdEditorWindow");
        }


        private void Zoom(WheelEvent evt)
        {
            zoom += evt.delta.y < 0 ? -.125f : .125f;
            SetZoomLevel(zoom);
        }

        private void SetZoomLevel(float newVal)
        {
            _display.transform.scale = Vector3.one * newVal;
        }

        private void OnEndDrag(EventBase evt)
        {
            _dragging = false;
        }

        public SdRenderTargetSo previousRenderTarget;
        public void OnEnable()
        {
            var root = rootVisualElement;
            UiAsset.CloneTree(root);

            root.Bind(new SerializedObject(this));

            //get the render target field
            var renderTargetField = root.Q<PropertyField>("soField");
            _display = root.Q<Image>("display");
            var saveButton = root.Q<ToolbarButton>("save-button");
            var copyButton = root.Q<ToolbarButton>("copy-button");
            var backButton = root.Q<Button>("back-button");
            var forwardButton = root.Q<Button>("forward-button");

            // Canvas interaction
            _canvas = root.Q<VisualElement>("canvas");
            if (_canvas != null)
            {
                _canvas.RegisterCallback<MouseDownEvent>(evt =>
                {
                    if (evt.button == 1)
                    {
                        _dragging = true;
                    }

                });
                _canvas.RegisterCallback<MouseLeaveEvent>(OnEndDrag);
                _canvas.RegisterCallback<MouseUpEvent>(OnEndDrag);
                _canvas.RegisterCallback<MouseMoveEvent>(evt =>
                {
                    if (_dragging)
                    {
                        _display.transform.position += new Vector3(evt.mouseDelta.x, evt.mouseDelta.y, 0);
                    }
                });
                _canvas.RegisterCallback<WheelEvent>(Zoom);
            }

            // -------- Image Display
            renderTargetField.RegisterValueChangeCallback((SerializedPropertyChangeEvent evt) =>
            {
                //get the new render target
                renderTarget = (SdRenderTargetSo)evt.changedProperty.objectReferenceValue;
                // Debug.Log("render target changed: " + renderTarget?.name ?? "null");
                //set the image to the first image
                if (renderTarget != null)
                {
                    if (previousRenderTarget != null)
                    {
                        previousRenderTarget.OnImagesSet -= SetImage;
                    }
                    renderTarget.OnImagesSet += SetImage;
                    previousRenderTarget = renderTarget;
                    currentImageIndex = 0;
                    _display.style.backgroundImage = renderTarget.images[currentImageIndex];
                    //set the size of the display to the size of the image
                    _display.style.width = renderTarget.images[currentImageIndex].width;
                    _display.style.height = renderTarget.images[currentImageIndex].height;

                }
                else
                {
                    _display.style.backgroundImage = null;
                }
            });

            // -------- Save Button
            saveButton.clickable.clicked += () =>
            {
                //open a save file dialog
                var path = EditorUtility.SaveFilePanel("Save Image", "", "image", "png");

                //save the selected image to the path
                var image = renderTarget.images[currentImageIndex];
                var pngData = image.EncodeToPNG();

                File.WriteAllBytes(path, pngData);

                AssetDatabase.Refresh();
                //make the asset readable and uncompressed
                var meta = File.ReadAllText(path + ".meta");
                meta = Regex.Replace(meta, "isReadable: 0", "isReadable: 1");
                meta = Regex.Replace(meta, "textureCompression: [0-9]", "textureCompression: 0", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                File.WriteAllText(path + ".meta", meta);
                //reimport the asset
                AssetDatabase.Refresh();
                // AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);


            };

            // -------- Copy Button
            copyButton.clickable.clicked += () =>
            {

                Texture2D texture = renderTarget.images[currentImageIndex];
                if (texture == null) return;
                string fileName = "clipboardTemp.png";
                string filePath = Path.Combine(Application.temporaryCachePath, fileName);

                byte[] png = texture.EncodeToPNG();
                File.WriteAllBytes(filePath, png);


                DROPFILES df = new DROPFILES();
                df.pFiles = (uint)Marshal.SizeOf(df);

                byte[] filePathBytes = Encoding.ASCII.GetBytes(filePath + '\0');
                int bytes = filePathBytes.Length;

                IntPtr hGlobal = Marshal.AllocHGlobal(Marshal.SizeOf(df) + bytes + 1);
                IntPtr currentPtr = hGlobal;

                Marshal.StructureToPtr(df, currentPtr, false);
                currentPtr = IntPtr.Add(currentPtr, Marshal.SizeOf(df));

                Marshal.Copy(filePathBytes, 0, currentPtr, bytes);

                if (ClipboardHelper.OpenClipboard(IntPtr.Zero))
                {
                    try
                    {
                        ClipboardHelper.EmptyClipboard();
                        ClipboardHelper.SetClipboardData(15 /*CF_HDROP*/, hGlobal);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                    finally
                    {
                        ClipboardHelper.CloseClipboard();
                    }
                }

                // Marshal.FreeHGlobal(hGlobal);

            };

            // -------- Nav Buttons
            backButton.clickable.clicked += () =>
            {
                currentImageIndex--;
                if (currentImageIndex < 0)
                {
                    currentImageIndex = renderTarget.images.Count - 1;
                }

                _display.style.backgroundImage = renderTarget.images[currentImageIndex];
            };
            forwardButton.clickable.clicked += () =>
            {
                currentImageIndex++;
                if (currentImageIndex >= renderTarget.images.Count)
                {
                    currentImageIndex = 0;
                }

                _display.style.backgroundImage = renderTarget.images[currentImageIndex];
            };


        }

        private void SetImage(List<Texture2D> list)
        {
            if (list.Count == 0)
            {
                return;
            }
            else if (currentImageIndex >= list.Count)
            {
                currentImageIndex = 0;
            }
            _display.style.backgroundImage = list[currentImageIndex];
            //set the size of the display to the size of the image
            _display.style.width = list[currentImageIndex].width;
            _display.style.height = list[currentImageIndex].height;

            this.Repaint();
        }
    }
}
