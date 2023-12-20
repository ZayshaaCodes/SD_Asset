using System.Collections.Generic;
using UnityEngine;

namespace GPT.API
{
    [CreateAssetMenu(fileName = "GptAgent", menuName = "GPT/Agent")]
    public class GptAgent : ScriptableObject
    {
        public                   string            agentName;
        [TextArea(2, 20)] public string            systemMessage;
        [NonReorderable]  public List<GptFunction> functions;
    }

}