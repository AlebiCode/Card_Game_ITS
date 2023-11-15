using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class TutorialPanel : MonoBehaviour {

    [SerializeField] private GameObject[] pagesContent;
    [SerializeField] private GameObject rightButton;
    [SerializeField] private GameObject leftButton;
    private int currentPageIndex;

    private void OnEnable() {
        pagesContent[currentPageIndex].SetActive(false);
        currentPageIndex = 0;
        pagesContent[currentPageIndex].SetActive(true);
        UpdateArrowButtons();
    }

    public void TurnPageRight() {

        if (currentPageIndex >= pagesContent.Length - 1)
            return;

        pagesContent[currentPageIndex].SetActive(false);
        currentPageIndex++;
        pagesContent[currentPageIndex].SetActive(true);

        UpdateArrowButtons();
    }

    public void TurnPageLeft() {

        if (currentPageIndex == 0)
            return;

        pagesContent[currentPageIndex].SetActive(false);
        currentPageIndex--;
        pagesContent[currentPageIndex].SetActive(true);

        UpdateArrowButtons();

    }

    private void UpdateArrowButtons() {

        if(pagesContent.Length == 1) {
            leftButton.SetActive(false);
            rightButton.SetActive(false);
            return;
        }

        if (currentPageIndex == 0) {
            leftButton.SetActive(false);
            rightButton.SetActive(true);
            return;
        }

        if (currentPageIndex == pagesContent.Length - 1) {
            leftButton.SetActive(true);
            rightButton.SetActive(false);
            return;
        }

        leftButton.SetActive(true);
        rightButton.SetActive(true);


    }

}
