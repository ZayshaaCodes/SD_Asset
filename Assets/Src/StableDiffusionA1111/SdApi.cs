using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEngine;
using UnityEngine.Networking;

namespace StableDiffusion
{

    public class SdApi
    {
        private readonly IApiRequestHandler _apiRequestHandler;

        public List<Texture> outImages = new List<Texture>();
        Texture2D currentPreviewImage = null;
        string currentImageStringHash = null;

        public SdApi(IApiRequestHandler apiRequestHandler)
        {
            _apiRequestHandler = apiRequestHandler;
        }

        private bool isGenerating = false;
        public bool IsGenerating => isGenerating;
        private float progress = 0;
        public float Progress => progress;

        public SdOptionsInfo optionsInfo;

        public IEnumerator Generate(JObject data, Action<List<Texture>> callback, Action<SdProgressData, Texture2D> progressCallback = null)
        {

            File.WriteAllText("Assets/req.json", JsonConvert.SerializeObject(data, Formatting.Indented));

            if (isGenerating)
            {
                Debug.LogWarning("Already generating");
                yield break;
            }

            isGenerating = true;
            //start progresss checking
            if (progressCallback != null)
            {
                EditorCoroutineUtility.StartCoroutineOwnerless(CheckProgress(p =>
                {
                    //use basic sha256 hash to check if the image string has changed
                    var newImageStringHash = HashUtils.Sha256(p.current_image);

                    //if the image string has changed, update the image
                    if (newImageStringHash != currentImageStringHash)
                    {
                        // Debug.Log($"Image string changed, updating image...");
                        currentPreviewImage = TextureUtils.FromBase64String(p.current_image);
                    }

                    currentImageStringHash = newImageStringHash;
                    progress = p.progress;

                    progressCallback.Invoke(p , currentPreviewImage);
                }));
            }

            // Use the _apiRequestHandler to send the request
            yield return _apiRequestHandler.SendPostRequest<SdGenerateResponse>($"sdapi/v1/{(data["init_images"] != null ? "img2img" : "txt2img")}", data,
            res =>
            {
                //write the result to a new file named "result.json" using unity's project folder
                File.WriteAllText("Assets/result.json", JsonConvert.SerializeObject(res, Formatting.Indented));

                outImages = res.images;

                callback?.Invoke(res.images);
            });


            isGenerating = false;

            currentPreviewImage = null;
            currentImageStringHash = null;
        }

        public IEnumerator Interrogate(Texture image, Action<SdInterrogateResponse> callback)
        {
            yield return _apiRequestHandler.SendPostRequest<SdInterrogateResponse>("sdapi/v1/interrogate", new Dictionary<string, object> { { "image", image }, { "model", "clip" } }, response =>
            {
                callback(response);
            });
        }

        [System.Serializable]
        public class SdInterrogateResponse
        {
            public List<SdInterrogateDetail> detail;
        }

        [System.Serializable]
        public class SdInterrogateDetail
        {
            public List<string> loc;
            public string msg;
            public string type;
        }

        //sdapi/v1/interrupt
        public IEnumerator Interrupt(Action callback)
        {
            yield return _apiRequestHandler.SendGetRequest<SdProgressData>("sdapi/v1/interrupt", null, response =>
            {
                callback?.Invoke();
            });
        }

        public IEnumerator CheckProgress(Action<SdProgressData> callback)
        {
            while (isGenerating)
            {
                yield return _apiRequestHandler.SendGetRequest<SdProgressData>("sdapi/v1/progress", null, response =>
                {
                    callback(response);
                });

                yield return new EditorWaitForSeconds(.25f);
            }
        }

        [Serializable]
        public class SdProgressData
        {
            public float progress;
            public float eta_relative;
            public SdProgressState state;
            public string current_image;
            public string textinfo;


            [JsonIgnore]
            public string Info
            {
                get => $"step {state.sampling_step} / {state.sampling_steps}   \tjob {state.job_no + 1} / {state.job_count}  \t{progress * 100:F2}%";
            }
        }

        [Serializable]
        public class SdProgressState
        {
            public bool skipped;
            public bool interrupted;
            public string job;
            public int job_count;
            public string job_timestamp;
            public int job_no;
            public int sampling_step;
            public int sampling_steps;
        }

        public IEnumerator GetModels(Action<List<SdModelInfo>> callback)
        {
            yield return _apiRequestHandler.SendGetRequest<List<SdModelInfo>>("sdapi/v1/sd-models", null, response =>
            {
                callback(response);
            });
        }

        public IEnumerator SetModel(string title, Action callback = null)
        {
            yield return _apiRequestHandler.SendPostRequest<SdModelInfo>("sdapi/v1/options", new Dictionary<string, string> { { "sd_model_checkpoint", title } }, response =>
            {
                callback?.Invoke();
            });
        }

        [System.Serializable]
        public class SdModelInfo
        {
            public string title;
            public string model_name;
            public string hash;
            public string sha256;
            public string filename;
            public string config;
        }

        public IEnumerator GetSamplers(Action<List<SdSamplerInfo>> callback)
        {
            yield return _apiRequestHandler.SendGetRequest<List<SdSamplerInfo>>("sdapi/v1/samplers", null, response =>
            {
                callback(response);
            });
        }


