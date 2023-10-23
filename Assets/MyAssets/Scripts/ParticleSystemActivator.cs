using UnityEngine;
using UnityEngine.UI;

public class ParticleSystemActivator : MonoBehaviour {
    public ParticleSystem particleSystem; // Reference to your Particle System
    private bool isParticleSystemActive = false;

    private void Start() {
        // Make sure the Particle System is not active at the beginning.
        particleSystem.Stop();
        isParticleSystemActive = false;
    }

    private void Update() {
        // Check for UI element selection (e.g., Button click, Toggle change, etc.)
        if (Input.GetMouseButtonDown(0)) // Change this condition based on your UI interaction
        {
            // Check if the Particle System is currently active
            if (!isParticleSystemActive) {
                // Activate the Particle System
                particleSystem.Play();
                isParticleSystemActive = true;
            } else {
                // Deactivate the Particle System
                particleSystem.Stop();
                isParticleSystemActive = false;
            }
        }
    }
}
