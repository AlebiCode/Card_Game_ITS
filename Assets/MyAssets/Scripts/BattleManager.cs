using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class BattleManager : MonoBehaviour
{

    private const int pointsNeededToWin = 2;

    public static BattleManager instance;

    [SerializeField] private EnemyBrain EnemyAI;

    [SerializeField] private TablePanel tablePanel;
    [SerializeField] private CombatPanel combatPanel;
    [SerializeField]private Card[] deckCarteNemiche = new Card[3];
    [SerializeField]private Card[] deckCarteAmiche = new Card[3];
    [SerializeField]private Card cartaSelezionataAmica;
    [SerializeField]private Card cartaSelezionataNemica;

    [SerializeField] private Dice[] allyDices = new Dice[6];
    [SerializeField] private Dice[] enemyDices = new Dice[6];

    private EnemyData enemyData;
    private bool allyWaitingForReroll;
    private bool enemyWaitingForReroll;
    private Coroutine fightCoroutine;
    private Coroutine enemyDiceSelectionCoroutine;

    private int playerScore = 0;
    private int enemyScore = 0;

    public UnityEvent<int> onPlayerScoreChanged = new();
    public UnityEvent<int> onEnemyScoreChanged = new();

    public int PlayerScore {
        get { return playerScore; } 
        set { playerScore = value;
            onPlayerScoreChanged.Invoke(playerScore);}
    }

    public int EnemyScore {
        get { return enemyScore; }
        set {
            enemyScore = value;
            onEnemyScoreChanged.Invoke(enemyScore);}
    }
    public Dice[] EnemyDices => enemyDices;
    public Dice[] AllyDices => allyDices;

    private void Awake()
    {
        instance = this;
    }

    private void OnEnable() {
        ResetScore();
    }

    public void LoadData(CardData[] carteProprie, EnemyData enemyData)
    {
        this.enemyData = enemyData;
        LoadDeckCardDatas(carteProprie, enemyData.EnemyCardDatas);
    }
    private void LoadDeckCardDatas(CardData[] carteProprie, CardData[] carteAvversario)
    {
        for (int i = 0; i < 3; i++)
        {
            deckCarteAmiche[i].LoadData(carteProprie[i]);
            deckCarteNemiche[i].LoadData(carteAvversario[i]);
        }
    }
    public void StartMatch() 
    {
        Debug.Log("Match Start!");
        StopAllCoroutines();

        combatPanel.SetInputsActive(true);
        allyWaitingForReroll = enemyWaitingForReroll = false;
        for (int i = 0; i < allyDices.Length; i++)
                allyDices[i].LockDice(false);
        for (int i = 0; i < enemyDices.Length; i++)
                enemyDices[i].LockDice(false);

        cartaSelezionataNemica.LoadData(deckCarteNemiche[Random.Range(0, 3)].CardData);
        cartaSelezionataAmica.LoadData(tablePanel.selectedCard.CardData);

        StartCoroutine(EnterCombatAnimations());

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
        UpdateWinnerScore(Random.Range(0,2)==0);
        tablePanel.gameObject.SetActive(true);
        combatPanel.gameObject.SetActive(false);
    }
    private IEnumerator EnemyDiceSelectionCoroutine()
    {
        //ENEMY AI HERE

        EnemyAI.TurnLoop();
    
        for (int i = 0; i < Random.Range(0,6); i++)
        {
            Debug.Log("Hmmmmm...");
            yield return new WaitForSeconds(1);
            //enemyDices[i].LockDice(true);
        }
        
        RerollDices_enemy();
    }

    private void ResetScore() {
        EnemyScore = 0;
        PlayerScore = 0;
    }

    private void UpdateWinnerScore(bool isPlayerTheRoundWinner) {

        if (isPlayerTheRoundWinner) {
            PlayerScore++;
            if(playerScore >= pointsNeededToWin) {
                StartCoroutine(WinBattle());
            }
        } else {
            EnemyScore++;
            if(playerScore< pointsNeededToWin) {
                StartCoroutine(LoseBattle());
            }
        }
    }

    private IEnumerator WinBattle() {
        yield return null;
        Debug.Log("Hai Vinto");
        ResetScore();
    }

    private IEnumerator LoseBattle() {
        yield return null;
        Debug.Log("Hai perso");
        ResetScore();
    }

    private IEnumerator EnterCombatAnimations()
    {
        float duration = cartaSelezionataAmica.EnterCombatSceneAnim();
        yield return new WaitForSeconds(duration); 
        cartaSelezionataNemica.EnterCombatSceneAnim();
    }

}
