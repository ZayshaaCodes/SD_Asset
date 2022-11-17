using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ViewportTestingEditor : EditorWindow
{
    [SerializeField]                  private VisualTreeAsset vta;
    [SerializeField]                  private VisualTreeAsset layerItemVta;
    [SerializeField]                  private string          message;
    [SerializeField, Range(.125f, 3)] private float           zoom = 1;

    //brush settings
    [SerializeField]                     private Color32          drawColor;
    [SerializeField, Range(1,     256)]  private float            drawRad = 32;
    [SerializeField, Range(-.99f, .99f)] private float            brushCurve;
    [SerializeField]                     private List<ImageLayer> imageLayers = new();

    [SerializeField] private Texture2D defaultBase;
    [SerializeField] private Texture2D defaultPaint;
    [SerializeField] private Texture2D defaultMask;

    [SerializeField] private int2 imageSize = new int2(512, 512);

    //VisualElements
    private VisualElement _canvas;

    private VisualElement _imageStackElement;

    private ListView _layersElement;


    //state
    private bool _dragging;
    private bool _drawing;

    [MenuItem("SD/test")]
    public static void ShowWindow()
    {
        CreateWindow<ViewportTestingEditor>().Show();
    }

    private void CreateGUI()
    {
        if (vta == null) return;
        var root = rootVisualElement;
        vta.CloneTree(root);
        var so = new SerializedObject(this);
        root.Bind(so);

        _imageStackElement = root.Q<VisualElement>("imageStack");
        InitLayers();

        // Layers List
        _layersElement = root.Q<ListView>("layersList");
        if (_layersElement != null)
        {
            _layersElement.itemIndexChanged += (from, to) =>
            {
                var c    = _imageStackElement.childCount - 1;
                var item = _imageStackElement[c - from];
                _imageStackElement.RemoveAt(c - from);
                _imageStackElement.Insert(c - to, item);
            };

            _layersElement.makeItem += () =>
            {
                var clone = layerItemVta.CloneTree();
                return clone;
            };
            _layersElement.bindItem += (listElement, i) =>
            {
                if (listElement.Q<VisualElement>("preview") is { } preview)
                {
                    preview.style.backgroundImage = imageLayers[i].image;
                    listElement.name              = imageLayers[i].name;
                }

                if (listElement.Q<Toggle>("visToggle") is { } vToggle)
                {
                    vToggle.RegisterValueChangedCallback(OnVisToggleChange);
                    vToggle.userData = i;
                }
            };

            _layersElement.unbindItem += (listElement, i) =>
            {
                if (listElement.Q<VisualElement>("preview") is { } preview)
                {
                    preview.style.backgroundImage = null;
                    listElement.name              = "";
                }
                // if (listElement.Q<Toggle>("enabledToggle") is { } eToggle)
                //     eToggle.RegisterValueChangedCallback(evt =>
                //     {
                //     });

                if (listElement.Q<Toggle>("visToggle") is { } v)
                    v.UnregisterValueChangedCallback(OnVisToggleChange);
            };
        }

        // Drawing Canvas
        _canvas = root.Q<VisualElement>("canvas");
        if (_canvas != null)
        {
            _canvas.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button == 1)
                {
                    _dragging = true;
                }

                if (evt.button == 0)
                {
                    _drawing = true;
                    DrawEvent(evt);
                }
            });
            _canvas.RegisterCallback<MouseMoveEvent>(DrawEvent);
            _canvas.RegisterCallback<MouseLeaveEvent>(EndDrag);
            _canvas.RegisterCallback<MouseUpEvent>(EndDrag);
            _canvas.RegisterCallback<WheelEvent>(Zoom);
        }

        // Handles
        if (root.Q<IMGUIContainer>("handlesOverlay") is { } handlesOverlay)
        {
            _canvas?.Add(handlesOverlay);
            handlesOverlay.onGUIHandler = () =>
            {
                var center  = handlesOverlay.WorldToLocal(Event.current.mousePosition + handlesOverlay.worldBound.position);
                var zoomRad = drawRad * zoom;
                Handles.DrawWireDisc(center, Vector3.forward, zoomRad);
                float steps = 30;
                for (int i = 1; i <= 30; i++)
                {
                    var x1 = (i - 1) / steps * zoomRad;
                    var x2 = i / steps * zoomRad;
                    var y1 = -Sigmoid(1 - (i - 1) / steps, brushCurve) * zoomRad;
                    var y2 = -Sigmoid(1 - i / steps,       brushCurve) * zoomRad;

                    Handles.DrawLine(center + new Vector2(x1,  y1), center + new Vector2(x2,  y2));
                    Handles.DrawLine(center + new Vector2(-x1, y1), center + new Vector2(-x2, y2));
                }
            };
        }

        // Zoom Slider
        if (root.Q<PropertyField>("zoomSlider") is { } zoomSlider)
        {
            zoomSlider.RegisterValueChangeCallback(evt =>
            {
                SetZoomLevel(evt.changedProperty.floatValue);
            });
        }
    }

    private void OnVisToggleChange(ChangeEvent<bool> evt)
    {
        var index = imageLayers.Count - 1 - (int)((Toggle)evt.target).userData;
        var item  = _imageStackElement.ElementAt(index);
        item.visible = evt.newValue;
    }

    private void InitLayers()
    {
        imageLayers.Clear();
        AddLayer(new("Base", defaultBase is null ? new Texture2D(512, 512) : defaultBase, LayerType.Base));
        if (defaultPaint != null)
            AddLayer(new("Paint 1", defaultPaint));
        else
            AddBlankPaintover("Paint 1");

        if (defaultMask != null)
            AddLayer(new("Mask 1", defaultMask, LayerType.Mask));
        else
            AddBlankMask("Mask 1");
    }

    public void AddLayer(ImageLayer layer)
    {
        var stackElement = new VisualElement() { style = { backgroundImage = layer.image }, name = layer.name };
        stackElement.AddToClassList("stack-image");
        _imageStackElement.Add(stackElement);

        imageLayers.Insert(0, layer);
    }

    public void AddBlankMask(string layerName)
    {
        var texture = new Texture2D(512, 512);
        FillTexture(texture, new(0, 0, 0, 0));
        AddLayer(new(layerName, texture));
    }

    public void AddBlankPaintover(string layerName)
    {
        var texture = new Texture2D(512, 512);
        FillTexture(texture, new(0, 0, 0, 0));
        AddLayer(new(layerName, texture));
    }

    private void DrawEvent<T>(MouseEventBase<T> evt) where T : MouseEventBase<T>, new()
    {
        if (_dragging)
        {
            if (evt.modifiers == EventModifiers.Alt)
            {
                brushCurve = math.clamp(brushCurve - evt.mouseDelta.x / 100, -.99f, .99f);
                drawRad    = math.clamp(drawRad - evt.mouseDelta.y / 2,      1,     256);
            }
            else
            {
                _imageStackElement.transform.position += (Vector3)evt.mouseDelta;
            }
        }

        var mousePos = _imageStackElement.WorldToLocal(evt.mousePosition);


        // if (drawing)
        // {
        //     // var fadecolor = drawColor;
        //     // fadecolor.a = 0;
        //     var texture = (drawMode == DrawMode.Mask ? _maskTexture : _paintTexture);
        //     var data    = texture.GetRawTextureData<Color32>();
        //     new DrawJob()
        //     {
        //         center      = new int2((int)mousePos.x, 512 - (int)mousePos.y),
        //         pixels      = data,
        //         rad         = drawRad * Event.current.pressure,
        //         targetcolor = evt.modifiers == EventModifiers.Alt ? new Color32(0, 0, 0, 0) : drawColor,
        //         sig         = brushCurve
        //     }.Run();
        //     texture.Apply();
        //     data.Dispose();
        // }

        Repaint();
        message = $"{_imageStackElement.transform.scale.x * 100:F2}% | {_imageStackElement.worldBound} | {mousePos}";
    }

    private void SetZoomLevel(float evtNewValue)
    {
        _imageStackElement.transform.scale = Vector3.one * zoom;
    }

    private void Zoom(WheelEvent evt)
    {
        zoom += evt.delta.y < 0 ? -.125f : .125f;
        SetZoomLevel(zoom);
    }

    private static float Sigmoid(float t, float k)
    {
        k *= -1;
        return (k * t - t) / (2 * k * t - k - 1);
    }

    private void FillTexture(Texture2D texture, Color32 color)
    {
        new ClearJob() { data = texture.GetRawTextureData<Color32>(), clearColor = color }.Run(texture.width * texture.height);
        texture.Apply();
    }

    [BurstCompile] public struct ClearJob : IJobFor
    {
        public Color32              clearColor;
        public NativeArray<Color32> data;

        public void Execute(int i)
        {
            data[i] = clearColor;
        }
    }

    [BurstCompile] public struct DrawJob : IJob
    {
        public NativeArray<Color32> pixels;
        public int2                 center;
        public Color32              targetColor;
        public float                rad;
        public float                sig;

        public void Execute()
        {
            for (int j = (int)math.max(0, center.y - rad); j < (int)math.min(512, center.y + rad); j++)
            for (int i = (int)math.max(0, center.x - rad); i < (int)math.min(512, center.x + rad); i++)
            {
                var pixPos   = new int2(i, j);
                var distance = math.length((pixPos - center));
                if (distance < rad)
                {
                    var pixcolor = pixels[i + j * 512];

                    var newColor = Color32.Lerp(pixcolor, targetColor, Sigmoid(1 - distance / rad, sig));

                    pixels[i + j * 512] = newColor;
                }
            }
        }
    }

    private void EndDrag(EventBase evt)
    {
        _dragging = false;
        _drawing  = false;
    }

    private void OnDragEvent(IDragAndDropEvent evt)
    {
        DragAndDrop.visualMode = DragAndDropVisualMode.Generic;

        // switch (evt)
        // {
        //     case DragExitedEvent exitEvent:
        //         Debug.Log($"DragExitedEvent: {exitEvent.target}");
        //
        //         if (DragAndDrop.objectReferences?.Length >= 1)
        //         {
        //             var obj = DragAndDrop.objectReferences[0];
        //             if (obj is Texture2D tex)
        //             {
        //                 _imageStackElement.style.backgroundImage = tex;
        //             }
        //         }
        //
        //         break;
        //     case DragEnterEvent enterEvent:
        //         break;
        //     case DragUpdatedEvent updateEvent:
        //         break;
        // }
    }
}