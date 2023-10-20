using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioCardManager : MonoBehaviour {

    public AudioSource sound;

    public AudioClip selectAudio;
    public AudioClip deselectAudio;

    private void PlaySound(AudioClip audio) {
        if (audio != null) {
            sound.PlayOneShot(audio);
            //sound. = null;
        } else {
            Debug.LogError("Couldn't find the sound " + audio);
        }
    }

    public void PlaySelectAudio() {
        PlaySound(selectAudio);
    }

    public void PlayDeSelectAudio() {
        PlaySound(deselectAudio);
    }

}
