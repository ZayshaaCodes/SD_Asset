using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class CnApi
{
    private ApiRequestHandler apiRequestHandler;

    public CnApi(ApiRequestHandler apiRequestHandler)
    {
        this.apiRequestHandler = apiRequestHandler;
    }

    // Route GET /controlnet/model_list
    // Get the list of available ControlNet models. Returns a dictionary of the form {"model_list": [...]}. Each value of "model_list" 
    //is a valid candidate for the "model" property of the ControlNetUnitRequest object described below.
    public IEnumerator GetModelList(Action<modelListResponse> callback)
    {
        yield return apiRequestHandler.SendGetRequest<modelListResponse>("controlnet/model_list", null, response =>
        {
            callback(response);
        });
    }

    //Route GET /controlnet/module_list
    //Get the list of available preprocessors. Returns a dictionary of the form {"module_list": [...]}. Each value of "module_list" is a 
    //valid candidate for the "module" property of the ControlNetUnitRequest object described below.
    // Request parameters:
    // alias_names=true : whether to get the ui alias names instead of internal keys. Defaults to false
    public IEnumerator GetModuleList(Action<moduleListResponse> callback, bool alias_names = false)
    {
        var body = new Dictionary<string, string>();
        body.Add("alias_names", alias_names.ToString());

        yield return apiRequestHandler.SendGetRequest<moduleListResponse>("controlnet/module_list", body, response =>
        {
            callback(response);
        });
    }
}

[System.Serializable]
public class moduleListResponse
{
    public List<string> module_list;
}

[System.Serializable]
public class modelListResponse
{
    public List<string> model_list;
}