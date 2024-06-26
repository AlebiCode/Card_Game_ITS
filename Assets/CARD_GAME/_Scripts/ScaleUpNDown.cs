using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ScaleUpNDown : MonoBehaviour
{
    // Attach this script to the element you want to scale Up and Down

    [SerializeField] private Vector2 scaleUp;
    [SerializeField] private Vector2 scaleDown;
    [SerializeField] private float scaleTime;
    [SerializeField] private float delayBetweenAnimation;

    // Update is called once per frame
    void Start()
    {
        StartCoroutine(Scaling());
    }

    private IEnumerator Scaling()
    {
        this.transform.DOScale(scaleUp, scaleTime).SetEase(Ease.Linear);
        yield return new WaitForSeconds(delayBetweenAnimation);
        this.transform.DOScale(scaleDown, scaleTime).SetEase(Ease.Linear);
        yield return new WaitForSeconds(delayBetweenAnimation);
        StartCoroutine(Scaling());
    }
}
