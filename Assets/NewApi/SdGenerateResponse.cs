using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class SdGenerateResponse {
    [JsonConverter(typeof(Texture2DListConverter))]
    public List<Texture2D> images;
    public SdAttributes attributes;
    public string info;
}