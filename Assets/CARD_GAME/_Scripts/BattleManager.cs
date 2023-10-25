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
    [SerializeField] private Card cartaSelezionataAmica;
    [SerializeField] private Card cartaSelezionataNemica;

    [SerializeField] private Vector3 diceAnimationOffset = Vector3.left * 1;
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
        lockAndRollButton.SetActive(false);

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
        yield return RollDices(3);

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
                allyDices[i].StartRollAnimation();
            }
            if (!enemyDices[i].IsLocked)
            {
                enemyDices[i].StartRollAnimation();
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
        combatPanel.SetInputsActive(false);
        if (fightCoroutine != null)
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
        TablePanel.instance.CombatPanelToTablePanelAnimation();
    }
    private IEnumerator EnemyDiceSelectionCoroutine()
    {
        //ENEMY AI HERE

        // EnemyAI.TurnLoop();

        for (int i = 0; i < Random.Range(2, 6); i++)
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
                CalculateSkillAttack(skill, ref enemyFightData, isAlly);
            else
                CalculateSkillAttack(skill, ref playerFightData, isAlly);

            Debug.Log((isAlly ? "Ally" : "Enemy") + " uses skill " + skill.name);
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

    private void ResetScore()
    {
        EnemyScore = 0;
        PlayerScore = 0;
    }

    private void UpdateWinnerScore(bool isPlayerTheRoundWinner)
    {

        if (isPlayerTheRoundWinner)
        {
            PlayerScore++;
            if (playerScore >= pointsNeededToWin)
            {
                StartCoroutine(EndBattle(true));
            }
        }
        else
        {
            EnemyScore++;
            if (enemyScore >= pointsNeededToWin)
            {
                StartCoroutine(EndBattle(false));
            }
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
            allyDices[i].transform.position -= diceAnimationOffset;
            enemyDices[i].transform.position += diceAnimationOffset;
        }
        float duration = cartaSelezionataAmica.EnterCombatSceneAnim();
        StartCoroutine(AnimateEnteringDicesAlly(0.5f));
        yield return new WaitForSeconds(duration);
        duration = cartaSelezionataNemica.EnterCombatSceneAnim();
        StartCoroutine(AnimateEnteringDicesEnemy(0.5f));
        yield return new WaitForSeconds(duration);
    }
    private IEnumerator AnimateEnteringDicesAlly(float diceDuration)
    {
        for (int i = 0; i < allyDices.Length; i++)
        {
            allyDices[i].gameObject.SetActive(true);
            Vector3 targetPos = allyDices[i].transform.position + diceAnimationOffset;
            allyDices[i].transform.DOMove(targetPos, diceDuration);
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
            Vector3 targetPos = enemyDices[i].transform.position - diceAnimationOffset;
            enemyDices[i].transform.DOMove(targetPos, diceDuration);
            while (enemyDices[i].transform.position != targetPos)
            {
                yield return null;
            }
        }
    }

}
