using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GPT.API
{
    [Serializable]public class SerializableParamPropsDict : ISerializationCallbackReceiver
    {
        //the decorators for changing default property drawer for list of objects
        
        [SerializeField]private string[] keys;
        [SerializeField ]private ParameterProperties[] values;

        private Dictionary<string, ParameterProperties> dictionary = new Dictionary<string, ParameterProperties>();

        public void OnBeforeSerialize()
        {
            keys   = dictionary.Keys.ToArray();
            values = dictionary.Values.ToArray();
        }

        public void OnAfterDeserialize()
        {
            dictionary = new Dictionary<string, ParameterProperties>();
            for(int i = 0; i < keys.Length; i++)
            {
                dictionary.Add(keys[i], values[i]);
            }
        }

        public void Add(string key, ParameterProperties value)
        {
            dictionary.Add(key, value);
        }

        public bool Remove(string key)
        {
            return dictionary.Remove(key);
        }

        public bool TryGetValue(string key, out ParameterProperties value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        public ParameterProperties this[string key]
        {
            get => dictionary[key];
            set => dictionary[key] = value;
        }
    }

}