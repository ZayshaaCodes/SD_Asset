using Newtonsoft.Json.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Workflow", menuName = "ComfyClient/Workflow", order = 0)]
public class ComfyApiWorkflow : ScriptableObject
{
    public Object apiJsonFile;
    public JObject data; 

    private void OnEnable() {
        //load from the file
        if (apiJsonFile != null) {
            data = JObject.Parse(apiJsonFile.ToString());
        }
    }
}
