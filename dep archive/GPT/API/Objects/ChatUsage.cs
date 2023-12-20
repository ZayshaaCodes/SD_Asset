using System;

namespace GPT.API.Objects
{
    [Serializable]
    public class ChatUsage
    {
        public int prompt_tokens;
        public int completion_tokens;
        public int total_tokens;
    }
}