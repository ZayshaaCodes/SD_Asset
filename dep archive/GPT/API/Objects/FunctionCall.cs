using System;

namespace GPT.API.Objects
{
    [Serializable]
    public class FunctionCall
    {
        public string name;
        public string arguments;
    }
}