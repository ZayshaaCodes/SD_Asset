using UnityEngine;
using UnityEditor;
using System.IO;

public class SpriteSheetExtractor : MonoBehaviour
{
    [MenuItem("Assets/Sprite Sheet/Extract Sprites")]
    static void ExtractSprites()
    {
        // Ensure an object is selected
        if (Selection.activeObject == null) return;

        // Ensure the selection is a Texture2D
        Texture2D spriteSheet = Selection.activeObject as Texture2D;
        if (spriteSheet == null) return;

        string path = AssetDatabase.GetAssetPath(spriteSheet);
        string directory = Path.GetDirectoryName(path);

        // Load all sprites from the sprite sheet
        Object[] objects = AssetDatabase.LoadAllAssetsAtPath(path);
        foreach (Object obj in objects)
        {
            if (obj is Sprite)
            {
                Sprite sprite = (Sprite)obj;
                SaveSpriteAsPNG(sprite, directory);
            }
        }
    }

    static void SaveSpriteAsPNG(Sprite sprite, string directory)
    {
        if (sprite == null) return;

        // Create a new Texture and copy the sprite's data to it
        Rect spriteRect = sprite.rect;
        Texture2D tex = new Texture2D((int)spriteRect.width, (int)spriteRect.height, TextureFormat.RGBA32, false);
        Color[] pixels = sprite.texture.GetPixels(
            (int)spriteRect.x,
            (int)spriteRect.y,
            (int)spriteRect.width,
            (int)spriteRect.height);
        tex.SetPixels(0, 0, (int)spriteRect.width, (int)spriteRect.height, pixels);
        tex.Apply();

        // Encode the texture to a PNG
        byte[] bytes = tex.EncodeToPNG();
        Object.DestroyImmediate(tex);

        // Write the PNG to a file
        File.WriteAllBytes(Path.Combine(directory, sprite.name + ".png"), bytes); 
        AssetDatabase.Refresh();
    }

}
