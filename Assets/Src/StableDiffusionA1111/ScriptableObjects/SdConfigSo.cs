using UnityEngine;


namespace StableDiffusion
{
    [CreateAssetMenu(fileName = "Sd Config", menuName = "StableDiffusion/SdConfigSo", order = 1)]
    public class SdConfigSo : ScriptableObject
    {
        public SdConfig config;

        public void Reset()
        {
            // config.Defaults();
        }

    }
}
