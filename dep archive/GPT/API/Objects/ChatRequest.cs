using System;
using System.Collections.Generic;

namespace GPT.API.Objects
{
    [Serializable]
    public class ChatRequest
    {
        public string           model;
        public List<Message>    messages;
        public List<GptFunction> functions;
        public bool             stream;
    }
}