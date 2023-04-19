using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioList", menuName = "ScriptableObjects/AudioList", order = 2)]
public class AudioList : ScriptableObject
{
    [SerializeField] private List<AudioPlayable> _audios;
    private Dictionary<string, AudioClip> _audioClips;

    public void Initialize()
    {
        _audioClips = new Dictionary<string, AudioClip>();
        foreach (var audioP in _audios)
        {
            _audioClips.Add(audioP.IDName, audioP.Audio);
        }    
    }
    
    public AudioClip GetAudio(string s)
    {
        return _audioClips.TryGetValue(s, out AudioClip audioClip) ? audioClip : null;
    }
}

[Serializable]
struct AudioPlayable
{
    public string IDName;
    public AudioClip Audio;
}