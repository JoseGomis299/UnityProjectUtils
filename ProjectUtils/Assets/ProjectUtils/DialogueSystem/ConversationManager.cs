using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectUtils.Helpers;
using TMPro;
using Transitions;
using UnityEngine;
using UnityEngine.UI;

public class ConversationManager : MonoBehaviour
{
    [Header("Bindings")]
    [SerializeField] private GameObject conversationLayout;
    [SerializeField] private GameObject optionPrefab;
    [SerializeField] private Image[] actorsImages;
    [SerializeField] private GameObject optionSelector;
    [SerializeField] private Transform optionSelectorVerticalLayout;
    [SerializeField] private TMP_Text conversationText;
    [SerializeField] private TMP_Text actorNameText;

    [Header("Transitions")]
    [SerializeField] private TransitionPlayer boxTransitions;
    [SerializeField] private TransitionPlayer[] imageTransitionPlayers;

    [Header("Conversation Parameters")]
    [SerializeField] private int writingSpeed;
    [SerializeField] private SkippingType skippingType;

    [field: SerializeReference]
    public OptionsTransitionMode optionsTransitionMode { get; private set; } = OptionsTransitionMode.DoNotHide;
    [HideInInspector] public Actor lastActor;
    
    //Events
    public Action<Conversation> onConversationFinished; 
    public event Action<ConversationOption> onOptionSelected;
    public event Action onEndMainConversation;
    private enum SkippingType
    {
        IncreaseWritingSpeed, SkipText
    }
    public enum OptionsTransitionMode
    {
        DoNotHideAutomatic, DoNotHide, Hide
    }

    private Task _writeText;
    private bool _skipWriting;
    private Dictionary<Actor, Image> _actorImages;
    private Dictionary<Actor, TransitionPlayer> _imagesTransitions;
    private List<Actor> _actors;
    private Conversation _currentConversation;
    private int _writingSpeed;
    private bool _fromOptions;
    
    public static ConversationManager instance;

    private void Awake()
    {
        if (instance != null) Destroy(gameObject);
        else instance = this;
        
        _writingSpeed = writingSpeed;
        
        optionSelector.SetActive(false);
        conversationLayout.SetActive(false);
    }
    
    public async void StartConversation(Conversation conversation)
    {
        foreach (var image in actorsImages)
        {
            image.gameObject.SetActive(false);
        }
        conversationText.text = "";
        actorNameText.text = "";
        
        _currentConversation = conversation;
        _currentConversation.InitializeValues();
        conversationLayout.SetActive(true);
        actorsImages[0].sprite = _currentConversation.GetCurrentInteraction().actorSprite;
        await boxTransitions.PlayTransitionAsync(0);
        InitializeImages();

        _actors = new List<Actor>();
        _actorImages = new Dictionary<Actor, Image>();
        _imagesTransitions = new Dictionary<Actor, TransitionPlayer>();
        lastActor = null;
        
        _currentConversation.StartConversation();
    }

    private void InitializeImages()
    {
        for (var i = 1; i < actorsImages.Length; i++)
        {
            var image = actorsImages[i];
            image.gameObject.SetActive(false);
        }
    }
    
