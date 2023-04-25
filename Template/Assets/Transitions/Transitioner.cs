using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class Transitioner : MonoBehaviour
{
    private enum TimeScale
    {
        Scaled,
        Unscaled
    }
    [Header("Transitioner Parameters")]
    [SerializeField] private TimeScale timeScale;
    private SpriteRenderer _spriteRenderer;
    private Image _image;

    [Serializable]
    public struct KeyFrame
    { 
        [Header("Key Frame Parameters")]
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public Color color;

        public void SetValues(Transform transform, SpriteRenderer spriteRenderer, Image image)
        {
            position = transform.position;
            rotation = transform.eulerAngles;
            scale = transform.localScale;

            if (spriteRenderer != null) color = spriteRenderer.color;
            else if (image != null) color = image.color;
        }
        

        public static KeyFrame operator -(KeyFrame a)
        {
            return new KeyFrame
                { position = -a.position, rotation = -a.rotation, scale = -a.scale, color =  -1*a.color };
        }

        public static KeyFrame operator +(KeyFrame a, KeyFrame b)
        {
            return new KeyFrame
                { position = a.position+b.position, rotation = a.rotation+b.rotation, scale = a.scale+b.scale, color = a.color + b.color };  
        }
        
        public static KeyFrame operator -(KeyFrame a, KeyFrame b)
        {
            return a + (-b);
        }
        
        public static KeyFrame operator *(KeyFrame a, float b)
        { 
            return new KeyFrame
                { position = a.position*b, rotation = a.rotation*b, scale = a.scale*b, color = a.color*b};
        }
        
        public static bool operator ==(KeyFrame a, KeyFrame b)
        { 
            return a.position == b.position && a.rotation == b.rotation && a.scale == b.scale && a.color == b.color;
        }

        public static bool operator !=(KeyFrame a, KeyFrame b)
        {
            return !(a == b);
        }
    }
    
    [Serializable]
    public struct Transition
    {
        [Header("Transition Parameters")]
        public AnimationCurve animationCurve;
        public float duration;
        public bool playOnStart;
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
            Debug.Log( listEquals);
            return a.animationCurve.Equals(b.animationCurve) && a.duration == b.duration &&
                   a.playOnStart == b.playOnStart && listEquals;

        }

        public static bool operator !=(Transition a, Transition b)
        {
            return !(a == b);
        }
    }
    
    [Header("Transition List")]
    [SerializeField] private List<Transition> transitions;
    
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _image = GetComponent<Image>();
        if (_spriteRenderer == null) _spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
        transitions ??= new List<Transition>();
    }
    
    private async void Start()
    {
        if(!Application.IsPlaying(gameObject)) return;
        
        for (int i = 0; i < 10; i++)
        {
            await Task.Yield();
        }
        for (int i = 0; i < transitions.Count; i++)
        {
            if (transitions[i].playOnStart)
            {
                PlayTransition(transitions[i]);
            }
        }
    }
    
    public void AddKeyFrame(int index)
    {
        Debug.Log(index);
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

    private void SetTransformToFrame(KeyFrame targetFrame)
    {
        var myTransform = transform;
        myTransform.position = targetFrame.position;
        myTransform.eulerAngles = targetFrame.rotation;
        myTransform.localScale = targetFrame.scale;

        if (_spriteRenderer != null) _spriteRenderer.color = targetFrame.color;
        else if (_image != null) _image.color = targetFrame.color;
    }

    public async void PlayTransition(int index)
    {
        if (!Application.IsPlaying(gameObject))
        {
            await PlayTransitionFixedAsync(transitions[index]);
            return;
        }
        
        switch (timeScale)
        {
            case TimeScale.Scaled:
                await PlayTransitionScaledAsync(transitions[index]);
                break;
            case TimeScale.Unscaled:
                await PlayTransitionUnscaledAsync(transitions[index]);
                break;
        }
    }
    
    public async void PlayTransition(Transition transition)
    {
        if (!Application.IsPlaying(gameObject))
        {
            await PlayTransitionFixedAsync(transition);
            return;
        }
        
        switch (timeScale)
        {
            case TimeScale.Scaled:
               await PlayTransitionScaledAsync(transition);
                break;
            case TimeScale.Unscaled:
               await PlayTransitionUnscaledAsync(transition);
                break;
        }
    }
    
    private async Task PlayTransitionUnscaledAsync(Transition transition)
    {
        for (int i = 0; i < transition.keyFrames.Count-1; i++)
        {
            await PlayTransitionUnscaledAsync(transition.keyFrames[i], transition.keyFrames[i+1], transition.animationCurve, transition.duration);
        }
    }
    
    private async Task PlayTransitionUnscaledAsync(KeyFrame initialFrame, KeyFrame targetFrame, AnimationCurve animationCurve, float duration)
    {
        float timer = Time.unscaledDeltaTime;
        KeyFrame scaleDelta = targetFrame - initialFrame;

        while (timer < duration)
        {
            Debug.Log(timer/duration);
            SetTransformToFrame(initialFrame + scaleDelta * (animationCurve.Evaluate(timer/duration)));
            await Task.Yield();
            timer += Time.unscaledDeltaTime;
        }

        SetTransformToFrame(initialFrame + scaleDelta);
    } 
    
    private async Task PlayTransitionScaledAsync(Transition transition)
    {
        for (int i = 0; i < transition.keyFrames.Count-1; i++)
        {
            await PlayTransitionScaledAsync(transition.keyFrames[i], transition.keyFrames[i+1], transition.animationCurve, transition.duration/(transition.keyFrames.Count-1));
        }
    }
    
    private async Task PlayTransitionScaledAsync(KeyFrame initialFrame, KeyFrame targetFrame, AnimationCurve animationCurve, float duration)
    {
        float timer = Time.deltaTime;
        KeyFrame scaleDelta = targetFrame - initialFrame;

        while (timer < duration)
        {
            SetTransformToFrame(initialFrame + scaleDelta * (animationCurve.Evaluate(timer/duration)));
            await Task.Yield();
            timer += Time.deltaTime;
        }

        SetTransformToFrame(initialFrame + scaleDelta);
    } 
    
    private async Task PlayTransitionFixedAsync(Transition transition)
    {
        for (int i = 0; i < transition.keyFrames.Count-1; i++)
        {
            await PlayTransitionFixedAsync(transition.keyFrames[i], transition.keyFrames[i+1], transition.animationCurve, transition.duration);
        }
    }
    
    private async Task PlayTransitionFixedAsync(KeyFrame initialFrame, KeyFrame targetFrame, AnimationCurve animationCurve, float duration)
    {
        float timer = Time.fixedDeltaTime;
        KeyFrame scaleDelta = targetFrame - initialFrame;
        

        while (timer < duration)
        {
            SetTransformToFrame(initialFrame + scaleDelta * (animationCurve.Evaluate(timer/duration)));
            await Task.Yield();
            timer += Time.fixedDeltaTime;
        }

        SetTransformToFrame(initialFrame + scaleDelta);
    } 
}
