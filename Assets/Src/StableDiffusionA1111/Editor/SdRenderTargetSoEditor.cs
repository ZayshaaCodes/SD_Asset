using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace StableDiffusion
{
    //custom editor, default inspector and get the target
    [UnityEditor.CustomEditor(typeof(SdRenderTargetSo))]
    public class SdRenderTargetSoEditor : UnityEditor.Editor
    {
        public VisualTreeAsset UiAsset;

        private SdRenderTargetSo tar;

        private void OnEnable()
        {
            tar = (SdRenderTargetSo)target;
        }

        public override void OnInspectorGUI()
        {

            //loop the images list and draw a object field for each
            for (int i = 0; i < tar.images.Count; i++)
            {
                //first check it's not null
                if (tar.images[i] == null)
                {
                    tar.images.RemoveAt(i);
                    i--;
                    continue;
                }

                tar.images[i] = (Texture2D)EditorGUILayout.ObjectField(tar.images[i], typeof(Texture2D), false);
            }

            if (tar.images != null)
            {
                EditorGUILayout.LabelField("Images");

                EditorGUILayout.BeginHorizontal();
                int c = 0;
                for (int i = 0; i < tar.images.Count; i++)
                {
                    var rect = EditorGUILayout.GetControlRect(false, 256);
                    rect.width = rect.height;
                    EditorGUI.DrawPreviewTexture(rect, tar.images[i]);
                    c++;

                    //if c == 3, start a new row
                    if (c == 2)
                    {
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        c = 0;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }

}