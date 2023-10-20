using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField] private List<AudioSource> cardAudio;
    [SerializeField] private List<AudioSource> uiAudio;

    [SerializeField] private AudioClip genericUiSelect;

    private void Awake()
    {
        instance = this;
    }

    public static void PlayCardAudio(AudioClip audioClip)
    {
        AudioSource audioSource = instance.cardAudio[0];
        instance.cardAudio.RemoveAt(0);
        audioSource.clip = audioClip;
        audioSource.Play();
        instance.cardAudio.Add(audioSource);
    }

    public static void PlayUiAudio(AudioClip audioClip)
    {
        AudioSource audioSource = instance.uiAudio[0];
        instance.uiAudio.RemoveAt(0);
        audioSource.clip = audioClip;
        audioSource.Play();
        instance.uiAudio.Add(audioSource);
    }
    public static void PlayUiSelectAudio()
    {
        PlayUiAudio(instance.genericUiSelect);
    }

}
