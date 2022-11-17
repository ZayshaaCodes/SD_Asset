using UnityEngine;

namespace Editor
{
    [System.Serializable]
    public class ImageLayer
    {
        public string    name;
        public Texture2D image;
        public 

            public ImageLayer(string name, Texture2D image)
        {
            this.name  = name;
            this.image = image;
        }
    }
}