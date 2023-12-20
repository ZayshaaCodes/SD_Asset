using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StableDiffusion
{
    [System.Serializable]
    public class SdConfig
    {
        // [HideInInspector] public List<string> enabledFields = new List<string>();
        // public AlwaysOnScripts alwayson_scripts;

        [Multiline(2)] public string prompt = "test";
        [Multiline(2)] public string negative_prompt = "";
        [Range(1, 100)] public int steps = 20;
        [Range(1, 30)] public float cfg_scale = 10;
        [Range(128, 1024)] public int width = 512;
        [Range(128, 1024)] public int height = 512;
        [Range(1, 50)] public int n_iter = 1;
        [Range(1, 4)] public int batch_size = 1;
        public int seed = -1;
        public bool restore_faces = false;
        public bool tiling = false;
        public string sampler_index = "Euler a";
        public List<string> script_args = new List<string>();
        public bool include_init_images = false;
        public string script_name = "";
        [JsonConverter(typeof(TextureListConverter))]
        public List<Texture> init_images = new List<Texture>();
        [JsonConverter(typeof(TextureConverter))]
        public Texture mask = null;
        [Range(0, 1)] public float denoising_strength = .5f;
        public SdResizeMode resize_mode = 0;
        [Range(0, 20)] public int mask_blur = 5;
        [Range(0, 5)] public int inpainting_fill = 1;
        public bool inpaint_full_res = false;
        public int inpaint_full_res_padding = 0;
        public int inpainting_mask_invert = 0;

        public JObject ToJObject()
        {

            var cfg = new JObject();
            
            return JObject.FromObject(this);
        }
    }
}