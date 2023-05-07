using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Task = System.Threading.Tasks.Task;

[CreateAssetMenu(fileName = "Conversation", menuName = "DialogSystem/Conversation")]
public class Conversation : ScriptableObject
{
    public List<Interaction> interactions = new List<Interaction>();
    public List<ConversationOption> options = new List<ConversationOption>();
    private int _currentInteraction;
    public Actor lastActor { get; private set; }

    public void InitializeValues()
    {
        _currentInteraction = 0;
        interactions ??= new List<Interaction>();
    }

    public void StartConversation()
    {
        interactions ??= new List<Interaction>();
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
            ConversationManager.OptionsTransitionMode.DoNotHideAutomatic && options.Count > 0)
        {
            if (_currentInteraction >= interactions.Count-1 )
            {
                if(interactions[_currentInteraction].soundEffect != null && AudioManager.Instance != null) AudioManager.Instance.PlaySound(interactions[_currentInteraction].soundEffect);
                await ConversationManager.instance.WriteTextAsync(GetCurrentInteraction().text);
                ConversationManager.instance.lastActor = GetCurrentInteraction().actor;
                EndConversation();
                return;
            }
        } 
        else if (_currentInteraction >= interactions.Count)
        {
            EndConversation();
            return;
        }

        if(interactions[_currentInteraction].soundEffect != null && AudioManager.Instance != null) AudioManager.Instance.PlaySound(interactions[_currentInteraction].soundEffect);
        await ConversationManager.instance.WriteTextAsync(GetCurrentInteraction().text);
    }

    public void EndConversation()
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
}
