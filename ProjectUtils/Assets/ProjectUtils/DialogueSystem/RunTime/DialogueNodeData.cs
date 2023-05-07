using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueNodeData 
{
    public string Guid;
    public string DialogueText;
    public Actor Actor;
    public string Emotion;
    public AudioClip SoundEffect;
    public Vector2 Position;
}
