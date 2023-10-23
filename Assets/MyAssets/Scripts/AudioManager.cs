using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Sources")]
    [SerializeField] private List<AudioSource> cardAudio;
    [SerializeField] private List<AudioSource> uiAudio;
    //[SerializeField] private List<AudioSource> combatAudio;
    [SerializeField] private AudioSource diceRollLoop;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip genericUiSelect;
    [SerializeField] private AudioClip genericUiConfirm;
    [SerializeField] private AudioClip attackBlocked;
    [SerializeField] private AudioClip attackDodged;
    [SerializeField] private AudioClip attackHit;

    private void Awake()
    {
        instance = this;
    }

    public static void PlayCardAudio(AudioClip audioClip, float delay = 0)
    {
        AudioSource audioSource = instance.cardAudio[0];
        instance.cardAudio.RemoveAt(0);
        audioSource.clip = audioClip;
        audioSource.PlayDelayed(delay);
        instance.cardAudio.Add(audioSource);
    }
    public static void PlayCombatAttackBlocked()
    {
        PlayCardAudio(instance.attackBlocked);
    }
    public static void PlayCombatAttackDodged()
    {
        PlayCardAudio(instance.attackDodged);
    }
    public static void PlayCombatAttackHit()
    {
        PlayCardAudio(instance.attackHit);
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
    public static void PlayUiConfirmAudio()
    {
        PlayUiAudio(instance.genericUiConfirm);
    }

    public static void StartDiceRollLoop()
    {
        if(!instance.diceRollLoop.isPlaying)
            instance.diceRollLoop.Play();
    }
    public static void StopDiceRollLoop()
    {
        instance.diceRollLoop.Stop();
    }

}
