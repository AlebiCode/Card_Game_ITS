using System;
using UnityEngine;
using UnityEngine.UI;

public class ParticleSystemActivator : MonoBehaviour {
    public ParticleSystem selectionEffect;
    public ParticleSystem attackEffect;
    public ParticleSystem defenseEffect;
    public ParticleSystem dodgeEffect;
    private bool isSelected = false;

    private void Start() {
        // Make sure the Particle System is not active at the beginning.
        //Affinché l'effetto particellare si veda normale setto la sua scala a 0.1f di default, in caso non venga fatto
        //particleSystem.gameObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        //selectionEffect?.Stop();
        //attackEffect?.Stop();
        //defenseEffect?.Stop();
        //dodgeEffect?.Stop();
    }


    public void ActivateAnimation(VFX_TYPE vfx) {
        if (!isSelected) {
            switch (vfx) {
                case VFX_TYPE.SELECT:
                    StartAnimation(selectionEffect);
                    break;
                case VFX_TYPE.ATTACK:
                    StartAnimation(attackEffect);
                    break;
                case VFX_TYPE.DEFENSE:
                    StartAnimation(defenseEffect);
                    break;
                case VFX_TYPE.DODGE:
                    StartAnimation(dodgeEffect);
                    break;
                default:
                    Debug.LogWarning("Start Animation");
                    break;
            }
        }
    }
            public void DeactivateAnimation(VFX_TYPE vfx) {
            if (isSelected) {
                switch (vfx) {
                case VFX_TYPE.SELECT:
                    StopAnimation(selectionEffect);
                    break;

                case VFX_TYPE.ATTACK:
                    StopAnimation(attackEffect);
                    break;
                case VFX_TYPE.DEFENSE:
                    StopAnimation(defenseEffect);
                    break;
                case VFX_TYPE.DODGE:
                    StopAnimation(dodgeEffect);
                    break;
                default:
                    Debug.LogWarning("Start Animation");
                    break;
            }
        }
    }

            private void StartAnimation(ParticleSystem particleSystem) {
                if (particleSystem != null) {
            particleSystem.gameObject.SetActive(true);
            particleSystem.Play();
                        if (particleSystem.loop)
                            isSelected = true;
                } else {
                    Debug.LogError("Couldn't locate Particle Effect for " + gameObject.name);
                }

            }

    private void StopAnimation(ParticleSystem particleSystem) {
        ParticleSystem[] psList;
        if (particleSystem != null) {
            if (isSelected && particleSystem.isPlaying) {
                particleSystem.gameObject.SetActive(false);
                particleSystem.Stop();
                isSelected = false;
            }
        } else {
            Debug.LogError("Couldn't locate Particle Effect for " + gameObject.name);
        }

    }

}
    public enum VFX_TYPE {
    SELECT = 0,
    ATTACK = 1,
    DEFENSE = 2,
    DODGE = 3
}
