using UnityEngine;
using UnityEngine.UI;

public class ParticleSystemActivator : MonoBehaviour {
    public ParticleSystem particleSystem; // Reference to your Particle System
    private bool isParticleSystemActive = false;

    private void Start() {
        // Make sure the Particle System is not active at the beginning.
        //Affinché l'effetto particellare si veda normale setto la sua scala a 0.1f di default, in caso non venga fatto
        particleSystem.gameObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        particleSystem.Stop();
        isParticleSystemActive = false;
    }


    public void ActivateAnimation() {
        if (particleSystem != null) {
            if (!isParticleSystemActive) {
                particleSystem.Play();
                isParticleSystemActive = true;
            }
        } else {
            Debug.LogError("Couldn't locate Particle Effect for " + gameObject.name);
        }

    }

    public void DeactivateAnimation() {
        if (particleSystem != null) {
            if (isParticleSystemActive) {
                particleSystem.Stop();
                isParticleSystemActive = false;
            }
        } else {
            Debug.LogError("Couldn't locate Particle Effect for " + gameObject.name);
        }
    }
}

