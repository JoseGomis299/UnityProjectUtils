using System;
using System.Collections.Generic;
using System.Linq;
using ProjectUtils.DialogueSystem.RunTime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = System.Object;

namespace ProjectUtils.DialogueSystem.Editor
{
    public class DialogueGraphView : GraphView
    {
        public readonly Vector2 defaultNodeSize = new Vector2(100, 150);
        public List<ExposedProperty> exposedProperties = new List<ExposedProperty>();
        public Blackboard blackboard;

        private NodeSearchWindow _nodeSearchWindow;
        private Vector2 mousePosition;
    
        public DialogueGraphView(EditorWindow editorWindow)
        {
            styleSheets.Add(Resources.Load<StyleSheet>("DialogueGraph"));
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();
        
            AddElement(GenerateEntryPointNode());
            AddSearchWindow(editorWindow);
        
            serializeGraphElements += CutCopyOperation;
            unserializeAndPaste += PasteOperation;
            RegisterCallback<DragUpdatedEvent>(OnDragUpdatedEvent);
            RegisterCallback<DragPerformEvent>(OnDragPerformEvent);
        }

        private void PasteOperation(string operationname, string data)
        {
            // Deserialize the JSON data from the clipboard
            var copyData = JsonUtility.FromJson<CopyPasteData>(data);

            // Create new nodes based on the serialized data
            List<DialogueNode> pastedNodes = new List<DialogueNode>();
            Vector2 positionSum = Vector2.zero;
            foreach (var nodeData in copyData.nodes)
            {
                var pastedNode = CreateDialogueNode(new DialogueNodeData()
                {
                    Actor = nodeData.Actor,
                    DialogueText = nodeData.DialogueText,
                    Emotion = nodeData.Emotion,
                    Guid = GUID.Generate().ToString(),
                    Position = nodeData.Position,
                    SoundEffect = nodeData.SoundEffect
                }, nodeData.Position);
            
                positionSum += nodeData.Position;
                pastedNodes.Add(pastedNode);
                AddElement(pastedNode);
            }
        
            positionSum /= copyData.nodes.Count;

            // Select the pasted nodes
            ClearSelection();
            for (var i = 0; i < pastedNodes.Count; i++)
            {
                var pastedNode = pastedNodes[i];
                pastedNode.SetPosition(new Rect(mousePosition+copyData.nodes[i].Position-positionSum, defaultNodeSize));
                AddToSelection(pastedNode);
            }
        }

        [Serializable]
        public class CopyPasteData
        {
            public List<DialogueNodeData> nodes = new List<DialogueNodeData>();
        }
    
        private string CutCopyOperation(IEnumerable<GraphElement> elements)
        {
            var data = new CopyPasteData();
        
            // Serialize the selected nodes
            foreach (var element in elements)
            {
                if (element is DialogueNode node)
                {
                    var nodeData = new DialogueNodeData()
                    {
                        Actor = node.Actor,
                        Emotion = node.Emotion,
                        SoundEffect = node.SoundEffect,
                        DialogueText = node.DialogueText,
                        Position = node.GetPosition().position,
                    };
                    data.nodes.Add(nodeData);
                }
            }

            // Copy the serialized data to the clipboard
            var jsonData = JsonUtility.ToJson(data);
            EditorGUIUtility.systemCopyBuffer = jsonData;

            return jsonData;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            VisualElement contentViewContainer = ElementAt(1);
            Vector3 screenMousePosition = evt.localMousePosition;
            Vector2 worldMousePosition = screenMousePosition - contentViewContainer.transform.position;
            worldMousePosition *= 1 / contentViewContainer.transform.scale.x;
            mousePosition = worldMousePosition;
        }

    
    
        private void AddSearchWindow(EditorWindow editorWindow)
        {
            _nodeSearchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
            nodeCreationRequest += context =>
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _nodeSearchWindow);
            _nodeSearchWindow.Init(editorWindow,this);
        }

        private Port GeneratePort(Node node, Direction potDirection, Port.Capacity capacity = Port.Capacity.Single)
        {
            return node.InstantiatePort(Orientation.Horizontal, potDirection, capacity, typeof(string));
        }

