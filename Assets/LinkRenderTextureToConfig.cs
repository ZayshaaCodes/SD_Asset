using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StableDiffusion
{

    [RequireComponent(typeof(Camera)), ExecuteAlways]
    public class LinkRenderTextureToConfig : MonoBehaviour
    {
        public SdConfigComponent configComponent;
        public RenderTexture renderTexture;
        // Start is called before the first frame update
        void OnEnable()
        {
            if (configComponent == null)
                configComponent = GetComponent<SdConfigComponent>();

            if (renderTexture == null)
                renderTexture = GetComponent<Camera>().targetTexture;
        }

        public void UpdateAspectRatio()
        {
            var aspectRatio = GetAspectRatio();
            // Debug.Log("Updating aspect ratio: " + aspectRatio);
            renderTexture.Release(); // release the old render texture, required to change the size: https://docs.unity3d.com/ScriptReference/RenderTexture.html
            renderTexture.height = (int)(renderTexture.width / aspectRatio);
            renderTexture.Create();
        }

        public float GetAspectRatio()
        {
            var aspectRatio = configComponent.GetAspectRatio();
            return aspectRatio;
        }
    }
}
