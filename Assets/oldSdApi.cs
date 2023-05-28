using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEngine;
using UnityEngine.Networking;

public static class oldSdApi
{
    public static string apuUri = "http://localhost:7860/sdapi/v1/";
    public static string funcUri = "http://localhost:7860/run/predict/";

    public static string Tob64String(this Texture2D texture)
    {
        if (texture == null) return null;
        return Convert.ToBase64String(texture.EncodeToPNG());
    }

    public static IEnumerator Generate(oldRequestData requestData, Action<List<SdImage>> callback)
    {
        var images = new List<SdImage>();
        var hasInitImage = requestData.init_images.Count > 0;
        var request = new UnityWebRequest($"http://localhost:7860/sdapi/v1/{(hasInitImage ? "img2img" : "txt2img")}", UnityWebRequest.kHttpVerbPOST);

        UploadHandlerRaw uH = new UploadHandlerRaw(requestData.GetFieldDataBytes());
        DownloadHandler dH = new DownloadHandlerBuffer();

        request.uploadHandler = uH;
        request.downloadHandler = dH;

        request.useHttpContinue = false;
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var resJson = JObject.Parse(dH.text);
            var imagesToken = resJson["images"];
            var infoToken = resJson["info"];

            var info = infoToken.ToString();
            var seedmatch = Regex.Match(info, @"\sseed: (\d+)").Groups[1];

            var c = imagesToken!.Count();
            for (int i = 0; i < c; i++)
            {
                var t2d = new Texture2D(requestData.width, requestData.height);
                t2d.LoadImage(Convert.FromBase64String(imagesToken[i]!.ToString()));
                t2d.Apply();

                images.Add(new SdImage()
                {
                    image = t2d,
                    info = info,
                    attributes = null
                });
            }

            uH.Dispose();
            request.Dispose();

            callback?.Invoke(images);
        }
    }

    // public static IEnumerator CheckProgress(Action<ProgressData> callback)
    // {
    //     yield return ApiRequest("progress", "GET", null, (s, json) =>
    //     {
    //         JObject jo = JObject.Parse(json);
    //         var pd = new ProgressData();

    //         pd.image = jo["current_image"]!.ToString();
    //         pd.percent = jo["progress"]!.Value<float>();

    //         var state = jo["state"];
    //         if (state != null)
    //         {
    //             pd.skipped = state["skipped"]?.Value<bool>() ?? false;
    //             pd.interrupted = state["interrupted"]?.Value<bool>() ?? false;
    //             pd.job = state["job"]?.Value<string>() ?? "";
    //             pd.job_count = state["job_count"]?.Value<int>() ?? 0;
    //             pd.job_no = state["job_no"]?.Value<int>() ?? 0;
    //             pd.sampling_step = state["sampling_step"]?.Value<int>() ?? 0;
    //             pd.sampling_steps = state["sampling_steps"]?.Value<int>() ?? 0;
    //         }

    //         callback(pd);
    //     });
    // }


    public static void GetModels(List<string> models)
    {
        // {
        //     "title": "sd-v1-4-full-ema.ckpt [06c50424]",
        //     "model_name": "sd-v1-4-full-ema",
        //     "hash": "06c50424",
        //     "filename": "C:\\Users\\carad\\Desktop\\SD\\stable-diffusion-webui\\models\\Stable-diffusion\\sd-v1-4-full-ema.ckpt",
        //     "config": "C:\\Users\\carad\\Desktop\\SD\\stable-diffusion-webui\\repositories\\stable-diffusion\\configs/stable-diffusion/v1-inference.yaml"
        // }
        models.Clear();
        EditorCoroutineUtility.StartCoroutineOwnerless(ApiRequest("sd-models", "GET", null, (s, o) =>
        {
            var jarr = JArray.Parse(o);
            foreach (var token in jarr)
            {
                models.Add(token["title"]!.ToString());
            }
        }));
    }

    public static IEnumerator GetConfig(Action<ApiConfig> callback)
    {
        yield return ApiRequest("options", "GET", null, (s, json) =>
        {
            callback(JsonConvert.DeserializeObject<ApiConfig>(json));
        });
    }

    public static void Interrupt()
    {
        EditorCoroutineUtility.StartCoroutineOwnerless(ApiRequest("interrupt", "POST"));
    }

    public static void SetModel(string model)
    {
        EditorCoroutineUtility.StartCoroutineOwnerless(ApiFuncRequest(291, "POST", new() { { "data", new List<string>() { model } } }));
    }

    public static void GetSamplers(List<string> samplers)
    {
        samplers.Clear();
        EditorCoroutineUtility.StartCoroutineOwnerless(ApiRequest("samplers", "GET", null, (s, o) =>
        {
            var jarr = JArray.Parse(o);
            foreach (var token in jarr)
            {
                samplers.Add(token["name"]?.ToString());
            }
        }));
    }

    public static IEnumerator ApiRequest(string endpoint, string reqMethod, Dictionary<string, object> parameters = null, Action<string, string> callback = null)
    {
        if (parameters is null) parameters = new();

        var reqBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(parameters, Formatting.Indented));

        var webRequest = new UnityWebRequest(apuUri + endpoint, reqMethod);

        DownloadHandler dH = new DownloadHandlerBuffer();
        webRequest.downloadHandler = dH;
        webRequest.uploadHandler = new UploadHandlerRaw(reqBytes);
        webRequest.useHttpContinue = false;
        webRequest.SetRequestHeader("Content-Type", "application/json");

        yield return webRequest.SendWebRequest();

        //Debug.Log(webRequest.result);
        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            // Debug.Log(dH.text);
            callback?.Invoke(dH.text, dH.text);
        }

        webRequest.Dispose();
    }

    public static IEnumerator ApiFuncRequest(int funcNumber, string reqMethod, Dictionary<string, object> parameters = null, Action<string, string> callback = null)
    {
        if (parameters is null) parameters = new();
        parameters.Add("fn_index", funcNumber);

        var reqBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(parameters, Formatting.Indented));

        var webRequest = new UnityWebRequest(funcUri, reqMethod);

        DownloadHandler dH = new DownloadHandlerBuffer();
        webRequest.downloadHandler = dH;
        webRequest.uploadHandler = new UploadHandlerRaw(reqBytes);
        webRequest.useHttpContinue = false;
        webRequest.SetRequestHeader("Content-Type", "application/json");

        yield return webRequest.SendWebRequest();

        //Debug.Log(webRequest.result);
        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            // Debug.Log(dH.text);
            callback?.Invoke(dH.text, dH.text);
        }

        webRequest.Dispose();
    }

    // ReSharper disable InconsistentNaming
    public class ApiConfig
    {

        public string outdir_txt2img_samples;
        public string outdir_img2img_samples;
        public string outdir_extras_samples;
        public string outdir_txt2img_grids;
        public string outdir_img2img_grids;
        public string outdir_save;
        public List<string> realesrgan_enabled_models;
        public string sd_model_checkpoint;
        public bool img2img_color_correction;
        public bool img2img_fix_steps;
        public bool show_progressbar;
        public float show_progress_every_n_steps;
        public bool show_progress_grid;
        public List<string> hide_samplers;
    }
}

// // attribute to support json serialization
// public struct ProgressData
// {
//     public float percent;
//     public string image;
//     public int steps;
//     public bool skipped;
//     public bool interrupted;
//     public string job;
//     public int job_count;
//     public string job_timestamp;
//     public int job_no;
//     public int sampling_step;
//     public int sampling_steps;

//     [JsonIgnore]
//     public string Info
//     {
//         get => $"step {sampling_step} / {sampling_steps}   \tjob {job_no + 1} / {job_count}  \t{percent * 100:F2}%";
//     }

//     public override string ToString()
//     {
//         var json = JsonConvert.SerializeObject(this, Formatting.Indented);
//         return json;
//     }
// }