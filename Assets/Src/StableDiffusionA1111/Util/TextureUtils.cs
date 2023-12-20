using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public static class TextureUtils
{
    public static string ToBase64String(Texture texture)
    {
        if (texture == null) return "";

        switch (texture)
        {
            case Texture2D t2d:
                return Convert.ToBase64String(t2d.EncodeToPNG());
            case RenderTexture rt: // store and restore active
                var prev = RenderTexture.active;
                RenderTexture.active = rt;
                var t2d_rt = new Texture2D(rt.width, rt.height);
                t2d_rt.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                t2d_rt.Apply();
                RenderTexture.active = prev;
                return Convert.ToBase64String(t2d_rt.EncodeToPNG());
            default:
                throw new Exception("Texture type not supported");
        }

    }

    public static Texture2D FromBase64String(string base64)
    {
        if (string.IsNullOrEmpty(base64)) return null;
        var t2d = new Texture2D(2, 2);
        t2d.LoadImage(Convert.FromBase64String(base64));
        t2d.Apply();

        return t2d;
    }
}

public class TextureConverter : JsonConverter<Texture>
{
    string prefix = "data:image/png;base64,";

    public override Texture ReadJson(JsonReader reader, Type objectType, Texture existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var base64 = (string)reader.Value;
        var t2d = new Texture2D(2, 2); // Size will be replaced by LoadImage

        if (base64.StartsWith(prefix))
        {
            base64 = base64.Substring(prefix.Length);
        }
        
        t2d.LoadImage(Convert.FromBase64String(base64));
        return t2d;
    }

    public override void WriteJson(JsonWriter writer, Texture value, JsonSerializer serializer)
    {
        var ts = TextureUtils.ToBase64String(value);
        if (string.IsNullOrEmpty(ts))
        {
            writer.WriteNull();
            return;
        }
        
        var base64 = prefix + ts;
        writer.WriteValue(base64);
    }
}

public class TextureListConverter : JsonConverter
{
    string prefix = "data:image/png;base64,";
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(List<Texture>);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var textureList = new List<Texture>();
        var base64List = serializer.Deserialize<List<string>>(reader);

        for (int i = 0; i < base64List.Count; i++)
        {
            var base64 = base64List[i];
            if (base64.StartsWith(prefix))
            {
                base64 = base64.Substring(prefix.Length);
            }

            var texture = new Texture2D(2, 2);
            texture.LoadImage(Convert.FromBase64String(base64));
            textureList.Add(texture);
        }

        return textureList;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var textureList = (List<Texture>)value;
        var base64List = new List<string>();

        foreach (var texture in textureList)
        {
            if (texture == null) continue;
            var base64 = prefix + TextureUtils.ToBase64String(texture);
            base64List.Add(base64);
        }

        serializer.Serialize(writer, base64List);
    }
}
