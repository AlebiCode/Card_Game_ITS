using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField] private AudioSource cardAudio;
    [SerializeField] private AudioSource uiAudio;

    private void Awake()
    {
        instance = this;
    }

    public static void PlayCardAudio(AudioClip audioClip)
    {
        instance.cardAudio.clip = audioClip;
        instance.cardAudio.Play();
    }
    public static void PlayUiAudio(AudioClip audioClip)
    {
        instance.uiAudio.clip = audioClip;
        instance.uiAudio.Play();
    }

}
