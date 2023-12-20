// using UnityEditor;
// using UnityEditor.UIElements;
// using UnityEngine;
// using UnityEngine.UIElements;
// using Button = UnityEngine.UIElements.Button;

// namespace Editor
// {
//     public class StableDiffusionViewport : EditorWindow
//     {
//         private StableDiffusionEditor _editor;
//         public  SdImage               sdImage;
//         public  Texture2D             mask;

//         [SerializeField] private string message = "Test";

//         private VisualElement maskLayer;
//         private VisualElement imageLayer;

//         private bool drawing     = false;
//         private bool outOfBounds = false;

//         private bool _tempShowEnabled = false;

//         [SerializeField] private VisualTreeAsset vta;

//         public void SetViewAspect(float aspect)
//         {
//             imageLayer.style.paddingBottom = Length.Percent(50 + (aspect - 1) * 100);
//             maskLayer.style.paddingBottom  = Length.Percent(50 + (aspect - 1) * 100);
//         }

//         public Texture2D TempShowTextureStart(float aspect)
//         {
//             maskLayer.visible = false;
//             _tempShowEnabled  = true;
//             var temp = new Texture2D(512, 512);
//             imageLayer.style.backgroundImage = temp;
//             SetViewAspect(aspect);
//             Repaint();
//             return temp;
//         }

//         public void TempShowTextureEnd()
//         {
//             maskLayer.visible = true;
//             _tempShowEnabled  = false;
            
//             imageLayer.style.backgroundImage = sdImage?.image;

//             if (sdImage?.image == null) return;
//             SetViewAspect(sdImage.image.height / (float)sdImage.image.width);
//         }

//         private void Update()
//         {
//             if (_tempShowEnabled)
//             {
//                 Repaint();
//             }
//         }

//         private void CreateGUI()
//         {
//             if (vta == null) return;

//             _editor = GetWindow<StableDiffusionEditor>();

//             mask = new Texture2D(512, 512);

//             var root = rootVisualElement;
//             vta.CloneTree(root);
//             root.Bind(new SerializedObject(this));

//             maskLayer  = root.Q<VisualElement>("mask_layer");
//             imageLayer = root.Q<VisualElement>("image_layer");

//             // set images to layers, move to a function
//             maskLayer.style.backgroundImage  = mask;
//             imageLayer.style.backgroundImage = sdImage?.image;

//             if (root.Q<Button>("save_btn") is { } saveBtn)
//             {
//                 saveBtn.clicked += () =>
//                 {
//                     // AssetDatabase.CreateAsset(sdImage.image, "assets/" + sdImage.data.prompt + "-" + sdImage.data.seed + ".asset");
//                     AssetDatabase.Refresh();
//                 };
//             }

//             if (root.Q<Button>("clear_btn") is { } clear)
//             {
//                 clear.clicked += () =>
//                 {
//                     ClearImage();
//                 };
//             }

//             root.Bind(new(this));
//         }

//         private void ClearImage()
//         {
//             sdImage = new SdImage();
//             _editor.requestData.init_images.Clear();
//             imageLayer.style.backgroundImage = new();
//         }

//         public void SetImage(SdImage sdImage)
//         {
//             SetViewAspect(sdImage.image.height/ (float)sdImage.image.width);
//             this.sdImage                     = sdImage;
//             imageLayer.style.backgroundImage = sdImage.image;
//             Repaint();
//         }
//     }
// }