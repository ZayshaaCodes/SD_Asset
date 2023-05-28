using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Unity.EditorCoroutines.Editor;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class SdControlnetCamera : MonoBehaviour
{
    public SdAttributes requestData = SdAttributes.defaults;
    public RenderTexture outputTexture;
    public List<Texture2D> results;
    public RawImage targetUiImage;

    public SdApi sdApi;
    public CnApi cnApi;

    // onenable
    void OnEnable()
    {
        sdApi = new SdApi(new ApiRequestHandler("http://localhost:7860/"));
        cnApi = new CnApi(new ApiRequestHandler("http://localhost:7860/"));

        // Debug.Log("Getting models");
        // EditorCoroutineUtility.StartCoroutineOwnerless(cnApi.GetModelList(res =>
        // {
        //     Debug.Log($"Got {res.model_list.Count} models");
        //     //write the models to a new file named "models.json" using unity's project folder
        //     File.WriteAllText("Assets/models.json", JsonConvert.SerializeObject(res, Formatting.Indented));
        // }));

        // Debug.Log("Getting modules");
        // EditorCoroutineUtility.StartCoroutineOwnerless(cnApi.GetModuleList(res =>
        // {
        //     Debug.Log($"Got {res.module_list.Count} modules");
        //     //write the modules to a new file named "modules.json" using unity's project folder
        //     File.WriteAllText("Assets/modules.json", JsonConvert.SerializeObject(res, Formatting.Indented));
        // }));

    }


    // Update is called once per frame
    void Update()
    {
        // if space is pressed, generate
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(Generate());
        }
    }

    public IEnumerator Generate(){
        // convert the render texture to a texture2d
            var rt = new Texture2D(outputTexture.width, outputTexture.height, TextureFormat.RGBA32, false);
            var currentRT = RenderTexture.active;
            RenderTexture.active = outputTexture;
            rt.ReadPixels(new Rect(0, 0, outputTexture.width, outputTexture.height), 0, 0);
            rt.Apply();
            RenderTexture.active = currentRT;

            foreach (var arg in requestData.alwayson_scripts.controlNet.args)
            {
                arg.input_image = rt;
            }

            yield return sdApi.Generate(requestData, images =>
            {
                results = images;
                if (targetUiImage != null){
                    targetUiImage.texture = results[0];
                }
            });
    }
}

#if UNITY_EDITOR
//custom edutor with "generate button"
[CustomEditor(typeof(SdControlnetCamera))]
public class SdControlnetCameraEditor : Editor
{
    SdControlnetCamera tar;
    void OnEnable()
    {
        tar = (SdControlnetCamera)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SdControlnetCamera myScript = (SdControlnetCamera)target;
        if (GUILayout.Button("Generate"))
        {
            EditorCoroutineUtility.StartCoroutine(tar.Generate(), tar);
        }
        //render each of the texture is tar.results is not null

        if (tar.results != null)
        {
            //render at half size, render in a grid that is 2 wide, use guilayout
            int c = 0;
            
            GUILayout.BeginHorizontal();
            foreach (var tex in tar.results)
            {
                if (tex != null)
                {
                    var w = tex.width / 2;
                    var h = tex.height / 2;
                    GUILayout.Label(tex, GUILayout.Width(w), GUILayout.Height(h));
                    c++;
                    if (c % 2 == 0)
                    {
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                    }
                }
            }
            GUILayout.EndHorizontal();


        }
        
    }

}
#endif

