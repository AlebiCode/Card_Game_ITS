using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardAudioProfile", menuName = "ScriptableObjects/Create CardAudioProfile", order = 1)]
public class CardAudioProfile : ScriptableObject
{
    [SerializeField] private AudioClip[] audioClips;

    public AudioClip GetRandomClip()
    {
        if(audioClips == null || audioClips.Length == 0)
            return null;

        return audioClips[Random.Range(0,audioClips.Length)];
    }

}
