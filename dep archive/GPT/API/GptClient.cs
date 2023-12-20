using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using GPT.API.Objects;
using Newtonsoft.Json;
using UnityEngine;

namespace GPT.API
{
    public class GptClient
    {
        private          string                 _key;
        private          string                 _org;
        private readonly JsonSerializerSettings _settings;
        private readonly OpenAiEndpointProvider _endpoints;

        public GptClient(string apiKey, string org = null)
        {
            _key = apiKey;
            _org = org;

            _settings = new()
            {
                NullValueHandling = NullValueHandling.Ignore,
            };
            _settings.Converters.Add(new MessageConverter());
            _endpoints          = new("v1");
        }

        public HttpClient GetClient()
        {
            HttpClient client             = new();
            client.BaseAddress = new("https://api.openai.com/");

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_key}");
            if (_org != null)
            {
                client.DefaultRequestHeaders.Add("OpenAI-Organization", $"{_org}");
            }
            
            return client;
        }

        public IEnumerator DoChatCompletionStreaming(List<Message> messages, List<GptFunction> functions, Action<string> updateCallback)
        {
            ChatRequest chatReq = new ChatRequest()
            {
                messages = messages,
                model    = "gpt-4",
                stream   = true
            };
            if (functions != null)
            {
                chatReq.functions = functions;
            }

            string json = JsonConvert.SerializeObject(chatReq, Formatting.Indented, _settings);
            File.WriteAllText("Assets/GPT/last_request.json", json);

            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, _endpoints.ChatCompletionCreate()) 
            {
                Content = content
            };

            var httpClient = GetClient();
            var response   = httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            while (!response.IsCompleted)
            {
                yield return null;
            }
            
            response.Result.EnsureSuccessStatusCode();
            var stream = response.Result.Content.ReadAsStreamAsync();
            var reader = new StreamReader(stream.Result);
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLineAsync();
                    
                //example: data: {"id":"chatcmpl-7aYRPCnaEGSSnT2MOn0H0n1YeW0LS","object":"chat.completion.chunk","created":1688946555,"model":"gpt-4-0613","choices":[{"index":0,"delta":{"content":"User"},"finish_reason":null}]}
                //try parse the json with newtonsoft
                // need to prune the "data: " from the start of the line
                if (line.Result.StartsWith("data: "))
                {
                    var pruned = line.Result.Substring(6);
                    
                    //if is says "[DONE]" then we're done and we can break out of the loop
                    if (pruned == "[DONE]")
                    {
                        break;
                    }
    
                    try
                    {
                        var parsed = JsonConvert.DeserializeObject<ChatCompletion>(pruned, _settings);
                        if (parsed != null)
                        {
                            updateCallback(parsed.choices[0].delta.content);
                        }
                        else
                        {
                            //if it's not a valid chat completion, log it as a string
                            Debug.Log(line.Result);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log(pruned + "\n" + e);
                    }
                    

                }
                //destroy the client
                
                yield return null;
            }
                
            httpClient.Dispose();

        }
        
    }
}