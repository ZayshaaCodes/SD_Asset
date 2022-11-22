using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace SdEditorApi
{
    [BurstCompile] public struct UpdateBrushTextureJob : IJob
    {
        public NativeArray<Color32> pixels;
        public Color32              color;
        public int                  size;
        public float                rad;
        public float                sig;

        public void Execute()
        {
            var     center      = new int2(size, size) / 2;
            Color32 fadeToColor = color;
            fadeToColor.a = 0;

            for (int j = 0; j < size; j++)
            for (int i = 0; i < size; i++)
            {
                var pixPos   = new int2(i, j);
                var distance = math.length((pixPos - center));
                if (distance < rad)
                {
                    pixels[i + j * size] = Color32.Lerp(
                        fadeToColor,
                        color,
                        TextureJobUtil.Sigmoid(1 - distance / rad, sig));
                }
                else
                {
                    pixels[i + j * size] = new(255, 255, 255, 0);
                }
            }
        }
    }

    [BurstCompile] public struct DrawJob : IJob
    {
        public            NativeArray<Color32> pixels;
        [ReadOnly] public int2                 center;
        [ReadOnly] public Color32              targetColor;
        [ReadOnly] public float                rad;
        [ReadOnly] public float                sig;

        public void Execute()
        {
            for (int j = (int)math.max(0, center.y - rad); j < (int)math.min(512, center.y + rad); j++)
            for (int i = (int)math.max(0, center.x - rad); i < (int)math.min(512, center.x + rad); i++)
            {
                var pixPos   = new int2(i, j);
                var distance = math.length((pixPos - center));
                if (distance < rad)
                {
                    var pixcolor = pixels[i + j * 512];

                    var newColor = Color32.Lerp(pixcolor, targetColor, TextureJobUtil.Sigmoid(1 - distance / rad, sig));

                    pixels[i + j * 512] = newColor;
                }
            }
        }
    }

    [BurstCompile] public struct CopyTextureJob : IJob
    {
        [ReadOnly] public NativeArray<Color32> source;
        [ReadOnly] public bool                 convertARGBtoRGBA;
        public            NativeArray<Color32> dest;

        public void Execute()
        {
            if (convertARGBtoRGBA)
                for (int i = 0; i < source.Length; i++)
                {
                    var s = source[i];
                    dest[i] = new(s.g, s.b, s.a, s.r);
                }
            else
                for (int i = 0; i < source.Length; i++)
                    dest[i] = source[i];
        }
    }

    [BurstCompile] public struct ClearJob : IJobFor
    {
        [ReadOnly] public Color32              clearColor;
        public            NativeArray<Color32> data;

        public void Execute(int i)
        {
            data[i] = clearColor;
        }
    }

    [BurstCompile] internal struct MixTextureJob : IJobFor
    {
        public            NativeArray<Color32> baseTexture;
        [ReadOnly] public NativeArray<Color32> overlayTexture;
        [ReadOnly] public float                overlayOpacity;

        public void Execute(int i)
        {
            baseTexture[i] = Color32.Lerp(baseTexture[i], overlayTexture[i], (overlayOpacity / 100) * (overlayTexture[i].a / 255f));
        }
    }
    
    [BurstCompile] internal struct MixMaskJob : IJobFor
    {
        public            NativeArray<Color32> baseTexture;
        [ReadOnly] public NativeArray<Color32> overlayTexture;
        [ReadOnly] public float                overlayOpacity;

        public void Execute(int i)
        {
            var baseColor    = baseTexture[i];
            var overlayColor = overlayTexture[i];
            var v            = (byte)math.max((int)baseColor.r, overlayColor.r);
            
            baseTexture[i] = new Color32(v,v,v,255);
        }
    }


    public static class TextureJobUtil
    {
        public static float Sigmoid(float t, float k)
        {
            k *= -1;
            return (k * t - t) / (2 * k * t - k - 1);
        }

        public static void CopyTexture(this Texture2D source, Texture2D dest)
        {
            new CopyTextureJob()
            {
                source            = source.GetRawTextureData<Color32>(),
                dest              = dest.GetRawTextureData<Color32>(),
                convertARGBtoRGBA = source.format == TextureFormat.ARGB32
            }.Run();
            dest.Apply();
        }

        public static void Fill(this Texture2D source, Color32 color)
        {
            new ClearJob()
            {
                data       = source.GetRawTextureData<Color32>(),
                clearColor = color
            }.Run(source.width * source.height);

            source.Apply();
        }
    }
}