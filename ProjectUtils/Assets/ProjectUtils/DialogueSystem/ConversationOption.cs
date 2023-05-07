using UnityEngine;

namespace ProjectUtils.DialogueSystem
{
    [CreateAssetMenu(fileName = "Conversation Option", menuName = "DialogSystem/Conversation Option")]
    public class ConversationOption : ScriptableObject
    {
        public string text;
        public Conversation nextConversation;

        public bool Equals(ConversationOption other) => other.text.Equals(text);
    }
}