        [System.Serializable]
        public class SdSamplerInfo
        {
            public string name;
            public List<string> aliases;
            public Dictionary<string, string> options;
        }


        public IEnumerator GetUpscalers(Action<List<SdUpscalerInfo>> callback)
        {
            yield return _apiRequestHandler.SendGetRequest<List<SdUpscalerInfo>>("sdapi/v1/upscalers", null, response =>
            {
                callback(response);
            });
        }

        [System.Serializable]
        public class SdUpscalerInfo
        {
            public string name;
            public string model_name;
            public string model_path;
            public string model_url;
            public int scale;
        }

        public IEnumerator GetHypernetworks(Action<List<SdHypernetworkInfo>> callback)
        {
            yield return _apiRequestHandler.SendGetRequest<List<SdHypernetworkInfo>>("sdapi/v1/hypernetworks", null, response =>
            {
                callback(response);
            });
        }

        [System.Serializable]
        public class SdHypernetworkInfo
        {
            public string name;
            public string path;
        }


        public IEnumerator GetFaceRestorers(Action<List<SdFaceRestorerInfo>> callback)
        {
            yield return _apiRequestHandler.SendGetRequest<List<SdFaceRestorerInfo>>("sdapi/v1/face-restorers", null, response =>
            {
                callback(response);
            });
        }

        [System.Serializable]
        public class SdFaceRestorerInfo
        {
            public string name;
            public string cmd_dir;
        }

        public IEnumerator GetRealesrganModels(Action<List<SdRealesrganModelInfo>> callback)
        {
            yield return _apiRequestHandler.SendGetRequest<List<SdRealesrganModelInfo>>("sdapi/v1/realesrgan-models", null, response =>
            {
                callback(response);
            });
        }

        [System.Serializable]
        public class SdRealesrganModelInfo
        {
            public string name;
            public string path;
            public int scale;
        }

        public IEnumerator GetPromptStyles(Action<List<SdPromptStyleInfo>> callback)
        {
            yield return _apiRequestHandler.SendGetRequest<List<SdPromptStyleInfo>>("sdapi/v1/prompt-styles", null, response =>
            {
                callback(response);
            });
        }

        [System.Serializable]
        public class SdPromptStyleInfo
        {
            public string name;
            public string prompt;
            public string negative_prompt;
        }


        public IEnumerator GetEmbeddings(Action<List<SdEmbeddingInfo>> callback)
        {
            yield return _apiRequestHandler.SendGetRequest<List<SdEmbeddingInfo>>("sdapi/v1/embeddings", null, response =>
            {
                callback(response);
            });
        }

        [System.Serializable]
        public class SdEmbeddingInfo
        {
            public Dictionary<string, SdEmbeddingInfoDetail> loaded;
            public Dictionary<string, SdEmbeddingInfoDetail> skipped;
        }

        [System.Serializable]
        public class SdEmbeddingInfoDetail
        {
            public int step;
            public string sd_checkpoint;
            public string sd_checkpoint_name;
            public int shape;
            public int vectors;
        }

        //sdapi/v1/memory
        public IEnumerator GetMemory(Action<SdMemRootInfo> callback)
        {
            yield return _apiRequestHandler.SendGetRequest<SdMemRootInfo>("sdapi/v1/memory", null, response =>
            {
                callback(response);
            });
        }

        public IEnumerator GetModel(Action<string> callback)
        {
            yield return _apiRequestHandler.SendGetRequest<SdOptionsInfo>("sdapi/v1/options", null, response =>
            {
                optionsInfo = response;
                callback?.Invoke(response.sd_model_checkpoint);
            });
        }

        public IEnumerator GetSdOptions(Action<SdOptionsInfo> callback)
        {
            yield return _apiRequestHandler.SendGetRequest<SdOptionsInfo>("sdapi/v1/options", null, response =>
            {
                optionsInfo = response;
                callback?.Invoke(response);
            });
        }


        [Serializable]
        public class SdRamInfo
        {
            public long free;
            public long used;
            public long total;
        }

        [Serializable]
        public class SdSystemInfo
        {
            public long free;
            public long used;
            public long total;
        }

        [Serializable]
        public class SdActiveInfo
        {
            public long current;
            public long peak;
        }

        [Serializable]
        public class SdAllocatedInfo
        {
            public long current;
            public long peak;
        }

        [Serializable]
        public class SdReservedInfo
        {
            public long current;
            public long peak;
        }

        [Serializable]
        public class SdInactiveInfo
        {
            public long current;
            public long peak;
        }

        [Serializable]
        public class SdEventsInfo
        {
            public int retries;
            public int oom;
        }

        [Serializable]
        public class SdCudaInfo
        {
            public SdSystemInfo system;
            public SdActiveInfo active;
            public SdAllocatedInfo allocated;
            public SdReservedInfo reserved;
            public SdInactiveInfo inactive;
            public SdEventsInfo events;
        }

        [Serializable]
        public class SdMemRootInfo
        {
            public SdRamInfo ram;
            public SdCudaInfo cuda;
        }

    }
}