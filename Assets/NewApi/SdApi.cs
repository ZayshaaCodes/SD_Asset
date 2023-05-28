using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Unity.EditorCoroutines.Editor;
using UnityEngine;

public class SdApi
{
    private readonly IApiRequestHandler _apiRequestHandler;
    public List<Texture2D> outImages = new List<Texture2D>();

    public SdApi(IApiRequestHandler apiRequestHandler)
    {
        _apiRequestHandler = apiRequestHandler;
    }

    private bool isGenerating = false;

    public IEnumerator Generate(SdAttributes requestData, Action<List<Texture2D>> callback, Action<SdProgressData> progressCallback = null)
    {
        File.WriteAllText("Assets/req.json", JsonConvert.SerializeObject(requestData, Formatting.Indented));

        if (isGenerating)
        {
            Debug.LogWarning("Already generating");
            yield break;
        }

        isGenerating = true;
        //start progresss checking
        if (progressCallback != null)
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(CheckProgress(pd =>
            {
                progressCallback.Invoke(pd);
            }));
        }

        // Use the _apiRequestHandler to send the request
        // yield return _apiRequestHandler.SendPostRequest<SdGenerateResponse>($"sdapi/v1/{(requestData.init_images.Count > 0 ? "img2img" : "txt2img")}", requestData,
        yield return _apiRequestHandler.SendPostRequest<SdGenerateResponse>($"sdapi/v1/{(0 > 0 ? "img2img" : "txt2img")}", requestData,
        res =>
        {
            //write the result to a new file named "result.json" using unity's project folder
            File.WriteAllText("Assets/result.json", JsonConvert.SerializeObject(res, Formatting.Indented));

            Debug.Log($"Generated {res.images.Count} images");

            outImages = res.images;

            callback?.Invoke(res.images);
        });

        isGenerating = false;
    }


    public IEnumerator CheckProgress(Action<SdProgressData> callback)
    {
        while (isGenerating)
        {
            yield return _apiRequestHandler.SendGetRequest<SdProgressData>("sdapi/v1/progress", null, response =>
            {
                callback(response);
            });

            yield return new EditorWaitForSeconds(2);
        }
    }

}
