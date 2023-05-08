using System;
using UnityEngine;

namespace ProjectUtils.DialogueSystem
{
    [CreateAssetMenu(fileName = "Conversation Option", menuName = "DialogSystem/Conversation Option")]
    public class ConversationOption : ScriptableObject, IEquatable<ConversationOption>
    {
        public string text;
        public Conversation nextConversation;

        public bool Equals(ConversationOption other) => other != null && other.nextConversation.Equals(nextConversation) && other.text.Equals(text);
    }
}
