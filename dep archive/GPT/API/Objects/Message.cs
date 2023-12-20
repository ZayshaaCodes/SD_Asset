using System;
using Newtonsoft.Json;
using UnityEngine;

namespace GPT.API.Objects
{
    [Serializable]
    public class Message
    {
        public                RoleEnum     role;
        public                string       name;
        [TextArea(2,20)] public string       content;
        public                FunctionCall function_call;
    }

    //how could i lay this out the editor window for working with this? i'd love for it to be all-in-1! perhaps give a basic layout with ascii.
    //lets remove directives and include a system message and an addon field for data that can be set for extra context as part of the agent details.
    //the function list would also be a part of the agent details.
    //
    // agent, holds a system message, and some extra context info to input project data from the unity project, it should also hold a list of available functions for it to call to do a task. 
    // the manager window will allow you to create new tasks and assign an agent to it. it's then given a task context that tracks the state of the task, and chat conversation. in the form of a list of messages.
    // the manager window will also allow you to create new agents and enable/disable functions for them.
    // functions will be a list that includes some basic info about what the function is.
    
    // this is needed because the API cant accept empty strings in the function call, or the name. content is fine.
    public class MessageConverter : JsonConverter<Message>
    {
        public override void WriteJson(JsonWriter writer, Message value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            if (value.name != null && value.name != "")
            {
                writer.WritePropertyName("name");
                writer.WriteValue(value.name);
            }

            writer.WritePropertyName("role");
            writer.WriteValue(value.role.ToString().ToLower());
            writer.WritePropertyName("content");
            writer.WriteValue(value.content);
            if (value.function_call != null && value.function_call.name != "")
            {
                writer.WritePropertyName("function_call");
                writer.WriteStartObject();
                writer.WritePropertyName("name");
                writer.WriteValue(value.function_call.name);
                writer.WritePropertyName("arguments");
                writer.WriteValue(value.function_call.arguments);
                writer.WriteEndObject();
            }

            writer.WriteEndObject();
        }

        public override Message ReadJson(JsonReader reader, Type objectType, Message existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            //map all the values
            var message = new Message();
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                {
                    break;
                }

                if (reader.TokenType == JsonToken.PropertyName)
                {
                    var propertyName = reader.Value?.ToString();
                    reader.Read();
                    switch (propertyName)
                    {
                        case "name":
                            message.name = reader.Value.ToString();
                            break;
                        case "role": // this is a string in the API, but an enum in the object
                            message.role = (RoleEnum)Enum.Parse(typeof(RoleEnum), reader.Value.ToString());
                            break;
                        case "content":
                            message.content = reader.Value.ToString();
                            break;
                        case "function_call":
                            message.function_call = serializer.Deserialize<FunctionCall>(reader);
                            break;
                    }
                }
            }

            return message;
        }
    }
}