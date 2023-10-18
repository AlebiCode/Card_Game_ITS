using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dice : MonoBehaviour
{
    public enum diceFace { notRolled, red, blue, yellow }

    private diceFace result = diceFace.notRolled;
    private bool locked = false;

    public diceFace Result => result;
    public bool IsLocked => locked;


    public void RollDice()
    {
        int roll = Random.Range(0,6);
        if(roll <= 2)
            result = diceFace.red;
        else if(roll <= 4)
            result = diceFace.yellow;
        else
            result = diceFace.blue;

        GetComponentInChildren<TMPro.TMP_Text>().text = result.ToString();
    }

    public void LockDice(bool value)
    {
        locked = value;

        // Attiva/Disattiva grafica o fx di selezione
        GetComponent<Image>().color = value ? Color.yellow : Color.white;
    }

}
