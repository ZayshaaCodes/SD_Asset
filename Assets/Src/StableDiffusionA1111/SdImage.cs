using Newtonsoft.Json;
using UnityEngine;

namespace StableDiffusion
{

    [System.Serializable]
    public class SdImage
    {
        [JsonConverter(typeof(TextureConverter))]
        public Texture image;
        public string info;
    }
}