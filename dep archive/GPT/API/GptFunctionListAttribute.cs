using System;
using UnityEngine;

namespace GPT.API
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class GptFunctionListAttribute : PropertyAttribute
    {
    }
}