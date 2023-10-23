using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitlePanel : MonoBehaviour
{
    public AudioClip audioClip;

    public void PlayAudioClip()
    {
        if (audioClip)
            AudioManager.PlayUiAudio(audioClip);
    }
}
