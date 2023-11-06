using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CardBuffsManager : MonoBehaviour
{
    [SerializeField] GameObject parryGameObject;
    [SerializeField] GameObject dodgeGameObject;
    [SerializeField] private TMP_Text parryText;
    [SerializeField] private TMP_Text dodgeText;

    private void Awake() {
        



        parryText.text = "";
        dodgeText.text = "";
    }

    public void UpdateParryCount(int parry) {
        if (!parryGameObject.activeInHierarchy)
            parryGameObject.SetActive(true);

        if (parry <= 0) {
            parryGameObject.SetActive(false);
            return;
        }

        parryText.text = parry + "";
    }

    public void UpdateDodgeChance(int dodgeChance) {
        if(!dodgeGameObject.activeInHierarchy)
            dodgeGameObject.SetActive(true);


        dodgeText.text = dodgeChance + "";
    }


}
