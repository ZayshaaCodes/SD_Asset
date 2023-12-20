using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace StableDiffusion.Editor.MasterControl
{
    public class SdMasterControlWindow : EditorWindow
    {
        ApiRequestHandler _apiRequestHandler = new("http://localhost:7860/");
        SdApi             _sdApi;

        [SerializeField] private SdConfig config;

        [SerializeField] private List<ControlNetConfig> _controlNetConfigs = new();

        private VisualElement _leftElement;
        private VisualElement _rightElement;
        private VisualElement _splitter;

        private Dictionary<ToolbarToggle, VisualElement> tabs;

        //addon data
        private                  List<string> _aspectRatioList = new() { "Any", "16:9", "16:10", "4:3", "5:4", "1:1" };
        [SerializeField] private bool         flipAspect       = false;
        [SerializeField] private string       selectedAspect   = "Any";
        [SerializeField] private float        aspectRatio      = 1;

        //model data
        private                  List<string> _modelList    = new();
        [SerializeField] private string       selectedModel = "?";

        //sampler data
        private List<string> _samplerList = new();

        [SerializeField] public string button_text = "Generate";


        [FormerlySerializedAs("UxmlWindowTemplate")]
        [SerializeField]
        private VisualTreeAsset uxmlWindowTemplate;

        private string _scriptPath;

        // ctrl = %, alt = &, shift = #
        [MenuItem("Window/Stable Diffusion/Master Control Window %#q")]
        public static void ShowWindow()
        {
            var window = GetWindow<SdMasterControlWindow>("Sd Master Control");

            //set the postion of the window
            var pos = window.position;
            pos.x           = 100;
            pos.y           = 100;
            window.position = pos;
        }

        public void OnEnable()
        {
            _sdApi = new SdApi(_apiRequestHandler);

            //get the root path of this script with reflection
            _scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
            if (uxmlWindowTemplate == null)
            {
                uxmlWindowTemplate = Resources.Load<VisualTreeAsset>(nameof(SdMasterControlWindow));
                if (uxmlWindowTemplate == null)
                {
                    Debug.LogError("Could not find a UXML file with the same name as the class, assign it manually in the inspector");
                    return;
                }
            }

            var root = rootVisualElement;
            uxmlWindowTemplate.CloneTree(root);

            InitTabs();

            _modelList   = GetDataToList<SdApi.SdModelInfo>("models.json", _sdApi.GetModels, model => model.title);
            _samplerList = GetDataToList<SdApi.SdSamplerInfo>("samplers.json", _sdApi.GetSamplers, sampler => sampler.name);


            var windowSo = new SerializedObject(this);
            root.Bind(windowSo);

            var configControlTemplate = root.Q<TemplateContainer>("ConfigControl");
            var configProp            = windowSo.FindProperty("config");
            if (configProp != null)
                configControlTemplate.BindProperty(configProp);
            var aspectDropdown                                 = configControlTemplate.Q<DropdownField>("aspect_dropdown");
            if (aspectDropdown != null) aspectDropdown.choices = _aspectRatioList;
            var samplerDropdown                                = configControlTemplate.Q<DropdownField>("sampler_dropdown");

            if (samplerDropdown != null) samplerDropdown.choices = _samplerList;
            var modelDropdown                                    = root.Q<DropdownField>("model_dropdown");
            if (modelDropdown != null)
            {
                modelDropdown.choices = _modelList;
                // modelDropdown.RegisterValueChangedCallback(evt =>
                // {
                //     if (evt.newValue != "?")
                //     {
                //         selectedModel = evt.newValue.ToString();
                //         EditorCoroutineUtility.StartCoroutine(_sdApi.SetModel(selectedModel), this);
                //     }
                // });
            }

            
            EditorCoroutineUtility.StartCoroutine(_sdApi.GetModel(s =>
            {
                // Debug.Log($"Model: {s}");
                selectedModel = s;
            }), this);


            _splitter = root.Q<VisualElement>("splitter");
            _splitter.AddManipulator(new DraggableSeperator(true));
            
            var genButton = root.Q<Button>("gen_button");
            if (genButton != null)
            {
                genButton.clickable.clicked += () =>
                {
                    var configJson = config.ToJObject();
                    if (_controlNetConfigs != null)
                    {
                        var argsArr  = new JArray();
                        
                        foreach (var netConfig in _controlNetConfigs)
                        {
                            var netConfigJson = netConfig.ToJobject();
                            argsArr.Add(netConfigJson);
                            
                        }

                        var aos            = new JObject();
                        var controlNetsObj = new JObject();
                        controlNetsObj.Add("args", argsArr);
                        aos.Add("controlnet", controlNetsObj);

                        configJson.Add("alwayson_script", aos);
                    }

                    Debug.Log(configJson.ToString());
                    // EditorCoroutineUtility.StartCoroutine(_sdApi.Generate(configJson, textures =>
                    // {
                    //     Debug.Log("Generated, count:" + textures.Count);
                    // }, null), this);
                };
            }
            
        }

        private void InitTabs()
        {
            //build tabs
            tabs = new Dictionary<ToolbarToggle, VisualElement>();
            var tabBar = rootVisualElement.Q<Toolbar>("tab_bar");
            if (tabBar == null)
            {
                Debug.Log("tabBar is null");
                return;
            }

            var generalTab        = tabBar.Q<ToolbarToggle>("general_tab");
            var controlNetTab     = tabBar.Q<ToolbarToggle>("controlnet_tab");
            var generalTabView    = tabBar.parent.Q<VisualElement>("general_tab_view");
            var controlNetTabView = tabBar.parent.Q<VisualElement>("controlnet_tab_view");

            tabs.Add(generalTab,    generalTabView);
            tabs.Add(controlNetTab, controlNetTabView);

            //register tab callbacks
            foreach (var tab in tabs)
            {
                tab.Key.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                    {
                        foreach (var tab2 in tabs)
                        {
                            if (tab2.Key != tab.Key)
                            {
                                tab2.Key.SetValueWithoutNotify(false);
                                tab2.Value.style.display = DisplayStyle.None;
                            }
                        }

                        tab.Value.style.display = DisplayStyle.Flex;
                    }
                });
            }
        }

        private List<string> GetDataToList<T>(string samplersJson, System.Func<Action<List<T>>, IEnumerator> getSamplers, System.Func<T, string> func) where T : class
        {
            //first try to load model json from disk
            //if that fails, try to load from api
            var jsonText = Resources.Load<TextAsset>(samplersJson);

            List<string> list = new();

            if (jsonText == null)
            {
                EditorCoroutineUtility.StartCoroutine(getSamplers(samplers =>
                {
                    for (int i = 0; i < samplers.Count; i++)
                    {
                        list.Add(func(samplers[i]));
                    }

                    var scriptDirectory = System.IO.Path.GetDirectoryName(_scriptPath);

                    //save json to disk
                    System.IO.File.WriteAllText(scriptDirectory + $"/Resources/{samplersJson}", Newtonsoft.Json.JsonConvert.SerializeObject(samplers));
                }), this);
            }
            else
            {
                var samplers = Newtonsoft.Json.JsonConvert.DeserializeObject<List<T>>(jsonText.text);

                for (int i = 0; i < samplers.Count; i++)
                {
                    list.Add(func(samplers[i]));
                }
            }

            return list;
        }
    }
}