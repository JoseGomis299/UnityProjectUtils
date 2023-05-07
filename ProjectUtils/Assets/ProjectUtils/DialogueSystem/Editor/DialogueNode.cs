using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ProjectUtils.DialogueSystem.Editor
{
    public class DialogueNode : Node
    {
        public string GUID;
        public string DialogueText;
        public Actor Actor;
        public string Emotion;
        public AudioClip SoundEffect;
        public bool EntryPoint;
    }
}

