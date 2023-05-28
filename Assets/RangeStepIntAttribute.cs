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
        this.min = min;
        this.max = max;
        this.step = step;
    }
}

[CustomPropertyDrawer(typeof(RangeStepIntAttribute))]
internal sealed class RangeStepIntDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var att = attribute as RangeStepIntAttribute;

        // Get the current property value and clamp it to the min/max range.
        int value = Mathf.Clamp(property.intValue, att.min, att.max);

        // Calculate the number of steps from the minimum value.
        int steps = Mathf.RoundToInt((value - att.min) / (float)att.step);

        // Draw the slider with steps.
        EditorGUI.BeginChangeCheck();
        steps = EditorGUI.IntSlider(position, label, steps, 0, (att.max - att.min) / att.step);

        if (EditorGUI.EndChangeCheck())
        {
            // If the user has changed the slider value, update the property value accordingly.
            property.intValue = steps * att.step + att.min;
        }
    }

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