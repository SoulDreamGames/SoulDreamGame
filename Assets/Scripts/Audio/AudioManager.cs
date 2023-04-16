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
            
        _audioSource.PlayOneShot(audioClip);
    }

    public void PlayAudioLoop(string s)
    {
        AudioClip audioClip = audioList.GetAudio(s);
        if (!audioClip) return;

        _audioSource.clip = audioClip;
        _audioSource.Play();
    }

    public void StopPlayingAll()
    {
        _audioSource.Stop();
    }
}
