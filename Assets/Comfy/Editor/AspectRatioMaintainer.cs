using UnityEngine;
using UnityEngine.UIElements;

public class AspectRatioMaintainer : VisualElement
{
    [SerializeField] private float aspectRatio = 2.0f; // Width:Height ratio

    public AspectRatioMaintainer()
    {
        RegisterCallback<GeometryChangedEvent>(e => MaintainAspectRatio());
    }

    private void MaintainAspectRatio()
    {
        float parentWidth = this.resolvedStyle.width;
        float newHeight = parentWidth / aspectRatio;
        style.height = newHeight;
    }

    // Factory for UIBuilder
    public new class UxmlFactory : UxmlFactory<AspectRatioMaintainer, UxmlTraits> { }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        UxmlFloatAttributeDescription m_AspectRatio = new UxmlFloatAttributeDescription { name = "aspect-ratio", defaultValue = 2.0f };

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);
            AspectRatioMaintainer arm = ve as AspectRatioMaintainer;
            if (arm != null)
            {
                arm.aspectRatio = m_AspectRatio.GetValueFromBag(bag, cc);
            }
        }
    }

    // Property to expose aspectRatio to UxmlTraits
    public float AspectRatio
    {
        get { return aspectRatio; }
        set { aspectRatio = value; MaintainAspectRatio(); }
    }
}
