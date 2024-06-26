using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitlePanel : MonoBehaviour
{
    public AudioClip confirmAudioClip;
    public AudioClip music;

    private void OnEnable()
    {
        StartCoroutine(AudioCoroutine());
    }

    private IEnumerator AudioCoroutine()
    {
        yield return null;
        if (music)
            AudioManager.PlayAudio(music, 3, loop: true);
    }

    public void PlayAudioClip()
    {
        if (confirmAudioClip)
            AudioManager.PlayAudio(confirmAudioClip, 4);
    }
}
