using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TransitionManager : MonoBehaviour
{
    [Header("Fade Panel")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private GameObject currentPanel;
    [SerializeField] private float fadeSpeed = 10;

    public CanvasGroup FadeCanvasGroup => fadeCanvasGroup;

    public void FadeToPanel(GameObject newPanel)
    {
        fadeCanvasGroup.alpha = 0;
        fadeCanvasGroup.gameObject.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(MainCoroutine(newPanel));
    }

    private IEnumerator MainCoroutine(GameObject newPanel)
    {
        yield return FadeCoroutine(1);

        currentPanel?.SetActive(false);
        currentPanel = newPanel;
        newPanel.SetActive(true);

        yield return FadeCoroutine(0);
        fadeCanvasGroup.gameObject.SetActive(false);
    }
    private IEnumerator FadeCoroutine(float targetAlpha)
    {
        while (fadeCanvasGroup.alpha != targetAlpha)
        {
            fadeCanvasGroup.alpha = Mathf.MoveTowards(fadeCanvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            yield return null;
        }
    }

}
