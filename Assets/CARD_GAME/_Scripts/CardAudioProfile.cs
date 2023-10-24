using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardAudioProfile", menuName = "ScriptableObjects/Create CardAudioProfile", order = 1)]
public class CardAudioProfile : ScriptableObject
{
    [SerializeField] private AudioClip[] entranceClips;
    [SerializeField] private AudioClip[] linesClips;

    public AudioClip GetRandomEntranceClip()
    {
        if (entranceClips == null || entranceClips.Length == 0)
            return null;

        return entranceClips[Random.Range(0, entranceClips.Length)];
    }
    public AudioClip GetRandomLinesClip()
    {
        if (linesClips == null || linesClips.Length == 0)
            return null;

        return linesClips[Random.Range(0, linesClips.Length)];
    }

}
