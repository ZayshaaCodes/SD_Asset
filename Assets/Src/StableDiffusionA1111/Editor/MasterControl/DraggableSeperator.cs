using UnityEngine;
using UnityEngine.UIElements;

public class DraggableSeperator : MouseManipulator
{
    private Vector2 m_Start;
    private float startWidth;

    bool flipped = false;

    public DraggableSeperator() { }

    public DraggableSeperator(bool flipped)
    {
        this.flipped = flipped;
    }
    
    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<MouseDownEvent>(OnMouseDown);
        target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        target.RegisterCallback<MouseUpEvent>(OnMouseUp);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
        target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
        target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
    }

    private void OnMouseDown(MouseDownEvent evt)
    {
        m_Start = evt.mousePosition;
        startWidth = target.parent.resolvedStyle.width;
        target.CaptureMouse();
        evt.StopPropagation();
    }

    private void OnMouseUp(MouseUpEvent evt)
    {
        if (target.HasMouseCapture())
        {
            target.ReleaseMouse();
            evt.StopPropagation();
        }
    }

    private void OnMouseMove(MouseMoveEvent evt)
    {
        if (!target.HasMouseCapture())
            return;

        float deltaX = evt.mousePosition.x - m_Start.x;

        float min = target.parent.resolvedStyle.minWidth.value;
        float max = target.parent.resolvedStyle.maxWidth.value;
        if (max <= min) max = float.MaxValue;
        if (min <= 0) min = 100;

        // //get the clampped value from the min and max width of the parent
        var clammedWidth = Mathf.Clamp(startWidth + (flipped ? -1 : 1) * deltaX, min, max);
        target.parent.style.width = clammedWidth;

        evt.StopPropagation();
    }
}