using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class CreditsPanel : MonoBehaviour
{
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject titlePanel;
    [SerializeField] private float fadeTime;
    [SerializeField] private float intervalBetweenFadeInAndFadeOut;
    [SerializeField] private List<TextMeshProUGUI> programmersCredits;
    [SerializeField] private List<TextMeshProUGUI> uiCredits;
    [SerializeField] private List<TextMeshProUGUI> dueDCredits;
    [SerializeField] private List<TextMeshProUGUI> audioCredits;
    [SerializeField] private List<TextMeshProUGUI> vFXCredits;
    [SerializeField] private List<TextMeshProUGUI> designersCredits;
    [SerializeField] private List<TextMeshProUGUI> producersCredits; 
    private List<List<TextMeshProUGUI>> credits = new List<List<TextMeshProUGUI>>();
    void Start()
    {
        InitCredits(); 
    }

    public void OnCreditStart() 
    {
        StartCoroutine(StartCredits());
    }

    private void InitCredits()
    {
        credits.Add(programmersCredits);
        credits.Add(uiCredits);
        credits.Add(dueDCredits);
        credits.Add(audioCredits);
        credits.Add(vFXCredits);
        credits.Add(designersCredits);
        credits.Add(producersCredits);
    }

    public void EndCredits() 
    {
        foreach (var creds in credits) 
        { 
            foreach (var text in creds) 
            {
                text.DOFade(0f, fadeTime);
                text.gameObject.SetActive(false);
            }
        }
        StopCoroutine(StartCredits());
        titlePanel.SetActive(true);
        creditsPanel.SetActive(false);
    }

    private IEnumerator StartCredits()
    { 
        yield return new WaitForSeconds(0.5f);
        foreach (var creds in credits) 
        { 
            foreach (var text in creds) 
            {
                text.gameObject.SetActive(true);
                text.DOFade(1f, fadeTime);
            }

            yield return new WaitForSeconds(intervalBetweenFadeInAndFadeOut);

            foreach (var text in creds)
            {
                text.DOFade(0f, fadeTime);
            }

            yield return new WaitForSeconds(fadeTime + 0.1f);

            foreach (var text in creds) 
            {
                text.gameObject.SetActive(false);
            }

            yield return new WaitForSeconds(0.5f);
        }

        EndCredits();
    }
}
