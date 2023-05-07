using System;
using UnityEngine;

namespace ProjectUtils.DialogueSystem
{
    [Serializable]
    public struct Interaction
    {
        [Header("Parameters")]
        public Actor actor;
        [Multiline] public string text;
        public Sprite actorSprite;
        public AudioClip soundEffect;
    }
}
