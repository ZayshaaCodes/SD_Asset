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
    public float     opacity;
    public bool      active;

    public ImageLayer(string name, Texture2D image, LayerType type = LayerType.Paint)
    {
        this.name  = name;
        this.image = image;
        this.type  = type;
        active     = true;
        opacity    = 100;
    }
}