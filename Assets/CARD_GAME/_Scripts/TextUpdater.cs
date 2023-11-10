using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(TMP_Text))]
public class TextUpdater : MonoBehaviour
{
    [SerializeField] private TMP_Text ownText;
    [SerializeField] private bool hideWhenZero;
    [SerializeField] private bool animatePunch;

    public void UpdateValue(int value) {
        ownText.text = value + "";
        ownText.gameObject.SetActive(!hideWhenZero || value > 0);
        if(animatePunch)
            ownText.transform.DOPunchScale(ownText.transform.localScale * 1.4f, 0.1f);
    }

    /*
    public void UpdateText(string newText) {
        ownText.text = newText;
    }*/
}
