using System;

namespace GPT.API.Objects
{
    [Serializable]
    public class ChatChoice
    {
        public int     index;
        public Delta   delta;
        public Message message;
        public string  finish_reason;
    }
}