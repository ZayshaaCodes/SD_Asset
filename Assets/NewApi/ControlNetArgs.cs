using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

[System.Serializable]
public class ControlNetArgs
{
    // [JsonConverter(typeof(Texture2DConverter))]
    // public Texture2D mask = null;
    public string module = "depth";
    public string model = "control_v11f1p_sd15_depth [cfd03158]";
    [Range(0, 2)] public float weight = 1;
    public ResizeMode resize_mode;
    //public bool lowvram = false;
    public int processor_res = 512;
    //public int threshold_a = 0;
    //public int threshold_b = 0;
    public float guidance_start = 0;
    public float guidance_end = 1;
    public ControlMode control_mode;
    public bool pixel_perfect;
    [JsonConverter(typeof(Texture2DConverter))]
    public Texture2D input_image = null;
}

[System.Serializable]
public enum ControlMode
{
    Balanced = 0,
    MyPrompt = 1,
    ControlNet = 2,
}

[System.Serializable]
public class AlwaysOnScripts
{
    [JsonConverter(typeof(AlwaysOnScriptConverter))]
    public ControlNet controlNet;
}



[System.Serializable]
public class ControlNet : AlwaysOnScript
{
    public List<ControlNetArgs> args;

    public override string GetRequestJson()
    {
        var jroot = new JObject();
        var jargs = new JArray();
        
        //loop through each set of args, if enabled, add to the json
        for (int i = 0; i < args.Count; i++)
            jargs.Add(JObject.FromObject(args[i]));

        jroot.Add("args", jargs);

        return jroot.ToString();
    }

}

public abstract class AlwaysOnScript
{
    public abstract string GetRequestJson();
}

//cutom converter for IAlwaysOnScript objects that uses the GetRequestJson() method to serialize
public class AlwaysOnScriptConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return typeof(AlwaysOnScript).IsAssignableFrom(objectType);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var script = (AlwaysOnScript)value;
        writer.WriteRawValue(script.GetRequestJson());
    }
}