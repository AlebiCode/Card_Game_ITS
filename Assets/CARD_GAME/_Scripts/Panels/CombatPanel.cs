using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatPanel : MonoBehaviour
{
    public List<Dice> diceList = new List<Dice>();
    private bool inputsActive = true;

    public void SetInputsActive(bool value)
    {
        inputsActive = value;
    }

    public void OnDiceClick(Dice dice)
    {
        if (!inputsActive)
            return;

        AudioManager.PlayUiConfirmAudio();

        // Controllare che siamo in fase di reroll, altrimenti return
        if (dice.IsLocked)
        {
            dice.LockDice(false);
            dice.particle?.DeactivateAnimation(VFX_TYPE.SELECT);
        }
        else
        {
            dice.LockDice(true);
            dice.particle?.ActivateAnimation(VFX_TYPE.SELECT);
        }
    }

    public void LockAndRolls()
    {
        if (!inputsActive)
            return;
        AudioManager.PlayUiConfirmAudio();
        BattleManager.instance.RerollDices_Ally();
    }

    private void OnDisable()
    {
        foreach (Dice d in diceList)
        { 
            d.GetComponent<Image>().color = Color.white;
        }

        diceList.Clear();
    }
}
