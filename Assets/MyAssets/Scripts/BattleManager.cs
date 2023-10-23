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

    private int playerScore;
    private int enemyScore;
    private FightData playerFightData;
    private FightData enemyFightData;
    private struct FightData
    {
        public int damageTaken;
        public int parryIteration;
        public int dodgePercent;
    }

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

        playerFightData = new FightData();
        enemyFightData = new FightData();

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
    private IEnumerator FightCoroutine()
    {
        Debug.Log("Fight!!");
        List<SkillData> allySkills = SkillChecker(cartaSelezionataAmica, allyDices);
        List<SkillData> enemySkills = SkillChecker(cartaSelezionataNemica, enemyDices);
        ExecuteSkillsDefences(allySkills, true);
        ExecuteSkillsDefences(enemySkills, false);
        Debug.Log("Player <Parry: " + playerFightData.parryIteration + "><Dodge: " + playerFightData.dodgePercent + ">");
        Debug.Log("Enemy <Parry: " + enemyFightData.parryIteration + "><Dodge: " + enemyFightData.dodgePercent + ">");
        ExecuteSkillsAttacks(allySkills, true);
        ExecuteSkillsAttacks(enemySkills, false);
        Debug.Log("Player <Damage Taken: " + playerFightData.damageTaken + ">");
        Debug.Log("Enemy <Damage Taken: " + enemyFightData.damageTaken + ">");
        yield return new WaitForSeconds(3);
        UpdateWinnerScore(playerFightData.damageTaken <= enemyFightData.damageTaken);
        tablePanel.gameObject.SetActive(true);
        combatPanel.gameObject.SetActive(false);
    }
    private IEnumerator EnemyDiceSelectionCoroutine()
    {
        //ENEMY AI HERE

       // EnemyAI.TurnLoop();
    
        for (int i = 0; i < Random.Range(2,6); i++)
        {
            yield return new WaitForSeconds(1);
            enemyDices[i].LockDice(true);
        }

        yield return new WaitForSeconds(1);
        RerollDices_enemy();
    }

    #region Skill check, calculation and execution

    private List<SkillData> SkillChecker(Card card, Dice[] dices)
    {
        int skillIterations = 0; //Questa variabile è utile solo per la grafica
        List<Dice.diceFace> diceResult = new List<Dice.diceFace>();
        List<SkillData> skillToExec = new List<SkillData>();
        bool isAlly = card == cartaSelezionataAmica;

        foreach (var dice in dices)
        {
            diceResult.Add(dice.Result);
        }

        foreach (var skill in card.CardData.Skills)
        {
            List<Dice.diceFace> personalDiceResults = new List<Dice.diceFace>(diceResult);
            CheckRequisites(personalDiceResults, skill.Skill_colorCost, skill, skillToExec, isAlly, skillIterations);
        }
        return skillToExec;
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
                    //Debug.Log((isAlly ? "Ally" : "Enemy") + " can use skill " + skill.name);
                    //SFX
                    CheckRequisites(diceResults, skillReq, skill, skillToExec, isAlly, skilliterations);
                }
            }
            else
            {
                //Debug.Log("Skill Fallita");
                return;
            }
        }
    }
    private void ExecuteSkillsDefences(List<SkillData> skillsToExec, bool isAlly)
    {
        foreach (SkillData skill in skillsToExec)
        {
            if (isAlly)
                CalculateSkillDefence(skill, ref playerFightData);
            else
                CalculateSkillDefence(skill, ref enemyFightData);

            Debug.Log((isAlly ? "Ally" : "Enemy") + " uses skill " + skill.name);
        }
    }
    private void ExecuteSkillsAttacks(List<SkillData> skillsToExec, bool isAlly)
    {
        foreach (SkillData skill in skillsToExec)
        {
            if (isAlly)
                CalculateSkillAttack(skill, ref playerFightData);
            else
                CalculateSkillAttack(skill, ref enemyFightData);

            Debug.Log((isAlly ? "Ally" : "Enemy") + " uses skill " + skill.name);
        }
    }

    private void CalculateSkillDefence(SkillData skillToCalc, ref FightData fightData)
    {
        fightData.parryIteration += skillToCalc.DefInstances;
        fightData.dodgePercent += skillToCalc.Dodge;
    }

    private void CalculateSkillAttack(SkillData skillToCalc, ref FightData defenderFightData)
    {
        for (int i = 0; i < skillToCalc.AtkInstances; i++)
        {
            if (defenderFightData.parryIteration == 0)
            {
                int randomChance = Random.Range(1, 101);
                if (randomChance > defenderFightData.dodgePercent)
                {
                    defenderFightData.damageTaken += skillToCalc.Damage;
                    //Hit
                }
                else
                {
                    //Dodged
                }
            }
            else
            {
                //Blocked
                defenderFightData.parryIteration--;
            }
        }
    }
    
    #endregion

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
        Debug.Log("Score: " + playerScore + "-" + enemyScore);
    }

    private IEnumerator EndBattle(bool hasPlayerWon) {
        Debug.Log("Partita Conclusa. " + (hasPlayerWon ? "Player won." : "Player lost."));
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
