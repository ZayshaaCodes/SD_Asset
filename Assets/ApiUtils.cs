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

public static class ApiUtils
{
    public enum ApiFunction
    {
        GetModels = 0, GetSamplers = 289, GetCurModel = 317, SetModel = 302
    }

    public static string Tob64String(this Texture2D texture)
    {
        if (texture == null) return null;
        return Convert.ToBase64String(texture.EncodeToPNG());
    }

    public static IEnumerator Generate(RequestData requestData, Action<List<SdImage>> callback)
    {
        var images       = new List<SdImage>();
        var hasInitImage = requestData.init_images.Count > 0;
        var request      = new UnityWebRequest($"http://localhost:7860/sdapi/v1/{(hasInitImage ? "img2img" : "txt2img")}", UnityWebRequest.kHttpVerbPOST);

        UploadHandlerRaw uH = new UploadHandlerRaw(requestData.GetFieldDataBytes());
        DownloadHandler  dH = new DownloadHandlerBuffer();

        request.uploadHandler   = uH;
        request.downloadHandler = dH;

        request.useHttpContinue = false;
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var resJson     = JObject.Parse(dH.text);
            var imagesToken = resJson["images"];
            var infoToken   = resJson["info"];
            
            var info        = infoToken.ToString();
            var seedmatch   = Regex.Match(info, @"\sseed: (\d+)").Groups[1];

            var c           = imagesToken!.Count();
            for (int i = 0; i < c; i++)
            {
                var t2d = new Texture2D(requestData.width, requestData.height);
                t2d.LoadImage(Convert.FromBase64String(imagesToken[i]!.ToString()));
                t2d.Apply();
                images.Add(new (t2d, info));
            }

            uH.Dispose();
            request.Dispose();

            callback?.Invoke(images);
        }
    }


    public static void GetModels(List<string> models)
    {
        models.Clear();
        EditorCoroutineUtility.StartCoroutineOwnerless(ApiRequest(ApiFunction.GetModels, null, (s, o) =>
        {
            if (o["data"]?[0]?["choices"] is { } choices)
            {
                foreach (var choice in choices)
                {
                    models.Add(choice.ToString());
                }
            }
        }));
    }

    public static IEnumerator GetCurModel(Action<string> callback)
    {
        yield return ApiRequest(ApiFunction.GetCurModel, null, (s, jo) =>
        {
            callback?.Invoke(jo["data"]?[0]?.ToString());
        });
    }

    public static void SetModel(string model)
    {
        EditorCoroutineUtility.StartCoroutineOwnerless(ApiRequest(ApiFunction.SetModel, new() { { "data", new List<string>() { model } } }));
    }

    public static void GetSamplers(List<string> samplers)
    {
        samplers.Clear();
        EditorCoroutineUtility.StartCoroutineOwnerless(ApiRequest(ApiFunction.GetSamplers, null, (s, o) =>
        {
            if (o["data"]?[0] is { } choices)
            {
                foreach (var choice in choices)
                {
                    samplers.Add(choice.ToString());
                }
            }
        }));
    }

    public static IEnumerator ApiRequest(ApiFunction function, Dictionary<string, object> parameters = null, Action<string, JObject> callback = null) =>
        ApiRequest((int)function, parameters, callback);

    public static IEnumerator ApiRequest(int function, Dictionary<string, object> parameters = null, Action<string, JObject> callback = null)
    {
        if (parameters is null) parameters = new();
        parameters.Add("fn_index", function);

        var reqBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(parameters, Formatting.Indented));

        var webRequest = new UnityWebRequest("http://localhost:7860/api/predict/", UnityWebRequest.kHttpVerbPOST);

        DownloadHandler dH = new DownloadHandlerBuffer();
        webRequest.downloadHandler = dH;
        webRequest.uploadHandler   = new UploadHandlerRaw(reqBytes);
        webRequest.useHttpContinue = false;
        webRequest.SetRequestHeader("Content-Type", "application/json");

        yield return webRequest.SendWebRequest();

        //Debug.Log(webRequest.result);
        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            // Debug.Log(dH.text);
            var reqJson = JObject.Parse(dH.text);
            callback?.Invoke(dH.text, reqJson);
        }

        webRequest.Dispose();
    }
}