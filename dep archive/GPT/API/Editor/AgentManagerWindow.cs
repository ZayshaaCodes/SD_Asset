using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GPT.API.Editor
{
    public class AgentManagerWindow : EditorWindow
    {
        public VisualTreeAsset WindowAsset;
        public List<GptAgent>  agents;
        public GptAgent        selectedAgent;

        private DropdownField _functionDropdownField;
        private VisualElement _functionsHeaderContainer;

        //ctrl alt q
        [MenuItem("Window/GPT/Agent Manager %&q")]
        public static void ShowWindow()
        {
            var window = GetWindow<AgentManagerWindow>();
            window.titleContent = new GUIContent("Agent Manager");
            window.minSize      = new UnityEngine.Vector2(400, 200);
            window.Show();
        }

        public void OnEnable()
        {
            var root = rootVisualElement;
            WindowAsset.CloneTree(root);

            UpdateAgents();
            InitAgentList(root);
            
            //find the "func-header" container
            _functionsHeaderContainer = root.Q<VisualElement>("func-header");
            
            //find the dropdown field
            _functionDropdownField         = root.Q<DropdownField>("add-func-dropdown");
            _functionDropdownField.choices = GptFunctionInfo.GetFunctionNames();
            // when the dropdown field changes, add the function to the list if it's not already there
            _functionDropdownField.RegisterValueChangedCallback(evt =>
            {
                _functionDropdownField.index = 0;

                //find the functionInfo
                if (GptFunctionInfo.functions.TryGetValue(evt.newValue, out var functionInfo))
                {
                    //the first index is a space, so if the index is zero, return
                    if (evt.newValue == " " || evt.newValue == "") return;

                    //check if the function is already in the list
                    var functionList = selectedAgent.functions;
                    if (functionList.Any(func => func.name == functionInfo.name)) return;

                    //add the function to the list
                    functionList.Add(GptFunctionInfo.functions[evt.newValue]);
                    EditorUtility.SetDirty(selectedAgent);
                }
            });
            UpdateFuncHeaderVis();

            //get the "func-listview"
            var funcListView = root.Q<ListView>("func-listview");
            //disable reordering and any animation
            funcListView.selectionType           = SelectionType.Single;
            funcListView.style.borderTopColor = new Color(0f, 0f, 0f, 0.5f);
            funcListView.style.borderTopWidth = 1;

            funcListView.makeItem = () =>
            {
                //
                var container = new VisualElement(); // root container, horizontal
                container.style.flexDirection = FlexDirection.Row;
                container.style.flexGrow      = 1;
                //dark gray bottom and top border
                container.style.borderBottomColor = new Color(0f, 0f, 0f, 0.5f);
                container.style.borderBottomWidth = 1;

                var labels = new VisualElement();
                //padding
                labels.style.paddingLeft   = 3;
                labels.style.paddingTop    = 3;
                labels.style.paddingRight  = 3;
                labels.style.paddingBottom = 3;

                labels.style.flexGrow = 1;
                var nameLabel = new Label() { name = "name-label" };
                var descLabel = new Label() { name = "desc-label" };
                nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                nameLabel.style.flexGrow                = 1;
                descLabel.style.fontSize                = 10;
                descLabel.style.flexGrow                = 1;
                labels.Add(nameLabel);
                labels.Add(descLabel);
                container.Add(labels);

                //remove button, square with an x, aligned right by label being stretched
                var removeButton = new Button(() =>
                {
                    var func = container.userData as GptFunction;
                    selectedAgent.functions.Remove(func);
                    EditorUtility.SetDirty(selectedAgent);
                })
                {
                    text = "X"
                };
                removeButton.style.width                   = 20;
                removeButton.style.height                  = 20;
                removeButton.style.alignSelf               = Align.Center;
                removeButton.style.unityTextAlign          = TextAnchor.MiddleCenter;
                removeButton.style.unityFontStyleAndWeight = FontStyle.Bold;
                container.Add(removeButton);

                return container;
            };
            funcListView.bindItem = (element, i) =>
            {
                element.userData = selectedAgent.functions[i];
                //get the name and description labels
                var container = element;
                var nameLabel = container.Q<Label>("name-label");
                var descLabel = container.Q<Label>("desc-label");

                //use data binding
                var so                  = new SerializedObject(selectedAgent);
                var arrayElementAtIndex = so.FindProperty("functions").GetArrayElementAtIndex(i);
                var prop                = arrayElementAtIndex.FindPropertyRelative("name");
                var descProp            = arrayElementAtIndex.FindPropertyRelative("description");
                nameLabel.BindProperty(prop);
                descLabel.BindProperty(descProp);
            };
        }

        private void UpdateFuncHeaderVis()
        {
            _functionsHeaderContainer.style.display = selectedAgent != null ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void InitAgentList(VisualElement root)
        {
            // this is the display root to bind the selected agent to
            var agentDisplay = root.Q<VisualElement>("agent-display");
            // this is the list of agents
            var agentList = root.Q<ListView>("agent-list");
            agentList.makeItem = () =>
            {
                var label = new Label();
                label.style.paddingBottom = 2;
                label.style.paddingLeft   = 2;
                label.style.paddingRight  = 2;
                label.style.paddingTop    = 2;

                label.AddToClassList("agent-list-item");
                return label;
            };

            agentList.bindItem    = (element, i) => ((Label)element).text = agents[i].agentName;
            agentList.itemsSource = agents;

            //when the agent list selection changes, bind the display-root to the selected agent
            agentList.selectionChanged += (list) => // just log list data for testing
            {
                var arr = list.ToArray();
                if (arr.Length > 0)
                {
                    if (arr[0] is GptAgent agent)
                    {
                        selectedAgent = agent;
                        var agentSo = new SerializedObject(agent);
                        agentDisplay.Bind(agentSo);
                    }
                }
                else
                {
                    selectedAgent = null;
                    agentDisplay.Unbind();
                }

                UpdateFuncHeaderVis();
            };
        }

        private void UpdateAgents()
        {
            agents?.Clear();
            agents ??= new();

            //find all the GptAgents in the project
            var guids = AssetDatabase.FindAssets("t:GptAgent");
            foreach (var guid in guids)
            {
                var path  = AssetDatabase.GUIDToAssetPath(guid);
                var agent = AssetDatabase.LoadAssetAtPath<GptAgent>(path);
                agents.Add(agent);
            }
        }
    }
}