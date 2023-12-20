using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

// namespace StableDiffusion
// {
//     [CustomPropertyDrawer(typeof(SdConfig)), System.Serializable]
//     public class ConfigDrawer : PropertyDrawer
//     {
//         public VisualTreeAsset UiAsset;
//         public StyleSheet styleSheet;
//
//         public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//         {
//             SerializedProperty enabledFieldsProperty = property.FindPropertyRelative("enabledFields");
//
//             //create and "add override" dropdown menu that will add the selected property to the enabledFields list when clicked
//             Rect dropdownPosition = new Rect(position.x, position.y, position.width, 15);
//
//             EditorGUI.BeginChangeCheck();
//
//             //render the dropdown menu
//             int selected = EditorGUI.Popup(dropdownPosition, "Add Override", 0, SdConfig.FieldNames);
//
//             //if the user selected a property, add it to the enabledFields list
//             if (selected > 0)
//             {
//                 // cant add the same property twice, check if it already exists in the enabledFields list
//                 bool alreadyExists = false;
//                 for (int i = 0; i < enabledFieldsProperty.arraySize; i++)
//                 {
//                     if (enabledFieldsProperty.GetArrayElementAtIndex(i).stringValue == SdConfig.FieldNames[selected])
//                     {
//                         alreadyExists = true;
//                     }
//                 }
//                 // if it doesnt already exist, add it
//                 if (!alreadyExists)
//                 {
//                     enabledFieldsProperty.InsertArrayElementAtIndex(enabledFieldsProperty.arraySize);
//                     enabledFieldsProperty.GetArrayElementAtIndex(enabledFieldsProperty.arraySize - 1).stringValue = SdConfig.FieldNames[selected];
//                 }
//             }
//
//             //shift the position down
//             position.y += 35;
//
//
//             // for each string in the enabledFields list, render a property field and render a property field for the corresponding property, do null checks
//             for (int i = 0; i < enabledFieldsProperty.arraySize; i++)
//             {
//                 var enabledField = enabledFieldsProperty.GetArrayElementAtIndex(i);
//                 string enabledFieldString = enabledField.stringValue;
//
//                 SerializedProperty propertyToRender = property.FindPropertyRelative(enabledFieldString);
//                 if (propertyToRender != null)
//                 {
//                     float propertyHeight = EditorGUI.GetPropertyHeight(propertyToRender, GUIContent.none);
//                     Rect propertyRect = new Rect(position.x, position.y, position.width, propertyHeight);
//                     //add an 'x' button to the left of the property field that will remove the property from the enabledFields list when clicked
//                     Rect removeButtonRect = new Rect(propertyRect.x, propertyRect.y, 20, propertyRect.height);
//                     if (GUI.Button(removeButtonRect, "x"))
//                     {
//                         enabledFieldsProperty.DeleteArrayElementAtIndex(i);
//                     }
//                     // an up and down button to send the property up or down in the list, leave the button labels blank, stack them vertically
//                     Rect upButtonRect = new Rect(propertyRect.x + 20, propertyRect.y, 20, propertyRect.height / 2);
//                     Rect downButtonRect = new Rect(propertyRect.x + 20, propertyRect.y + propertyRect.height / 2, 20, propertyRect.height / 2);
//                     if (GUI.Button(upButtonRect, ""))
//                     {
//                         enabledFieldsProperty.MoveArrayElement(i, i - 1);
//                     }
//                     if (GUI.Button(downButtonRect, ""))
//                     {
//                         enabledFieldsProperty.MoveArrayElement(i, i + 1);
//                     }
//
//                     // render the property field shifted to the right with the label
//                     EditorGUI.PropertyField(new Rect(propertyRect.x + 50, propertyRect.y, propertyRect.width - 50, propertyRect.height), propertyToRender, new GUIContent(propertyToRender.displayName));
//                     position.y += propertyHeight + EditorGUIUtility.standardVerticalSpacing;
//                 }
//             }
//
//             // if (GUI.Button(new Rect(position.x, position.y, position.width, 20), "Log Config To Json"))
//             // {
//             //     Debug.Log(((SdConfigTesting)property.serializedObject.targetObject).config.ToJson());
//             // }
//
//
//             if (EditorGUI.EndChangeCheck())
//             {
//                 property.serializedObject.ApplyModifiedProperties();
//             }
//         }
//
//         public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//         {
//             SerializedProperty enabledFieldsProperty = property.FindPropertyRelative("enabledFields");
//             float height = 0;
//             // for each string in the enabledFields list, render a property field and render a property field for the corresponding property, do null checks
//             for (int i = 0; i < enabledFieldsProperty.arraySize; i++)
//             {
//                 string enabledField = enabledFieldsProperty.GetArrayElementAtIndex(i).stringValue;
//                 SerializedProperty propertyToRender = property.FindPropertyRelative(enabledField);
//                 if (propertyToRender != null)
//                 {
//                     height += EditorGUI.GetPropertyHeight(propertyToRender, GUIContent.none, true);
//                 }
//             }
//
//
//             return height + 60;
//         }
//     }
//
//
// }