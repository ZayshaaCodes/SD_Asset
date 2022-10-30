using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[AttributeUsage (AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public sealed class RangeExAttribute : PropertyAttribute
{
    public readonly int min;
    public readonly int max;
    public readonly int step;
 
    public RangeExAttribute (int min, int max, int step)
    {
        this.min  = min;
        this.max  = max;
        this.step = step;
    }
}

[CustomPropertyDrawer (typeof(RangeExAttribute))]
internal sealed class RangeExDrawer : PropertyDrawer
{
    private int value = 512;
 
    //
    // Methods
    //

    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
    {
        var rangeAttribute = (RangeExAttribute)base.attribute;
 
        if (property.propertyType == SerializedPropertyType.Integer)
        {
            value = EditorGUI.IntSlider (position, label, value, rangeAttribute.min, rangeAttribute.max);
 
            value             = (value / rangeAttribute.step) * rangeAttribute.step;
            property.intValue = value;
        }
        else
        {
            EditorGUI.LabelField (position, label.text, "Use Range with float or int.");
        }
    }
}