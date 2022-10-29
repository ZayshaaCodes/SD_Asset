using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
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

    public static void SetModel(string model)
    {
        EditorCoroutineUtility.StartCoroutineOwnerless(ApiRequest(ApiFunction.SetModel, new() { { "data", new List<string>() {model} } }));
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
    }

    public static IEnumerator GetCurModel(Action<string> callback)
    {
        yield return ApiRequest(ApiFunction.GetCurModel, null, (s, jo) =>
        {
            callback?.Invoke(jo["data"]?[0]?.ToString());
        });
    }
}