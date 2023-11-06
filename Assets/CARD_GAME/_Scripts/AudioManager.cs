using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public AudioMixer mainMixer;

    [Header("Audio Sources")]
    [SerializeField] private GameObject[] audiosourceParents;
    private List<AudioSource>[] sourceGroups;
    //[SerializeField] private List<AudioSource> cardAudio;
    //[SerializeField] private List<AudioSource> uiAudio;
    //[SerializeField] private List<AudioSource> combatAudio;
    [SerializeField] private AudioSource diceRollLoop;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip genericUiSelect;
    [SerializeField] private AudioClip genericUiConfirm;
    [SerializeField] private AudioClip genericUiConfirm2;
    [SerializeField] private AudioClip diceLocked;
    [SerializeField] private AudioClip attackBlocked;
    [SerializeField] private AudioClip attackDodged;
    [SerializeField] private AudioClip attackHit;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        sourceGroups = new List<AudioSource>[audiosourceParents.Length];
        for (int i = 0; i < audiosourceParents.Length; i++)
        {
            sourceGroups[i] = new List<AudioSource>(audiosourceParents[i].GetComponentsInChildren<AudioSource>());
        }
    }

    public static void PlayAudio(AudioClip audioClip, int sourceGroup, float delay = 0)
    {
        AudioSource audioSource = instance.sourceGroups[sourceGroup][0];
        instance.sourceGroups[sourceGroup].RemoveAt(0);
        audioSource.clip = audioClip;
        audioSource.PlayDelayed(delay);
        instance.sourceGroups[sourceGroup].Add(audioSource);
    }

    public static void PlayCombatAttackBlocked()
    {
        PlayAudio(instance.attackBlocked, 0);
    }
    public static void PlayCombatAttackDodged()
    {
        PlayAudio(instance.attackDodged, 0);
    }
    public static void PlayCombatAttackHit()
    {
        PlayAudio(instance.attackHit, 0);
    }

    public static void PlayUiSelectAudio()
    {
        PlayAudio(instance.genericUiSelect, 4);
    }
    public static void PlayUiConfirmAudio()
    {
        PlayAudio(instance.genericUiConfirm, 4);
    }
    public static void PlayUiConfirmAudio2()
    {
        PlayAudio(instance.genericUiConfirm2, 4);
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
    public static void PlayUiDiceLocked()
    {
        PlayAudio(instance.diceLocked, 4);
    }

    public void SetVolume(float volume)
    {
        mainMixer.SetFloat("Volume", volume);
    }
}
