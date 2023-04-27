using System;
using System.Collections.Generic;
using UnityEngine;

namespace Transitions
{
    [Serializable]
    public class Transition 
    {
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
