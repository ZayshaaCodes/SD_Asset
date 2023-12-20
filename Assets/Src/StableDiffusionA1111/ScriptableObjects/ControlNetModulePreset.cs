using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace StableDiffusion
{
    [Serializable]
    [CreateAssetMenu(fileName = "ControlNetArgs", menuName = "StableDiffusion/ControlNetArgs", order = 1)]
    public class ControlNetModulePreset : ScriptableObject
    {
        // [JsonConverter(typeof(TextureConverter))]
        public string module = "depth";
        public string model  = "control_v11f1p_sd15_depth [cfd03158]";

        public ControlNetModuleArgs args;

        public JObject ToJobject()
        {
            JObject jObj = new JObject();

            jObj.Add("module", module);
            jObj.Add("model",  model);
            args.AppendArgsToJobj(jObj);

            return jObj;
        }
    }

    [Serializable]
    public class ControlNetModuleArgs
    {
        [Range(0, 2)] public float        weight         = 1;
        public               SdResizeMode resize_mode    = SdResizeMode.Crop;
        public               bool         lowvram        = false;
        public               int          processor_res  = 512;
        public               int          threshold_a    = 32;
        public               int          threshold_b    = 32;
        public               float        guidance_start = 0;
        public               float        guidance_end   = 1;
        public               ControlMode  control_mode   = ControlMode.Balanced;
        public               bool         pixel_perfect  = false;

        [JsonConverter(typeof(TextureConverter))]
        public Texture mask = null;

        [JsonConverter(typeof(TextureConverter))]
        public Texture input_image = null;

        public ControlNetModuleArgs()
        {
            weight         = 1;
            resize_mode    = SdResizeMode.Crop;
            lowvram        = false;
            processor_res  = 512;
            threshold_a    = 32;
            threshold_b    = 32;
            guidance_start = 0;
            guidance_end   = 1;
            control_mode   = ControlMode.Balanced;
            pixel_perfect  = false;
            input_image    = null;
        }

        public void AppendArgsToJobj(JObject jobj) // only add the fields that are not default
        {
            // add all the fields
            if (Math.Abs(weight - 1) > .001f) jobj.Add("weight",               weight);
            if (resize_mode != SdResizeMode.Stretch) jobj.Add("resize_mode",   resize_mode.ToString());
            if (lowvram) jobj.Add("lowvram",                                   lowvram);
            if (processor_res != 512) jobj.Add("processor_res",                processor_res);
            if (threshold_a != 32) jobj.Add("threshold_a",                     threshold_a);
            if (threshold_b != 32) jobj.Add("threshold_b",                     threshold_b);
            if (guidance_start != 0) jobj.Add("guidance_start",                guidance_start);
            if (Math.Abs(guidance_end - 1) > .01) jobj.Add("guidance_end",     guidance_end);
            if (control_mode != ControlMode.Balanced) jobj.Add("control_mode", control_mode.ToString());
            if (pixel_perfect) jobj.Add("pixel_perfect",                       pixel_perfect);
            if (input_image != null) jobj.Add("input_image",                   input_image.name);
            if (mask != null) jobj.Add("mask",                                 mask.name);
        }
 
        public JObject ToJobject() // add all the fields
        {
            JObject jObj = new JObject();
            AppendArgsToJobj(jObj);
            return jObj;
        }
    }
}