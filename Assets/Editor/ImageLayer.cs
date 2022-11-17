using UnityEngine;

public enum LayerType
{
    Base, Mask, Paint
}

[System.Serializable]
public class ImageLayer
{
    public string    name;
    public Texture2D image;
    public LayerType type;

    public ImageLayer(string name, Texture2D image, LayerType type = LayerType.Paint)
    {
        this.name  = name;
        this.image = image;
        this.type  = type;
    }
}