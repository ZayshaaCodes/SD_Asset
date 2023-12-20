using System;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace GPT.API
{
    //this is the class that will use reflection to build a list of available functions
    public static class GptFunctionInfo
    {
        public static Dictionary<string, GptFunction> functions = new Dictionary<string, GptFunction>();

        //static constructor to build the list of functions, needs to run on compile
        [UnityEditor.InitializeOnLoadMethod]
        public static void InitGptFunctionInfo()
        {
            //add all thes functions here, find all types that inherit from GptFunction
            //add them to the dictionary
            var types = typeof(GptFunction).Assembly.GetTypes();

            //add a blank type at the start of the list, this will be the default value for the dropdown
            functions.Add(" ", null);

            foreach (var type in types)
            {
                //functions must be inherited from GptFunction, skip the GptAction type
                if (type.IsSubclassOf(typeof(GptFunction)) && type != typeof(GptAction))
                {
                    var function = Activator.CreateInstance(type) as GptFunction;
                    if (function?.name == "" || function?.name == null) return;
                    functions.Add(function.name, function);
                }
            }
        }


        public static List<string> GetFunctionNames()
        {
            List<string> names = new List<string>();
            foreach (var function in functions)
            {
                names.Add(function.Key);
            }

            return names;
        }
    }

    [Serializable] public class GptFunction
    {
        public string        name;
        public string        description;
        public ParameterInfo parameters;

        public virtual string Invoke(Dictionary<string, string> args)
        {
            return "";
        }
    }
    
    
    [Serializable] public class GptAction : GptFunction // same thing but with no return value, will always return null
    {
    }

    /// <summary>
    /// Used to serialize the parameters of a function
    /// <returns>
    /// type: The type of the parameter
    /// properties: The properties of the parameter 
    /// required: The required properties of the parameter
    [Serializable] public class ParameterInfo
    {
        public string                     type = "object";
        public SerializableParamPropsDict properties;
        public string[]                   required;
    }

    /// <summary>
    /// Used to serialize the properties of a parameter
    /// <returns>
    /// type: The type of the parameter
    /// description: The description of the parameter
    /// enum: The possible values of the parameter
    /// </returns>
    /// </summary>
    [Serializable] public class ParameterProperties
    {
        //serialize to lowercase
        [JsonConverter(typeof(TypeToLowercaseStringConverter))]
        public string type;

        public string   description;
        public string[] @enum;
    }

    //used because the openai api expects the type to be lowercase
    public class TypeToLowercaseStringConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString().ToLower());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value.ToString().ToLower();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Type);
        }
    }
}