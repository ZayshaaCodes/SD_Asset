using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public sealed class RangeStepIntAttribute : PropertyAttribute
{
    public readonly int min;
    public readonly int max;
    public readonly int step;

    public RangeStepIntAttribute(int min, int max, int step)
    {
        this.min  = min;
        this.max  = max;
        this.step = step;
    }
}

[CustomPropertyDrawer(typeof(RangeStepIntAttribute))]
internal sealed class RangeStepIntDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var att = attribute as RangeStepIntAttribute;

        SliderInt slider = new SliderInt(fieldInfo.HumanName(), att.min, att.max);
        slider.showInputField = true;
        
        slider.BindProperty(property);
        slider.RegisterValueChangedCallback(
            evt =>
            {
                var roundedVal = Mathf.RoundToInt(evt.newValue / (float)att.step) * att.step;
                slider.SetValueWithoutNotify(roundedVal);
                property.SetUnderlyingValue(roundedVal);
            });

        return slider;
    }
}