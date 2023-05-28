using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

public class SdConfigTesting : MonoBehaviour
{
    
    public SdConfig config;
    public SdConfigOverrides overrides;

    public void OnEnable()
    {
        if (config == null) return;
    }    
}

[System.Serializable]
public class SdConfigOverrides
{
    public SdConfigTesting(Parameters)
    {
        
    }
}

//custom editor for the overrides
[CustomEditor(typeof(SdConfigTesting))]
public class SdConfigTestingEditor : Editor
{
    SdConfigTesting tar;
    void OnEnable()
    {
        tar = (SdConfigTesting)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SdConfigTesting myScript = (SdConfigTesting)target;
        if (GUILayout.Button("Generate"))
        {
            EditorCoroutineUtility.StartCoroutine(tar.Generate(), tar);
        }
        //render each of the texture is tar.results is not null

        if (tar.results != null)
        {
            //render at half size, render in a grid that is 2 wide, use guilayout
            int c = 0;
            
            GUILayout.BeginHorizontal();
            foreach (var t in tar.results)
            {
                GUILayout.BeginVertical();
                GUILayout.Label(t);
                GUILayout.Box(t);
                GUILayout.EndVertical();
                c++;
                if (c % 2 == 0)
                {
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }
            }
            GUILayout.EndHorizontal();
        }
    }
}