using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public struct Interaction
{
    [Header("Parameters")]
    public Actor actor;
    [Multiline] public string text;
    public Sprite actorSprite;
    public AudioClip soundEffect;
}
