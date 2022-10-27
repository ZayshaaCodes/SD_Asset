using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

[ExecuteAlways]
public class ApiTest : MonoBehaviour
{
    // Start is called before the first frame update

    Dictionary<string, object> postData = new();

    public Texture2D output;

    void Start()
    {
        output = new Texture2D(512, 512, TextureFormat.RGB24, false);
        StartCoroutine(test());
    }

    private IEnumerator test()
    {
        postData = new()
        {
            { "enable_hr", false },
            { "denoising_strength", 0 },
            { "firstphase_width", 0 },
            { "firstphase_height", 0 },
            { "prompt", "doggo" },
            { "styles", new List<string>() { "string" } },
            { "seed", -1 },
            { "subseed", -1 },
            { "subseed_strength", 0 },
            { "seed_resize_from_h", -1 },
            { "seed_resize_from_w", -1 },
            { "batch_size", 1 },
            { "n_iter", 1 },
            { "steps", 10 },
            { "cfg_scale", 7 },
            { "width", 512 },
            { "height", 512 },
            { "restore_faces", false },
            { "tiling", false },
            { "negative_prompt", "" },
            { "eta", 0 },
            { "s_churn", 0 },
            { "s_tmax", 0 },
            { "s_tmin", 0 },
            { "s_noise", 1 },
            { "sampler_index", "Euler A" }
        };

        var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(postData, Formatting.Indented));

        var request = new UnityWebRequest("http://localhost:7860/sdapi/v1/txt2img", UnityWebRequest.kHttpVerbPOST);

        UploadHandlerRaw uH = new UploadHandlerRaw(data);
        DownloadHandler  dH = new DownloadHandlerBuffer();
        request.uploadHandler   = uH;
        request.downloadHandler = dH;

        request.useHttpContinue = false;
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        print("Error: " + request.result);
        if (request.result == UnityWebRequest.Result.Success)
        {
            var reqJson = JObject.Parse(dH.text);
            var imageStr  = reqJson["images"][0].ToString();
            
            var imgData = Convert.FromBase64String(imageStr);
            output.LoadImage(imgData);
        }
    }
}