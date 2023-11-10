using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    private const float PITCH_MAX_VARIATION = 0.05f;
    private const float VOLUME_DIMINISH_ON_SOUND_REPEAT = 0.1f;
    private const float TIME_TO_FORGET_SOUND = 1f;
    private const int REPETITION_VOULME_DIMINISHER_MAX_LENGHT = 3;

    public static AudioManager instance;
    public AudioMixer mainMixer;

    [Header("Audio Sources")]
    [SerializeField] private GameObject[] audiosourceParents;
    [SerializeField] private AudioSource diceRollLoop;

    private static List<AudioSource>[] sourceGroups;
    private static List<AudioClip> lastPlayedClips = new List<AudioClip>();
    private static float soundRepetitionTimer = 0;
    //[SerializeField] private List<AudioSource> cardAudio;
    //[SerializeField] private List<AudioSource> uiAudio;
    //[SerializeField] private List<AudioSource> combatAudio;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip genericUiSelect;
    [SerializeField] private AudioClip genericUiConfirm;
    [SerializeField] private AudioClip genericUiConfirm2;
    [SerializeField] private AudioClip diceLocked;
    [SerializeField] private AudioClip attackBlocked;
    [SerializeField] private AudioClip attackDodged;
    [SerializeField] private AudioClip attackHit;
    [SerializeField] private AudioClip scoreUpdateWin;
    [SerializeField] private AudioClip scoreUpdateLose;

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
    private void Update()
    {
        soundRepetitionTimer += Time.deltaTime;
        if (soundRepetitionTimer >= TIME_TO_FORGET_SOUND && lastPlayedClips.Count > 0)
        {
            lastPlayedClips.RemoveAt(0);
            soundRepetitionTimer = 0;
        }
    }

    public static void PlayAudio(AudioClip audioClip, int sourceGroup, float delay = 0, bool pitchVariation = false)
    {
        AudioSource audioSource = sourceGroups[sourceGroup][0];
        sourceGroups[sourceGroup].RemoveAt(0);
        sourceGroups[sourceGroup].Add(audioSource);

        audioSource.clip = audioClip;
        audioSource.volume = VolumeDiminisherOnRepeat(audioClip, sourceGroup);
        audioSource.pitch = pitchVariation ? 1 + Random.Range(-PITCH_MAX_VARIATION, PITCH_MAX_VARIATION) : 1;
        audioSource.PlayDelayed(delay);

        lastPlayedClips.Add(audioClip);
        if(lastPlayedClips.Count > REPETITION_VOULME_DIMINISHER_MAX_LENGHT)
            lastPlayedClips.RemoveAt(0);
        soundRepetitionTimer = 0;
    }

    private static float VolumeDiminisherOnRepeat(AudioClip currentClip, int sourceGroup)
    {
        int alreadyPlayed = 0;
        for (int i = 0; i < lastPlayedClips.Count; i++)
        {
            if(lastPlayedClips[i] == currentClip)
                alreadyPlayed++;
        }
        return 1 - (VOLUME_DIMINISH_ON_SOUND_REPEAT * alreadyPlayed);
    }

    public static void PlayCombatAttackBlocked()
    {
        PlayAudio(instance.attackBlocked, 0, pitchVariation: true);
    }
    public static void PlayCombatAttackDodged()
    {
        PlayAudio(instance.attackDodged, 0, pitchVariation: true);
    }
    public static void PlayCombatAttackHit()
    {
        PlayAudio(instance.attackHit, 0, pitchVariation: true);
    }

    public static void PlayUiSelectAudio()
    {
        PlayAudio(instance.genericUiSelect, 4, pitchVariation: true);
    }
    public static void PlayUiConfirmAudio()
    {
        PlayAudio(instance.genericUiConfirm, 4, pitchVariation: true);
    }
    public static void PlayUiConfirmAudio2()
    {
        PlayAudio(instance.genericUiConfirm2, 4, pitchVariation: true);
    }
    public static void PlayUiScoreUpdate(bool playerHasWon)
    {
        PlayAudio(playerHasWon ? instance.scoreUpdateWin : instance.scoreUpdateLose, 4);
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
        PlayAudio(instance.diceLocked, 4, pitchVariation: true);
    }

    public void SetVolume(float volume)
    {
        mainMixer.SetFloat("Volume", volume);
    }
}
