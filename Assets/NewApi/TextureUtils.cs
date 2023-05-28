using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public static class TextureUtils
{
    public static string ToBase64String(Texture2D texture)
    {
        if (texture == null) return null;
        return Convert.ToBase64String(texture.EncodeToPNG());
    }

    public static Texture2D FromBase64String(string base64, int width, int height)
    {
        var t2d = new Texture2D(width, height);
        t2d.LoadImage(Convert.FromBase64String(base64));
        t2d.Apply();

        return t2d;
    }
}

public class Texture2DConverter : JsonConverter
{
    string prefix = "data:image/png;base64,";
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Texture2D);
    }


    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
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

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var texture = (Texture2D)value;
        var ts = TextureUtils.ToBase64String(texture);
        if (string.IsNullOrEmpty(ts))
        {
            writer.WriteNull();
            return;
        }
        var base64 = prefix + ts;
        writer.WriteValue(base64);
    }
}

public class Texture2DListConverter : JsonConverter
{
    string prefix = "data:image/png;base64,";
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(List<Texture2D>);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var textureList = new List<Texture2D>();
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
        var textureList = (List<Texture2D>)value;
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
