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

    [SerializeField] private GameObject lockAndRollButton;
    [SerializeField] private GameObject battlePanel;
    [SerializeField] private TablePanel tablePanel;
    [SerializeField] private CombatPanel combatPanel;
    [SerializeField] private EndPanel endPanel;
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

    private int currentRound = 1;
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

    public int CurrentRound => currentRound;

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
        Debug.Log("------Fight Preparation Start------");
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
        //enemySelectedCardIndex = Random.Range(0, 3);

        enemyCombatCard.LoadData(EnemySelectedCard.CardData);
        allyCombatCard.LoadData(tablePanel.selectedCard.CardData);

        StopAllCoroutines();
        StartCoroutine(StartMatch_Coroutine());
    }

    public int ChooseEnemyCardIndex()
    {
        int randomIndex = Random.Range(0, enemySelectedCardIndexList.Count);
        int enemyCardIndex = enemySelectedCardIndexList[randomIndex];
        enemySelectedCardIndexList.RemoveAt(randomIndex);
        Debug.Log("enemy card index -----> "+enemyCardIndex);
        return enemyCardIndex;
    }

    private IEnumerator StartMatch_Coroutine()
    {
        allyCombatCard.BuffsManager.ResetStats();
        enemyCombatCard.BuffsManager.ResetStats();
        playerFightData.damageTaken = 0;
        enemyFightData.damageTaken = 0;
        
        onEnemyCardDamaged?.Invoke(enemyFightData.damageTaken);
        onPlayerCardDamaged?.Invoke(playerFightData.damageTaken);

        yield return EnterCombatAnimations();
        yield return RollDices(1.5f, false);

        combatPanel.SetInputsActive(true);
        lockAndRollButton.SetActive(true);
        StartCoroutine(EnemyDiceSelectionCoroutine());
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
    private IEnumerator RollDices(float rollingTime, bool lockAfterRoll = false)
    {
        bool noDiceRolled = true;
        for (int i = 0; i < allyDices.Length; i++)
        {
            if (!allyDices[i].IsLocked)
            {
                allyDices[i].StartRollAnimation(1);
                noDiceRolled = false;
            }
            if (!enemyDices[i].IsLocked)
            {
                enemyDices[i].StartRollAnimation(1);
                noDiceRolled = false;
            }
        }
        if(noDiceRolled)
            yield break;

        AudioManager.StartDiceRollLoop();

        yield return new WaitForSeconds(rollingTime);

        AudioManager.StopDiceRollLoop();
        for (int i = 0; i < allyDices.Length; i++)
        {
            if (!allyDices[i].IsLocked)
            {
                allyDices[i].RollDice();
                allyDices[i].StopRollAnimation();
                if(lockAfterRoll)
                    allyDices[i].LockDice(true);
            }
            if (!enemyDices[i].IsLocked)
            {
                enemyDices[i].RollDice();
                enemyDices[i].StopRollAnimation();
                if (lockAfterRoll)
                    enemyDices[i].LockDice(true);
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
    private IEnumerator RerollAndFight()
    {
        yield return RollDices(2, true);

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
    private IEnumerator FightCoroutine() {

        Debug.Log("------Fight------");
        List<SkillData> allySkills = SkillChecker(allyCombatCard, allyDices);
        List<SkillData> enemySkills = SkillChecker(enemyCombatCard, enemyDices);
        Debug.Log("Execute Player Defense");
        yield return StartCoroutine(ExecuteSkillsDefences(allySkills, true));
        Debug.Log("Player Defense Executed");
        Debug.Log("Execute Enemy Defense");
        yield return StartCoroutine(ExecuteSkillsDefences(enemySkills, false));
        Debug.Log("Enemy Defense Executed");
        MoveDebugger(allySkills, enemySkills);

        yield return StartCoroutine(ExecuteSkillsAttacks(allySkills, true));
        yield return StartCoroutine(ExecuteSkillsAttacks(enemySkills, false));
        Debug.Log("Player <Damage Taken: " + playerFightData.damageTaken + "><Attacks Blocked: " + playerFightData.successfullBlocks + "><Attacks Dodged: " + playerFightData.successfullDodges + ">");
        Debug.Log("Enemy <Damage Taken: " + enemyFightData.damageTaken + "><Attacks Blocked: " + enemyFightData.successfullBlocks + "><Attacks Dodged: " + enemyFightData.successfullDodges + ">");
        yield return new WaitForSeconds(3);

        UpdateWinnerScore();
        Debug.Log("Score: " + playerScore + "-" + enemyScore + "\n------Fight Over------");

    }
    private IEnumerator EnemyDiceSelectionCoroutine()
    {
        //ENEMY AI HERE

        EnemyBrain.Instance.AI_Loop();
        yield return StartCoroutine(EnemyBrain.Instance.EnemyDiceLockingCoroutine(enemyDices));
        RerollDices_enemy();

    }

    private void MoveDebugger(List<SkillData> allySkills, List<SkillData> enemySkills)
    {
        string skillsUsed = "";
        string attackInstances = "";
        foreach (SkillData skill in allySkills)
        {
            skillsUsed += '(' + skill.name + ") ";
            if(skill.AtkInstances > 0)
                attackInstances += '(' + skill.Damage.ToString() + "x" + skill.AtkInstances + ") ";
        }
        Debug.Log("Ally used skills:" + skillsUsed + "\n"
            + "Player <Parry Instances: " + playerFightData.parryIteration + "><Dodge Chance: " + playerFightData.dodgePercent + "><Attacks: " + attackInstances + ">");
        skillsUsed = attackInstances = "";
        foreach (SkillData skill in enemySkills)
        {
            skillsUsed += '(' + skill.name + ") ";
            if (skill.AtkInstances > 0)
                attackInstances += '(' + skill.Damage.ToString() + "x" + skill.AtkInstances + ") ";
        }
        Debug.Log("Enemy used skills:" + skillsUsed + "\n"
            + "Enemy <Parry Instances: " + enemyFightData.parryIteration + "><Dodge Chance: " + enemyFightData.dodgePercent + "><Attacks: " + attackInstances + ">");
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
    private IEnumerator ExecuteSkillsDefences(List<SkillData> skillsToExec, bool isAlly)
    {
        foreach (SkillData skill in skillsToExec)
        {
            yield return new WaitForSeconds(0.5f);

            if (isAlly)
                CalculateSkillDefence(skill, ref playerFightData,isAlly);
            else
                CalculateSkillDefence(skill, ref enemyFightData,isAlly);
           
        }
    }
    private IEnumerator ExecuteSkillsAttacks(List<SkillData> skillsToExec, bool isAlly)
    {
        foreach (SkillData skill in skillsToExec)
        {
           
            for (int i = 0; i < skill.AtkInstances; i++) {

                yield return new WaitForSeconds(0.5f);

                if (isAlly)
                    CalculateSkillAttack(skill, ref enemyFightData, isAlly);
                else
                    CalculateSkillAttack(skill, ref playerFightData, isAlly);
            }
            
        }
    }

    private void CalculateSkillDefence(SkillData skillToCalc, ref FightData fightData, bool isAlly)
    {
        fightData.parryIteration += skillToCalc.DefInstances;
        fightData.dodgePercent += skillToCalc.Dodge;

        UpdateParryCount((isAlly ? allyCombatCard : enemyCombatCard), fightData.parryIteration);
        UpdateDodgeChance((isAlly ? allyCombatCard : enemyCombatCard), fightData.dodgePercent);

    }

    private void CalculateSkillAttack(SkillData skillToCalc, ref FightData defenderFightData, bool isAlly)
    {
        
            if (defenderFightData.parryIteration == 0)
            {
                if (skillToCalc.Precise || Random.Range(1, 101) > defenderFightData.dodgePercent)
                {//hit
                    defenderFightData.damageTaken += skillToCalc.Damage;
                    AudioManager.PlayCombatAttackHit();
                    if (isAlly) {
                        PlayVFX(enemyCombatCard, VFX_TYPE.ATTACK); //attack means get damaged
                        onEnemyCardDamaged?.Invoke(defenderFightData.damageTaken);
                    }
                    else {
                        PlayVFX(allyCombatCard, VFX_TYPE.ATTACK); //attack means get damaged
                        onPlayerCardDamaged?.Invoke(defenderFightData.damageTaken);
                    }
                }
                else
                {//dodge
                    AudioManager.PlayCombatAttackDodged();
                    PlayVFX((isAlly ? enemyCombatCard : allyCombatCard), VFX_TYPE.DODGE);
                    defenderFightData.successfullDodges++;
                    
                }
            }
            else
            {//block
                AudioManager.PlayCombatAttackBlocked();
                PlayVFX((isAlly ? enemyCombatCard : allyCombatCard), VFX_TYPE.DEFENSE);
                defenderFightData.successfullBlocks++;
                defenderFightData.parryIteration--;
                UpdateParryCount((isAlly?enemyCombatCard:allyCombatCard),defenderFightData.parryIteration);
            }
        
    }

    void UpdateParryCount(Card card, int parryCount) {
        card.BuffsManager.UpdateParryCount(parryCount);
    }

    void UpdateDodgeChance(Card card, int dodgeChance) {
        card.BuffsManager.UpdateDodgeChance(dodgeChance);
    }

    void PlayVFX(Card card, VFX_TYPE type) {
        Debug.Log("Activating effect " + type + " fo " + card.name + " of race " + card.CardData.Race);
        card.particleSystemList.ActivateAnimation(type, card.CardData.Race);
    }

    #endregion

    private void ResetScore()
    {
        enemyScore = 0;
        playerScore = 0;
    }

    private void UpdateWinnerScore()
    {

        if (playerFightData.damageTaken <= enemyFightData.damageTaken)
        {
            PlayerScore++;
            if (playerScore >= pointsNeededToWin)
            {
                StartCoroutine(EndBattle(true));
                return;
            }
        }
        else if (playerFightData.damageTaken > enemyFightData.damageTaken)
        {
            EnemyScore++;
            if (enemyScore >= pointsNeededToWin)
            {
                StartCoroutine(EndBattle(false));
                return;
            }
        }

        //if (currentRound == 3)
        //    return;

        currentRound++;
        tablePanel.RenterPanel();
        combatPanel.gameObject.SetActive(false);
        TablePanel.instance.CombatPanelToTablePanelAnimation();

    }

    private IEnumerator EndBattle(bool hasPlayerWon)
    {
        Debug.Log("Partita Conclusa. " + (hasPlayerWon ? "Player won." : "Player lost."));
        this.GetComponent<AudioSource>().Stop();
        ResetScore();
        endPanel.gameObject.SetActive(true);
        tablePanel.gameObject.SetActive(false);
        combatPanel.gameObject.SetActive(false);
        EndPanel.instance.ShowWinText(hasPlayerWon);
        yield return new WaitForSeconds(4);
        //EndPanel.instance.ResetEndPanel();
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

    internal void SelectEnemyCard() {
        enemySelectedCardIndex = ChooseEnemyCardIndex();
    }
}
