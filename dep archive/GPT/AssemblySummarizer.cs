using System;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace GPT
{
    public static class AssemblySummarizer
    {
        public static string SummarizeAssembly(Assembly assembly, string rootNamespace = null)
        {
            StringBuilder summary = new StringBuilder();

            // Get all the types in the assembly.
            Type[] types = assembly.GetTypes();
            
            bool doMatchNamespace = !string.IsNullOrEmpty(rootNamespace);
            summary.AppendLine("Types in the assembly: " + assembly.FullName);
            //append info about if it matches the namespace
            summary.AppendLine($"Matching namespace: {doMatchNamespace}");
            foreach (Type type in types)
            {
                if (type.IsClass)
                {
                    if (doMatchNamespace && (type.Namespace == null || !type.Namespace.StartsWith(rootNamespace)))
                    {
                        continue;
                    } 
                    if (type.IsAbstract) continue;
                    if (type.Name.StartsWith("<")) continue; // C# compiler generated
                    if (type.Name.StartsWith("__")) continue; // Unity internal
                    summary.AppendLine($"C: {type.Name} | {type.Namespace}");
                }
            }

            return summary.ToString();
        }
    }
}