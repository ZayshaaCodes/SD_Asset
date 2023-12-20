using System;
using System.Collections;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public interface IApiRequestHandler
{
    IEnumerator SendPostRequest<T>(string endpoint, object data, Action<T> callback = null);
    IEnumerator SendGetRequest<T>(string endpoint, object data = null, Action<T> callback = null);

    // UnityWebRequest SimpleGetRequest(string endpoint);

    // async versions
    // Task<T> SendPostRequestAsync<T>(string endpoint, object data);
    // Task<T> SendGetRequestAsync<T>(string endpoint, object data = null);


}

public class ApiRequestHandler : IApiRequestHandler
{
    private readonly string _baseUrl;

    public ApiRequestHandler(string baseUrl)
    {
        _baseUrl = baseUrl;
    }

    public IEnumerator SendPostRequest<T>(string endpoint, object data, Action<T> callback = null)
    {
        var reqBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data, Formatting.Indented));

        var webRequest = new UnityWebRequest(_baseUrl + endpoint, UnityWebRequest.kHttpVerbPOST);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.uploadHandler = new UploadHandlerRaw(reqBytes);
        webRequest.useHttpContinue = false;
        webRequest.SetRequestHeader("Content-Type", "application/json");

        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            // Debug.Log(typeof(T).ToString());
            var responseObject = JsonConvert.DeserializeObject<T>(webRequest.downloadHandler.text);
            callback?.Invoke(responseObject);
        }

        webRequest.Dispose();
    }

    public IEnumerator SendGetRequest<T>(string endpoint,  object data = null, Action<T> callback = null)
    {
        var webRequest = new UnityWebRequest(_baseUrl + endpoint, UnityWebRequest.kHttpVerbGET);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.useHttpContinue = false;
        if (data != null)
        {
            var reqBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data, Formatting.Indented));
            webRequest.uploadHandler = new UploadHandlerRaw(reqBytes);
        }

        yield return webRequest.SendWebRequest();

        //save it to a file rq
        File.WriteAllText(Application.dataPath + "/../" + "test.json", webRequest.downloadHandler.text);

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            var responseObject = JsonConvert.DeserializeObject<T>(webRequest.downloadHandler.text);
            callback?.Invoke(responseObject);
        }

        webRequest.Dispose();
    }
    //
    // public UnityWebRequest SimpleGetRequest(string endpoint)
    // {
    //     var webRequest = new UnityWebRequest(_baseUrl + endpoint, UnityWebRequest.kHttpVerbGET);
    //     webRequest.downloadHandler = new DownloadHandlerBuffer();
    //     webRequest.useHttpContinue = false;
    //     
    //     return webRequest;
    // }

    // public async Task<T> SendPostRequestAsync<T>(string endpoint, object data)
    // {
    //     var reqBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data, Formatting.Indented));

    //     var webRequest = new UnityWebRequest(_baseUrl + endpoint, UnityWebRequest.kHttpVerbPOST);
    //     webRequest.downloadHandler = new DownloadHandlerBuffer();
    //     webRequest.uploadHandler = new UploadHandlerRaw(reqBytes);
    //     webRequest.useHttpContinue = false;
    //     webRequest.SetRequestHeader("Content-Type", "application/json");

    //     await Task.Run(() => webRequest.SendWebRequest());

    //     if (webRequest.result == UnityWebRequest.Result.Success)
    //     {
    //         var responseObject = JsonConvert.DeserializeObject<T>(webRequest.downloadHandler.text);
    //         return responseObject;
    //     }

    //     webRequest.Dispose();
    //     return default;
    // }

    // public async Task<T> SendGetRequestAsync<T>(string endpoint, object data = null)
    // {
    //     var webRequest = new UnityWebRequest(_baseUrl + endpoint, UnityWebRequest.kHttpVerbGET);
    //     webRequest.downloadHandler = new DownloadHandlerBuffer();
    //     webRequest.useHttpContinue = false;
    //     if (data != null)
    //     {
    //         var reqBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data, Formatting.Indented));
    //         webRequest.uploadHandler = new UploadHandlerRaw(reqBytes);
    //     }

    //     await Task.Run(() => webRequest.SendWebRequest());

    //     if (webRequest.result == UnityWebRequest.Result.Success)
    //     {
    //         var responseObject = JsonConvert.DeserializeObject<T>(webRequest.downloadHandler.text);
    //         return responseObject;
    //     }

    //     webRequest.Dispose();
    //     return default;
    // }
}
