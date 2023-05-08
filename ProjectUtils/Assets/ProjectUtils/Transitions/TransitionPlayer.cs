using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Transitions
{
    [ExecuteAlways, Icon("Assets/ProjectUtils/Transitions/Editor/Icons/TransitionPlayerIcon.png"), AddComponentMenu("Transitions/Transition Player")]
    public class TransitionPlayer : MonoBehaviour
    {
        private enum TimeScale
        {
            Scaled,
            Unscaled,
            Fixed
        }
        [Header("TransitionPlayer Parameters")]
        [SerializeField] private TimeScale timeScale;
        private SpriteRenderer _spriteRenderer;
        private Image _image;

        [Header("Transition List")]
        [SerializeField] private List<Transition> transitions = new();
    
        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _image = GetComponent<Image>();
            if (_spriteRenderer == null) _spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
            transitions ??= new List<Transition>();
        }

        private void Start()
        {
            if(!Application.IsPlaying(gameObject)) return;

            foreach (var transition in transitions.Where(transition => transition.playOnStart))
            {
                PlayTransition(transition);
            }
        }


#if UNITY_EDITOR
        private void Update()
        {
            if(Application.IsPlaying(gameObject)) return;
            System.Random random = new System.Random();
            for (int i = 1; i < transitions.Count; i++)
            {
                if(transitions[i-1].id == transitions[i].id) {transitions[i].id = random.Next();}
            }
        }

        private float GetTime(TimeScale scale)
        {
            switch (scale)
            {
                case TimeScale.Scaled : return Time.deltaTime;
                case TimeScale.Unscaled : return Time.unscaledDeltaTime;
            }

            return Time.fixedDeltaTime;
        }
        
        private void AddKeyFrame(int index)
        {
            KeyFrame keyFrame = new KeyFrame();
            keyFrame.SetValues(transform, _spriteRenderer, _image);
            transitions[index].keyFrames.Add(keyFrame);
        }
    
        public void AddKeyFrame(Transition transition)
        {
            AddKeyFrame(GetTransitionIndex(transition));
        }
    
        private int GetTransitionIndex(Transition transition)
        {
            for (int i = 0; i < transitions.Count; i++)
            {
                if (transitions[i] == transition) return i;
            }
            return -1;
        }
    
#endif
    
        private void SetTransformToFrame(KeyFrame targetFrame)
        {
            var myTransform = transform;
            myTransform.position = targetFrame.position;
            myTransform.eulerAngles = targetFrame.rotation;
            myTransform.localScale = targetFrame.scale;

            if (_spriteRenderer != null) _spriteRenderer.color = targetFrame.color;
            else if (_image != null) _image.color = targetFrame.color;
        }
        public async Task PlayTransitionAsync(int index)
        { 
            await PlayTransitionAsync(transitions[index]);
        }
    
        public async Task PlayTransitionAsync(Transition transition)
        {
#if UNITY_EDITOR
            if (!Application.IsPlaying(gameObject))
            {
                await PlayTransitionAsync(transition, TimeScale.Fixed);
                return;
            }
#endif
            await PlayTransitionAsync(transition, timeScale);
        }
   
        public void PlayTransition(int index)
        { 
            PlayTransition(transitions[index]);
        }
    
        public void PlayTransition(Transition transition)
        {
            StartCoroutine(PlayTransitionCoroutine(transition));
        }

        private async Task PlayTransitionAsync(Transition transition, TimeScale scale)
        {
            for (int i = 0; i < transition.keyFrames.Count-1; i++)
            {
                await PlayTransitionAsync(transition.keyFrames[i], transition.keyFrames[i+1], transition.animationCurve, transition.duration, scale);
            }
        }
    
        private async Task PlayTransitionAsync(KeyFrame initialFrame, KeyFrame targetFrame, AnimationCurve animationCurve, float duration, TimeScale scale)
        {
            float timer = GetTime(scale);
            KeyFrame scaleDelta = targetFrame - initialFrame;

            while (timer < duration)
            {
                SetTransformToFrame(initialFrame + scaleDelta * (animationCurve.Evaluate(timer/duration)));
                await Task.Yield();
                timer += GetTime(scale);
            }

            SetTransformToFrame(initialFrame + scaleDelta);
        } 
        
        private IEnumerator PlayTransitionCoroutine(Transition transition)
        {
            for (int i = 0; i < transition.keyFrames.Count-1; i++)
            {
                yield return StartCoroutine(PlayTransitionCoroutine(transition.keyFrames[i], transition.keyFrames[i+1], transition.animationCurve, transition.duration));
            }
        }
    
        private IEnumerator PlayTransitionCoroutine(KeyFrame initialFrame, KeyFrame targetFrame, AnimationCurve animationCurve, float duration)
        {
            float timer = GetTime(timeScale);
            KeyFrame scaleDelta = targetFrame - initialFrame;

            while (timer < duration)
            {
                SetTransformToFrame(initialFrame + scaleDelta * (animationCurve.Evaluate(timer/duration)));
                yield return null;
                timer +=  GetTime(timeScale);
            }

            SetTransformToFrame(initialFrame + scaleDelta);
        } 
    
    }
}