        private Node GenerateEntryPointNode()
        {
            var node = new DialogueNode
            {
                title = "START",
                GUID = Guid.NewGuid().ToString(),
                EntryPoint = true
            };

            var generatePort = GeneratePort(node, Direction.Output);
            generatePort.portName = "Next";
            node.outputContainer.Add(generatePort);
        
            node.capabilities &= ~Capabilities.Renamable;
        
            node.RefreshExpandedState();
            node.RefreshPorts();
        
            node.SetPosition(new Rect(100, 200, 100, 150));
            return node;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            ports.ForEach(port =>
            {
                if (startPort != port && startPort.node != port.node)
                {
                    compatiblePorts.Add(port);
                }
            });
            return compatiblePorts;
        }

        public DialogueNode CreateDialogueNode(DialogueNodeData nodeData, Vector2 position)
        {
            var title = "Dialogue Node";
            if (nodeData.DialogueText != null)
                title = nodeData.Actor ? $"{nodeData.Actor.name}\n{nodeData.DialogueText}" : $"{nodeData.DialogueText}";

            var dialogueNode = new DialogueNode
            {
                title = title,
                Actor = nodeData.Actor,
                DialogueText = nodeData.DialogueText,
                Emotion = nodeData.Emotion,
                SoundEffect = nodeData.SoundEffect,
                GUID = Guid.NewGuid().ToString()
            };
        
            var inputPort = GeneratePort(dialogueNode, Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "Input";
            dialogueNode.inputContainer.Add(inputPort);

            var button = new Button(() => AddChoicePort(dialogueNode));
            button.text = "New Choice";
            dialogueNode.titleContainer.Add(button);

            styleSheets.Add(Resources.Load<StyleSheet>("Node"));
            AddEditableFields(dialogueNode);

            dialogueNode.RefreshExpandedState();
            dialogueNode.RefreshPorts();

            dialogueNode.SetPosition(new Rect(position, defaultNodeSize));
            return dialogueNode;
        }

        private void AddEditableFields(DialogueNode dialogueNode)
        {
            var textField = new TextField(String.Empty);
            textField.RegisterValueChangedCallback(evt =>
            {
                dialogueNode.DialogueText = evt.newValue.Trim();
                dialogueNode.title = dialogueNode.Actor ? $"{dialogueNode.Actor.name}\n{evt.newValue}" : evt.newValue;
            });
            textField.SetValueWithoutNotify(dialogueNode.DialogueText);
            dialogueNode.mainContainer.Add(new Label("Dialogue Text"));
            dialogueNode.mainContainer.Add(textField);
        
            var actorField = new ObjectField
            {
                objectType = typeof(Actor),
                allowSceneObjects = false
            };
 
            var emotionField = new DropdownField(string.Empty);
            actorField.RegisterValueChangedCallback(v =>
            {
                dialogueNode.Actor = v.newValue as Actor;
                dialogueNode.title = $"{dialogueNode.Actor.name}\n{dialogueNode.DialogueText}";
                emotionField.choices = dialogueNode.Actor.GetEmotionsNames();
            });
            actorField.SetValueWithoutNotify(dialogueNode.Actor);
            dialogueNode.mainContainer.Add(new Label("Actor"));
            dialogueNode.mainContainer.Add(actorField);
        
            var image = new Image();
            image.style.maxHeight = new StyleLength(120);
            image.style.maxWidth = new StyleLength(120);
            image.style.alignSelf = Align.Center;
            image.style.justifyContent = Justify.Center; 
        
            if(dialogueNode.Actor != null) 
            {
                image.sprite = dialogueNode.Actor.GetEmotionSprite(dialogueNode.Emotion);
                emotionField.choices = dialogueNode.Actor.GetEmotionsNames();
            }
            emotionField.RegisterValueChangedCallback(evt =>
            {
                if(dialogueNode.Actor == null) return;
                dialogueNode.Emotion = evt.newValue;
                image.sprite = dialogueNode.Actor.GetEmotionSprite(evt.newValue);
            });
            emotionField.SetValueWithoutNotify(dialogueNode.Emotion);
            dialogueNode.mainContainer.Add(new Label("Emotion"));
            dialogueNode.mainContainer.Add(emotionField);
            dialogueNode.mainContainer.Add(image);
        
            var soundField = new ObjectField
            {
                objectType = typeof(AudioClip),
                allowSceneObjects = false
            };
 
            soundField.RegisterValueChangedCallback(v =>
            {
                dialogueNode.SoundEffect = v.newValue as AudioClip;
            });
            soundField.SetValueWithoutNotify(dialogueNode.SoundEffect);
            dialogueNode.mainContainer.Add(new Label("On play Sound effect"));
            dialogueNode.mainContainer.Add(soundField);
        }

        public void AddChoicePort(DialogueNode dialogueNode, string overriddenPortName = "")
        {
            var generatedPort = GeneratePort(dialogueNode, Direction.Output);

            // var oldLabel = generatedPort.contentContainer.Q<Label>("type");
            // generatedPort.contentContainer.Remove(oldLabel);

            var outputPortCount = dialogueNode.outputContainer.Query("connector").ToList().Count;
            var choicePortName = string.IsNullOrEmpty(overriddenPortName) ? $"Choice {outputPortCount+1}" : overriddenPortName;

            var textField = new TextField()
            {
                name = string.Empty,
                value = choicePortName
            };
            textField.RegisterValueChangedCallback(evt => generatedPort.portName = evt.newValue);
            generatedPort.contentContainer.Add(new Label("  "));
            generatedPort.contentContainer.Add(textField);
            var deleteButton = new Button(() => RemovePort(dialogueNode, generatedPort))
            {
                text = "X"
            };
            generatedPort.contentContainer.Add(deleteButton);
        
            generatedPort.portName = choicePortName;
            dialogueNode.outputContainer.Add(generatedPort);
            dialogueNode.RefreshExpandedState();
            dialogueNode.RefreshPorts();
        }

        private void RemovePort(DialogueNode dialogueNode, Port generatedPort)
        {
            var targetEdge = edges.ToList().Where(x =>
                x.output.portName == generatedPort.portName && x.output.node == generatedPort.node);

            if (targetEdge.Any())
            {
                var edge = targetEdge.First();
                edge.input.Disconnect(edge);
                RemoveElement( targetEdge.First());
                return;
            }

            dialogueNode.outputContainer.Remove(generatedPort);
            dialogueNode.RefreshPorts();
            dialogueNode.RefreshExpandedState();
        }

        public void CreateNode(DialogueNodeData dialogueNodeData, Vector2 position)
        {
            AddElement(CreateDialogueNode(dialogueNodeData, position));
        }

        public void AddPropertyToBlackBoard(ExposedProperty exposedProperty)
        {
            var localPropertyName = exposedProperty.propertyName;
            var localPropertyValue = exposedProperty.propertyValue;
            while (exposedProperties.Any(x=>x.propertyName==localPropertyName))
            {
                localPropertyName = $"{localPropertyName}(1)";
            }
        
            var property = new ExposedProperty();
            property.propertyName = localPropertyName;
            property.propertyValue = localPropertyValue;
            exposedProperties.Add(property);
            

            var container = new VisualElement();
            var blackboardField = new BlackboardField {name = localPropertyName, text = property.propertyName, typeText = "string" };
            container.Add(blackboardField);

            var dragger = new BlackboardFieldDragger(this);
            blackboardField.AddManipulator(dragger);
            

            var propertyValueTextField = new TextField("Value: ")
            {
                value = localPropertyValue
            };
            propertyValueTextField.RegisterValueChangedCallback(evt =>
            {
                var changingPropertyIndex = exposedProperties.FindIndex(x => x.propertyName == property.propertyName);
                exposedProperties[changingPropertyIndex].propertyValue = evt.newValue;
            });
            var blackBoardValueRow = new BlackboardRow(blackboardField, propertyValueTextField);
            blackboard.Add(blackBoardValueRow);

            blackboard.Add(container);
        }
        
        private void OnDragUpdatedEvent(DragUpdatedEvent evt)
        {
            if (DragAndDrop.objectReferences.Length > 0)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                evt.StopPropagation();
            }
        }

        private void OnDragPerformEvent(DragPerformEvent evt)
        {
            if (DragAndDrop.objectReferences.Length > 0)
            {
                // Handle the dropped object(s)
                foreach (Object obj in DragAndDrop.objectReferences)
                {
                    // Check if the dragged object is a BlackboardField
                    BlackboardField blackboardField = obj as BlackboardField;
                    if (blackboardField != null)
                    {
                        // Create a new node in the graph based on the blackboardField
                        CreateNodeInGraph(blackboardField);
                    }
                }

                DragAndDrop.AcceptDrag();
                evt.StopPropagation();
            }
        }
        
        private void CreateNodeInGraph(BlackboardField blackboardField)
        {
            // Create a new node in the graph based on the blackboardField
            // Perform any necessary logic to extract data from the blackboardField and create the corresponding node
            // ...
        }

        public void ClearBlackBoardAndExposedProperties()
        {
            exposedProperties.Clear();
            blackboard.Clear();
        }

    }
}
