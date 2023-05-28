using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;

[CreateAssetMenu(fileName = "SdConfig", menuName = "ScriptableObjects/SdConfig")]
public class SdConfig : ScriptableObject{
    public SdAttributes base_attributes = new SdAttributes();
}

[Serializable]
public class SdAttributes
{
    [Range(0, 1)] public float denoising_strength = .5f;
    public ResizeMode resize_mode = 0;
    [Range(0, 20)] public int mask_blur = 5;
    [JsonConverter(typeof(Texture2DListConverter))]
    public List<Texture2D> init_images = new List<Texture2D>();
    [JsonConverter(typeof(Texture2DConverter))]
    public Texture2D mask = null;
    [Range(0, 5)] public int inpainting_fill = 1;
    public bool inpaint_full_res = false;
    public int inpaint_full_res_padding = 0;
    public int inpainting_mask_invert = 0;
    [RangeStepInt(128, 1024, 128)] public int width = 512;
    [RangeStepInt(128, 1024, 128)] public int height = 512;
    [Range(1, 10)] public int batch_size = 1;
    [Range(1, 10)] public int n_iter = 1;
    [Range(1, 100)] public int steps = 20;
    [Range(1, 30)] public float cfg_scale = 10;
    [Multiline(2)] public string prompt = "test";
    [Multiline(2)] public string negative_prompt = "";
    public int seed = -1;
    public bool restore_faces = false;
    public bool tiling = false;
    public float eta = 0;
    public string sampler_index = "Euler a";
    public float image_cfg_scale = 0;
    public float initial_noise_multiplier = 0;
    public bool do_not_save_samples = false;
    public bool do_not_save_grid = false;
    public Dictionary<string, string> override_settings = new Dictionary<string, string>();
    public bool override_settings_restore_afterwards = true;
    public List<string> script_args = new List<string>();
    public bool include_init_images = false;
    public string script_name = "";
    // public bool send_images = true;
    // public bool save_images = false;
    public AlwaysOnScripts alwayson_scripts;

    public static SdAttributes defaults => new SdAttributes()
    {
        denoising_strength = .5f,
        resize_mode = 0,
        mask_blur = 5,
        init_images = new List<Texture2D>(),
        mask = null,
        inpainting_fill = 1,
        inpaint_full_res = false,
        inpaint_full_res_padding = 0,
        inpainting_mask_invert = 0,
        width = 512,
        height = 512,
        batch_size = 1,
        n_iter = 1,
        steps = 20,
        cfg_scale = 10,
        prompt = "test",
        negative_prompt = "",
        // styles = new List<string>(),
        seed = -1,
        // subseed = -1,
        // subseed_strength = 0,
        // seed_resize_from_h = -1,
        // seed_resize_from_w = -1,
        restore_faces = false,
        tiling = false,
        eta = 0,
        sampler_index = "Euler a",
        image_cfg_scale = 0,
        initial_noise_multiplier = 0,
        do_not_save_samples = false,
        do_not_save_grid = false,
        // s_min_uncond = 0,
        // s_churn = 0,
        // s_tmax = 0,
        // s_tmin = 0,
        // s_noise = 1,
        // override_settings = new Dictionary<string, string>(),
        // override_settings_restore_afterwards = true,
        script_args = new List<string>(),
        include_init_images = false,
        script_name = "",
        // send_images = false,
        // save_images = false,
        alwayson_scripts = new AlwaysOnScripts()
    };
}