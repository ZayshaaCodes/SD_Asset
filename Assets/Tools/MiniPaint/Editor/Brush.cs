using System;
using SdEditorApi;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;

[Serializable]
public class Brush
{
    //brush settings
    [Range(1,     256)]  public float radius  = 32;
    [Range( -.99f, .99f)] public float falloff = 0;
    [Range(-.99f, .99f)] public   float spacing = .02f;

    public void GetCursorImage(Texture2D texture, Color32 color)
    {
        int size = Mathf.CeilToInt(radius * 2);

        if (texture.width != size)
        {
            texture.Reinitialize(size, size);
        }

        new UpdateBrushTextureJob()
        {
            pixels = texture.GetRawTextureData<Color32>(),
            rad    = radius,
            sig    = falloff,
            size   = size,
            color  = color
        }.Run();
        texture.Apply();
    }

    public void DrawHandle()
    {
        Handles.color = new(1f, 1f, 1f, 0.5f);
        Handles.DrawWireDisc(Vector3.zero, Vector3.forward, radius);
    }
}