using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardParticleSys : ParticleSystemActivator
{
    [Header("0=HUMANS, 1=DRUIDS, 2=WIZARDS, 3=ROBOTS, 4=GHOSTS")]

    [SerializeField] private ParticleSystem[] attackEffect = new ParticleSystem[5];
    [SerializeField] private ParticleSystem[] defenseEffect = new ParticleSystem[5];
    [SerializeField] private ParticleSystem[] dodgeEffect = new ParticleSystem[5];
    [SerializeField] private ParticleSystem[] specialAttack = new ParticleSystem[5];


    public void ActivateAnimation(VFX_TYPE vfx, RACES race ) {
            switch (vfx) {
                case VFX_TYPE.ATTACK:
                    StartAnimation(attackEffect[(int)race]);
                    break;
                case VFX_TYPE.DEFENSE:
                    StartAnimation(defenseEffect[(int)race]);
                    break;
                case VFX_TYPE.DODGE:
                    StartAnimation(dodgeEffect[(int)race]);
                    break;
                default:
                    Debug.LogWarning("Start Animation");
                    break;
            }
    }

    private void StartAnimation(ParticleSystem particleSystem) {
        if (particleSystem != null) {
            particleSystem.gameObject.SetActive(true);
            particleSystem.Play();
        } else {
            Debug.LogError("Couldn't locate Particle Effect for " + gameObject.name);
        }
    }

    public void DeactivateAnimation(VFX_TYPE vfx, RACES race) {
            switch (vfx) {
                case VFX_TYPE.SELECT:
                    StopAnimation(selectionEffectPlayer);
                    break;

                case VFX_TYPE.ATTACK:
                    StopAnimation(attackEffect[(int)race]);
                    break;
                case VFX_TYPE.DEFENSE:
                    StopAnimation(defenseEffect[(int)race]);
                    break;
                case VFX_TYPE.DODGE:
                    StopAnimation(dodgeEffect[(int)race]);
                    break;
                default:
                    Debug.LogWarning("Start Animation");
                    break;
            }
        } /**/
    private void StopAnimation(ParticleSystem particleSystem) {
        if (particleSystem != null) {
            if (particleSystem.isPlaying) {
                particleSystem.gameObject.SetActive(false);
                particleSystem.Stop();
            }
        } else {
            Debug.LogError("Couldn't locate Particle Effect for " + gameObject.name);
        }

    }

}
