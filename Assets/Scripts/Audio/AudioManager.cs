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
        AudioClip audio = audioList.GetAudio(s);
        if(audio)
            _audioSource.PlayOneShot(audio);
    }
}
