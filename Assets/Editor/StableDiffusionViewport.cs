using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

namespace Editor
{
    public class StableDiffusionViewport : EditorWindow
    {
        private StableDiffusionEditor _editor;
        public  SdImage   sdImage;
        public  Texture2D mask;

        [SerializeField] private string message     = "Test";

        private VisualElement maskLayer;
        private VisualElement imageLayer;

        private bool drawing     = false;
        private bool outOfBounds = false;

        private                  bool            tempShowEnabled = false;
        
        [SerializeField] private VisualTreeAsset vta;

        public void TempShowTextureStart(Texture2D tex, float aspect)
        {
            
            maskLayer.visible       = false;
            tempShowEnabled         = true;
            imageLayer.style.backgroundImage = tex;
            imageLayer.style.paddingBottom = Length.Percent(50 + (aspect - 1) * 100);
            Repaint();
        }

        public void TempShowTextureEnd()
        {
            maskLayer.visible = true;
            tempShowEnabled   = false;
            if (sdImage != null)
            {
                imageLayer.style.backgroundImage = sdImage?.image;
            }
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
            
            if (vta == null) return;

            _editor = GetWindow<StableDiffusionEditor>();

            mask = new Texture2D(512, 512);
            
            var root = rootVisualElement;
            vta.CloneTree(root);
            root.Bind(new SerializedObject(this));

            maskLayer  = root.Q<VisualElement>("mask_layer");
            imageLayer = root.Q<VisualElement>("image_layer");

            // set images to layers, move to a function
            maskLayer.style.backgroundImage  = mask;
            imageLayer.style.backgroundImage = sdImage?.image;

            if (root.Q<Button>("save_btn") is { } saveBtn)
            {
                saveBtn.clicked += () =>
                {
                    // AssetDatabase.CreateAsset(sdImage.image, "assets/" + sdImage.data.prompt + "-" + sdImage.data.seed + ".asset");
                    AssetDatabase.Refresh();
                };
            }
            
            if (root.Q<Button>("clear_btn") is { } clear)
            {
                clear.clicked += () =>
                {
                    ClearImage();
                };
            }

            root.Bind(new(this));
        }

        private void ClearImage()
        {
            sdImage       = new SdImage(null,"");
            _editor.requestData.init_images.Clear();
            imageLayer.style.backgroundImage = new ();
        }

        public void SetImage(SdImage sdImage)
        {
            this.sdImage                     = sdImage;
            imageLayer.style.backgroundImage = sdImage.image;
            Repaint();
        }
    }
}