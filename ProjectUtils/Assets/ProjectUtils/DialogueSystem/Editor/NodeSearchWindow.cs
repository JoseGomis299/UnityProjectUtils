using System.Collections.Generic;
using ProjectUtils.DialogueSystem.RunTime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectUtils.DialogueSystem.Editor
{
    public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private DialogueGraphView _graphView;
        private EditorWindow _window;

        public void Init(EditorWindow window, DialogueGraphView graphView)
        {
            _window = window;
            _graphView = graphView;
        }
    
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Elements"), 0),
                new SearchTreeEntry(new GUIContent("Dialogue Node"))
                {
                    userData = new DialogueNode(), level = 1
                }
            };
            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            var worldMousePosition = _window.rootVisualElement.ChangeCoordinatesTo(_window.rootVisualElement.parent,
                context.screenMousePosition - _window.position.position);
            var localMousePosition = _graphView.contentViewContainer.WorldToLocal(worldMousePosition);
            switch (SearchTreeEntry.userData)
            {
                case DialogueNode:
                    _graphView.CreateNode(new DialogueNodeData(), localMousePosition);
                    return true;
                default: return false;
            }
        }
    }
}
