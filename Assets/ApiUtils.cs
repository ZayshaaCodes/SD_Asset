using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

public static class ApiUtils
{
    public static IEnumerator ApiRequest(int functionId, Dictionary<string, object> parameters = null, Action<string> callback = null)
    {
        if (parameters is null) parameters = new();
        parameters.Add("fn_index", functionId);

        var reqBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(parameters, Formatting.Indented));

        var webRequest = new UnityWebRequest("http://localhost:7860/api/predict/", UnityWebRequest.kHttpVerbPOST);

        DownloadHandler dH = new DownloadHandlerBuffer();
        webRequest.downloadHandler = dH;
        webRequest.uploadHandler   = new UploadHandlerRaw(reqBytes);
        webRequest.useHttpContinue = false;
        webRequest.SetRequestHeader("Content-Type", "application/json");

        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            // Debug.Log(dH.text);
            var reqJson = JObject.Parse(dH.text);
        }

        callback?.Invoke(dH.text);
    }
}