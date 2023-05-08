using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectUtils.DialogueSystem
{
    [CreateAssetMenu(fileName = "Conversation", menuName = "DialogSystem/Conversation")]
    public class Conversation : ScriptableObject, IEquatable<Conversation>
    {
        public List<Interaction> interactions = new List<Interaction>();
        public List<ConversationOption> options = new List<ConversationOption>();
        private int _currentInteraction;
        public Actor lastActor { get; private set; }

        public void StartConversation()
        {
            _currentInteraction = -1;
            NextInteractionAsync();
        }

        public async void NextInteractionAsync()
        {
            if (_currentInteraction < 0) lastActor = null;
            else
            {
                lastActor = GetCurrentInteraction().actor;
                ConversationManager.instance.lastActor = lastActor;
            }
            _currentInteraction++;

            if (ConversationManager.instance.optionsTransitionMode ==
                ConversationManager.OptionsTransitionMode.DoNotHideAutomatic && _currentInteraction >= interactions.Count-1 && options.Count > 0)
            {
                if(interactions[_currentInteraction].soundEffect != null && AudioManager.Instance != null) AudioManager.Instance.PlaySound(interactions[_currentInteraction].soundEffect);
                await ConversationManager.instance.WriteTextAsync(GetCurrentInteraction().text);
                ConversationManager.instance.lastActor = GetCurrentInteraction().actor;
                EndConversation();
                return;
            }

            if (_currentInteraction >= interactions.Count)
            {
                EndConversation();
                return;
            }

            if(interactions[_currentInteraction].soundEffect != null && AudioManager.Instance != null) AudioManager.Instance.PlaySound(interactions[_currentInteraction].soundEffect);
            await ConversationManager.instance.WriteTextAsync(GetCurrentInteraction().text);
        }

        private void EndConversation()
        {
            ConversationManager.instance.onConversationFinished?.Invoke(this);
            if(options.Count > 0) ConversationManager.instance.ShowOptions();
            else
            {
                ConversationManager.instance.HideConversationAsync();
            }
        }
        
        public Interaction GetCurrentInteraction()
        {
            return interactions[_currentInteraction];
        }

        public bool Equals(Conversation other)
        {
            return other != null && interactions.SequenceEqual(other.interactions) && options.SequenceEqual(other.options);
        }
    }
}
