using System;
using System.Net.Http;
using System.Net.WebSockets;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using System.Threading;
using System.Text;
using System.Collections;
using System.Threading.Tasks;

public class ComfyClient : MonoBehaviour
{
    public ComfyApiWorkflow workflow ;
    private ClientWebSocket webSocket = null;
    private ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);

    private void Start()
    {
        ConnectWebSocket();
    }

    private void OnDestroy()
    {
        webSocket?.Dispose();
    }


    private void ConnectWebSocket()
    {
        webSocket = new ClientWebSocket();
        Connect();
    }

    private async void Connect()
    {
        await webSocket.ConnectAsync(new Uri("ws://127.0.0.1:8188/ws"), CancellationToken.None);

        Debug.Log("Connected to comfy");

        StartCoroutine(ReceiveMessages());
    }

    private IEnumerator ReceiveMessages()
    {
        while (true)
        {
            yield return ReceiveMessage();

            if (webSocket.State != WebSocketState.Open)
            {
                Debug.Log("Websocket closed");
                break;
            }
        }
    }

    private async Task ReceiveMessage()
    {
        WebSocketReceiveResult result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);

        if (result.MessageType == WebSocketMessageType.Text)
        {
            string message = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
            Debug.Log("Received: " + message);
        }
    }

    public void Render()
    {
        QueuePrompt();
    }

    public void QueuePrompt()
    {
        //create a new prompt
        JObject data = new JObject
        {
            { "prompt", workflow.data }
        };
        string url = "http://localhost:8188/prompt";

        print(data.ToString());

        var cont = new StringContent(data.ToString(), System.Text.Encoding.UTF8, "application/json");

        //send the prompt to comfy
        var client = new HttpClient();
        client.PostAsync(url, cont);
    }
}

//custom inspector for ComfyClient
[CustomEditor(typeof(ComfyClient))]
public class ComfyClientEditor : Editor
{
    public ComfyClient tar;
    public override void OnInspectorGUI()
    {
        //default inspector
        DrawDefaultInspector();

        tar = (ComfyClient)target;

        //Render button
        if (GUILayout.Button("Render"))
        {
            tar.Render();
        }

        if (tar?.workflow != null)
        {
            //start a group box
            GUILayout.BeginVertical("box");
            var editor = CreateEditor(tar.workflow);
            editor.OnInspectorGUI();
            GUILayout.EndVertical();
        }
    }
}
