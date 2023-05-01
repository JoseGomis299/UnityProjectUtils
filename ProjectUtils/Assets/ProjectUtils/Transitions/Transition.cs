using System;
using System.Collections.Generic;
using UnityEngine;

namespace Transitions
{
    [Serializable]
    public class Transition 
    {
        protected bool Equals(Transition other)
        {
            return Equals(animationCurve, other.animationCurve) && duration.Equals(other.duration) && playOnStart == other.playOnStart && id == other.id && Equals(keyFrames, other.keyFrames);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Transition)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(animationCurve, duration, playOnStart, id, keyFrames);
        }

        [Header("Transition Parameters")]
        public AnimationCurve animationCurve  = AnimationCurve.Linear(0,0,1,1);
        public float duration = 1;
        public bool playOnStart;

        [HideInInspector] public int id;

        [Header("Transition Key Frames")]
        public List<KeyFrame> keyFrames;

        public static bool operator ==(Transition a, Transition b)
        {
            if (a.keyFrames.Count != b.keyFrames.Count) return false;

            bool listEquals = true;
            for (int i = 0; i < a.keyFrames.Count; i++)
            {
                if (a.keyFrames[i] != b.keyFrames[i]) listEquals = false;
            }

            Debug.Log( a.animationCurve.Equals(b.animationCurve));
            Debug.Log( a.duration == b.duration);
            Debug.Log( a.playOnStart == b.playOnStart);
            Debug.Log(a.id);
            Debug.Log(b.id);
            Debug.Log( listEquals);
            return a.animationCurve.Equals(b.animationCurve) && a.duration == b.duration &&
                   a.playOnStart == b.playOnStart && listEquals && a.id == b.id;

        }

        public static bool operator !=(Transition a, Transition b)
        {
            return !(a == b);
        }
    }
}
