using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using Unity.EditorCoroutines.Editor;
using UnityEngine;

[ExecuteAlways]
public class WebSocketServer : MonoBehaviour
{
    HttpListener listener;
    string url = "http://localhost:8182/";

    public string status;
    private Thread listenerThread;
    private ConcurrentQueue<HttpListenerContext> contextQueue;

    // Assuming you have a reference to your RenderTexture here
    public RenderTexture renderTexture;

    void OnEnable()
    {
        listener = new HttpListener();
        listener.Prefixes.Add(url);
        listener.Start();

        listenerThread = new Thread(ListenerThread);
        listenerThread.Start();

        contextQueue = new ConcurrentQueue<HttpListenerContext>();

        status = "Listening on " + url;
        //use editor coroutine to listen for requests
        EditorCoroutineUtility.StartCoroutineOwnerless(Listen());
    }
    private void ListenerThread()
    {
        while (listener.IsListening)
        {
            var context = listener.GetContext();
            contextQueue.Enqueue(context);
        }
    }

    private IEnumerator Listen()
    {
        while (true)
        {
            if (contextQueue.TryDequeue(out var context))
            {
                var request = context.Request;
                var response = context.Response;
                Texture2D texture = RenderTextureToTexture2D(renderTexture);

                byte[] png = texture.EncodeToPNG();

                response.ContentLength64 = png.Length;
                response.ContentType = "image/png";
                response.StatusCode = 200;
                response.StatusDescription = "OK";
                response.OutputStream.Write(png, 0, png.Length);
                response.OutputStream.Close();
            }

            yield return null;
        }
    }

    Texture2D RenderTextureToTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGBA32, false);
        RenderTexture currentActiveRT = RenderTexture.active;
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        tex.Apply();
        RenderTexture.active = currentActiveRT;
        return tex;
    }

    void OnDisable()
    {
        if (listener != null)
        {
            listener.Stop();
            listener.Close();

            status = "Stopped";
        }
        if (listenerThread != null)
        {
            listenerThread.Abort();
        }

    }
}
