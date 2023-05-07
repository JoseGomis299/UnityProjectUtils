using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueNode : Node
{
    public string GUID;
    public string DialogueText;
    public Actor Actor;
    public string Emotion;
    public AudioClip SoundEffect;
    public bool EntryPoint;
}

