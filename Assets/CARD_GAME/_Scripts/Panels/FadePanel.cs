using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadePanel : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject currentPanel;
    [SerializeField] private float fadeSpeed = 10;
    
    public void FadeToPanel(GameObject newPanel)
    {
        canvasGroup.alpha = 0;
        gameObject.SetActive(true);

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
        gameObject.SetActive(false);
    }
    private IEnumerator FadeCoroutine(float targetAlpha)
    {
        while (canvasGroup.alpha != targetAlpha)
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            yield return null;
        }
    }

}
