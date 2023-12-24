using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
// void OnMouseEvent(), this will handle any mose events for that tool
// void OnGUI(), this will draw any gui elements for the tool
// void OnEnable(), this will be called when the tool is selected
// void OnDisable(), this will be called when the tool is deselected

namespace MiniPaint.Editor
{
    public abstract class MiniPaintTool
    {
        public abstract void OnMouseEvent(PaintContext ctx, Vector2 position, int mouseButton, EventType type, EventModifiers modifiers);
        public abstract void OnGUI();
        public abstract void OnEnable();
        public abstract void OnDisable();
        public abstract Texture2D GetIcon();
    }

    [System.Serializable]
    public class PaintContext
    {
        public List<ImageLayer> layers;
        public int activeLayerIndex;
        public Color32 bgColor;
        public Color32 fgColor;
    }
}