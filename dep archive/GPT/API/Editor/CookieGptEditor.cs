// using System;
// using System.Collections.Generic;
// using System.Linq;
// using Newtonsoft.Json.Linq;
// using UnityEditor;
// using UnityEditor.UIElements;
// using UnityEngine;
// using UnityEngine.UIElements;
//
// namespace GPT.Editor
// {
//     //simple unity editor window that provites a UI for the user to interact with the GPT-4 converstation API
//     public class CookieGptEditor : EditorWindow
//     {
//         public VisualTreeAsset visualTree;
//         public VisualTreeAsset messageTemplate;
//         public string          _input;
//         
//         [SerializeField] private GptClient _gptClient;
//         
//         Dictionary<string, IAiCommand> _functions = new();
//
//         // ctrl alt q
//         // ctrl = %, alt = &, shift = #
//         [MenuItem("Window/Stable Diffusion/Cookie GPT Editor %&w")]
//         public static void ShowWindow()
//         {
//             var window = GetWindow<CookieGptEditor>("Cookie GPT");
//             var pos = window.position;
//             pos.x = 100;
//             pos.y = 100;
//             window.position = pos;
//         }
//
//         private void CreateGUI()
//         {
//             var dirSum = AssemblySummarizer.SummarizeAssembly(typeof(GptClient).Assembly, "GPT");
//             Debug.Log(dirSum);
//             
//             //log the message to the console
//             Debug.Log("Creating GUI");
//             
//             //if the client is null, create a new one
//             if (_gptClient == null)
//             {
//                 _gptClient = new("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");
//             }
//             _functions.Add("DebugLog", new DebugLog());
//             _functions.Add("CreateFile", new CreateFile());
//             _functions.Add("ListFiles", new ListFiles());
//             
//             var root = rootVisualElement;
//             visualTree.CloneTree(root);
//
//             var so = new SerializedObject(this);
//             root.Bind(so);
//
//             var sendButton = root.Q<Button>("send-button");
//
//             var messagesListview = root.Q<ListView>("messages-listview");
//             if (messagesListview != null)
//             {
//                 var messages = so.FindProperty("_gptClient").FindPropertyRelative("_messages");
//                 messagesListview.BindProperty(messages);
//                 messagesListview.reorderable = false;
//                 messagesListview.makeItem = () =>
//                 {
//                     var message = messageTemplate.CloneTree();
//                     return message;
//                 };
//                 messagesListview.bindItem = (element, i) =>
//                 {
//                     // var message = messages.GetArrayElementAtIndex(i);
//                     var mobj = _gptClient.GetMessage(i);
//
//                     var role = element.Q<Label>("role-label");
//                     role.text = mobj.role;
//                     
//                     var content = element.Q<Label>("content-label");
//                     //if there is a function call, show the function call name and arguments
//                     if (mobj.function_call != null)
//                     {
//                         content.text = mobj.function_call.name + "( " + mobj.function_call.arguments + " )";
//                     }
//                     else
//                     {
//                         content.text = mobj.content;
//                     }
//                 };
//             }
//             
//             sendButton.clicked += () =>
//             {
//                 var input  = _input;
//                 
//                 var res = _gptClient.DoChatCompletion(input, _functions.Values.ToList());
//                 HandleChatCompletion(res);
//             };
//         }
//
//         private void HandleChatCompletion(ChatCompletion res)
//         {
//             if (res.choices[0].message.function_call != null)
//             {
//                 var    funcName = res.choices[0].message.function_call.name;
//                 string args     = res.choices[0].message.function_call.arguments;
//                 var    argsDict = JObject.Parse(args).ToObject<Dictionary<string, string>>();
//                 switch (_functions[funcName])
//                 {
//                     case IAiAction aiAction:
//                         var ret = aiAction.Invoke(argsDict);
//                         
//                         _gptClient.AppendMessage(new Message()
//                         {
//                             role = "function",
//                             name = "funcName",
//                             content = ret
//                         });
//                         break;
//                     case { } aiCommand:
//                         aiCommand.Invoke(argsDict);
//                         break;
//                     default:
//                         throw new ArgumentOutOfRangeException();
//                 }
//             }
//         }
//     }
// }