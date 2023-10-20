using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class TextUpdater : MonoBehaviour
{

    [SerializeField] TMP_Text ownText;

    public void UpdateValue(int value) {
        ownText.text = value + "";
    }

    public void UpdateText(string newText) {
        ownText.text = newText;
    }
}
