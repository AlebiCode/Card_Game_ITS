using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsButton : MonoBehaviour
{
    [SerializeField] private RectTransform pausePanel;
    [SerializeField] private Vector3 offScreenPosition;
    [SerializeField] private Vector3 inScreenPosition;
    [SerializeField] private float speed;
    private bool isShowing = false;

    public void TogglePausePanelOnAndOff()
    {
        /*if (!pausePanel.activeInHierarchy) 
        {
            pausePanel.SetActive(true);
        }
        else
        {
            pausePanel.SetActive(false);
        }*/
        StopAllCoroutines();
        StartCoroutine(SlideMovementCoroutine());
    }

    private IEnumerator SlideMovementCoroutine()
    {
        isShowing = !isShowing;
        Vector2 targetPos = isShowing ? new Vector2(-pausePanel.sizeDelta.x, 0) : Vector2.zero;
        while (pausePanel.anchoredPosition != targetPos)
        {
            pausePanel.anchoredPosition = Vector2.Lerp(pausePanel.anchoredPosition, targetPos, speed * Time.deltaTime);
            yield return null;
        }
    }

}
