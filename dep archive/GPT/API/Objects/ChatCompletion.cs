using System;
using System.Collections.Generic;

namespace GPT.API.Objects
{
    [Serializable]
    public class ChatCompletion
    {
        public string           id;
        public string           @object;
        public long             created;
        public string           model;
        public List<ChatChoice> choices;
        public ChatUsage        usage;

        //tostring to log the choices
        public override string ToString()
        {
            string result = "";
            foreach (var choice in choices)
            {
                result += $"Choice {choice.index}: {choice.message.content}\n";
                //if there's function calls, log thos also
                if (choice.message.function_call != null)
                {
                    result += $"Function call: {choice.message.function_call.name}({choice.message.function_call.arguments})\n";
                }
            }


            return result;
        }
    }
}