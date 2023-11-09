using System;
using UnityEngine;
using UnityEngine.UI;

public class ParticleSystemActivator : MonoBehaviour {
    public ParticleSystem selectionEffectPlayer;
    public ParticleSystem selectionEffectEnemy;
    // public ParticleSystem attackEffect;
    // public ParticleSystem defenseEffect;
    // public ParticleSystem dodgeEffect;
    // public ParticleSystem specialAttack;
    private bool isSelected = false;


        // Make sure the Particle System is not active at the beginning.
        //Affinché l'effetto particellare si veda normale setto la sua scala a 0.1f di default, in caso non venga fatto
        //particleSystem.gameObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        //selectionEffect?.Stop();
        //attackEffect?.Stop();
        //defenseEffect?.Stop();
        //dodgeEffect?.Stop();

    public void StartSelectionAnim(bool isEnemy) {
        if (isEnemy) {
            SelectionEffectActivation(selectionEffectEnemy);
        } else {
            SelectionEffectActivation(selectionEffectPlayer);
        }
        
    }

    private void SelectionEffectActivation(ParticleSystem selection) {
        if (selection != null) {
            selection.gameObject.SetActive(true);
            selection.Play();
            isSelected = true;
        } else {
            Debug.LogError("Couldn't locate Selection Particle Effect for " + gameObject.name);
        }

    }


    public void StopSelectionAnim() {
        //ParticleSystem[] psList;
        if (selectionEffectPlayer != null) {
            if (isSelected && selectionEffectPlayer.isPlaying) {
                selectionEffectPlayer.gameObject.SetActive(false);
                selectionEffectPlayer.Stop();
                isSelected = false;
            }
        } else {
            Debug.LogError("Couldn't locate Particle Effect for " + gameObject.name);
        }

    }
}

