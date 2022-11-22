using System.Collections.Generic;
using System.Linq;
using CodiceApp;
using Editor;
using SdEditorApi;
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
    [SerializeField, Range(.125f, 3)] private float           zoom     = 1;
    [SerializeField]                  public  bool            autoSend = true;

    //brush settings
    [SerializeField, ColorUsage(false)]  private Color32          drawColor = new(125, 0, 125, 255);
    [SerializeField, Range(0,     1)]    private float            drawAlpha = 1;
    [SerializeField, Range(1,     256)]  private float            drawRad   = 32;
    [SerializeField, Range(-.99f, .99f)] private float            brushCurve;
    [SerializeField]                     private List<ImageLayer> imageLayers = new();
    [SerializeField]                     private int              selectedLayerIndex;

    private float     lastBrushCurve;
    private Color32   lastBrushColor;
    private Texture2D cursurImage;

    [SerializeField] private Texture2D defaultBase;
    [SerializeField] private Texture2D defaultPaint;
    [SerializeField] private Texture2D defaultMask;
    [SerializeField] private int2      imageSize = new int2(512, 512);

    //VisualElements
    private VisualElement _canvas;
    private VisualElement _imageStackElement;
    private ListView      _layersElement;
    private VisualElement _brushPreviewElement;

    //state
    private bool             _dragging;
    private bool             _drawing;
    private SerializedObject _so;

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
        _so = new SerializedObject(this);
        root.Bind(_so);

        drawColor = new Color32(0, 0, 0, 255);

        var cursorSize = Mathf.CeilToInt(drawRad * 2);
        cursurImage = new Texture2D(1, 1);

        _imageStackElement = root.Q<VisualElement>("imageStack");
        InitLayers();

        //layerL List Buttons
        if (root.Q<Button>("addMaskButton") is { } maskButton)
        {
            maskButton.clicked += () =>
            {
                AddBlankMask("Mask");
            };
        }

        if (root.Q<Button>("addPaintButton") is { } paintButton)
        {
            paintButton.clicked += () =>
            {
                AddBlankPaintover("Paint");
            };
        }

        // Send Button
        if (root.Q<Button>("sendButton") is { } sendButton)
        {
            sendButton.clicked += () =>
            {
                var editor = GetWindow<StableDiffusionEditor>();
                if (editor == null) return;

                editor.requestData.init_images.Clear();

                editor.requestData.init_images.Add(BuildOutputImage());
                editor.requestData.mask = BuildOutputMask();
            };
        }

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
            _layersElement.onSelectedIndicesChange += (i) =>
            {
                if (i.Count() == 0)
                    return;
                selectedLayerIndex = i.First();
            };

            _layersElement.makeItem += () =>
            {
                var clone = layerItemVta.CloneTree();
                return clone;
            };
            _layersElement.bindItem += OnLayersElementBindItem;

            _layersElement.unbindItem += OnLayersElementUnbindItem;
        }

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

                if (evt.button == 0)
                {
                    _drawing = true;
                    DrawEvent(evt);
                }
            });
            _canvas.RegisterCallback<MouseMoveEvent>(DrawEvent);
            _canvas.RegisterCallback<MouseLeaveEvent>(OnEndDrag);
            _canvas.RegisterCallback<MouseUpEvent>(OnEndDrag);
            _canvas.RegisterCallback<WheelEvent>(Zoom);
        }

        // Draw Handles
        if (root.Q<IMGUIContainer>("handlesOverlay") is { } handlesOverlay)
        {
            _canvas?.Add(handlesOverlay);
            _brushPreviewElement = new VisualElement() { style = { backgroundImage = cursurImage, position = Position.Absolute } };
            handlesOverlay.Add(_brushPreviewElement);
            handlesOverlay.onGUIHandler = () =>
            {
                UpdateBrushTexture(); 

                var center  = handlesOverlay.WorldToLocal(Event.current.mousePosition + handlesOverlay.worldBound.position);
                var zoomRad = drawRad * zoom;
                _brushPreviewElement.style.left   = center.x - drawRad * zoom;
                _brushPreviewElement.style.top    = center.y - drawRad * zoom;
                _brushPreviewElement.style.width  = drawRad * 2 * zoom;
                _brushPreviewElement.style.height = drawRad * 2 * zoom;
                Handles.color                     = new Color(1f, 1f, 1f, 0.5f);
                Handles.matrix                    = Matrix4x4.TRS(center, Quaternion.identity, Vector3.one * zoom);

                Handles.DrawWireDisc(Vector3.zero, Vector3.forward, drawRad);

                // float steps = 30;
                // for (int i = 1; i <= 30; i++)
                // {
                //     var x1 = (i - 1) / steps * zoomRad;
                //     var x2 = i / steps * zoomRad;
                //     var y1 = -Sigmoid(1 - (i - 1) / steps, brushCurve) * zoomRad;
                //     var y2 = -Sigmoid(1 - i / steps,       brushCurve) * zoomRad;
                //     // Handles.DrawLine(center + new Vector2(x1,  y1), center + new Vector2(x2,  y2));
                //     // Handles.DrawLine(center + new Vector2(-x1, y1), center + new Vector2(-x2, y2));
                //     
                // }
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

    public Texture2D BuildOutputMask()
    {
        Texture2D ret        = null;
        var       maskImages = imageLayers.FindAll(layer => layer.type == LayerType.Mask && layer.active);
        if (maskImages.Count > 0)
        {
            ret = new(512, 512);
            
            maskImages[0].image.CopyTexture(ret); 
        }

        for (var i = 1; i < maskImages.Count; i++)
        {
            new MixMaskJob()
            {
                baseTexture    = ret.GetRawTextureData<Color32>(),
                overlayTexture = maskImages[i].image.GetRawTextureData<Color32>(),
                overlayOpacity = maskImages[i].opacity
            }.Run(ret.width * ret.height);
        }

        if (ret != null) ret.Apply();

        return ret;
    }

    public Texture2D BuildOutputImage()
    {
        ImageLayer baseImage = imageLayers.Find(layer => layer.type == LayerType.Base && layer.active);

        Texture2D ret = null;

        if (baseImage?.image != null)
        {
            ret = new Texture2D(baseImage.image.width, baseImage.image.height);
            baseImage.image.CopyTexture(ret);
        }
        
        var paintImages = imageLayers.FindAll(layer => layer.type == LayerType.Paint && layer.active);
        if (paintImages.Count > 0)
        {
            ret = new(512, 512);
            paintImages[0].image.CopyTexture(ret);
        }
        
        for (var i = 1; i < paintImages.Count; i++)
        {
            new MixTextureJob()
            {
                baseTexture    = ret!.GetRawTextureData<Color32>(),
                overlayTexture = paintImages[i].image.GetRawTextureData<Color32>(),
                overlayOpacity = paintImages[i].opacity
            }.Run(ret.width * ret.height);
        }
        if (ret != null) ret.Apply();

        return ret;
    }

    private void OnLayersElementBindItem(VisualElement listElement, int i)
    {
        if (listElement.Q<VisualElement>("preview") is { } preview)
        {
            preview.style.backgroundImage = imageLayers[i].image;
            listElement.name              = imageLayers[i].name;
        }

        listElement.AddManipulator(new ContextualMenuManipulator(evt =>
        {
            evt.menu.AppendAction("delete", OnDeleteLayer, CtxStatus, i);
            // evt.menu.AppendAction("clear", OnDeleteLayer, CtxStatus, i );
        }));

        if (listElement.Q<Slider>("opacitySlider") is { } opacSlider)
        {
            opacSlider.RegisterValueChangedCallback(evt =>
            {
                var element = GetStackElement(i);
                element.style.opacity = opacSlider.value / 100;
            });
        }

        if (listElement.Q<Toggle>("visToggle") is { } vToggle)
        {
            vToggle.RegisterValueChangedCallback(OnVisToggleChange);
            vToggle.userData = i;
        }
    }

    private void OnDeleteLayer(DropdownMenuAction obj)
    {
        // Debug.Log(obj.userData);
        RemoveLayer((int)obj.userData);
    }

    private DropdownMenuAction.Status CtxStatus(DropdownMenuAction obj)
    {
        return DropdownMenuAction.Status.Normal;
    }

    private void OnLayersElementUnbindItem(VisualElement listElement, int i)
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

        if (listElement.Q<Toggle>("visToggle") is { } v) v.UnregisterValueChangedCallback(OnVisToggleChange);
    }

    private VisualElement GetStackElement(int i)
    {
        return _imageStackElement[imageLayers.Count - 1 - i];
    }

    private void OnVisToggleChange(ChangeEvent<bool> evt)
    {
        var item = GetStackElement((int)((Toggle)evt.target).userData);
        item.visible = evt.newValue;
    }

    private void InitLayers()
    {
        imageLayers.Clear();

        var baseCopy = new Texture2D(512, 512);
        if (defaultBase != null)
        {
            defaultBase.CopyTexture(baseCopy);
        }

        AddLayer(new("Base", baseCopy, LayerType.Base));

        if (defaultPaint != null)
        {
            var newCopy = new Texture2D(defaultPaint.width, defaultPaint.height);
            defaultPaint.CopyTexture(newCopy);
            AddLayer(new("Paint 1", newCopy));
        }
        else
            AddBlankPaintover("Paint 1");

        if (defaultMask != null)
        {
            var newCopy = new Texture2D(defaultMask.width, defaultMask.height);
            defaultMask.CopyTexture(newCopy);
            AddLayer(new("Mask 1", newCopy, LayerType.Mask));
        }
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
        FillTexture(texture, new(0, 0, 0, 255));
        AddLayer(new(layerName, texture) { type = LayerType.Mask }); 
    }

    public void AddBlankPaintover(string layerName)
    {
        var texture = new Texture2D(512, 512);
        FillTexture(texture, new(0, 0, 0, 0));
        AddLayer(new(layerName, texture) { type = LayerType.Paint });
    }

    private void RemoveLayer(int index)
    {
        var layer = imageLayers[index];
        if (layer.type != LayerType.Base)
        {
            _imageStackElement.RemoveAt(imageLayers.Count - 1 - index);
            imageLayers.RemoveAt(index);
        }
    }

    private void DrawEvent<T>(MouseEventBase<T> evt) where T : MouseEventBase<T>, new()
    {
        if (_dragging)
        {
            if (evt.modifiers == EventModifiers.Alt)
            {
                brushCurve = math.clamp(brushCurve + evt.mouseDelta.x / 100, -.99f, .99f);
                drawRad    = math.clamp(drawRad - evt.mouseDelta.y / 2,      1,     256);
            }
            else
            {
                _imageStackElement.parent.transform.position += (Vector3)evt.mouseDelta;
            }
        }

        var mousePos = _imageStackElement.parent.WorldToLocal(evt.mousePosition);


        if (_drawing)
        {
            // var fadecolor = drawColor;
            // fadecolor.a = 0;
            var layer   = imageLayers[selectedLayerIndex];
            var texture = layer.image;
            var alpha   = (byte)(drawAlpha * 255);

            var clearColor = layer.type == LayerType.Mask ? new Color32(0,     0,     0,     255) : new(0, 0, 0, 0);
            var brushColor = layer.type == LayerType.Mask ? new Color32(alpha, alpha, alpha, 255) : new(drawColor.r, drawColor.g, drawColor.b, alpha);

            var data = texture.GetRawTextureData<Color32>();
            new DrawJob()
            {
                center      = new int2((int)mousePos.x, 512 - (int)mousePos.y),
                pixels      = data,
                rad         = drawRad * Event.current.pressure,
                targetColor = evt.modifiers == EventModifiers.Alt ? clearColor : brushColor,
                sig         = brushCurve
            }.Run();
            texture.Apply();
            data.Dispose();
        }

        Repaint();
        message = $"{_imageStackElement.transform.scale.x * 100:F2}% | {_imageStackElement.worldBound} | {mousePos}";
    }

    private void UpdateBrushTexture()
    {
        int size = Mathf.CeilToInt(drawRad * 2);

        var colorChange = lastBrushColor.r != drawColor.r
                       || lastBrushColor.g != drawColor.g
                       || lastBrushColor.b != drawColor.b;

        if (brushCurve != lastBrushCurve || colorChange || cursurImage.width != size)
        {
            if (cursurImage.width != size)
            {
                cursurImage.Reinitialize(size, size);
            }

            new UpdateBrushTextureJob()
            {
                pixels = cursurImage.GetRawTextureData<Color32>(),
                rad    = drawRad,
                sig    = brushCurve,
                size   = size,
                color  = drawColor
            }.Run();
            cursurImage.Apply();
        }

        lastBrushColor = drawColor;
        lastBrushCurve = brushCurve;
    }

    private void SetZoomLevel(float newVal)
    {
        _imageStackElement.parent.transform.scale = Vector3.one * newVal;
    }

    private void Zoom(WheelEvent evt)
    {
        zoom += evt.delta.y < 0 ? -.125f : .125f;
        SetZoomLevel(zoom);
    }

    private void FillTexture(Texture2D texture, Color32 color)
    {
        new ClearJob() { data = texture.GetRawTextureData<Color32>(), clearColor = color }.Run(texture.width * texture.height);
        texture.Apply();
    }

    private void OnEndDrag(EventBase evt)
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

    public void SetBaseImage(Texture2D sdImageImage)
    {
        ImageLayer baseImage = imageLayers.Find(layer => layer.type == LayerType.Base && layer.active);
        if (baseImage != null)
        {
            sdImageImage.CopyTexture(baseImage.image);
        }
    }
}