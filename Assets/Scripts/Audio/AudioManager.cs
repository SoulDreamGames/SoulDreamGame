using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private AudioSource _audioSource;
    [SerializeField] private AudioList audioList;

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        audioList.Initialize();
    }

    public void PlayAudioButton(string s)
    {
        AudioClip audioClip = audioList.GetAudio(s);
        if (!audioClip) return;
        
        Debug.Log("Playing audio clip: " + audioClip.name);
        _audioSource.PlayOneShot(audioClip);
    }
    
    public void PlayAudioButtonWithVolume(string s, float volume)
    {
        AudioClip audioClip = audioList.GetAudio(s);
        if (!audioClip) return;
        
        Debug.Log("Playing audio clip: " + audioClip.name);
        _audioSource.PlayOneShot(audioClip, volume);
    }

    public void PlayAudioLoop(string s)
    {
        AudioClip audioClip = audioList.GetAudio(s);
        if (!audioClip) return;
        
        if (_audioSource.clip == audioClip && _audioSource.isPlaying) return;
        
        _audioSource.clip = audioClip;
        _audioSource.Play();
    }

    public void StopPlayingAll()
    {
        _audioSource.Stop();
    }
}
