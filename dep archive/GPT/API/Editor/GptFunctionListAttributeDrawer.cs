using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GPT.API.Editor
{
    [CustomPropertyDrawer(typeof(GptFunctionListAttribute), true)]
    public class GptFunctionListAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property, label, true);
        }

        //a baseic property drawer for a list of functions
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // Create property container element.
            var container = new VisualElement();

            // Create property fields.
            var agentName = new PropertyField(property.FindPropertyRelative("agentName"));
            var systemMessage = new PropertyField(property.FindPropertyRelative("systemMessage"));
            
            //functions will just be a list of labels with and add button at the bottom with a dropdown to select the function from a predefined list
            var element = new VisualElement();
            var functionList = property.FindPropertyRelative("functions");
            var functionCount = functionList.arraySize;
            for (int i = 0; i < functionCount; i++)
            {
                var funcElement = new VisualElement();
                
                var function = functionList.GetArrayElementAtIndex(i);
                var name = function.FindPropertyRelative("name").stringValue;
                var description = function.FindPropertyRelative("description").stringValue;
                var label = new Label($"{name}: {description}");
                funcElement.Add(label);
                
                //add a button to remove the function from the list
                var removeButton = new Button(() =>
                {
                });
                removeButton.text = "Remove";
                funcElement.Add(removeButton);
                
                element.Add(funcElement);
            }
            
            var dropdown = new PopupField<string>("Add Function", GptFunctionInfo.GetFunctionNames(), 0);
            
            // when the dropdown field changes, add the function to the list if it's not already there
            // using propertyies to get array items to avoid errors
            dropdown.RegisterValueChangedCallback(evt =>
            {
                dropdown.index = 0;

                //if the index is zero, return
                if (evt.newValue == " ") return;
                
                var function = GptFunctionInfo.functions[evt.newValue];
                var functionProp = property.FindPropertyRelative("functions");

                //only insert an item that doesnt already exist in the list
                for (int i = 0; i < functionProp.arraySize; i++)
                {
                    var name = functionProp.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue;
                    if (name == function.name) return;
                }
                functionProp.InsertArrayElementAtIndex(0);
                
                //apply the values from the function into the new item
                var newFunction = functionProp.GetArrayElementAtIndex(0);
                newFunction.FindPropertyRelative("name").stringValue = function.name;
                newFunction.FindPropertyRelative("description").stringValue = function.description;
                
                var label = new Label($"{function.name}: {function.description}");
                element.Add(label);
                
                //apply the changes to the property
                property.serializedObject.ApplyModifiedProperties();
            });
            //set the style of the dropdown so it's just a button

            //a button to clear the function list
            var clearButton = new Button(() =>
            {
                var functionProp = property.FindPropertyRelative("functions");
                functionProp.ClearArray();
                element.Clear();
                
                //apply the changes to the property
                property.serializedObject.ApplyModifiedProperties();
            });
            clearButton.text = "Clear";
            container.Add(clearButton);
            
            //apply the changes to the property
            property.serializedObject.ApplyModifiedProperties();
            
            
            container.Add(dropdown);
            

            // Add fields to the container.
            container.Add(agentName);
            container.Add(systemMessage);
            container.Add(element);

            return container;
        }
    }
}