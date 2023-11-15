using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class EndPanel : MonoBehaviour
{
    public static EndPanel instance;

    [SerializeField] private GameObject victoryText;
    [SerializeField] private ParticleSystem[] fireWorks;
    [SerializeField] private GameObject gameOverText;
    [SerializeField] private List<AudioClip> winnerAudio;
    [SerializeField] private List<AudioClip> gameOverAudio;

    private void Awake()
    {
        instance = this;
    }
    
    public void ShowWinText(bool hasPlayerWon)
    {
        if (hasPlayerWon)
        {
            victoryText.SetActive(true);
            AudioManager.PlayAudio(winnerAudio[Random.Range(0, winnerAudio.Count)], 3);
            foreach (var ps in fireWorks)
            {
                ps.Play();
            }
        }
            
        if (!hasPlayerWon)
        {
            gameOverText.SetActive(true);
            AudioManager.PlayAudio(gameOverAudio[Random.Range(0, gameOverAudio.Count)], 3);

        }
    }
    public void ResetEndPanel()
    {
        victoryText.SetActive(false);
        gameOverText.SetActive(false);
        this.gameObject.SetActive(false);
    }
}
