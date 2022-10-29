using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class MaskEditor : EditorWindow
{
    private                  Texture2D            preview;
    private                  Texture2D            mask;
    [SerializeField] private string               message = "Test";
    private                  NativeArray<Color32> cdata;

    private bool drawing     = false;
    private bool outOfBounds = false;


    [MenuItem("SD/MaskEditor")]
    public static void ShowWindow()
    {
        GetWindow<MaskEditor>();
    }

    private void OnGUI()
    {
        if (Event.current.type != EventType.Repaint) return;
        if (drawing && !outOfBounds)
        {
            var mousePixel = (int2)(float2)(Event.current.mousePosition);
            mousePixel.y = 512 - mousePixel.y;
            Debug.Log("drawing at: " + mousePixel);
            
            
            cdata[(mousePixel.y) * 512 + (mousePixel.x)] = new Color32(0, 0, 0, 255);
            cdata[(mousePixel.y+1) * 512 + (mousePixel.x)] = new Color32(0, 0, 0, 255);
            cdata[(mousePixel.y) * 512 + (mousePixel.x+1)] = new Color32(0, 0, 0, 255);
            cdata[(mousePixel.y) * 512 + (mousePixel.x-1)] = new Color32(0, 0, 0, 255);
            cdata[(mousePixel.y-1) * 512 + (mousePixel.x)] = new Color32(0, 0, 0, 255);
            mask.Apply();
        }
    }

    private void Update()
    {
        if (drawing && !outOfBounds)
        {
            Repaint();
        }
    }

    public void SetPreview(Texture2D tex)
    {
        if (tex.width != preview.width || tex.height != preview.height)
        {
            preview.Reinitialize(tex.width, tex.height);
        }
        preview.SetPixels(tex.GetPixels());
        preview.filterMode = FilterMode.Point;
        preview.Apply();
        Repaint();
    }

    private void CreateGUI()
    {
        mask = new Texture2D(512, 512);
        preview = new Texture2D(512, 512);

        cdata = mask.GetRawTextureData<Color32>();
        for (int i = 0; i < cdata.Length; i++)
        {
            cdata[i] = new Color32(0, 0, 0, 0);
        }

        mask.Apply();

        var root = rootVisualElement;
        
        var vta  = Resources.Load<VisualTreeAsset>("MaskEditorUI");
        vta.CloneTree(root);
        root.Bind(new SerializedObject(this));

        // var maskElement = root.Q<VisualElement>("mask_img");
        var previewElement = root.Q<VisualElement>("preview_img");
        
        // maskElement.style.backgroundImage = mask;
        previewElement.style.backgroundImage = preview;
        
        // maskElement.RegisterCallback<MouseDownEvent>(evt =>
        // {
        //     drawing     = true;
        //     outOfBounds = false;
        // });
        // maskElement.RegisterCallback<MouseUpEvent>(evt =>
        // {
        //     drawing     = false;
        //     outOfBounds = false;
        // });
        // maskElement.RegisterCallback<MouseOutEvent>(evt =>
        // {
        //     if (drawing)
        //     {
        //         outOfBounds = true;
        //     }
        // });
        // maskElement.RegisterCallback<MouseEnterEvent>(evt =>
        // {
        //     if (drawing && evt.pressedButtons == 1)
        //     {
        //         outOfBounds = false;
        //     }
        // });

        // root.Add(new TextElement() { bindingPath = "message" });
        root.Bind(new SerializedObject(this));
    }
}