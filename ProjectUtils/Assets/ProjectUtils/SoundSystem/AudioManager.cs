using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource effectSource;
    
    void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void PlaySound(AudioClip clip)
    { 
        effectSource.PlayOneShot(clip);
    }

    public void ChangeMusic(AudioClip clip)
    {
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void ChangeMasterVolume(float value)
    {
        AudioListener.volume = value;
    }
    
    public float GetMasterVolume()
    {
        return AudioListener.volume;
    }

    public void ChangeMusicVolume(float value)
    {
        musicSource.volume = value;
    }

    public void ChangeEffectsVolume(float value)
    {
        effectSource.volume = value;
    }

    public void StopMusic()
    {
        musicSource.Pause();
    }

    public void StopEffect()
    {
        effectSource.Stop();
    }

    public void ResumeMusic()
    {
        musicSource.Play();
    }

    public bool EffectsIsPlaying()
    {
        return effectSource.isPlaying;
    }

    public void ToggleEffects()
    {
        effectSource.mute = !effectSource.mute;
    }
    
    public void ToggleMusic()
    {
        musicSource.mute = !musicSource.mute;
    }
}