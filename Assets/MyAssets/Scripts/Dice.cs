using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class Dice
{
    public enum ManaTpye { red, blue, yellow }
    
    public static ManaTpye RollDice()
    {
        int roll = Random.Range(0,6);
        if(roll <= 2)
            return ManaTpye.red;
        if(roll <= 4)
            return ManaTpye.yellow;
        return ManaTpye.blue;
    }

}
