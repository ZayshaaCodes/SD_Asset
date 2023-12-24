using UnityEngine;
// void OnMouseEvent(), this will handle any mose events for that tool
// void OnGUI(), this will draw any gui elements for the tool
// void OnEnable(), this will be called when the tool is selected
// void OnDisable(), this will be called when the tool is deselected

namespace MiniPaint.Editor
{
    [System.Serializable]
    public class PaintTool : MiniPaintTool
    {
        public override void OnMouseEvent(PaintContext ctx, Vector2 position, int mouseButton, EventType type, EventModifiers modifiers)
        {
            // Handle mouse events here
        }

        public override void OnGUI()
        {
            // Draw GUI elements here
        }

        public override void OnEnable()
        {
            // Called when the tool is selected
        }

        public override void OnDisable()
        {
            // Called when the tool is deselected
        }

        public override Texture2D GetIcon()
        {
            return null;
        }
    }
}