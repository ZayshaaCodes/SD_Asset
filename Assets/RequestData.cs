using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

public class Txt2ImgAttribute : Attribute
{
}

public class Img2ImgAttribute : Attribute
{
}

[Serializable]
public class RequestData
{
    private static List<FieldInfo> _txt2ImgFields = new();
    private static List<FieldInfo> _img2ImgFields = new();

    static RequestData()
    {
        foreach (var field in typeof(RequestData).GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            if (field.GetCustomAttribute<Txt2ImgAttribute>() != null) _txt2ImgFields.Add(field);
            if (field.GetCustomAttribute<Img2ImgAttribute>() != null) _img2ImgFields.Add(field);
        }
    }

    //both
    [Txt2Img, Img2Img,] public bool enable_hr = false;

    [Txt2Img, Img2Img, RangeEx(128, 512,  128)] public int firstphase_width  = 0;
    [Txt2Img, Img2Img, RangeEx(128, 512,  128)] public int firstphase_height = 0;
    [Txt2Img, Img2Img, RangeEx(128, 1024, 128)] public int width             = 512;
    [Txt2Img, Img2Img, RangeEx(128, 1024, 128)] public int height            = 512;

    [Txt2Img, Img2Img, Multiline(2)] public string prompt          = "";
    [Txt2Img, Img2Img]               public string negative_prompt = "";

    [Txt2Img, Img2Img]                public List<string> style              = new();
    [Txt2Img, Img2Img]                public int          seed               = -1;
    [Txt2Img, Img2Img]                public int          subseed            = -1;
    [Txt2Img, Img2Img]                public int          subseed_strength   = 0;
    [Txt2Img, Img2Img]                public int          seed_resize_from_h = -1;
    [Txt2Img, Img2Img]                public int          seed_resize_from_w = -1;
    [Txt2Img, Img2Img, Range(1, 10)]  public int          batch_size         = 1;
    [Txt2Img, Img2Img, Range(1, 10)]  public int          n_iter             = 1;
    [Txt2Img, Img2Img, Range(1, 100)] public int          steps              = 15;
    [Txt2Img, Img2Img, Range(1, 30)]  public float        cfg_scale          = 10;
    [Txt2Img, Img2Img]                public bool         restore_faces      = false;
    [Txt2Img, Img2Img]                public bool         tiling             = false;
    [Txt2Img, Img2Img]                public float        eta                = 0;
    [Txt2Img, Img2Img]                public float        s_noise            = 1;
    [Txt2Img, Img2Img]                public string       sampler_index      = "Euler a";

    //img2img
    [Img2Img, Range(0, 1)] public float           denoising_strength = .5f;
    [Img2Img]              public List<Texture2D> init_images        = new();
    [Img2Img, Range(0, 4)] public int             resize_mode        = 0;
    [Img2Img]              public Texture2D       mask;
    [Img2Img]              public int             mask_blur                = 4;
    [Img2Img]              public int             inpainting_fill          = 0;
    [Img2Img]              public bool            inpaint_full_res         = true;
    [Img2Img]              public int             inpaint_full_res_padding = 0;
    [Img2Img]              public int             inpainting_mask_invert   = 0;

    // public  s_churn", 0 
    // public  s_tmax", 0 
    // public  s_tmin", 0

    public byte[] Txt2ImgData => GetFieldDataBytes(Txt2ImgJson);
    public byte[] Img2ImgData => GetFieldDataBytes(Img2ImgJson);
    public string Txt2ImgJson => SerializeFields(_txt2ImgFields);
    public string Img2ImgJson => SerializeFields(_img2ImgFields);

    private string SerializeFields(List<FieldInfo> fields)
    {
        Dictionary<string, object> postData = new();
        foreach (var field in fields)
        {
            var value = field.GetValue(this);
            switch (value)
            {
                case Texture2D texture:
                    postData.Add(field.Name, texture.Tob64String());
                    break;
                case List<Texture2D> textureeList:
                    List<string> tStrings = new();
                    foreach (var t in textureeList) tStrings.Add(t.Tob64String());
                    postData.Add(field.Name, tStrings);
                    break;
                default:
                    postData.Add(field.Name, value);
                    break;
            }
        }

        return JsonConvert.SerializeObject(postData, Formatting.Indented);
    }

    private byte[] GetFieldDataBytes(string json)
    {
        return Encoding.UTF8.GetBytes(json);
    }
}