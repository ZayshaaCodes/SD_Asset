using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
//cutom inspector for scriptable object
[CustomEditor(typeof(ComfyApiWorkflow))]
public class ComfyWorkflow_ApiEditor : Editor
{
    public ComfyApiWorkflow tar;

    public override void OnInspectorGUI()
    {
        tar = (ComfyApiWorkflow)target;

        //object field
        tar.apiJsonFile = EditorGUILayout.ObjectField("File", tar.apiJsonFile, typeof(Object), false);
        
        //set the type to json, only allow json files
        if (tar.apiJsonFile != null && tar.apiJsonFile.GetType() != typeof(TextAsset))
        {
            tar.apiJsonFile = null;
        }


        if (GUILayout.Button("Load"))
        {
            //read json file
            tar.data = JObject.Parse(tar.apiJsonFile.ToString());
        }

        if (GUILayout.Button("Save"))
        {
            //write json file
            System.IO.File.WriteAllText(AssetDatabase.GetAssetPath(tar.apiJsonFile), tar.data.ToString());
        }

        if (GUILayout.Button("Log"))
        {
            Debug.Log(tar.data.ToString());
        }

        if (tar.data != null)
        {
            DrawJson(tar.data);
        }
    }

    // recursive function to draw json
    // for any keys named inputs, draw values as text fields
    public void DrawJson(JToken token)
    {
        if (token.Type == JTokenType.Object)
        {
            //beging a group
            foreach (JProperty prop in token)
            {
                EditorGUILayout.BeginVertical("Box");
                if (prop.Name == "inputs")
                {
                    EditorGUI.indentLevel++;
                    foreach (JProperty input in prop.Value)
                    {
                        if (input.Value.Type == JTokenType.Integer)
                        {
                            var value = input.Value.ToObject<long>();
                            input.Value = EditorGUILayout.LongField(input.Name, value);
                        }
                        else if (input.Value.Type == JTokenType.Float)
                        {
                            var value = input.Value.ToObject<float>();
                            input.Value = EditorGUILayout.FloatField(input.Name, value);
                        }
                        else if (input.Value.Type == JTokenType.Boolean)
                        {
                            var value = input.Value.ToObject<bool>();
                            input.Value = EditorGUILayout.Toggle(input.Name, value);
                        }
                        else if (input.Value.Type == JTokenType.String)
                        {
                            var value = input.Value.ToObject<string>();
                            input.Value = EditorGUILayout.TextField(input.Name, value);
                        }
                        else if (input.Value.Type == JTokenType.Array)
                        {
                            continue;
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(input.Name + " ( Node Ref )");
                            //colored yellow, rich text
                            EditorGUILayout.LabelField("<color=yellow>node_id: " + input.Value[0] + " | output_id: " + input.Value[1] + "</color>", new GUIStyle(GUI.skin.label) { richText = true });
                            EditorGUILayout.EndHorizontal();
                        }
                        else
                        {
                            EditorGUILayout.LabelField(input.Name);
                            DrawJson(input.Value);
                        }

                    }
                    EditorGUI.indentLevel--;
                }
                else if (prop.Name != "class_type")
                {
                    var propName = prop.Name;

                    //if the promp name is a number, append the value from it's class_type member
                    if (int.TryParse(prop.Name, out int n))
                    {
                        propName += " | " + prop.Value["class_type"];
                    }

                    EditorGUILayout.LabelField(propName);
                    DrawJson(prop.Value);
                }
                EditorGUILayout.EndVertical();
            }
        }
        else if (token.Type == JTokenType.Array)
        {
            foreach (JToken child in token)
            {
                DrawJson(child);
            }
        }
        else
        {
            EditorGUILayout.TextField(token.ToString());
        }

        //apply any changes
        EditorUtility.SetDirty(tar);

    }
}
