using DG.Tweening;
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

    [SerializeField] private GameObject lockAndRollButton;
    [SerializeField] private TablePanel tablePanel;
    [SerializeField] private CombatPanel combatPanel;
    [SerializeField] private Card[] deckCarteNemiche = new Card[3];
    [SerializeField] private Card[] deckCarteAmiche = new Card[3];
    [SerializeField] private Card allyCombatCard;
    [SerializeField] private Card enemyCombatCard;

    [SerializeField] private Vector3 diceAnimationOffset = Vector3.left * 1;
    [SerializeField] private Dice[] allyDices = new Dice[6];
    [SerializeField] private Dice[] enemyDices = new Dice[6];

    private List<int> enemySelectedCardIndexList = new List<int> { 0, 1, 2 };
    private int enemySelectedCardIndex;
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
        public int successfullBlocks;
        public int successfullDodges;
    }

    public UnityEvent<int> onPlayerScoreChanged = new();
    public UnityEvent<int> onEnemyScoreChanged = new();
    public UnityEvent<int> onPlayerCardDamaged = new();
    public UnityEvent<int> onEnemyCardDamaged = new();

    public int PlayerScore
    {
        get { return playerScore; }
        set
        {
            playerScore = value;
            onPlayerScoreChanged?.Invoke(playerScore);
        }
    }

    public int EnemyScore
    {
        get { return enemyScore; }
        set
        {
            enemyScore = value;
            onEnemyScoreChanged?.Invoke(enemyScore);
        }
    }
    public Dice[] EnemyDices => enemyDices;
    public Dice[] AllyDices => allyDices;
    public Card EnemySelectedCard => deckCarteNemiche[enemySelectedCardIndex];

    private void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
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
        combatPanel.gameObject.SetActive(true);
        combatPanel.SetInputsActive(false);
        lockAndRollButton.SetActive(false);

        playerFightData = new FightData();
        enemyFightData = new FightData();

        allyWaitingForReroll = enemyWaitingForReroll = false;
        for (int i = 0; i < allyDices.Length; i++)
        {
            allyDices[i].LockDice(false);
            allyDices[i].ResetColor();
            enemyDices[i].LockDice(false);
            enemyDices[i].ResetColor();
        }
        
        enemySelectedCardIndex = ChooseEnemyCardIndex();
        //enemySelectedCardIndex = Random.Range(0, 3);

        enemyCombatCard.LoadData(deckCarteNemiche[enemySelectedCardIndex].CardData);
        allyCombatCard.LoadData(tablePanel.selectedCard.CardData);

        StopAllCoroutines();
        StartCoroutine(StartMatch_Coroutine());
    }

    public int ChooseEnemyCardIndex()
    {
        int randomIndex = Random.Range(0, enemySelectedCardIndexList.Count);
        int enemyCardIndex = enemySelectedCardIndexList[randomIndex];
        enemySelectedCardIndexList.RemoveAt(randomIndex);
        return enemyCardIndex;
    }

    private IEnumerator StartMatch_Coroutine()
    {
        StartingDicesRoll(allyDices);
        StartingDicesRoll(enemyDices);

        yield return EnterCombatAnimations();
        yield return RollDices(1.5f);

        combatPanel.SetInputsActive(true);
        lockAndRollButton.SetActive(true);
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
        combatPanel.SetInputsActive(false);
        lockAndRollButton.SetActive(false);

        if (allyWaitingForReroll && enemyWaitingForReroll)
            StartCoroutine(RerollAndFight());
    }
    private void RerollDices_enemy()
    {
        enemyWaitingForReroll = true;

        if (allyWaitingForReroll && enemyWaitingForReroll)
            StartCoroutine(RerollAndFight());
    }
    private IEnumerator RollDices(float rollingTime)
    {
        AudioManager.StartDiceRollLoop();

        for (int i = 0; i < allyDices.Length; i++)
        {
            if (!allyDices[i].IsLocked)
            {
                allyDices[i].StartRollAnimation(1000, 1);
            }
            if (!enemyDices[i].IsLocked)
            {
                enemyDices[i].StartRollAnimation(1000, 1);
            }
        }

        yield return new WaitForSeconds(rollingTime);

        AudioManager.StopDiceRollLoop();
        for (int i = 0; i < allyDices.Length; i++)
        {
            if (!allyDices[i].IsLocked)
            {
                allyDices[i].RollDice();
                allyDices[i].StopRollAnimation(1);
            }
            if (!enemyDices[i].IsLocked)
            {
                enemyDices[i].RollDice();
                enemyDices[i].StopRollAnimation(1);
            }
        }
    }
    private IEnumerator RerollAndFight()
    {
        yield return RollDices(2);

        for (int i = 0; i < allyDices.Length; i++)
        {
            if (allyDices[i].IsLocked == false)
            {
                allyDices[i].LockDice(true);
            }
            if (enemyDices[i].IsLocked == false)
            {
                enemyDices[i].LockDice(true);
            }
        }

        yield return new WaitForSeconds(2);

        StartFight();
    }
    private void StartFightCheck()
    {
        for (int i = 0; i < allyDices.Length; i++)
        {
            if (!allyDices[i].IsLocked || !enemyDices[i].IsLocked)
                return;
        }
        StartFight();
    }

    private void StartFight()
    {
        if (fightCoroutine != null)
            StopCoroutine(fightCoroutine);
        fightCoroutine = StartCoroutine(FightCoroutine());
    }
    private IEnumerator FightCoroutine()
    {
        Debug.Log("Fight!!");

        List<SkillData> allySkills = SkillChecker(allyCombatCard, allyDices);
        List<SkillData> enemySkills = SkillChecker(enemyCombatCard, enemyDices);
        ExecuteSkillsDefences(allySkills, true);
        ExecuteSkillsDefences(enemySkills, false);

        string debugText = "";
        foreach (SkillData skill in allySkills)
            debugText += '(' + skill.name + ") ";
        Debug.Log("Ally used skills:\n" + debugText);
        debugText = "";
        foreach (SkillData skill in enemySkills)
            debugText += '(' + skill.name + ") ";
        Debug.Log("Enemy used skills:\n" + debugText);
        Debug.Log("Player <Parry Instances: " + playerFightData.parryIteration + "><Dodge Chance: " + playerFightData.dodgePercent + ">");
        Debug.Log("Enemy <Parry Instances: " + enemyFightData.parryIteration + "><Dodge Chance: " + enemyFightData.dodgePercent + ">");

        ExecuteSkillsAttacks(allySkills, true);
        ExecuteSkillsAttacks(enemySkills, false);
        Debug.Log("Player <Damage Taken: " + playerFightData.damageTaken + "><Attacks Blocked: " + playerFightData.successfullBlocks + "><Attacks Dodged: " + playerFightData.successfullDodges + ">");
        Debug.Log("Enemy <Damage Taken: " + enemyFightData.damageTaken + "><Attacks Blocked: " + enemyFightData.successfullBlocks + "><Attacks Dodged: " + enemyFightData.successfullDodges + ">");
        yield return new WaitForSeconds(3);

        UpdateWinnerScore();
        tablePanel.gameObject.SetActive(true);
        combatPanel.gameObject.SetActive(false);
        TablePanel.instance.CombatPanelToTablePanelAnimation();
    }
    private IEnumerator EnemyDiceSelectionCoroutine()
    {
        //ENEMY AI HERE

        EnemyAI.TurnLoop();
        /*
        for (int i = 0; i < Random.Range(2, 6); i++)
        {
            yield return new WaitForSeconds(1);
            enemyDices[i].LockDice(true);
        }
        */
        yield return new WaitForSeconds(1);
        RerollDices_enemy();
    }

    #region Skill check, calculation and execution

    private List<SkillData> SkillChecker(Card card, Dice[] dices)
    {
        int skillIterations = 0; //Questa variabile è utile solo per la grafica
        List<Dice.diceFace> diceResult = new List<Dice.diceFace>();
        List<SkillData> skillToExec = new List<SkillData>();
        bool isAlly = card == allyCombatCard;

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
        }
    }
    private void ExecuteSkillsAttacks(List<SkillData> skillsToExec, bool isAlly)
    {
        foreach (SkillData skill in skillsToExec)
        {
            if (isAlly)
                CalculateSkillAttack(skill, ref enemyFightData, isAlly);
            else
                CalculateSkillAttack(skill, ref playerFightData, isAlly);
        }
    }

    private void CalculateSkillDefence(SkillData skillToCalc, ref FightData fightData)
    {
        fightData.parryIteration += skillToCalc.DefInstances;
        fightData.dodgePercent += skillToCalc.Dodge;
    }

    private void CalculateSkillAttack(SkillData skillToCalc, ref FightData defenderFightData, bool isAlly)
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
                    if (isAlly)
                        onEnemyCardDamaged?.Invoke(skillToCalc.Damage);
                    else
                        onPlayerCardDamaged?.Invoke(skillToCalc.Damage);
                }
                else
                {
                    //Dodged
                    defenderFightData.successfullDodges++;
                }
            }
            else
            {
                //Blocked
                defenderFightData.successfullBlocks++;
                defenderFightData.parryIteration--;
            }
        }
    }

    #endregion

    private void ResetScore()
    {
        EnemyScore = 0;
        PlayerScore = 0;
    }

    private void UpdateWinnerScore()
    {

        if (playerFightData.damageTaken <= enemyFightData.damageTaken)
        {
            PlayerScore++;
            if (playerScore >= pointsNeededToWin)
            {
                StartCoroutine(EndBattle(true));
            }
        }
        else if (playerFightData.damageTaken > enemyFightData.damageTaken)
        {
            EnemyScore++;
            if (enemyScore >= pointsNeededToWin)
            {
                StartCoroutine(EndBattle(false));
            }
        }
        else
        {
            //in parità, vince il player ^^^^^^^^^^^^^
        }
        Debug.Log("Score: " + playerScore + "-" + enemyScore);
    }

    private IEnumerator EndBattle(bool hasPlayerWon)
    {
        Debug.Log("Partita Conclusa. " + (hasPlayerWon ? "Player won." : "Player lost."));
        yield return new WaitForSeconds(2);
        ResetScore();
        SceneManager.LoadScene(0);
    }


    private IEnumerator EnterCombatAnimations()
    {
        for (int i = 0; i < allyDices.Length; i++)
        {
            allyDices[i].transform.position += diceAnimationOffset;
            enemyDices[i].transform.position -= diceAnimationOffset;
        }

        allyCombatCard.gameObject.SetActive(false);
        enemyCombatCard.gameObject.SetActive(false);

        float duration = allyCombatCard.EnterCombatSceneAnim();
        StartCoroutine(tablePanel.ScaleAndMoveMyCard(allyCombatCard.gameObject));
        StartCoroutine(AnimateEnteringDicesAlly(0.5f));
        yield return new WaitForSeconds(duration);
        duration = enemyCombatCard.EnterCombatSceneAnim();
        StartCoroutine(tablePanel.ScaleAndMoveEnemyCard(enemyCombatCard.gameObject));
        StartCoroutine(AnimateEnteringDicesEnemy(0.5f));
        yield return new WaitForSeconds(duration);
    }
    private IEnumerator AnimateEnteringDicesAlly(float singleDiceEnteringDuration)
    {
        for (int i = 0; i < allyDices.Length; i++)
        {
            allyDices[i].gameObject.SetActive(true);
            Vector3 targetPos = allyDices[i].transform.position - diceAnimationOffset;
            allyDices[i].transform.DOMove(targetPos, singleDiceEnteringDuration);
            while (allyDices[i].transform.position != targetPos)
            {
                yield return null;
            }
        }
    }
    private IEnumerator AnimateEnteringDicesEnemy(float diceDuration)
    {
        for (int i = 0; i < enemyDices.Length; i++)
        {
            enemyDices[i].gameObject.SetActive(true);
            Vector3 targetPos = enemyDices[i].transform.position + diceAnimationOffset;
            enemyDices[i].transform.DOMove(targetPos, diceDuration);
            while (enemyDices[i].transform.position != targetPos)
            {
                yield return null;
            }
        }
    }

}
