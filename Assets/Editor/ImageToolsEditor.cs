using Unity.Collections;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
    public class ImageToolsEditor : EditorWindow
    {
        public SdImage   sdImage;
        public Texture2D   mask;

        [SerializeField] private string message     = "Test";
        [SerializeField] public  bool   genPreview  = true;
        [SerializeField] public  bool   img2imgMode = false;
        [SerializeField] public  bool   hideMask    = false;
        [SerializeField] public  bool   editMask    = false;

        private VisualElement maskLayer;
        private VisualElement imageLayer;

        private bool drawing     = false;
        private bool outOfBounds = false;

        private bool    tempShowEnabled = false;

        public void TempShowTextureStart(Texture2D tex)
        {
            maskLayer.visible                = false;
            tempShowEnabled                  = true;
            imageLayer.style.backgroundImage = tex;
        }

        public void TempShowTextureEnd()
        {
            maskLayer.visible                = true;
            tempShowEnabled                  = false;
            imageLayer.style.backgroundImage = sdImage.image;
        }

        private void Update()
        {
            if (tempShowEnabled)
            {
                Repaint();
            }
        }

        private void CreateGUI()
        {
            mask  = new Texture2D(512, 512);

            var root = rootVisualElement;

            var vta = Resources.Load<VisualTreeAsset>("MaskEditorUI");
            vta.CloneTree(root);
            root.Bind(new SerializedObject(this));

            maskLayer  = root.Q<VisualElement>("mask_layer");
            imageLayer = root.Q<VisualElement>("image_layer");

            maskLayer.style.backgroundImage  = mask;
            imageLayer.style.backgroundImage = sdImage?.image;

            if (root.Q<Button>("save_btn") is { } saveBtn)
            {
                saveBtn.clicked += () =>
                {
                    AssetDatabase.CreateAsset(sdImage.image, sdImage.data.prompt);
                    AssetDatabase.Refresh();
                };
            }

            root.Bind(new(this));
        }

        public void SetImage(SdImage sdImage)
        {
            this.sdImage                     = sdImage;
            imageLayer.style.backgroundImage = sdImage.image;
            Repaint();
        }
    }
}