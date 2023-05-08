using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectUtils.Helpers;
using TMPro;
using Transitions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ProjectUtils.DialogueSystem
{
    public class ConversationManager : MonoBehaviour
    {   
        [Header("Bindings")]
        [SerializeField] private GameObject conversationLayout;
        [SerializeField] private Image[] actorsImages;
        [SerializeField] private GameObject optionSelector;
        [SerializeField] private GameObject optionPrefab;
        private Transform optionSelectorVerticalLayout;
        [SerializeField] private TMP_Text actorNameText;
        [SerializeField] private TMP_Text conversationText;

        [Header("Conversation Parameters")]
        [SerializeField] private int writingSpeed;
        [SerializeField] private SkippingType skippingType;
        [SerializeField] private KeyCode[] skipButtons;

        [field: SerializeReference]
        public OptionsTransitionMode optionsTransitionMode { get; private set; } = OptionsTransitionMode.DoNotHide;
        [HideInInspector] public Actor lastActor;

        private enum SkippingType
        {
            IncreaseWritingSpeed, SkipText
        }
        public enum OptionsTransitionMode
        {
            DoNotHideAutomatic, DoNotHide, Hide
        }
        
        //Events
        public Action<Conversation> onConversationFinished; 
        public event Action<ConversationOption> onOptionSelected;
        public event Action onEndMainConversation;
        
        //TransitionPlayers
        private TransitionPlayer _boxTransitionPlayer;
        private TransitionPlayer[] _imageTransitionPlayers;

        //Private fields
        private Task _writeText;
        private bool _skipWriting;
        private Dictionary<Actor, Image> _actorImages;
        private Dictionary<Actor, TransitionPlayer> _imagesTransitions;
        private List<Actor> _actors;
        private Conversation _currentConversation;
        private int _writingSpeed;
        private bool _fromOptions;
    
        //Singleton
        public static ConversationManager instance;

        private void Awake()
        {
            if (instance != null) Destroy(gameObject);
            else instance = this;
        
            _writingSpeed = writingSpeed;
        
            SetBindings();
            
            optionSelector.SetActive(false);
            conversationLayout.SetActive(false);
        }

        private void SetBindings()
        {
            optionSelectorVerticalLayout = optionSelector.GetComponentInChildren<VerticalLayoutGroup>(true).transform;
            _boxTransitionPlayer = actorNameText.gameObject.GetComponentInParent<TransitionPlayer>(true);
            _imageTransitionPlayers = new[]
                { actorsImages[0].GetComponent<TransitionPlayer>(), actorsImages[1].GetComponent<TransitionPlayer>() };
        }
    
        public async void StartConversation(Conversation conversation)
        {
            //Initialize UI
            foreach (var image in actorsImages)
            {
                image.gameObject.SetActive(false);
            }
            conversationText.text = "";
            actorNameText.text = "";
            conversationLayout.SetActive(true);
            await _boxTransitionPlayer.PlayTransitionAsync(0);

            //Initialize variables
            _actors = new List<Actor>();
            _actorImages = new Dictionary<Actor, Image>();
            _imagesTransitions = new Dictionary<Actor, TransitionPlayer>();
            lastActor = null;
        
            //Start conversation
            _currentConversation = conversation;
            _currentConversation.StartConversation();
        }

        private void Update()
        {
            //Handle Input
            switch (skippingType)
            {
                case SkippingType.IncreaseWritingSpeed:
                    HandleWritingSpeed();
                    break;
                case SkippingType.SkipText:
                    HandelSkipText();
                    break;
            }
        }
    
        private void HandleWritingSpeed()
        {
            if (skipButtons.Any(Input.GetKeyDown) || Input.GetMouseButtonDown(0))
            {
                if (_writeText == null) return;
                if (_writeText.IsCompleted)
                {
                    _writeText = null;
                    _currentConversation.NextInteractionAsync();
                    return;
                }
            
                _writingSpeed = writingSpeed * 10;
            }
            else if (Input.GetKeyUp(KeyCode.Space))
            {
                _writingSpeed = writingSpeed;
            }
        }
    
        private void HandelSkipText()
        {
            if (skipButtons.Any(Input.GetKeyDown) || Input.GetMouseButtonDown(0))
            {
                if (_writeText == null) return;
                if (_writeText.IsCompleted)
                {
                    _writeText = null;
                    _currentConversation.NextInteractionAsync();
                    return;
                }
            
                _skipWriting = true;
            }
        }


        public async Task WriteTextAsync(string text)
        {
            await NextSpriteAsync();
            _writeText = _WriteTextAsync(text);
            await Task.WhenAll(_writeText);
        }

        private async Task _WriteTextAsync(string text)
        {
            //Reset text
            actorNameText.text = _currentConversation.GetCurrentInteraction().actor.name;
            conversationText.text = "";
        
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var character in text)
            {
                if (_skipWriting)
                {
                    _skipWriting = false;
                    break;
                }
                
                //Build the string and play voice sound
                stringBuilder.Append(character);
                conversationText.text = stringBuilder.ToString();
                if(AudioManager.Instance != null && _currentConversation.GetCurrentInteraction().actor.voice!= null) AudioManager.Instance.PlaySound(_currentConversation.GetCurrentInteraction().actor.voice);
                await UnityDelay.Delay(1000/_writingSpeed);
            }
            conversationText.text = text;
        }

        public async void HideConversationAsync()
        {
            //Reset _writeText
            _writeText = null;

            //Play animations and invoke onEndMainConversation
            await _imagesTransitions[lastActor].PlayTransitionAsync(2);
            Task[] tasks = new Task[_imageTransitionPlayers.Length];
            for (int i = 0; i < _imageTransitionPlayers.Length; i++)
            {
                tasks[i] = _imageTransitionPlayers[i].PlayTransitionAsync(3);
            }
            await Task.WhenAll(tasks);
            await _boxTransitionPlayer.PlayTransitionAsync(1);
            conversationLayout.SetActive(false);
            onEndMainConversation?.Invoke();
        }

        public async Task NextSpriteAsync()
        {
            Interaction currentInteraction = _currentConversation.GetCurrentInteraction();
            bool isNew = AddActorToCurrentConversation(currentInteraction.actor) || !IsOnStage(currentInteraction.actor);

            //If current actor is different to the last actor make last actor hide 
            if (lastActor != null && !currentInteraction.actor.Equals(lastActor))
            {
                //Check if current actor wasn't on the stage
                if (!IsOnStage(currentInteraction.actor) && actorsImages.All(x => x.gameObject.activeInHierarchy))
                {
                    //Check if it shares image with last actor
                    if (_imagesTransitions[lastActor].Equals(_imagesTransitions[currentInteraction.actor]))
                    {
                        //Since it is already behind, we don't have to do it again
                        if (optionsTransitionMode != OptionsTransitionMode.Hide)
                            await _imagesTransitions[lastActor].PlayTransitionAsync(2);
                    }
                    else _imagesTransitions[lastActor].PlayTransitionAsync(2);

                    await _imagesTransitions[currentInteraction.actor].PlayTransitionAsync(3);
                }
                else if(!_fromOptions) await _imagesTransitions[(lastActor)].PlayTransitionAsync(2);
            }
            
            //Set new sprite to the current actor
            _actorImages[currentInteraction.actor].sprite = currentInteraction.actorSprite;

            //If current actor is new, make it appear
            if(isNew || !_actorImages[currentInteraction.actor].gameObject.activeInHierarchy) 
            {
                _actorImages[currentInteraction.actor].gameObject.SetActive(true);
                await _imagesTransitions[currentInteraction.actor].PlayTransitionAsync(0);
            }
            
            //Make current actor go to the front
            if ((lastActor != null && !currentInteraction.actor.Equals(lastActor)) || isNew)
            {
                await _imagesTransitions[currentInteraction.actor].PlayTransitionAsync(1);
            }
            
            //Reset _fromOptions
            _fromOptions = false;
        }

        public void ShowOptions()
        {
            //Reset _writeText to block input
            _writeText = null;
            
            //Initialize options menu
            optionSelector.SetActive(true);
            if (optionsTransitionMode == OptionsTransitionMode.Hide)
            {
                _imagesTransitions[lastActor].PlayTransitionAsync(2);
                _fromOptions = true;
            }

            //Create option buttons
            foreach (var option in _currentConversation.options)
            {
                GameObject optionButton = Instantiate(optionPrefab, optionSelectorVerticalLayout);
                optionButton.GetComponentInChildren<TMP_Text>().text = option.text;
                optionButton.GetComponent<Button>().onClick.AddListener(() =>
                {
                    _currentConversation = option.nextConversation;
                    if (optionsTransitionMode == OptionsTransitionMode.Hide && IsOnStage(_currentConversation.interactions[0].actor))
                    {
                        _imagesTransitions[_currentConversation.interactions[0].actor].PlayTransitionAsync(1);
                    }

                    _currentConversation.StartConversation();
                    onOptionSelected?.Invoke(option);
                    HideOptions();
                });
            }
        }

        private void HideOptions()
        {
            optionSelectorVerticalLayout.DeleteChildren();
            optionSelector.SetActive(false);
        }

        private bool AddActorToCurrentConversation(Actor actor)
        {
            if(HasActed(actor)) return false;

            int index = _actors.Count % actorsImages.Length;

            _actors.Add(actor);
            _actorImages.Add(actor, actorsImages[index]);
            _imagesTransitions.Add(actor, _imageTransitionPlayers[index]);
            return true;
        }

        private bool IsOnStage(Actor actor) => actor.GetEmotionsNames().Any(emotion => _actorImages[actor].sprite.Equals(actor.GetEmotionSprite(emotion)));
        private bool HasActed(Actor value) => _actors.Any(actor => actor.Equals(value));

    }
}