    private void Update()
    {
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
        if (Input.GetKeyDown(KeyCode.Space))
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
        if (Input.GetKeyDown(KeyCode.Space))
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
            
            stringBuilder.Append(character);
            conversationText.text = stringBuilder.ToString();
            if(AudioManager.Instance != null && _currentConversation.GetCurrentInteraction().actor.voice!= null) AudioManager.Instance.PlaySound(_currentConversation.GetCurrentInteraction().actor.voice);
            await Task.Delay(1000/_writingSpeed);
        }
        conversationText.text = text;
    }

    public async void HideConversationAsync()
    {
        _writeText = null;

        await GetActorsImageTransition(lastActor).PlayTransitionAsync(2);
        Task[] tasks = new Task[imageTransitionPlayers.Length];
        for (int i = 0; i < imageTransitionPlayers.Length; i++)
        {
            tasks[i] = imageTransitionPlayers[i].PlayTransitionAsync(3);
        }
        await Task.WhenAll(tasks);
        await boxTransitions.PlayTransitionAsync(1);
        conversationLayout.SetActive(false);
        onEndMainConversation?.Invoke();
    }

    public async Task NextSpriteAsync()
    {
        Interaction currentInteraction = _currentConversation.GetCurrentInteraction();
        bool isNew = AddActorToCurrentConversation(currentInteraction.actor) || !IsOnStage(currentInteraction.actor);

        if (lastActor != null && !currentInteraction.actor.Equals(lastActor))
        {
            if (!IsOnStage(currentInteraction.actor) && actorsImages.All(x => x.gameObject.activeInHierarchy))
            {
                if(_imagesTransitions[lastActor].Equals(_imagesTransitions[currentInteraction.actor]))
                    await GetActorsImageTransition(lastActor).PlayTransitionAsync(2);
                else GetActorsImageTransition(lastActor).PlayTransitionAsync(2);
                await GetActorsImageTransition(currentInteraction.actor).PlayTransitionAsync(3);
            }
            else if(!_fromOptions) GetActorsImageTransition(lastActor).PlayTransitionAsync(2);
        }
        SetActorsImageSprite(currentInteraction.actor, currentInteraction.actorSprite);

        if(isNew || !_actorImages[currentInteraction.actor].gameObject.activeInHierarchy) 
        {
            _actorImages[currentInteraction.actor].gameObject.SetActive(true);
            await GetActorsImageTransition(currentInteraction.actor).PlayTransitionAsync(0);
        }

        if ((lastActor != null && !currentInteraction.actor.Equals(lastActor)) || isNew)
        {
            await GetActorsImageTransition(currentInteraction.actor).PlayTransitionAsync(1);
        }
        _fromOptions = false;
    }

    public void ShowOptions()
    {
        _writeText = null;
        optionSelector.SetActive(true);
        if (optionsTransitionMode == OptionsTransitionMode.Hide)
        {
            GetActorsImageTransition(lastActor).PlayTransitionAsync(2);
            _fromOptions = true;
        }

        foreach (var option in _currentConversation.options)
        {
           GameObject optionButton = Instantiate(optionPrefab, optionSelectorVerticalLayout);
           optionButton.GetComponentInChildren<TMP_Text>().text = option.text;
           optionButton.GetComponent<Button>().onClick.AddListener(() =>
           {
               _currentConversation = option.nextConversation;
               if (optionsTransitionMode == OptionsTransitionMode.Hide)
               {
                   _currentConversation.InitializeValues();
                   GetActorsImageTransition(_currentConversation.GetCurrentInteraction().actor).PlayTransitionAsync(1);
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

    public void SetActorsImageSprite(Actor actor, Sprite sprite)
    {
        _actorImages[actor].sprite = sprite;
    }

    private bool HasActed(Actor value)
    {
        foreach (var actor in _actors)
        {
            if (actor.Equals(value)) return true;
        }

        return false;
    }

    public bool AddActorToCurrentConversation(Actor actor)
    {
        if(HasActed(actor)) return false;

        int index = _actors.Count % actorsImages.Length;

        _actors.Add(actor);
        _actorImages.Add(actor, actorsImages[index]);
        _imagesTransitions.Add(actor, imageTransitionPlayers[index]);
        return true;
    }

    private TransitionPlayer GetActorsImageTransition(Actor actor)
    {
        return _imagesTransitions[actor];
    }

    private bool IsOnStage(Actor actor)
    {
        foreach (var actorsImage in actorsImages)
        {
            foreach (var emotion in actor.GetEmotionsNames())
            {
                if (actorsImage.sprite.Equals(actor.GetEmotionSprite(emotion))) return true;
            }
        }

        return false;
    }
}
