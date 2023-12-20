using System;
using System.Collections;
using System.Collections.Generic;
using GPT.API;
using GPT.API.AiFunctions;
using GPT.API.Objects;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GPT.Assistant
{
    [Serializable, CreateAssetMenu(fileName = "GptAssistant", menuName = "GPT/Assistant")]
    public class GptAssistant : ScriptableObject
    { 
        private GptClient        _client;
        public List<Message>    messages;
        private List<GptFunction> functions;
        
        [TextArea(5, 40)] public string currentOutput;
        
        public GptAssistant()
        {
            _client = new ("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");
            
            messages = new ()
            {
                new () { role = RoleEnum.System, content = "you're an AI assistent in the unity editor" },
            };
        }
        
        public IEnumerator DoChatCompletion()
        {
            currentOutput = "";
            yield return _client.DoChatCompletionStreaming(messages, functions, s =>
            {
                if (s != null)
                    currentOutput += s;
                
            });
        }
        
        public void AppendCurrentOutputAsAssistantMessage()
        {
            messages.Add(new Message() { role = RoleEnum.Assistant, content = currentOutput });
            currentOutput = "";
        }
        
        //append as user
        public void AppendCurrentOutputAsUserMessage()
        {
            messages.Add(new Message() { role = RoleEnum.User, content = currentOutput });
            currentOutput = "";
        }
    }

    //custom editor, using ui elements
    [CustomEditor(typeof(GptAssistant))]
    public class GptAssistantEditor : Editor
    {

        private GptAssistant tar;
        
        private void OnEnable()
        {
            tar = (GptAssistant) target;
        }
        
        public override VisualElement CreateInspectorGUI()
        {
            var ve = new VisualElement();
            //append a default inspector
            ve.Add(new IMGUIContainer(base.OnInspectorGUI));
            
            //add a button
            ve.Add(new Button(() =>
            {
                //get the target object
                var assistant = (GptAssistant) target;
                //add a message to the list
                // assistant.messages.Add(new Message() { role = "user", content = "hello" });
                //do a chat completion
                EditorCoroutineUtility.StartCoroutine(assistant.DoChatCompletion(), tar);
            })
            {
                text = "Send Message"
            });
            
            //add a button to append the current output as a message
            ve.Add(new Button(() =>
            {
                tar.AppendCurrentOutputAsAssistantMessage();
            })
            {
                text = "Append Current Output"
            });
            
            //add a button to append the current output as a message
            ve.Add(new Button(() =>
            {
                tar.AppendCurrentOutputAsUserMessage();
                EditorCoroutineUtility.StartCoroutine(tar.DoChatCompletion(), tar);
            })
            {
                text = "Append Current Output as User and Send"
            });
            
            return ve;
        }
    }


}