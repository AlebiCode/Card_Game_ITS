using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyBrain : MonoBehaviour
{
    public static EnemyBrain Instance { get; set; }

    [Header("DICES ROLL OUTCOME")]
    [SerializeField] private Dice[] diceRolled;
    [SerializeField] private Dice.diceFace[] diceRolledFaces;
    [SerializeField] private Dice[] diceToCopy;

    [SerializeField] private int diceRoll_RedMana = 0;
    [SerializeField] private int diceRoll_YellowMana = 0;
    [SerializeField] private int diceRoll_BlueMana = 0;
    [SerializeField] private int[] diceRoll_byManaColor;

    [Header("CARD")]
    [SerializeField] private EnemyData thisEnemy;
    [SerializeField] private EnemyPlaystyle thisPlaystyle;

    [SerializeField] private Card[] enemyCards;
    [SerializeField] private CardSelected selectedCardData;

    [Serializable()]
    public class CardSelected
    {
        public Card battlingCard;
        public Skill[] cardSkills;

        public CardSelected(Card _selectedCard)
        {
            battlingCard = _selectedCard;
        }

        [Header("ACTIVATED SKILL SET")]
        public int[] skillsActivatedSet_onRoll;

        [Header("DAMAGE")]
        //variables of effects from activated set of skills on roll
        public int totalDamageDone_onRoll=0;
        public int totalPrecisionDamage_onRoll=0;
        public int totalNormalDamage_onRoll=0;
        public int totalDefenceInstances_Activated = 0;
        public List<int> totalPreciseInstances_Activated;
        public List<int> totalDodgeInstances_Activated;

        [Header("TARGET SKILL SETS")]
        //variables of possibile sets for calculation
        public List<TargetSkillsSetsData> TargetSkillSetsData_List;

        public List<float> targetSkillSetsProbabilitiesAfterRoll_List;
        public List<int[]> targetSkillSetsDiceCombinationsByMana_List;
        public List<int[]> targetSkillSetsActivationsBySkill_List;
        public List<int> targetSkillSetsDamage_List;
    }

    [Header("SKILLS")]
    [SerializeField] private Skill[] selectedCardSkills;
    [SerializeField] private Skill firstSkill;
    [SerializeField] private Skill secondSkill;
    [SerializeField] private Skill thirdSkill;

    [Serializable()]
    public class Skill
    {
        public SkillData skillData;

        public Skill(SkillData _skillData)
        {
            skillData = _skillData;
            skill_manaCostList = _skillData.Skill_colorCost;
            EnemyBrain.Instance.GetSingleSkillManaCost(this);
        }

        [Header("Skill Mana Cost")]
        public List<Dice.diceFace> skill_manaCostList;
        public int skill_totalManaCost = 0;
        public int skill_RedManaCost = 0;
        public int skill_YellowManaCost = 0;
        public int skill_BlueManaCost = 0;
        public int[] skillManaCost_byManaType = new int[3];

        [Header("Activations")]
        public int skill_maximumActivations;
        public List<int> singleSkillActivationsDone_byManaType;
        public int singleSkillActivationsDone = 0;

        [Header("Effects on Roll")]
        //after roll effects activated
        public int skill_damage_byAttackInstancesActivated = 0;
        public int skill_attackInstancesActivated = 0;
        public int skill_defenceInstancesActivated = 0;
        public bool isAttackPrecise = false;
        public int skill_preciseInstancesActivated = 0;
        public List<int> skill_dodgeInstancesActivated;
    }

    [Serializable()]
    public class TargetSkillsSetsData
    {
        public int activationSetNumber;

        public TargetSkillsSetsData(int[] _diceComboSet)
        {
            skillSetDiceCombination_byManaType = _diceComboSet;
        }

        //questo per test maximized
        public TargetSkillsSetsData(int[] _diceComboSet, int _damage)
        {
            skillSetDiceCombination_byManaType = _diceComboSet;
            skillSet_totalDamage = _damage;
        }

        //[0] = first skill activations, [1] = second skill activations, [2] = third skill activations
        public int[] skillSet_activationsArray= new int[3];

        //[0] = red mana faces, [1] = yellow mana faces, [2] = blue mana faces
        public int[] skillSetDiceCombination_byManaType = new int[3];

        public float skillSet_probabilityPreRoll;
        public float skillSet_probabilityAfterRoll;

        //somma del danno di ogni abilità per le rispettive istanze e attivazioni del set
        public int skillSet_totalDamage = 0;
        public int skillSet_precisionDamage = 0;
        public int skillSet_normalDamage = 0;
        public float skillSet_damageMultipliedByChance;
        public int skillSet_defenceInstances;
        public int[] skillSet_preciseInstances = new int[3]; 
        public List<int> skillSet_dodgeInstances = new List<int>();

    }

    private void Awake()
    {
        Instance = this;
    }

    #region < get variables on first roll >

    public void GetDice()
    {
        diceRolled = Instances.BattleManager.EnemyDices;
        diceToCopy = new Dice[6];

        int i = 0;
        foreach(Dice die in diceRolled)
        {
            Dice next = new Dice();
            diceToCopy[i] = next;
            diceToCopy[i].SetResult(die.Result);
            i++;
        }
    }

    public void GetRolledDiceFaces()
    {
        //reset dice faces count
        diceRolledFaces = new Dice.diceFace[6];
        diceRoll_RedMana = 0;
        diceRoll_YellowMana = 0;
        diceRoll_BlueMana = 0;
        diceRoll_byManaColor = new int[3] { 0, 0, 0 };

        foreach (Dice die in diceRolled)
        {
            diceRolledFaces.Append(die.Result);

            switch (die.Result)
            {
                case Dice.diceFace.notRolled:
                    Debug.Log("face not rolled :'(");
                    break;

                case Dice.diceFace.red:
                    diceRoll_RedMana++;
                    break;

                case Dice.diceFace.yellow:
                    diceRoll_YellowMana++;
                    break;

                case Dice.diceFace.blue:
                    diceRoll_BlueMana++;
                    break;
            }
        }
        
        diceRoll_byManaColor = new int[3] { diceRoll_RedMana , diceRoll_YellowMana , diceRoll_BlueMana };

        string debugText = "";
        foreach (var d in diceRoll_byManaColor)
            debugText += d.ToString() + " ";
        Debug.Log("<Rolled dice faces (RYB)> = ( "+ debugText + ")");

    }

    public void GetCardDataAndSkillsData()
    {
        //get skills from card and reset values of roll
        selectedCardData = new CardSelected(Instances.BattleManager.EnemySelectedCard);

        firstSkill = new Skill(selectedCardData.battlingCard.CardData.Skills[0]);
        secondSkill = new Skill(selectedCardData.battlingCard.CardData.Skills[1]);
        thirdSkill = new Skill(selectedCardData.battlingCard.CardData.Skills[2]);
        selectedCardSkills =  new Skill[3] { firstSkill, secondSkill, thirdSkill };
        selectedCardData.cardSkills = selectedCardSkills;
    }

    public void GetSingleSkillManaCost(Skill _skill)
    {
        _skill.skill_totalManaCost = _skill.skill_manaCostList.Count;
        _skill.skill_RedManaCost = 0;
        _skill.skill_YellowManaCost = 0;
        _skill.skill_BlueManaCost = 0;

        foreach (var face in _skill.skill_manaCostList)
        {
            switch (face)
            {
                case Dice.diceFace.notRolled:
                    Debug.Log("face missing :(");
                    break;

                case Dice.diceFace.red:
                    _skill.skill_RedManaCost++;
                    break;

                case Dice.diceFace.yellow:
                    _skill.skill_YellowManaCost++;
                    break;

                case Dice.diceFace.blue:
                    _skill.skill_BlueManaCost++;
                    break;
            }
        }

        _skill.skillManaCost_byManaType = new int[3] { _skill.skill_RedManaCost , _skill.skill_YellowManaCost , _skill.skill_BlueManaCost };

        _skill.skill_maximumActivations = (int) (6 / (_skill.skill_totalManaCost));

    }

    public int GetSingleSkillActivationsFromDiceManaCombo(Skill _skill, int[] _manaCombination )
    {
        int activations = 0;

        _skill.singleSkillActivationsDone_byManaType = new List<int>();

        for (int i = 0; i < 3; i++)
        {
            if (_skill.skillManaCost_byManaType[i] != 0)
            {
                int activs = (int)(_manaCombination[i] / _skill.skillManaCost_byManaType[i]);

                _skill.singleSkillActivationsDone_byManaType.Add(activs);
            }
        }

        //get skill final activations
        _skill.singleSkillActivationsDone = _skill.singleSkillActivationsDone_byManaType.Min();

        return activations;
    }

    public void GetDamageAndEffectsActivatedForSingleSkillAfterRoll(Skill _skill)
    {

        //get attack instances 
        _skill.skill_attackInstancesActivated = _skill.skillData.AtkInstances * _skill.singleSkillActivationsDone;

        //get damage done by single skill activs
        _skill.skill_damage_byAttackInstancesActivated = (_skill.skillData.Damage * _skill.skill_attackInstancesActivated);

        //get precise and normal damage istances
        if (_skill.skillData.Precise)
        {
            _skill.skill_preciseInstancesActivated = _skill.skill_attackInstancesActivated;
            selectedCardData.totalPrecisionDamage_onRoll += _skill.skill_damage_byAttackInstancesActivated;
        }
        else
        {
            _skill.skill_preciseInstancesActivated = 0;
            selectedCardData.totalNormalDamage_onRoll += _skill.skill_damage_byAttackInstancesActivated;
        }
        selectedCardData.totalPreciseInstances_Activated.Add(_skill.skill_preciseInstancesActivated);

        //get skill defence istances
        _skill.skill_defenceInstancesActivated = _skill.skillData.DefInstances * _skill.singleSkillActivationsDone;
        selectedCardData.totalDefenceInstances_Activated += _skill.skill_defenceInstancesActivated;

        //get skill dodge istances
        _skill.skill_dodgeInstancesActivated = new List<int>();
        if (_skill.skillData.Dodge > 0)
        {
            for (int i = 0; i < _skill.singleSkillActivationsDone; i++)
            {
                _skill.skill_dodgeInstancesActivated.Add(_skill.skillData.Dodge);
            }
        }
        selectedCardData.totalDodgeInstances_Activated.AddRange(_skill.skill_dodgeInstancesActivated);
    }

    public void GetRolledActivationsAndDamageFromEachSkill()
    {
        selectedCardData.totalDodgeInstances_Activated = new List<int>();
        selectedCardData.totalPreciseInstances_Activated = new List<int>();
        selectedCardData.skillsActivatedSet_onRoll = new int[3] {0,0,0};

        for (int i = 0; i < selectedCardSkills.Length; i++)
        {
            //add to battlingCard list of skill activated set --> confront with other sets probabilities
            selectedCardData.skillsActivatedSet_onRoll[i] = GetSingleSkillActivationsFromDiceManaCombo(selectedCardSkills[i], diceRoll_byManaColor);

            GetDamageAndEffectsActivatedForSingleSkillAfterRoll(selectedCardSkills[i]);
        }

        //set total damage done by all skill activs
        selectedCardData.totalDamageDone_onRoll = selectedCardData.totalPrecisionDamage_onRoll + selectedCardData.totalNormalDamage_onRoll;

        Debug.Log($"EB - ROLLED EFFECTS:\n" +
                                 $"- damage =  {selectedCardData.totalDamageDone_onRoll}" +
                                 $"- defences = {selectedCardData.totalDefenceInstances_Activated}" +
                                 $"- dodge istances count = {selectedCardData.totalDodgeInstances_Activated.Count}" +
                                 $"- precise istances = {selectedCardData.totalPreciseInstances_Activated[0]}" +
                                 $"{selectedCardData.totalPreciseInstances_Activated[1]}" +
                                 $"{selectedCardData.totalPreciseInstances_Activated[2]}");
    
    }
    #endregion

    #region < check dice to lock >

    private int GetLockedDicesByColor(Dice.diceFace color)
    {
        int d = 0;
        for (int i=0; i<diceToCopy.Length; i++)
        {
            if (diceToCopy[i].Result == color&& diceToCopy[i].IsLocked)
            {
                d++;
            }
        }
        return d;
    }

    public void CheckDiceToLock()
    {
        //PRIORITÁ DELLE MOSSE: ORDINAMENTO
        //different skill priority orders
        List<Skill> skillsByPriority = new List<Skill>();

        skillsByPriority.Add(firstSkill);
        skillsByPriority.Add(secondSkill);
        skillsByPriority.Add(thirdSkill);

        //GetSingleSkillActivationsFromDiceManaCombo(s, diceRoll_byManaColor);

        int diceToLeave = firstSkill.singleSkillActivationsDone < firstSkill.skill_maximumActivations ? firstSkill.skill_totalManaCost * (firstSkill.singleSkillActivationsDone + 1) : firstSkill.skill_totalManaCost * firstSkill.singleSkillActivationsDone;

        foreach (Skill s in skillsByPriority)
        {

            //GetSingleSkillActivationsFromDiceManaCombo(s, diceRoll_byManaColor);
            int numberOfActivations = s.singleSkillActivationsDone;

            //maximum or target activations
            int maximumActivation = s.skill_maximumActivations;

            int redDiceLocked = GetLockedDicesByColor(Dice.diceFace.red);
            int yellowDiceLocked = GetLockedDicesByColor(Dice.diceFace.yellow);
            int blueDiceLocked = GetLockedDicesByColor(Dice.diceFace.blue);
            int GetUnlockedDice() => 6-( redDiceLocked + yellowDiceLocked + blueDiceLocked);

            int maxYprioritySkill = 0;
            int maxY = 0;
            bool isPrioritySkill = false;

            //condition on priority skill ( default is firstSkill)
            if (s == firstSkill)
                isPrioritySkill = true;
            else
            {
                isPrioritySkill = false;

                if (numberOfActivations == 0)
                    continue;
            }

            //blue mana check:
            //condition to lock blue dice for Priority skill bonus max iteration:
            //maxYprioritySkill = s.singleSkillActivationsDone < maximumActivation ? s.skill_BlueManaCost * (numberOfActivations + 1)-1 : s.skill_BlueManaCost * numberOfActivations;
            maxYprioritySkill = s.singleSkillActivationsDone < maximumActivation ? s.skill_BlueManaCost * (numberOfActivations + 1) : s.skill_BlueManaCost * numberOfActivations;
            maxY = isPrioritySkill ? maxYprioritySkill : s.skill_BlueManaCost * numberOfActivations;

            for (int y = 0; y < maxY; y++)
            {
                if (blueDiceLocked == maxY/*|| GetUnlockedDice() == diceToLeave*/)
                    break;
                if (s != firstSkill && GetUnlockedDice() == diceToLeave)
                    break;
                for (int x = 0; x < diceToCopy.Length; x++)
                {
                    if (diceToCopy[x].Result == Dice.diceFace.blue)
                    {
                        if (!diceToCopy[x].IsLocked)
                        {
                            diceToCopy[x].LockDiceDebug(true);
                            blueDiceLocked++;
                            if(isPrioritySkill)
                                diceToLeave--;
                            break;
                        }
                        //if (blueDiceLocked + diceToLeave == maxY)
                        //    break;
                    }
                }
                
            }

            //yellow mana check:
            //add condition -> skip if already done for blue mana cost
            //condition to lock yellow dice for Priority skill bonus max iteration:
            maxYprioritySkill = s.singleSkillActivationsDone < maximumActivation ? s.skill_YellowManaCost * (numberOfActivations + 1) : s.skill_YellowManaCost * numberOfActivations;
            maxY = isPrioritySkill ? maxYprioritySkill : s.skill_YellowManaCost * numberOfActivations;

            for (int y = 0; y < maxY; y++)
            {
                if (yellowDiceLocked == maxY/* || GetUnlockedDice() == diceToLeave*/)
                    break;
                if (s != firstSkill && GetUnlockedDice() == diceToLeave)
                    break;
                for (int x = 0; x < diceToCopy.Length; x++)
                {
                    if (diceToCopy[x].Result == Dice.diceFace.yellow)
                    {
                        if (!diceToCopy[x].IsLocked)
                        {
                            diceToCopy[x].LockDiceDebug(true);
                            yellowDiceLocked++;
                            if (isPrioritySkill)
                                diceToLeave--;
                            break;
                        }
                        //if (yellowDiceLocked + diceToLeave == maxY)
                        //    break;
                    }

                }
            }

            //red mana check:
            //add condition -> skip if already done for yellow or blue mana cost
            //condition to lock red dice for Priority skill bonus max iteration:
            maxYprioritySkill = s.singleSkillActivationsDone < maximumActivation ? s.skill_RedManaCost * (numberOfActivations + 1) : s.skill_RedManaCost * numberOfActivations;

            maxY = isPrioritySkill ? maxYprioritySkill : s.skill_RedManaCost * numberOfActivations;

            for (int y = 0; y < maxY; y++)
            {
                if (redDiceLocked == maxY/* || GetUnlockedDice() == diceToLeave*/)
                    break;
                if (s != firstSkill && GetUnlockedDice() == diceToLeave)
                    break;
                for (int x = 0; x < diceToCopy.Length; x++)
                {
                    if (diceToCopy[x].Result == Dice.diceFace.red)
                    {
                        if (!diceToCopy[x].IsLocked)
                        {
                            diceToCopy[x].LockDiceDebug(true);
                            redDiceLocked++;
                            if(isPrioritySkill)
                                diceToLeave--;
                            break;
                        }
                        //if (redDiceLocked + diceToLeave == maxY)
                        //    break;
                    }
                }
            }
        }
    }

    public IEnumerator EnemyDiceLockingCoroutine(Dice[] _diceRolled)
    {
        for (int x = 0; x < diceToCopy.Length; x++)
        {
            for (int y = 0; y < _diceRolled.Length; y++)
            {
                if (diceToCopy[x].Result == _diceRolled[y].Result)
                {
                    if (diceToCopy[x].IsLocked == true && _diceRolled[y].IsLocked == false)
                    {
                        yield return new WaitForSeconds(Random.Range(.2f, .6f));
                        _diceRolled[y].LockDice(true);
                        break;
                    }
                }
            }
        }
    }
    
    #endregion

    #region < MAIN LOOP >

    public void AI_Loop()
    {
        //START
        GetDice();
        GetRolledDiceFaces();
        GetCardDataAndSkillsData();

        //GET ROLLED EFFECTS
        GetRolledActivationsAndDamageFromEachSkill();

        //CHECK TO LOCK:
        //PER OGNI MOSSA SE POSSO FARLA, LOCKO I DADI NECESSARI PER FARLA : PROBABILITÁ 100%
        // RIPETERE LA STESSA MOSSA É SEMPRE PREFERIBILE A FARNE DUE DIVERSE
        // SE LA MOSSA CON PRIORITA' E'VICINA AD UN'ALTRA ATTIVAZIONE ALLORA LOCKA I DADI NECESSARI
        // (distanza massima: 1 mana rosso, 1 mana giallo)

        CheckDiceToLock();

        //Qui ha deciso e mostra quali rerollare
        string debugText = "";
        for (int x = 0; x < diceRolled.Length; x++)
        {
            Dice d = diceRolled[x];
            debugText += $"Dice {x} - {d.Result} - {d.IsLocked}\n";
        }
        Debug.Log("EB - " + debugText);


        //WIP
        //get all possible diceCombination to lock(by mana type)
        GetDiceCombinationsAndTargetSkillSets();
        GetTargetSkillSetsActivationsChancesDamageAndEffects();
        CheckMaxDamageBetweenSets(selectedCardData.TargetSkillSetsData_List);
    }
    #endregion

    #region < combinations chance calculations - WIP >

    public void GetDiceCombinationsAndTargetSkillSets()
    {
        selectedCardData.targetSkillSetsDiceCombinationsByMana_List = new List<int[]>();
        selectedCardData.TargetSkillSetsData_List = new List<TargetSkillsSetsData>();

        int i = 0;
        int j = 0;
        int k = 0;

        for(i = 0; i<=6; i++)
        {
            for(j=0; j<=6-i; j++)
            {
                k = 6 - j - i;

                int[] set = new int[3] {i,j,k};

                selectedCardData.targetSkillSetsDiceCombinationsByMana_List.Add(set);
                TargetSkillsSetsData newSet = new TargetSkillsSetsData(set);
                selectedCardData.TargetSkillSetsData_List.Add(newSet);
              
                //Debug.Log("Set probabilities after roll = " + set[0] + set[1] + set[2]);
            }

            j = 0;

        }
    }

    public List<int[]> GetSkillActivationsOfTargetSkillSets(List<TargetSkillsSetsData> _activationDataSets)
    {
        List<int[]> activations= new List<int[]>();
        int debug = 0;

        foreach (TargetSkillsSetsData setData in _activationDataSets)
        {
            debug++;
            setData.activationSetNumber = debug;

            setData.skillSet_activationsArray = new int[3];
            List<int> skillsActivationsSets = new List<int>();

            foreach (Skill skill in selectedCardData.cardSkills)
            {
                List<int> activationsByMana = new List<int>();

                for (int i = 0; i < setData.skillSetDiceCombination_byManaType.Length; i++)
                {
                    if (skill.skillManaCost_byManaType[i] != 0)
                    {
                        int activs = (int)(setData.skillSetDiceCombination_byManaType[i] / skill.skillManaCost_byManaType[i]);

                        activationsByMana.Add(activs);
                        //activsByMana[i] = activs;
                    }
                }

                int singleSkillActivationsForSet = activationsByMana.Min();

                skillsActivationsSets.Add(singleSkillActivationsForSet);
            }

            setData.skillSet_activationsArray = skillsActivationsSets.ToArray();

            activations.Add(setData.skillSet_activationsArray);
        }
        return activations;
    }

    public List<float> GetChanceOfTargetSkillSetsFromRoll(List<TargetSkillsSetsData> _activationDataSets)
    {
        //get all chances to do each diceCombination(by type)

        List<float> probabilities = new List<float>();

        foreach (TargetSkillsSetsData setData in _activationDataSets)
        {
            float _redDiceNeeded = setData.skillSetDiceCombination_byManaType[0] - diceRoll_RedMana;
            float _yellowDiceNeeded = setData.skillSetDiceCombination_byManaType[1] - diceRoll_YellowMana;
            float _blueDiceNeeded = setData.skillSetDiceCombination_byManaType[2] - diceRoll_BlueMana;

            float _probability_set_after_roll = (Mathf.Pow((1 / 2f), _redDiceNeeded > 0 ? _redDiceNeeded : 0))
                                              * (Mathf.Pow((1 / 3f), _yellowDiceNeeded > 0 ? _yellowDiceNeeded : 0))
                                              * (Mathf.Pow((1 / 6f), _blueDiceNeeded > 0 ? _blueDiceNeeded : 0));

            setData.skillSet_probabilityAfterRoll = _probability_set_after_roll;
            probabilities.Add(_probability_set_after_roll);

        }

        return probabilities;
        //List<float> MaxChanceOfActivationSet = new List<float>();
    }

    public struct Effects
    {
        public List<int> skillSetTotalDamage;
        public List<int> skillSetTotalDef;
        public List<int[]> skillSetTotalPrec;
        public List<int[]> skillSetTotalDodge;
    }
       
    public Effects GetDamageAndEffectsOfTargetSkillSets(List<TargetSkillsSetsData> _activationDataSets)
    {
        Effects effects = new Effects()
        {
            skillSetTotalDamage = new List<int>(),
            skillSetTotalDef = new List<int>(),
            skillSetTotalPrec = new List<int[]>(),
            skillSetTotalDodge = new List<int[]>()
        };
        string debugText = "";
        foreach (TargetSkillsSetsData setData in _activationDataSets)
        {
            //new damageSet con tutte attivazioni
            int skillsDamage = 0;
            int preciseDamage = 0;
            int normalDamage = 0;
            int skillsDefence = 0;

            //[0] = first skill istances, [1] = second skil istances, [2] = third skill istances
            int[] skillsPreciseIstances = new int[3] {0,0,0};

            //dodge list valido per intero set
            List<int> skillsDodge = new List<int>();

            for (int i = 0; i < setData.skillSet_activationsArray.Length; i++)
            {
                //get skill defence istances
                skillsDefence += (setData.skillSet_activationsArray[i] * selectedCardSkills[i].skillData.DefInstances);

                //get normal and precise damage
                if (selectedCardSkills[i].isAttackPrecise)
                {
                    skillsPreciseIstances[i] += setData.skillSet_activationsArray[i] * selectedCardSkills[i].skillData.AtkInstances;
                    preciseDamage += (setData.skillSet_activationsArray[i] * selectedCardSkills[i].skillData.Damage * selectedCardSkills[i].skillData.AtkInstances);
                }
                else
                {
                    normalDamage += (setData.skillSet_activationsArray[i] * selectedCardSkills[i].skillData.Damage * selectedCardSkills[i].skillData.AtkInstances);
                }

                //get skill dodge istances
                if (selectedCardSkills[i].skillData.Dodge > 0)
                {
                    //aggiungi un valore di dodge per ogni attivazione della skill
                    for (int j = 0; j < setData.skillSet_activationsArray[i]; j++)
                    {
                        skillsDodge.Add(selectedCardSkills[i].skillData.Dodge);
                    }
                }
            }

            skillsDamage = preciseDamage + normalDamage;

            //compile each TargetSkillsSetsData with own values
            setData.skillSet_totalDamage = skillsDamage;
            setData.skillSet_precisionDamage = preciseDamage;
            setData.skillSet_normalDamage = normalDamage;
            setData.skillSet_defenceInstances = skillsDefence;
            setData.skillSet_preciseInstances = skillsPreciseIstances;
            setData.skillSet_dodgeInstances = skillsDodge;

            //compile srtuct to pass to BattlingCard Lists values
            effects.skillSetTotalDamage.Add(skillsDamage);
            effects.skillSetTotalDef.Add(skillsDefence);
            effects.skillSetTotalPrec.Add(skillsPreciseIstances);
            effects.skillSetTotalDodge.Add(skillsDodge.ToArray());

            debugText +=
            " SetNum (" + setData.activationSetNumber + ")" +
            " - skills (" + setData.skillSet_activationsArray[0] + setData.skillSet_activationsArray[1] + setData.skillSet_activationsArray[2] + ")" +
            " - dice (" + setData.skillSetDiceCombination_byManaType[0] + setData.skillSetDiceCombination_byManaType[1] + setData.skillSetDiceCombination_byManaType[2] + ")" +
            " - probabilities after roll = " + setData.skillSet_probabilityAfterRoll +
            " - damage = " + setData.skillSet_totalDamage +
            " - defences = " + setData.skillSet_defenceInstances +
            " - dodge istances = " + setData.skillSet_dodgeInstances +
            " - precise istances = " + setData.skillSet_preciseInstances[0] + setData.skillSet_preciseInstances[1] + setData.skillSet_preciseInstances[2] + "\n";

        }

        Debug.Log("\nEB - ACTIVATION SETS DATA:\n" + debugText);

        return effects;
    }

    public void GetTargetSkillSetsActivationsChancesDamageAndEffects()
    {
        //Battling Card lists
        selectedCardData.targetSkillSetsActivationsBySkill_List = new List<int[]>();
        selectedCardData.targetSkillSetsProbabilitiesAfterRoll_List = new List<float>();
        selectedCardData.targetSkillSetsDamage_List = new List<int>();
        //selectedCardData.targetSkillSetsDefences_List = new List<int>();
        //selectedCardData.targetSkillSetsPreciseIstances_List = new List<int[]>();
        //selectedCardData.targetSkillSetsDodgeIstancesValues_List = new List<int[]>();

        //get skill activ for each set
        selectedCardData.targetSkillSetsActivationsBySkill_List = GetSkillActivationsOfTargetSkillSets(selectedCardData.TargetSkillSetsData_List);

        //Get chance of activation for each set
        selectedCardData.targetSkillSetsProbabilitiesAfterRoll_List = GetChanceOfTargetSkillSetsFromRoll(selectedCardData.TargetSkillSetsData_List);
        //order skill set probabilities general list by chance value(greater to smaller)
        selectedCardData.targetSkillSetsProbabilitiesAfterRoll_List.Reverse();

        //Get Damage and Effects 
        Effects effects = GetDamageAndEffectsOfTargetSkillSets(selectedCardData.TargetSkillSetsData_List);

        //compile Battling Card lists
        selectedCardData.targetSkillSetsDamage_List = effects.skillSetTotalDamage;
        //selectedCardData.targetSkillSetsDefences_List = effects.skillSetTotalDef;
        //selectedCardData.targetSkillSetsPreciseIstances_List = effects.skillSetTotalPrec;
        //selectedCardData.targetSkillSetsDodgeIstancesValues_List = effects.skillSetTotalDodge;

        //SHOW MAX CHANCE SETS AND ORDER THEM BY DAMAGE
        CheckMaxDamageBetweenSets(GetSetsWithMaxProbability(selectedCardData.TargetSkillSetsData_List));

        //max chanche and max damage or defence or dodge or else
    }

    public List<TargetSkillsSetsData> GetSetsWithMaxProbability(List<TargetSkillsSetsData> _activationDataSets)
    {
        //order target skill set list by chance values(greater to smaller)
        _activationDataSets.Sort((y, x) => x.skillSet_probabilityAfterRoll.CompareTo(y.skillSet_probabilityAfterRoll));

        List<TargetSkillsSetsData> maxSets = new List<TargetSkillsSetsData>();

        //show max 5 chance
        string debugChanceText = "";
        for (int h = 0; h <= 5; h++)
        {
            maxSets.Add(_activationDataSets[h]);

            debugChanceText +=
                "SetNum (" + _activationDataSets[h].activationSetNumber + ")" +
                " - skills (" + _activationDataSets[h].skillSet_activationsArray[0] +
                                _activationDataSets[h].skillSet_activationsArray[1] +
                                _activationDataSets[h].skillSet_activationsArray[2] + ")" +
                " - dice (" + _activationDataSets[h].skillSetDiceCombination_byManaType[0] +
                              _activationDataSets[h].skillSetDiceCombination_byManaType[1] +
                              _activationDataSets[h].skillSetDiceCombination_byManaType[2] + ")" +
                " - probabilities after roll = " + _activationDataSets[h].skillSet_probabilityAfterRoll +
                " - damage = " + _activationDataSets[h].skillSet_totalDamage +
                " - defences = " + _activationDataSets[h].skillSet_defenceInstances +
                " - dodge istances = (" + _activationDataSets[h].skillSet_dodgeInstances + ")" +
                " - precise istances = (" + _activationDataSets[h].skillSet_preciseInstances[0] +
                                            _activationDataSets[h].skillSet_preciseInstances[1] +
                                            _activationDataSets[h].skillSet_preciseInstances[2] + ")\n";
        }
        Debug.Log("\nEB - Top 5 Sets with max probabilities after roll:\n" + debugChanceText);

        return maxSets;
    }

    public List<TargetSkillsSetsData> CheckMaxDamageBetweenSets(List<TargetSkillsSetsData> _activationDataSets)
    {
        // List<float> CheckMaxDamage = new List<float>();
        _activationDataSets.Sort((y, x) => x.skillSet_totalDamage.CompareTo(y.skillSet_totalDamage));

        List<TargetSkillsSetsData> maxSets = new List<TargetSkillsSetsData>();

        string debugText = "";
        foreach (var set in _activationDataSets)
        {
            maxSets.Add(set);

            debugText += ("Set N. " + set.activationSetNumber + " - damage by set = " + set.skillSet_totalDamage.ToString() + " - " + set.skillSet_probabilityAfterRoll.ToString() + "\n");
        }
        Debug.Log("\nEB - MAX DAMAGE SETS:\n" + debugText);

        return maxSets;
    }

    #endregion

    #region < test buttons >

    public void RollDiceButtonTest()
    {
        RollDices();

        GetRolledDiceFaces();
    }

    public void CalculateSkillActivationsButtonTest()
    {
        GetCardDataAndSkillsData();

        GetRolledActivationsAndDamageFromEachSkill();

    }

    public void CalculateChanceButtonTest()
    {
        GetDiceCombinationsAndTargetSkillSets();

        GetChanceOfTargetSkillSetsFromRoll(selectedCardData.TargetSkillSetsData_List);

        GetSkillActivationsOfTargetSkillSets(selectedCardData.TargetSkillSetsData_List);

        GetDamageAndEffectsOfTargetSkillSets(selectedCardData.TargetSkillSetsData_List);

        //CheckMaxDamageBetweenSets(selectedCardData.TargetSkillSetsData_List);

    }

    public void RollDices()
    {
        foreach (Dice die in diceRolled)
        {
            die.RollDice();
        }
    }

    #endregion

}
