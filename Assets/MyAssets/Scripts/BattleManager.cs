using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;

    [SerializeField] private TablePanel tablePanel;
    [SerializeField] private CombatPanel combatPanel;
    [SerializeField]private Card[] carteNemiche = new Card[3];
    [SerializeField]private Card[] carteAmiche = new Card[3];

    [SerializeField] private Dice[] allyDices = new Dice[6];
    [SerializeField] private Dice[] enemyDices = new Dice[6];

    private int cartaSelNemicaIndex;
    private bool allyWaitingForReroll;
    private bool enemyWaitingForReroll;
    private Coroutine fightCoroutine;
    private Coroutine enemyDiceSelectionCoroutine;

    private Card CartaSelAmica => tablePanel.selectedCard;
    private Card CartaSelNemica => carteNemiche[cartaSelNemicaIndex];

    private void Awake()
    {
        instance = this;
    }

    public void LoadCardDatas(CardData[] carteProprie, CardData[] carteAvversario)
    {
        for (int i = 0; i < 3; i++)
        {
            carteAmiche[i].LoadData(carteProprie[i]);
            carteNemiche[i].LoadData(carteAvversario[i]);
        }
    }
    public void StartMatch() 
    {
        Debug.Log("Match Start!");

        combatPanel.SetInputsActive(true);
        allyWaitingForReroll = enemyWaitingForReroll = false;
        for (int i = 0; i < allyDices.Length; i++)
                allyDices[i].LockDice(false);
        for (int i = 0; i < enemyDices.Length; i++)
                enemyDices[i].LockDice(false);

        cartaSelNemicaIndex = Random.Range(0, 3);

        StartingDicesRoll(allyDices);
        StartingDicesRoll(enemyDices);
        
        if(enemyDiceSelectionCoroutine != null)
            StopCoroutine(enemyDiceSelectionCoroutine);
        enemyDiceSelectionCoroutine = StartCoroutine(EnemyDiceSelectionCoroutine());
    }
    private void StartingDicesRoll(Dice[] dices)
    {
        for (int i = 0; i < dices.Length; i++)
        {
            dices[i].RollDice();
        }
    }
    public void RerollDices_Ally()
    {
        allyWaitingForReroll = true;
        RerollDices();
    }
    private void RerollDices_enemy()
    {
        enemyWaitingForReroll = true;
        RerollDices();
    }
    private void RerollDices() 
    {
        if (!allyWaitingForReroll || !enemyWaitingForReroll)
            return;

        for (int i = 0; i < allyDices.Length; i++)
        {
            if (allyDices[i].IsLocked == false)
            {
                allyDices[i].RollDice();
                allyDices[i].LockDice(true);
            }
        }
        for (int i = 0; i < enemyDices.Length; i++)
        {
            if (enemyDices[i].IsLocked == false)
            {
                enemyDices[i].RollDice();
                enemyDices[i].LockDice(true);
            }
        }

        StartFightCheck();
    }
    private void StartFightCheck()
    {
        for(int i = 0; i < allyDices.Length; i++)
        {
            if (!allyDices[i].IsLocked || !enemyDices[i].IsLocked)
                return;
        }
        StartFight();
    }

    private void StartFight()
    {
        combatPanel.SetInputsActive(false);
        if(fightCoroutine != null)
            StopCoroutine(fightCoroutine);
        fightCoroutine = StartCoroutine(FightCoroutine());
    }
    private IEnumerator FightCoroutine()
    {
        Debug.Log("Fight!!");
        yield return new WaitForSeconds(3);
        tablePanel.gameObject.SetActive(true);
        combatPanel.gameObject.SetActive(false);
    }

    private IEnumerator EnemyDiceSelectionCoroutine()
    {
        //ENEMY AI HERE
        for (int i = 0; i < Random.Range(0,6); i++)
        {
            Debug.Log("Hmmmmm...");
            yield return new WaitForSeconds(1);
            enemyDices[i].LockDice(true);
        }
        RerollDices_enemy();
    }

}
