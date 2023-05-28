using System.Linq;
using ProjectUtils.DialogueSystem.RunTime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using MiniMap = UnityEditor.Experimental.GraphView.MiniMap;

namespace ProjectUtils.DialogueSystem.Editor
{
    public class DialogueGraph : EditorWindow
    {
        private DialogueGraphView _graphView;
        private string _fileName = "New Narrative";
    
        [MenuItem("Graph/Dialogue Graph")]
        public static void OpenDialogueGraphView()
        {
            var window = GetWindow<DialogueGraph>();
            window.titleContent = new GUIContent("Dialogue Graph");
        }

        private void OnEnable()
        {
            ConstructGraphView();
            GenerateToolBar();
            GenerateMiniMap();
            GenerateBlackBoard();
        }

        private void GenerateBlackBoard()
        {
            var blackBoard = new Blackboard(_graphView);
            blackBoard.Add(new BlackboardSection{title = "Exposed Properties"});
            blackBoard.addItemRequested += _blackboard => _graphView.AddPropertyToBlackBoard(new ExposedProperty());
            blackBoard.editTextRequested += (blackboard1, element, newValue) =>
            {
                var oldPropertyName = ((BlackboardField)element).text;
                if (_graphView.exposedProperties.Any(x => x.propertyName == newValue))
                {
                    EditorUtility.DisplayDialog("Error", "Property name already exists!", "OK");
                    return;
                }

                var propertyIndex = _graphView.exposedProperties.FindIndex(x => x.propertyName == oldPropertyName);
                _graphView.exposedProperties[propertyIndex].propertyName = newValue;
                ((BlackboardField)element).text = newValue;
            };
            blackBoard.SetPosition(new Rect(10,180,200,300));
            _graphView.blackboard = blackBoard;
            _graphView.Add(blackBoard);
        }

        private void GenerateMiniMap()
        {
            var miniMap = new MiniMap{anchored = true};
            var cords = _graphView.contentViewContainer.WorldToLocal(new Vector2(10, 30));
            miniMap.SetPosition(new Rect(cords.x, cords.y, 200, 140));
            _graphView.Add(miniMap);
        }

        private void GenerateToolBar()
        {
            var toolbar = new Toolbar();

            var fileNameTextField = new TextField("File Name: ");
            fileNameTextField.SetValueWithoutNotify("New Narrative");
            fileNameTextField.MarkDirtyRepaint();
            fileNameTextField.RegisterValueChangedCallback(evt => _fileName = evt.newValue);
            toolbar.Add(fileNameTextField);
        
            toolbar.Add(new Button(()=> RequestDataOperation(true)){text = "Save Data"});
            toolbar.Add(new Button(()=> RequestDataOperation(false)){text = "Load Data"});
            rootVisualElement.Add(toolbar);
        }

        private void RequestDataOperation(bool save)
        {
            if (string.IsNullOrEmpty(_fileName))
            {
                EditorUtility.DisplayDialog("Invalid file name!", "Please enter a valid file name.", "OK");
                return;
            }

            var saveUtility = GraphSaveUtility.GetInstance(_graphView);
            if(save)
                saveUtility.SaveGraph(_fileName);
            else
                saveUtility.LoadGraph(_fileName);
        }

        private void OnDisable()
        {
            rootVisualElement.Remove(_graphView);
        }

        private void ConstructGraphView()
        {
            _graphView = new DialogueGraphView(this)
            {
                name = "Dialogue Graph"
            };
        
            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);
        }
    }
}
