using UnityEditor;
using UnityEngine;

namespace StableDiffusion
{
    //do a default drawer for the scriptable object to start with
    // [CustomPropertyDrawer(typeof(ControlNetModulePreset))]
    public class ControlNetArgsSoDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //get the scriptable object
            var so = property.objectReferenceValue as ControlNetModulePreset;

            //draw the default inspector
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PropertyField(position, property, label, true);
            position.y += EditorGUI.GetPropertyHeight(property, label, true) + EditorGUIUtility.standardVerticalSpacing;
            
            if (so != null)
            {
                var soProp = new SerializedObject(so);
                var prop = soProp.GetIterator();
                prop.NextVisible(true);
                while (prop.NextVisible(false))
                {
                    var propHeight = EditorGUI.GetPropertyHeight(prop, true);
                    var propertyRect = new Rect(position.x, position.y, position.width, propHeight);
                    EditorGUI.PropertyField(propertyRect, prop, true);
                    position.y += propHeight + EditorGUIUtility.standardVerticalSpacing;
                }
                soProp.ApplyModifiedProperties();
            }

            EditorGUI.EndProperty();
        }

        //get height of the property
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var so = property.objectReferenceValue as ControlNetModulePreset;
            if (so == null) return base.GetPropertyHeight(property, label);
            var soProp = new SerializedObject(so);
            var prop = soProp.GetIterator();
            prop.NextVisible(true);
            float height = EditorGUI.GetPropertyHeight(property, label, true) + EditorGUIUtility.standardVerticalSpacing;
            while (prop.NextVisible(false))
            {
                height += EditorGUI.GetPropertyHeight(prop, true) + EditorGUIUtility.standardVerticalSpacing;
            }
            return height;
        }
    }

}
