﻿using System.Collections.Generic;
using System.Linq;
using MiniPaint.Editor;
using SdEditorApi;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class MiniPaintEditorWindow : EditorWindow
{
    [SerializeField] private VisualTreeAsset vta;
    [SerializeField] private VisualTreeAsset layerItemVta;
    [SerializeField] private string message;

    [SerializeField] private PaintContext context = new();
    // private Texture2D cursurImage;

    [SerializeField] private List<MiniPaintTool> tools = new();
    [SerializeField] private MiniPaintTool activeTool;

    // [SerializeField] private Texture2D defaultBase;
    [SerializeField] private Texture2D defaultPaint;
    // [SerializeField] private Texture2D defaultMask;
    [SerializeField] private int2 imageSize = new int2(512, 512);

    //VisualElements
    private VisualElement _canvas;
    private VisualElement _imageStackElement;
    private ListView _layersElement;
    private VisualElement _brushPreviewElement;

    //state
    private bool _dragging;
    private Vector2 _lastMousePos;
    [SerializeField, Range(.125f, 3)] private float _zoom = 1;

    // private bool             _drawing;
    private SerializedObject _so;

    [MenuItem("Tools/MiniPaint")]
    public static void ShowWindow()
    {
        CreateWindow<MiniPaintEditorWindow>().Show();
    }

    private void CreateGUI()
    {
        if (vta == null) return;
        var root = rootVisualElement;
        vta.CloneTree(root);
        _so = new SerializedObject(this);
        root.Bind(_so);

        // cursurImage = new(1, 1);
        // brush.GetCursorImage(cursurImage, fgColor);

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

        // // Send Button
        // if (root.Q<Button>("sendButton") is { } sendButton)
        // {
        //     // sendButton.clicked += () =>
        //     // {
        //     //     var editor = GetWindow<StableDiffusionEditor>();
        //     //     if (editor == null) return;

        //     //     editor.requestData.init_images.Clear();

        //     //     editor.requestData.init_images.Add(BuildOutputImage());
        //     //     editor.requestData.mask = BuildOutputMask();
        //     // };
        // }

        // Layers List
        _layersElement = root.Q<ListView>("layersList");
        if (_layersElement != null)
        {
            _layersElement.itemIndexChanged += (from, to) =>
            {
                var c = _imageStackElement.childCount - 1;
                var item = _imageStackElement[c - from];
                _imageStackElement.RemoveAt(c - from);
                _imageStackElement.Insert(c - to, item);
            };
            _layersElement.onSelectedIndicesChange += (i) =>
            {
                if (i.Count() == 0)
                    return;
                context.activeLayerIndex = i.First();
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
                activeTool?.OnMouseEvent(context, evt.localMousePosition, evt.button, EventType.MouseDown, evt.modifiers);
            });
            _canvas.RegisterCallback<MouseMoveEvent>(evt =>
            {
                _lastMousePos = _imageStackElement.parent.WorldToLocal(evt.mousePosition);
                activeTool?.OnMouseEvent(context, evt.localMousePosition, evt.button, EventType.MouseMove, evt.modifiers);
                UpdateMessage();
            });
            _canvas.RegisterCallback<MouseUpEvent>(evt =>
            {
                activeTool?.OnMouseEvent(context, evt.localMousePosition, evt.button, EventType.MouseUp, evt.modifiers);
            });
            _canvas.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                activeTool?.OnMouseEvent(context, evt.localMousePosition, evt.button, EventType.MouseLeaveWindow, evt.modifiers);
            });
            // _canvas.RegisterCallback<MouseMoveEvent>(DrawEvent);
            // _canvas.RegisterCallback<MouseLeaveEvent>(OnEndDrag);
            // _canvas.RegisterCallback<MouseUpEvent>(OnEndDrag);
            _canvas.RegisterCallback<WheelEvent>(Zoom);
        }
    }

    private void UpdateMessage()
    {
        var mousePos = _imageStackElement.parent.WorldToLocal(Event.current.mousePosition);
        message = $"{_zoom * 100:F2}% | {mousePos}";
    }

    private void OnLayersElementBindItem(VisualElement listElement, int i)
    {
        if (listElement.Q<VisualElement>("preview") is { } preview)
        {
            preview.style.backgroundImage = context.layers[i].image;
            listElement.name = context.layers[i].name;
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
            listElement.name = "";
        }
        // if (listElement.Q<Toggle>("enabledToggle") is { } eToggle)
        //     eToggle.RegisterValueChangedCallback(evt =>
        //     {
        //     });

        if (listElement.Q<Toggle>("visToggle") is { } v) v.UnregisterValueChangedCallback(OnVisToggleChange);
    }

    private VisualElement GetStackElement(int i)
    {
        return _imageStackElement[context.layers.Count - 1 - i];
    }

    private void OnVisToggleChange(ChangeEvent<bool> evt)
    {
        var item = GetStackElement((int)((Toggle)evt.target).userData);
        item.visible = evt.newValue;
    }

    private void InitLayers()
    {
        context.layers.Clear();

        // var baseCopy = new Texture2D(512, 512);
        // if (defaultBase != null)
        // {
        //     defaultBase.CopyTexture(baseCopy);
        // }

        // AddLayer(new("Base", baseCopy, LayerType.Base));

        if (defaultPaint != null)
        {
            var newCopy = new Texture2D(defaultPaint.width, defaultPaint.height);
            defaultPaint.CopyTexture(newCopy);
            AddLayer(new("Paint 1", newCopy));
        }
        else
            AddBlankPaintover("Paint 1");

        // if (defaultMask != null)
        // {
        //     var newCopy = new Texture2D(defaultMask.width, defaultMask.height);
        //     defaultMask.CopyTexture(newCopy);
        //     AddLayer(new("Mask 1", newCopy, LayerType.Mask));
        // }
        // else
        //     AddBlankMask("Mask 1");
    }


    public void AddLayer(ImageLayer layer)
    {
        var stackElement = new VisualElement() { style = { backgroundImage = layer.image }, name = layer.name };
        stackElement.AddToClassList("stack-image");
        _imageStackElement.Add(stackElement);

        context.layers.Insert(0, layer);
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
        var layer = context.layers[index];
        if (layer.type != LayerType.Base)
        {
            _imageStackElement.RemoveAt(context.layers.Count - 1 - index);
            context.layers.RemoveAt(index);
        }
    }

    // private void DrawEvent<T>(MouseEventBase<T> evt) where T : MouseEventBase<T>, new()
    // {
    //     if (_dragging)
    //     {
    //         if (evt.modifiers == EventModifiers.Alt) 
    //         {
    //             // brush.falloff = math.clamp(brush.falloff + evt.mouseDelta.x / 100, -.99f, .99f);
    //             // brush.radius  = math.clamp(brush.radius - evt.mouseDelta.y / 2,    1,     256);
    //             // brush.GetCursorImage(cursurImage, fgColor);
    //         }
    //         else
    //         {
    //             _imageStackElement.parent.transform.position += (Vector3)evt.mouseDelta;
    //         }
    //     }

    //     var mousePos = _imageStackElement.parent.WorldToLocal(evt.mousePosition);


    //     if (_drawing)
    //     {
    //         // var fadecolor = drawColor;
    //         // fadecolor.a = 0;
    //         // var layer   = context.layers[selectedLayerIndex];
    //         // var texture = layer.image;
    //         // // var a       = (byte)(alpha * 255);

    //         // var clearColor = layer.type == LayerType.Mask ? new Color32(0, 0, 0, 255) : new(0, 0, 0, 0);
    //         // var brushColor = layer.type == LayerType.Mask ? new Color32(a, a, a, 255) : new(fgColor.r, fgColor.g, fgColor.b, a);

    //         // var data = texture.GetRawTextureData<Color32>();
    //         // new DrawJob()
    //         // {
    //         //     center      = new int2((int)mousePos.x, 512 - (int)mousePos.y),
    //         //     pixels      = data,
    //         //     // rad         = brush.radius * Event.current.pressure,
    //         //     rad         = 10,
    //         //     targetColor = evt.modifiers == EventModifiers.Alt ? clearColor : brushColor,
    //         //     // sig         = brush.falloff
    //         //     sig = .1f
    //         // }.Run();
    //         // texture.Apply();
    //         // data.Dispose();
    //     }

    //     Repaint();
    //     message = $"{_imageStackElement.transform.scale.x * 100:F2}% | {_imageStackElement.worldBound} | {mousePos}";
    // }

    private void UpdateBrushTexture()
    {
    }

    private void SetZoomLevel(float newVal)
    {
        _imageStackElement.parent.transform.scale = Vector3.one * newVal;
    }

    private void Zoom(WheelEvent evt)
    {
        _zoom += evt.delta.y < 0 ? -.125f : .125f;
        SetZoomLevel(_zoom);
        UpdateMessage();
    }

    private void FillTexture(Texture2D texture, Color32 color)
    {
        new ClearJob() { data = texture.GetRawTextureData<Color32>(), clearColor = color }.Run(texture.width * texture.height);
        texture.Apply();
    }

    // private void OnEndDrag(EventBase evt)
    // {
    //     _dragging = false;
    //     // _drawing  = false;
    // }

    // private void OnDragEvent(IDragAndDropEvent evt)
    // {
    //     DragAndDrop.visualMode = DragAndDropVisualMode.Generic;

    //     // switch (evt)
    //     // {
    //     //     case DragExitedEvent exitEvent:
    //     //         Debug.Log($"DragExitedEvent: {exitEvent.target}");
    //     //
    //     //         if (DragAndDrop.objectReferences?.Length >= 1)
    //     //         {
    //     //             var obj = DragAndDrop.objectReferences[0];
    //     //             if (obj is Texture2D tex)
    //     //             {
    //     //                 _imageStackElement.style.backgroundImage = tex;
    //     //             }
    //     //         }
    //     //
    //     //         break;
    //     //     case DragEnterEvent enterEvent:
    //     //         break;
    //     //     case DragUpdatedEvent updateEvent:
    //     //         break;
    //     // }
    // }

    public void SetBaseImage(Texture2D sdImageImage)
    {
        ImageLayer baseImage = context.layers.Find(layer => layer.type == LayerType.Base && layer.active);
        if (baseImage != null)
        {
            sdImageImage.CopyTexture(baseImage.image);
        }
    }
}