using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectUtils.DialogueSystem
{
    [CreateAssetMenu(fileName = "Actor", menuName = "DialogSystem/Actor")]
    public class Actor : ScriptableObject, IEquatable<Actor>
    {
        public new string name;
        public AudioClip voice;
        [SerializeField] private List<Emotion> emotions;

        [Serializable]
        public struct Emotion
        {
            public string emotion;
            public Sprite sprite;
        }
    
        public Sprite GetEmotionSprite(string emotion)
        {
            foreach (var emo in emotions)
            {
                if (emo.emotion.Equals(emotion)) return emo.sprite;
            }
            return null;
        }

        public List<string> GetEmotionsNames()
        {
            var res = new List<string>();
            foreach (var emotion in emotions)
            {
                res.Add(emotion.emotion);
            }

            return res;
        }

        public bool Equals(Actor other) => other != null && other.voice.Equals(voice) && other.emotions.SequenceEqual(emotions) && other.name.Equals(name);
        
    }
}
