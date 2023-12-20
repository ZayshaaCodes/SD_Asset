using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.WebSockets;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Unity.EditorCoroutines.Editor;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ComfyEditorWindow : EditorWindow
{

    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;
    public ComfyApiWorkflow workflow;

    private ClientWebSocket webSocket = null;
    private Label _statusLabel;

    private bool _connected { get { return webSocket != null && webSocket.State == WebSocketState.Open; } }
    [SerializeField] private float _progress = 0f;
    [SerializeField] private bool _randomSeed;

    [MenuItem("Comfy/ComfyEditorWindow")]
    public static void ShowExample()
    {
        ComfyEditorWindow wnd = GetWindow<ComfyEditorWindow>();
        wnd.titleContent = new GUIContent("ComfyEditorWindow");
    }

    private async void StartWebSocket()
    {

        if (webSocket == null)
        {
            webSocket = new ClientWebSocket();
        }
        if (webSocket.State != WebSocketState.Open)
        {
            await webSocket.ConnectAsync(new Uri("ws://localhost:8188/ws"), CancellationToken.None);
            // Debug.Log("Connected to comfy");
            _statusLabel.text = "Connected to comfy";
        }

        //make the buffer big enough for 1024x1024 image
        var buffer = new ArraySegment<byte>(new byte[1024 * 1024 * 12]);
        while (webSocket != null && webSocket.State == WebSocketState.Open)
        {
            var rec = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
            // Debug.Log("Received: " + rec.Count + " bytes" + " type: " + rec.MessageType);
            if (rec.MessageType == WebSocketMessageType.Text)
            {
                HandleJsonMessage(buffer, rec);
            }
            else if (rec.MessageType == WebSocketMessageType.Binary)
            {
                HandleBinaryMessage(buffer, rec);
            }
        }
    }

    private void HandleBinaryMessage(ArraySegment<byte> buffer, WebSocketReceiveResult rec)
    {

        //trim the first 4 bytes
        var trimmed = new ArraySegment<byte>(buffer.Array, 8, rec.Count - 8).ToArray();

        //dump first few bytes
        // Debug.Log(BitConverter.ToString(trimmed, 0, 100) + "\nstring: " 
        //     + System.Text.Encoding.UTF8.GetString(trimmed, 0, 100));

        //save the image, format: 
        var tex = new Texture2D(512, 512, TextureFormat.RGB24, false);
        tex.LoadImage(trimmed);
        tex.Apply();

        rootVisualElement.Q<VisualElement>("image").style.backgroundImage = tex;
    }

    private void HandleJsonMessage(ArraySegment<byte> buffer, WebSocketReceiveResult rec)
    {
        var json = System.Text.Encoding.UTF8.GetString(buffer.Array, 0, rec.Count);
        fsData jData = fsJsonParser.Parse(json);
        // Debug.Log(json);

        //{"type":"status","data":{"status":{"exec_info":{"queue_remaining":0}},"sid":"4d7dff64066843d9872d7b19d31998e0"}}
        var typeString = jData.AsDictionary["type"].AsString;
        var data = jData.AsDictionary["data"].AsDictionary;
        if (typeString == "status")
        {
            //update status label for queue remaining
            var status = data["status"]
            .AsDictionary["exec_info"]
            .AsDictionary["queue_remaining"];

            _statusLabel.text = "Queue remaining: " + status.AsInt64;
        }
        //{"type": "progress", "data": {"value": 1, "max": 20}}
        else if (typeString == "progress")
        {
            //val
            var progress = data["value"].AsInt64;
            var max = data["max"].AsInt64;
            _progress = (float)progress / max * 100;
        }
    }

    public void CreateGUI()
    {
        m_VisualTreeAsset.CloneTree(rootVisualElement);

        var root = this.rootVisualElement;

        var connectbutton = root.Q<Button>("connect_button");
        connectbutton.clickable.clicked += () =>
        {
            StartWebSocket();
        };

        var editorgui = Editor.CreateEditor(workflow);
        var imguiwrapper = new IMGUIContainer(() =>
        {
            editorgui.OnInspectorGUI();
        });
        root.Q<VisualElement>("inputs").Add(imguiwrapper);



        root.Q<Button>("render_button").clickable.clicked += () =>
        {
            if (!_connected)
            {
                StartWebSocket();
            }

            //create a new prompt
            JObject data = new JObject
                {
                    { "prompt", workflow.data }
                };

            //find all the seed inputs in data
            if (_randomSeed)
            {

                var nodes = data["prompt"];
                foreach (JProperty node in nodes)
                {
                    var inputs = node.Value["inputs"];
                    foreach (JProperty input in inputs)
                    {
                        if (input.Name.Contains("seed"))
                        {
                            //set to a random value
                            input.Value = UnityEngine.Random.Range(0, 1000000);
                            Debug.Log("Set seed to " + input.Value);
                        }
                    }
                }
            }

            string url = "http://localhost:8188/prompt";

            Debug.Log(data.ToString());

            var cont = new StringContent(data.ToString(), System.Text.Encoding.UTF8, "application/json");

            //send the prompt to comfy
            var response = new System.Net.Http.HttpClient().PostAsync(url, cont).Result;
            var responseString = response.Content.ReadAsStringAsync().Result;
            Debug.Log(responseString);
        };

        root.Bind(new SerializedObject(this));


        //find status label
        _statusLabel = root.Q<Label>("status_label");
    }
}
