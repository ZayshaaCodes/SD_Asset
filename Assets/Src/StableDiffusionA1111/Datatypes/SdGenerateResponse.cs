using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace StableDiffusion
{
    public class SdGenerateResponse
    {
        [JsonConverter(typeof(TextureListConverter))]
        public List<Texture> images;
        public SdConfig attributes;
        public string info;
    }
}