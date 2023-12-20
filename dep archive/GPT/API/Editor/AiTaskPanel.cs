using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GPT.API.Editor
{
    public class AiTaskPanel : EditorWindow
    {
        [SerializeField] private VisualTreeAsset _windowAsset;

        [SerializeField] private List<GptAgent> _agents;
        [SerializeField] private string _agent;

        [SerializeField] private List<GptTuning> _tunings;
        [SerializeField] private string _tuning;

        [SerializeField] private string _goal;
        [SerializeField] private bool _projectTree = true;
        [SerializeField] private bool _hierarchy;

        [MenuItem("Window/GPT/Ai Task Panel %#&q")]
        public static void ShowWindow()
        {
            var window = GetWindow<AiTaskPanel>();
            window.titleContent = new UnityEngine.GUIContent("Ai Task Panel");
            window.minSize = new UnityEngine.Vector2(400, 200);
            window.Show();
        }

        public void OnEnable()
        {
            var root = rootVisualElement;
            _windowAsset.CloneTree(root);
            var so = new SerializedObject(this);
            root.Bind(so);

            var startButton = root.Q<Button>("start-button");
            startButton.clicked += StartButtonClicked;

            _tunings = new List<GptTuning>();
            _tunings.Add(new GptTuning() { name = "test", value = "test" });
            _tunings.Add(new GptTuning() { name = "test2", value = "test2" });

            _agents = new List<GptAgent>();
            //find all agents in project
            var guids = AssetDatabase.FindAssets("t:GptAgent");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var agent = AssetDatabase.LoadAssetAtPath<GptAgent>(path);
                _agents.Add(agent);
            }
        }

        private void StartButtonClicked()
        {
            //just log all the private fields
            Debug.Log($"Agent: { _agent }");
            Debug.Log($"Tuning: { _tuning }");

            Debug.Log($"Goal: {_goal}");
            Debug.Log($"Project Tree: {_projectTree}");
            Debug.Log($"Hierarchy: {_hierarchy}");

        }
    }

    [Serializable]
    public class GptTuning
    {
        public string name;
        public string value;
    }
}