using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public class SdImage
{
    [JsonConverter(typeof(Texture2DConverter))]
    public Texture2D image;
    public SdAttributes attributes;
    public string info;
}