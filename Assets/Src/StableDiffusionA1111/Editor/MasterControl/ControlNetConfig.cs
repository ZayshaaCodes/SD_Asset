using System;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace StableDiffusion.Editor.MasterControl
{
    // when preset is set, it will copy the values from the preset to the base args
    // when preset is null, it will use the base args
    // when overridePresetArgs is true, it will use the base args instead of the preset args 
    // but maintain the preset for the other values (Module, Model)
    // whenever the overridePresetArgs is changed, it will copy the values from the preset to the base args
    // it will have a function, ToJobject, that will return a JObject with the values based on the overridePresetArgs
    [Serializable]
    public class ControlNetConfig
    {
        public ControlNetModulePreset preset;
        public bool                   overridePresetArgs;
        public string                 baseModel  = "";
        public string                 baseModule = "";
        public ControlNetModuleArgs   baseArgs   = new ();

        public JObject ToJobject()
        {
            var bArgs = baseArgs.ToJobject();
            bArgs.Add("model",  baseModel);
            bArgs.Add("module", baseModule);
            var pArgs = preset != null ? preset.ToJobject() : null;

            if (preset == null) 
            {
                //useing all the base args and appending the model and module
                
                return bArgs;
            }
            else // preset is not null
            {
                if (!overridePresetArgs)
                {
                    return pArgs;
                }
                else // base is overriding preset args
                {
                    return bArgs;
                }
            }
        }
    }


    //custom drawer for ControlNetConfig
    [CustomPropertyDrawer(typeof(ControlNetConfig))]
    public class ControlNetConfigDrawer : PropertyDrawer
    {
        private Toggle        overridePresetArgsField;
        private PropertyField baseModelField;
        private PropertyField baseModuleField;
        private PropertyField baseArgsField;
        private ObjectField   presetField;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();

            var preset             = property.FindPropertyRelative("preset");
            var overridePresetArgs = property.FindPropertyRelative("overridePresetArgs");
            var baseModel          = property.FindPropertyRelative("baseModel");
            var baseModule         = property.FindPropertyRelative("baseModule");
            var baseArgs           = property.FindPropertyRelative("baseArgs");

            presetField             = new ObjectField("preset");
            presetField.objectType  = typeof(ControlNetModulePreset);
            presetField.bindingPath = preset.propertyPath;

            //change callback to update ui when preset is changed
            presetField.RegisterValueChangedCallback(evt =>
            {
                // Debug.Log("preset changed");
                var preset = evt.newValue as ControlNetModulePreset;
                if (preset != null)
                {
                    baseModel.stringValue  = preset.model;
                    baseModule.stringValue = preset.module;

                    baseArgs.FindPropertyRelative("weight").floatValue                = preset.args.weight;
                    baseArgs.FindPropertyRelative("resize_mode").enumValueIndex       = (int)preset.args.resize_mode;
                    baseArgs.FindPropertyRelative("lowvram").boolValue                = preset.args.lowvram;
                    baseArgs.FindPropertyRelative("processor_res").intValue           = preset.args.processor_res;
                    baseArgs.FindPropertyRelative("threshold_a").intValue             = preset.args.threshold_a;
                    baseArgs.FindPropertyRelative("threshold_b").intValue             = preset.args.threshold_b;
                    baseArgs.FindPropertyRelative("guidance_start").floatValue        = preset.args.guidance_start;
                    baseArgs.FindPropertyRelative("guidance_end").floatValue          = preset.args.guidance_end;
                    baseArgs.FindPropertyRelative("control_mode").enumValueIndex      = (int)preset.args.control_mode;
                    baseArgs.FindPropertyRelative("pixel_perfect").boolValue          = preset.args.pixel_perfect;
                    baseArgs.FindPropertyRelative("mask").objectReferenceValue        = preset.args.mask;
                    baseArgs.FindPropertyRelative("input_image").objectReferenceValue = preset.args.input_image;

                    overridePresetArgsField.visible = true;
                    if (overridePresetArgs.boolValue) setArgsEnabled(true);
                    else setArgsEnabled(false);

                    //apply changes
                    property.serializedObject.ApplyModifiedProperties();
                }
                else
                {
                    setArgsEnabled(true);
                    overridePresetArgsField.visible = false;
                }
            });

            overridePresetArgsField             = new Toggle("Override Preset");
            overridePresetArgsField.bindingPath = overridePresetArgs.propertyPath;
            overridePresetArgsField.RegisterValueChangedCallback(evt =>
            {
                setArgsEnabled(evt.newValue);
                
                //apply changes
                property.serializedObject.ApplyModifiedProperties();
            });

            baseModelField             = new PropertyField(baseModel);
            baseModelField.bindingPath = baseModel.propertyPath;

            baseModuleField             = new PropertyField(baseModule);
            baseModuleField.bindingPath = baseModule.propertyPath;

            baseArgsField             = new PropertyField(baseArgs);
            baseArgsField.bindingPath = baseArgs.propertyPath;

            container.Add(presetField);
            container.Add(overridePresetArgsField);
            container.Add(baseModelField);
            container.Add(baseModuleField);
            container.Add(baseArgsField);

            return container;
        }


        public void setArgsEnabled(bool enabled)
        {
            baseModelField.SetEnabled(enabled);
            baseModuleField.SetEnabled(enabled);
            baseArgsField.SetEnabled(enabled);
        }
    }
}