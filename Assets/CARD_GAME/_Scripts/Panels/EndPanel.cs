using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class EndPanel : MonoBehaviour
{
    public static EndPanel instance;

    [SerializeField] private GameObject victoryText;
    [SerializeField] private GameObject gameOverText;
    [SerializeField] private List<AudioClip> winnerAudio;
    [SerializeField] private List<AudioClip> gameOverAudio;
    [SerializeField] private AudioSource audioSource;
    private void Awake()
    {
        instance = this;
    }
    
    public void ShowWinText(bool hasPlayerWon)
    {
        if (hasPlayerWon)
        {
            victoryText.SetActive(true);
            audioSource.PlayOneShot(winnerAudio[Random.Range(0, winnerAudio.Count)]);
        }
            
        if (!hasPlayerWon)
        {
            gameOverText.SetActive(true);
            audioSource.PlayOneShot(gameOverAudio[Random.Range(0, gameOverAudio.Count)]);
        }
    }
    public void ResetEndPanel()
    {
        victoryText.SetActive(false);
        gameOverText.SetActive(false);
        this.gameObject.SetActive(false);
    }
}
