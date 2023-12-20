using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace StableDiffusion
{
    [ExecuteAlways]
    public class RenderTarget_RawImage : MonoBehaviour
    {
        public SdRenderTargetSo renderTarget;
        public RawImage rawImage;

        // public List<Texture> images;
        
        private void OnEnable()
        {
            if (renderTarget != null)
            {
                renderTarget.OnImagesSet += SetImages;
            }
        }

        private void OnDisable()
        {
            if (renderTarget != null)
            {
                renderTarget.OnImagesSet -= SetImages;
            }
        }

        public virtual void SetImages(List<Texture2D> images)
        {
            //log
            Debug.Log("SetImages called, images count: " + images.Count);
            rawImage.texture = images[0];
            // this.images = images;
        }
    }

    // custom inspector for SdRenderTarget that renders all the images in a grid under the default inspector
    [CustomEditor(typeof(RenderTarget_RawImage))]
    public class SdRenderTargetEditor : Editor
    {
        private RenderTarget_RawImage tar;

        private void OnEnable()
        {
            tar = (RenderTarget_RawImage)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            // //render a grid of images
            // if (tar.images != null)
            // {
            //     EditorGUILayout.Space();
            //     EditorGUILayout.LabelField("Images", EditorStyles.boldLabel);

            //     int cols = 2;
            //     int rows = (int)Mathf.Ceil((float)tar.images.Count / cols);

            //     int i = 0;
            //     for (int y = 0; y < rows; y++)
            //     {
            //         EditorGUILayout.BeginHorizontal();
            //         for (int x = 0; x < cols; x++)
            //         {
            //             if (i < tar.images.Count)
            //             {
            //                 GUILayout.Label(tar.images[i], GUILayout.Width(tar.images[i].width), GUILayout.Height(tar.images[i].height));
            //             }
            //             i++;
            //         }
            //         EditorGUILayout.EndHorizontal();
            //     }
            // }
        }
    }
}