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
internal sealed class RangeExDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var att = attribute as RangeExAttribute;

        BindableElement slider = null;
        switch (property.propertyType)
        {
            case SerializedPropertyType.Integer:
                slider = new SliderInt( fieldInfo.HumanName() , att.min, att.max);
                break;
            case SerializedPropertyType.Float:
                slider = new SliderInt( fieldInfo.HumanName() , att.min, att.max);

                break;
        }
        slider.showInputField = true;
        slider.BindProperty(property);
        slider.RegisterValueChangedCallback(evt =>
        {
            slider.SetValueWithoutNotify(Mathf.RoundToInt(evt.newValue / (float)att.step) * att.step);
        });
        
        return slider;
    }
}