using System.Collections.Generic;
using UnityEngine;

namespace GPT.API.AiFunctions
{
    [AutoLink]
    public class DebugLog : GptAction
    {
        public DebugLog()
        {
            name        = GetType().Name;
            description = "Logs a message to the Unity console";
            var paramProps = new SerializableParamPropsDict();
            paramProps.Add("message", new ParameterProperties()
            {
                type        = "string",
                description = "The message to log, can use rich text tags like colors"
            });
            parameters = new()
            {
                type = "object",
                properties = paramProps,
                required = new[] { "message" }
            };
        }

        public override string Invoke(Dictionary<string, string> args)
        {
            if (args.TryGetValue("message", out string message))
                Debug.Log($"<color=yellow>{message}</color>");

            return "";
        }
    }
    
    [AutoLink] // this one will just list files, path is relative to the project folder
    public class ListFiles : GptFunction
    {
        public ListFiles()
        {
            name        = GetType().Name;
            description = "Lists all files in a directory";
            var paramProps = new SerializableParamPropsDict();
            paramProps.Add("path", new ParameterProperties()
            {
                type        = "string",
                description = "The path to the directory to list files from"
            });
            parameters = new()
            {
                type = "object",
                properties = paramProps,
                required = new[] { "path" }
            };
        }

        public override string Invoke(Dictionary<string, string> args)
        {
            if (args.TryGetValue("path", out string path))
            {
                var files = System.IO.Directory.GetFiles(path);
                foreach (var file in files)
                {
                    Debug.Log(file);
                }
            }

            return "";
        }
    }
    
    [AutoLink] // this one will create a file, path is relative to the project folder, content is the content of the file
    public class CreateFile : GptFunction
    {
        public CreateFile()
        {
            name        = GetType().Name;
            description = "Creates a file, if the file already exists it will be overwritten";
            var paramProps = new SerializableParamPropsDict();
            paramProps.Add("path", new()
            {
                type        = "string",
                description = "The path to the file to create"
            });
            paramProps.Add("content", new()
            {
                type        = "string",
                description = "The content of the file to create"
            });
            
            parameters = new ParameterInfo()
            {
                type = "object",
                properties = paramProps, 
                required = new[] { "path", "content" }
            };
        }

        public override string Invoke(Dictionary<string, string> args)
        {
            if (args.TryGetValue("path", out string path) && args.TryGetValue("content", out string content))
            {
                System.IO.File.WriteAllText(path, content);
            }

            return "";
        }
    }
    
}