using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

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
    private int playerDamageTaken = 0;
    private int playerParryIteration = 0;
    private int playerDodgePercent = 0;
    private int enemyScore = 0;
    private int enemyDamageTaken = 0;
    private int enemyParryIteration = 0;
    private int enemyDodgePercent = 0;

    public UnityEvent<int> onPlayerScoreChanged = new();
    public UnityEvent<int> onEnemyScoreChanged = new();

    public int PlayerScore {
        get { return playerScore; } 
        set { playerScore = value;
            onPlayerScoreChanged?.Invoke(playerScore);}
    }

    public int EnemyScore {
        get { return enemyScore; }
        set {
            enemyScore = value;
            onEnemyScoreChanged?.Invoke(enemyScore);}
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

        combatPanel.SetInputsActive(true);
        allyWaitingForReroll = enemyWaitingForReroll = false;
        for (int i = 0; i < allyDices.Length; i++)
                allyDices[i].LockDice(false);
        for (int i = 0; i < enemyDices.Length; i++)
                enemyDices[i].LockDice(false);

        cartaSelezionataNemica.LoadData(deckCarteNemiche[Random.Range(0, 3)].CardData);
        cartaSelezionataAmica.LoadData(tablePanel.selectedCard.CardData);

        StopAllCoroutines();
        StartCoroutine(StartMatch_Coroutine());
    }
    private IEnumerator StartMatch_Coroutine()
    {
        StartingDicesRoll(allyDices);
        StartingDicesRoll(enemyDices);
        
        yield return EnterCombatAnimations();

        StartCoroutine(EnemyDiceSelectionCoroutine());
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
        AudioManager.StartDiceRollLoop();
        if (!allyWaitingForReroll || !enemyWaitingForReroll)
            return;
        AudioManager.StopDiceRollLoop();

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

    private void SkillChecker(Card card, Dice[] dices)
    {
        int skillIterations = 0; //Questa variabile è utile solo per la grafica
        List<Dice.diceFace> diceResult = new List<Dice.diceFace>();
        List<SkillData> skillToExec = new List<SkillData>();
        bool isAlly = card == cartaSelezionataAmica ? true : false;

        foreach (var dice in dices)
        {
            diceResult.Add(dice.Result);
        }

        foreach (var skill in card.CardData.Skills)
        {
            List<Dice.diceFace> personalDiceResults = new List<Dice.diceFace>(diceResult);
            CheckRequisites(personalDiceResults, skill.Skill_colorCost, skill, skillToExec, isAlly, skillIterations);
        }
    }

    private void CheckRequisites(List<Dice.diceFace> diceResults, List<Dice.diceFace> skillReq, SkillData skill, List<SkillData> skillToExec, bool isAlly, int skilliterations)
    {
        for (int i = 0; i < skillReq.Count; i++)
        {
            Dice.diceFace req = skillReq[i];
            if (diceResults.Contains(req))
            {
                diceResults.Remove(req);
                if (i == skillReq.Count - 1)
                {
                    //Siamo all'ultimo requisito, ergo la skill riesce
                    skilliterations++;
                    skillToExec.Add(skill);
                    CheckRequisites(diceResults, skillReq, skill, skillToExec, isAlly, skilliterations);

                }
            }
            else
            {
                Debug.Log("Skill Fallita");
            }
        }
    }
    private void ExecuteSkills(List<SkillData> skillsToExec, bool isAlly)
    {
        if (isAlly)
        {
            foreach (SkillData skill in skillsToExec)
            {
                CalculateSkill(skill, playerParryIteration, playerDodgePercent, enemyDamageTaken, enemyParryIteration, enemyDodgePercent);
            }
        }
        else
        {
            foreach (SkillData skill in skillsToExec)
            {
                CalculateSkill(skill, enemyParryIteration, enemyDodgePercent, playerDamageTaken, playerParryIteration, playerDodgePercent);
            }
        }
    }
    private void CalculateSkill(SkillData skillToCalc, int personalParryIteration, int personalDodgePercent, int adversaryDamageTaken, int adversaryParryIteration, int adversaryDodgePercent)
    {
        personalParryIteration += skillToCalc.DefInstances;
        personalDodgePercent += skillToCalc.Dodge;

        if (skillToCalc.Damage > 0)
        {
            if (adversaryParryIteration >= skillToCalc.AtkInstances)
            {
                for (int i = 0; i < skillToCalc.AtkInstances; i++)
                {
                    adversaryParryIteration--;
                }
            }

            if (adversaryParryIteration < skillToCalc.AtkInstances)
            {
                int remainingInstaces = skillToCalc.AtkInstances - adversaryParryIteration;
                while (adversaryParryIteration > 0)
                {
                    adversaryParryIteration--;
                }

                if (adversaryDodgePercent > 0)
                {
                    for (int i = 0; i < remainingInstaces; i++)
                    {
                        int randomChance = Random.Range(0, 100);
                        if (randomChance > adversaryDodgePercent)
                        {
                            adversaryDamageTaken += skillToCalc.Damage;
                        }
                        else
                        {

                        }
                    }
                }
                else
                {
                    for (int i = 0; i < remainingInstaces; i++)
                    {
                        adversaryDamageTaken += skillToCalc.Damage;
                    }
                }
            }

        }
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

       // EnemyAI.TurnLoop();
    
        for (int i = 0; i < Random.Range(2,6); i++)
        {
            Debug.Log("Hmmmmm...");
            yield return new WaitForSeconds(1);
            enemyDices[i].LockDice(true);
        }

        yield return new WaitForSeconds(1);
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
                StartCoroutine(EndBattle(true));
            }
        } else {
            EnemyScore++;
            if(enemyScore>= pointsNeededToWin) {
                StartCoroutine(EndBattle(false));
            }
        }
    }

    private IEnumerator EndBattle(bool hasPlayerWon) {
        Debug.Log("Partita Conclusa");
        yield return new WaitForSeconds(2);
        ResetScore();
        SceneManager.LoadScene(0);
    }


    private IEnumerator EnterCombatAnimations()
    {
        yield return cartaSelezionataAmica.EnterCombatSceneAnim();
        yield return cartaSelezionataNemica.EnterCombatSceneAnim();
    }

}
