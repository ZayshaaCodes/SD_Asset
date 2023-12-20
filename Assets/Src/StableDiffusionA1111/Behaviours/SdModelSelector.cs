using System.Collections.Generic;
using System.IO;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using StableDiffusion;

namespace StableDiffusion
{
    //this will just display a dropdown with all the models available, when one is selected it will do a request to an api to switch the model
    [ExecuteAlways, System.Serializable]
    public class SdModelSelector
    {
        public static List<StableDiffusion.SdApi.SdModelInfo> models = new List<StableDiffusion.SdApi.SdModelInfo>();
        static SdModelSelector()
        {
            if (File.Exists("Assets/StableDiffusion/cache/models.json"))
            {
                var jsondata = File.ReadAllText("Assets/StableDiffusion/cache/models.json");
                models = Newtonsoft.Json.JsonConvert.DeserializeObject<List<StableDiffusion.SdApi.SdModelInfo>>(jsondata);
            }
        }

        public static void UpdateModels(SdApi api) 
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(api.GetModels(mdlList =>
            {
                Debug.Log( $"Got {mdlList.Count} models");
                // save models to disk in Assets/StableDiffusion/cache/models.json, use use Unity IO lib
                var jsondata = Newtonsoft.Json.JsonConvert.SerializeObject(mdlList, Newtonsoft.Json.Formatting.Indented);
                //create the dir if it's missing
                Directory.CreateDirectory("Assets/StableDiffusion/cache");
                File.WriteAllText("Assets/StableDiffusion/cache/models.json", jsondata);
                models = mdlList;
            }));

        }

        public void SetModel(string newModelName)
        {
            // just invoke the event
            OnValueChanged?.Invoke(newModelName);
        }

        public SdApi.SdModelInfo selectedModel;
        // evnt to be fired when the model is changed
        public delegate void OnValueChangedDelegate(string newModelName);
        public event OnValueChangedDelegate OnValueChanged;
    }
    
    //custom property drawer that will use models data to create a dropdown that will update the selected model when changed
    [CustomPropertyDrawer(typeof(SdModelSelector))]
    public class SdModelSelectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // dig down to get the selected model property using 
            var modelSelector = property.FindPropertyRelative("selectedModel");
            var modelSelectorTitle = modelSelector.FindPropertyRelative("title");

            // get the list of models from the static SdModelSelector class
            var models = SdModelSelector.models;

            // create a list of strings from the models list
            var modelTitles = new List<string>();
            foreach (var model in models)
            {
                modelTitles.Add(model.title);
            }

            // get the index of the selected model
            var selectedIndex = modelTitles.IndexOf(modelSelectorTitle.stringValue);

            // create a dropdown with the list of models and the selected index
            var newSelectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, modelTitles.ToArray());

            // if the selected index changed
            if (newSelectedIndex != selectedIndex)
            {
                // update the selected index
                selectedIndex = newSelectedIndex;

                // update the selected model property with the selected index
                modelSelectorTitle.stringValue = modelTitles[selectedIndex];

                // fire the event, getting it through reflection and the fieldinfo
                (fieldInfo.GetValue(property.serializedObject.targetObject) as SdModelSelector).SetModel(modelTitles[selectedIndex]);
            }

            // save the changes
            property.serializedObject.ApplyModifiedProperties();
        }
    }




}