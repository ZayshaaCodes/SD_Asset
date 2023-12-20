using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StableDiffusion
{
    [Serializable]
    public class ControlNet
    {
        public List<ControlNetModulePreset> args;

        public string GetRequestJson()
        {
            var jroot = new JObject();
            var jargs = new JArray();

            //loop through each set of args, if enabled, add to the json
            for (int i = 0; i < args.Count; i++)
                jargs.Add(args[i].ToJobject());

            jroot.Add("args", jargs);

            return jroot.ToString();
        }
    }

    [Serializable]
    public enum ControlMode
    {
        Balanced = 0,
        MyPrompt = 1,
        ControlNet = 2,
    }

    [Serializable]
    public class AlwaysOnScripts
    {
        [JsonConverter(typeof(ControlNetConverter))]
        public ControlNet controlNet;
    }

    //cutom converter for IAlwaysOnScript objects that uses the GetRequestJson() method to serialize
    public class ControlNetConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(ControlNet).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var script = (ControlNet)value;
            writer.WriteRawValue(script.GetRequestJson());
        }
    }
}