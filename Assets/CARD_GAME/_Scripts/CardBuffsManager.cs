using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CardBuffsManager : MonoBehaviour
{
    [SerializeField] GameObject parryGameObject;
    [SerializeField] GameObject dodgeGameObject;
    [SerializeField] private TextUpdater parryTextUpdater;
    [SerializeField] private TextUpdater dodgeTextUpdater;
    [SerializeField] bool invertLocation;

    private void Awake() {
        parryTextUpdater.UpdateValue(0);
        dodgeTextUpdater.UpdateValue(0);

        if (invertLocation)
        {
            transform.localPosition = new Vector3(-transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
        }
    }



    public void UpdateParryCount(int parry) {
        if (!parryGameObject.activeInHierarchy)
            parryGameObject.SetActive(true);

        if (parry <= 0) {
            parryGameObject.SetActive(false);
            return;
        }

        parryTextUpdater.UpdateValue(parry);
    }

    public void UpdateDodgeChance(int dodgeChance) {
        if (dodgeChance <= 0) {
            dodgeGameObject.SetActive(false);
            return;
        }

        if(!dodgeGameObject.activeInHierarchy && dodgeChance>0)
            dodgeGameObject.SetActive(true);


        dodgeTextUpdater.UpdateValue(dodgeChance);
    }

    public void ResetStats() {
        UpdateDodgeChance(0);
        UpdateParryCount(0);
    }
}
