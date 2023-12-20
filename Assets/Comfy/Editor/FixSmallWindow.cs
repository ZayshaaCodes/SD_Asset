
using UnityEngine;
using UnityEditor;

//simple unity menu item to loop through all the windows in unity. print the size
public class FixSmallWindow : EditorWindow
{
    [MenuItem("Comfy/Fix Small Window")]
    static void Init()
    {
        //get a list of all the editor windows
        foreach (EditorWindow window in Resources.FindObjectsOfTypeAll<EditorWindow>())
        {
            //print the size of the window
            Debug.Log(window.GetType() + " " + window.position);
        }
    }
}