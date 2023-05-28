using System.Collections.Generic;
using System.Linq;
using ProjectUtils.DialogueSystem.RunTime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectUtils.DialogueSystem.Editor
{
    public class GraphSaveUtility
    {
        private DialogueGraphView _targetGraphView;
        private DialogueContainer _containerCache;
    
        private List<Edge> Edges => _targetGraphView.edges.ToList();
        private List<DialogueNode> Nodes => _targetGraphView.nodes.ToList().Cast<DialogueNode>().ToList();

        private DialogueContainer _dialogueContainer;
    
        public static GraphSaveUtility GetInstance(DialogueGraphView targetDialogueGraphView)
        {
            return new GraphSaveUtility()
            {
                _targetGraphView = targetDialogueGraphView
            };
        }

        private int num;
        public void SaveGraph(string fileName)
        {
            var dialogueContainer = ScriptableObject.CreateInstance<DialogueContainer>();
            if(!SaveNodes(dialogueContainer)) return;
            SaveExposedProperties(dialogueContainer);
            _dialogueContainer = dialogueContainer;

            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");

            num = 0;
            var targetNode = Edges.First(x=> ((DialogueNode)x.output.node).EntryPoint).input.node as DialogueNode;
            FromDialogueContainerToConversation(GetConnectedNodes(targetNode.GUID,ScriptableObject.CreateInstance<DialogueContainer>()), fileName);

            AssetDatabase.CreateAsset(dialogueContainer, $"Assets/Resources/{fileName}.asset");
            AssetDatabase.SaveAssets();
        }

        public Conversation FromDialogueContainerToConversation(DialogueContainer dialogueContainer, string fileName)
        {
            Conversation baseConversation = ScriptableObject.CreateInstance<Conversation>();
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Conversations"))
                AssetDatabase.CreateFolder("Assets/Resources", "Conversations");
            if (!AssetDatabase.IsValidFolder($"Assets/Resources/Conversations/{fileName}"))
                AssetDatabase.CreateFolder($"Assets/Resources/Conversations", $"{fileName}");


            for (var i = 0; i < dialogueContainer.DialogueNodeData.Count; i++)
            {
                var nodeData = dialogueContainer.DialogueNodeData[i];
                var nodePorts = dialogueContainer.NodeLinks.Where(x => x.BaseNodeGuid == nodeData.Guid).ToList();
            
                var interaction = new Interaction()
                {
                    actor = nodeData.Actor != null ? nodeData.Actor : null,
                    actorSprite =  nodeData.Actor != null ? nodeData.Actor.GetEmotionSprite(nodeData.Emotion) : null,
                    text = nodeData.DialogueText ?? "",
                    soundEffect = nodeData.SoundEffect != null? nodeData.SoundEffect : null
                };
                baseConversation.interactions.Add(interaction);

                switch (nodePorts.Count)
                {
                    case 0:
                        AssetDatabase.CreateAsset(baseConversation, $"Assets/Resources/Conversations/{fileName}/Conversation{num}.asset");
                        AssetDatabase.SaveAssets();
                        return baseConversation;
                    case 1:
                        break;
                    default:
                        int opNum = 0;
                        int _num = num;
                        foreach (var port in nodePorts)
                        {
                            var option = ScriptableObject.CreateInstance<ConversationOption>();
                            option.text = port.PortName;
                            num++;
                            option.nextConversation = FromDialogueContainerToConversation(GetConnectedNodes(port.TargetNodeGuid, ScriptableObject.CreateInstance<DialogueContainer>()), fileName);
                            baseConversation.options.Add(option);
                            opNum++;
                            AssetDatabase.CreateAsset(option, $"Assets/Resources/Conversations/{fileName}/Option_{_num}_{opNum}.asset");
                            AssetDatabase.SaveAssets();
                        }

                        AssetDatabase.CreateAsset(baseConversation, $"Assets/Resources/Conversations/{fileName}/Conversation{_num}.asset");
                        AssetDatabase.SaveAssets();
                        return baseConversation;
                }
            }

            AssetDatabase.CreateAsset(baseConversation, $"Assets/Resources/Conversations/{fileName}/Conversation{num}.asset");
            AssetDatabase.SaveAssets();
            return baseConversation;
        }
    
        private DialogueContainer GetConnectedNodes(string targetPort, DialogueContainer container)
        {
            var links = _dialogueContainer.NodeLinks.Where(x => x.BaseNodeGuid == targetPort).ToList();
            var nodes = _dialogueContainer.DialogueNodeData.Where(x => x.Guid == targetPort).ToList();
            container.DialogueNodeData.AddRange(nodes);
            container.NodeLinks.AddRange(links);
            foreach (var link in links)
            { 
                if(links.Count == 1)  GetConnectedNodes(link.TargetNodeGuid, container);
            }
            return container;
        }

        private void SaveExposedProperties(DialogueContainer dialogueContainer)
        {
            dialogueContainer.ExposedProperties.AddRange(_targetGraphView.exposedProperties);
        }

        private bool SaveNodes(DialogueContainer dialogueContainer)
        {
            if(!Edges.Any()) return false;
        
            Edge[] connectedPorts = Edges.Where(edge => edge.input.node != null).OrderByDescending(edge => ((DialogueNode)edge.output.node).EntryPoint).ToArray();     
            for (int i = 0; i < connectedPorts.Length; i++)
            {
                var outputNode = connectedPorts[i].output.node as DialogueNode;
                var inputNode = connectedPorts[i].input.node as DialogueNode;
            
                dialogueContainer.NodeLinks.Add(new NodeLinkData()
                {
                    BaseNodeGuid = outputNode.GUID,
                    PortName = connectedPorts[i].output.portName,
                    TargetNodeGuid = inputNode.GUID
                });
            }
        
            foreach (var dialogueNode in Nodes.Where(node=>!node.EntryPoint))
            {
                dialogueContainer.DialogueNodeData.Add(new DialogueNodeData()
                {
                    Guid = dialogueNode.GUID,
                    DialogueText = dialogueNode.DialogueText,
                    Actor = dialogueNode.Actor,
                    Emotion = dialogueNode.Emotion,
                    SoundEffect = dialogueNode.SoundEffect,
                    Position = dialogueNode.GetPosition().position
                });
            }
            return true;
        }
        
        
    
        public void LoadGraph(string fileName)
        {
            _containerCache = Resources.Load<DialogueContainer>(fileName);
            if (_containerCache == null)
            {
                EditorUtility.DisplayDialog("File Not Found", "Target dialogue graph file does not exists!", "OK");
                return;
            }

            ClearGraph();
            CreateNodes();
            ConnectNodes();
            CreateExposedProperties();
        }

        private void CreateExposedProperties()
        {
            _targetGraphView.ClearBlackBoardAndExposedProperties();
            foreach (var property in _containerCache.ExposedProperties)
            {
                _targetGraphView.AddPropertyToBlackBoard(property);
            }
        }

        private void ConnectNodes()
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                var connections = _containerCache.NodeLinks.Where(x => x.BaseNodeGuid == Nodes[i].GUID).ToList();
                for (int j = 0; j < connections.Count; j++)
                {
                    var targetNodeGuid = connections[j].TargetNodeGuid;
                    var targetNode = Nodes.First(x => x.GUID == targetNodeGuid);
                    LinkNodes(Nodes[i].outputContainer[j].Q<Port>(), (Port)targetNode.inputContainer[0]);
                
                    targetNode.SetPosition(new Rect(_containerCache.DialogueNodeData.First(x=>x.Guid==targetNodeGuid).Position, 
                        _targetGraphView.defaultNodeSize));
                }
            }
        }

        private void LinkNodes(Port output, Port input)
        {
            var tempEdge = new Edge
            {
                output = output,
                input = input
            };
        
            tempEdge?.input.Connect(tempEdge);
            tempEdge?.output.Connect(tempEdge);
            _targetGraphView.Add(tempEdge);
        }

        private void CreateNodes()
        {
            foreach (var nodeData in _containerCache.DialogueNodeData)
            {
                var tempNode = _targetGraphView.CreateDialogueNode(nodeData, Vector2.zero);
                tempNode.GUID = nodeData.Guid;
                _targetGraphView.AddElement(tempNode);

                var nodePorts = _containerCache.NodeLinks.Where(x => x.BaseNodeGuid == nodeData.Guid).ToList();
                nodePorts.ForEach(x=>_targetGraphView.AddChoicePort(tempNode, x.PortName));
            }
        }

        private void ClearGraph()
        {
            if(_containerCache.NodeLinks.Count == 0) return;
            Nodes.Find(x => x.EntryPoint).GUID = _containerCache.NodeLinks[0].BaseNodeGuid;

            foreach (var node in Nodes)
            {
                if(node.EntryPoint) continue;
                Edges.Where(x=>x.input.node==node).ToList().ForEach(edge => _targetGraphView.RemoveElement(edge));
            
                _targetGraphView.RemoveElement(node);
            }
        }
    }
}
