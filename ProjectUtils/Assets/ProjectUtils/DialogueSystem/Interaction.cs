using System;
using UnityEngine;

namespace ProjectUtils.DialogueSystem
{
    [Serializable]
    public struct Interaction : IEquatable<Interaction>
    {
        [Header("Parameters")]
        public Actor actor;
        [Multiline] public string text;
        public Sprite actorSprite;
        public AudioClip soundEffect;

        public bool Equals(Interaction other)
        {
            return Equals(actor, other.actor) && text == other.text && Equals(actorSprite, other.actorSprite) && Equals(soundEffect, other.soundEffect);
        }

        public override bool Equals(object obj)
        {
            return obj is Interaction other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(actor, text, actorSprite, soundEffect);
        }
    }
}
