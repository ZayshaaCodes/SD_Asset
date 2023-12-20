using UnityEngine;
using UnityEditor;
using Newtonsoft.Json.Linq;


namespace StableDiffusion
{

    [ExecuteAlways]
    public class SdConfigComponent : MonoBehaviour
    {
        public SdApi sdApi = new SdApi(new ApiRequestHandler("http://localhost:7860/"));

        public SdConfigSo baseConfig;
        public SdConfigSo overrideSo;
        [HideInInspector] public SdConfig config;

        public ControlNet controlNet;

        //get the output json from the config by aggregating the configs and appending the control net
        public JObject GetOutputJObj()
        {
            var baseJObj = baseConfig.config.ToJObject();
            var jobj = overrideSo ? overrideSo.config.ToJObject() : config.ToJObject();

            foreach (var property in jobj.Properties())
            {
                if (baseJObj.ContainsKey(property.Name))
                    baseJObj[property.Name] = property.Value;
                else
                    baseJObj.Add(property.Name, property.Value);
            }

            var aosJObj = JObject.FromObject(new AlwaysOnScripts
            {
                controlNet = controlNet
            });
            if (controlNet.args.Count > 0)
            {
                baseJObj.Add("alwayson_scripts", aosJObj);
            }

            return baseJObj;
        }

        public float GetAspectRatio()
        {
            var jobj = GetOutputJObj();
            var aspectRatio = jobj["width"].Value<float>() / jobj["height"].Value<float>();
            return aspectRatio;
        }

    }

    // custom inspector for SdConfigTesting
    [CustomEditor(typeof(SdConfigComponent))]
    public class SdConfigTestingEditor : Editor
    {
        private SdConfigComponent tar;

        private void OnEnable()
        {
            tar = (SdConfigComponent)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            //render an editor for the base config so
            if (tar.baseConfig != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                EditorGUILayout.LabelField("Base Config", EditorStyles.boldLabel);
                Editor.CreateEditor(tar.baseConfig).OnInspectorGUI();
            }

            //if the overrideSo is null, draw a prop field for 'config', else render the editor for the overrideSo
            if (tar.overrideSo == null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                EditorGUILayout.LabelField("Override Config", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("config"));
            }
            else
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Override Config", EditorStyles.boldLabel);
                Editor.CreateEditor(tar.overrideSo).OnInspectorGUI();
            }

            // EditorGUILayout.Space();

            // var s = tar.GetOutputJObj().ToString();;

            // text field for displaying the json
            // EditorGUILayout.TextArea(s);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
