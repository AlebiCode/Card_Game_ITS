using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatPanel : MonoBehaviour
{
    public List<GameObject> diceList;

    public void OnDiceClick(GameObject dice)
    {
        // Controllare che siamo in fase di reroll, altrimenti return

        if (diceList.Contains(dice))
        {
            DeselctDice(dice);
            return;
        }


        SelectDice(dice);

    }

    void SelectDice(GameObject dice)
    {
        diceList.Add(dice);

        // Attiva grafica o fx di selezione
        dice.GetComponent<Image>().color = Color.yellow;
    }

    void DeselctDice(GameObject dice)
    {
        diceList.Remove(dice);

        // Dosattova grafica o fx di selezione
        dice.GetComponent<Image>().color = Color.white;
    }

    public void LockAndRolls()
    { }

    private void OnDisable()
    {
        foreach (GameObject d in diceList)
        { 
            d.GetComponent<Image>().color = Color.white;
        }

        diceList.Clear();
    }
}
