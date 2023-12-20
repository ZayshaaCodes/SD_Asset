using System.Collections.Generic;
using GPT.API.AiFunctions;
using UnityEditor;
using UnityEngine.UIElements;

namespace GPT.API.Editor
{
    public class GptFunctionListWindow : EditorWindow
    {
        [GptFunctionList] public List<GptFunction> functions = new List<GptFunction>();

        [MenuItem("GPT/Function List")]
        public static void ShowWindow()
        {
            GetWindow<GptFunctionListWindow>("Function List");
        }

        //uielements default inspector
        private void CreateGUI()
        {
            var root = rootVisualElement;

            BuildFunctionList();

            foreach (var function in functions)
            {
                var container = new VisualElement();

                //create some labels to list the functions
                var label = new Label(function.name);
                label.style.fontSize = 20;
                container.Add(label);

                var description = new Label(function.description);
                description.style.fontSize = 10;
                container.Add(description);

                //create a button to copy the function name to the clipboard
                var button = new Button(() =>
                {
                    EditorGUIUtility.systemCopyBuffer = function.name;
                });
                button.text = "Copy Name";
                container.Add(button);
                root.Add(container);
            }
        }

        //use reflection to get all the GptFunctions and add them to the list
        private void BuildFunctionList()
        {
            functions.Clear();

            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsSubclassOf(typeof(GptFunction)) && type.GetCustomAttributes(typeof(AutoLinkAttribute), true).Length > 0)
                    {
                        // Debug.Log($"Found function: {type.Name}");
                        var instance = (GptFunction)System.Activator.CreateInstance(type);
                        functions.Add(instance);
                    }
                }
            }
        }
    }

    //custom propertydrawer for fields tagged with GptFunctionList attribute
}