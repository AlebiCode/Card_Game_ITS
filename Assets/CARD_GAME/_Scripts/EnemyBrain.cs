using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Profiling.Memory.Experimental;
using UnityEngine.XR;
using static EnemyBrain;

public class EnemyBrain : MonoBehaviour
{
    public static EnemyBrain Instance { get; set; }

    [Header("DICES ROLL OUTCOME")]
    [SerializeField] private Dice[] diceRolled;
    [SerializeField] private Dice.diceFace[] diceRolledFaces;

    [SerializeField] private int diceRoll_RedMana = 0;
    [SerializeField] private int diceRoll_YellowMana = 0;
    [SerializeField] private int diceRoll_BlueMana = 0;
    [SerializeField] private int[] diceRoll_byManaColor;

    [Header("CARD")]
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

        //variables of possibile sets for calculation
        public List<TargetSkillsSetsData> ActivationsSetsData_List;
        public List<float> setProbabilitiesAfterRoll_List;
        public List<int[]> diceCombinationsSets_byManaType;
        public List<int[]> skillsActivationsSets_ofDiceCombinationSets;
        public List<float> damageByEachSetOfActivations_List;
        public List<float> defencesByEachSetOfActivations_List;
        public List<int[]> preciseIstancesByEachSetOfActivation_List;
        public List<int[]> skillsDodgeIstancesValuesByEachSetOfActivation_List;
    }

    //[Header("DAMAGE")]
    //[SerializeField] private float totalDamage_AfterFirstRoll;  

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

        //public float damage_toDo;
        //public float damage_done_withActivs;
    }

    private void Awake()
    {
        Instance = this;
    }

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

        MaximizeSingleSkillCalculations();
    }

    public void CalculateChanceButtonTest()
    {
        DiceCombinationsCalculator();

        //get all chances to do each diceCombination (by type)
        GetChanceOfEachDiceCombinationFromRoll();

        GetSkillActivationsSetsFromEachDiceCombination(/*selectedCardData.ActivationsSetsData_List*/);

        GetDamageAndEffectsFromEachActivationSet(/*selectedCardData.ActivationsSetsData_List*/);

        CheckMaxDamageBetweenSets(selectedCardData.ActivationsSetsData_List);

    }

    public void RollDices()
    {
        foreach (Dice die in diceRolled)
        {
            die.RollDice();
        }
    }

    #endregion

    #region < get variables on first roll >

    public void GetDice()
    {
        diceRolled = BattleManager.instance.EnemyDices;
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

        /*Debug.Log("<Rolled dice faces (RYB)> = (");
        foreach (var d in diceRoll_byManaColor)
            Debug.Log(d);
        Debug.Log(")");*/

    }

    public void GetCardDataAndSkillsData()
    {
        //get skills from card and reset values of roll
        selectedCardData = new CardSelected(BattleManager.instance.EnemySelectedCard);

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
        selectedCardData.skillsActivatedSet_onRoll = new int[3] {0,0,0 };

        for (int i = 0; i < selectedCardSkills.Length; i++)
        //foreach (Skill skill in selectedCardSkills)
        {
            //add to battlingCard list of skill activated set --> confront with other sets probabilities
            selectedCardData.skillsActivatedSet_onRoll[i]=(GetSingleSkillActivationsFromDiceManaCombo(selectedCardSkills[i], diceRoll_byManaColor));

            GetDamageAndEffectsActivatedForSingleSkillAfterRoll(selectedCardSkills[i]);

        }

        //set total damage done by all skill activs
        selectedCardData.totalDamageDone_onRoll = selectedCardData.totalPrecisionDamage_onRoll + selectedCardData.totalNormalDamage_onRoll;

        Debug.Log($"EB - ROLLED EFFECTS - damage =  {selectedCardData.totalDamageDone_onRoll}" +
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
        for (int i=0; i<diceRolled.Length; i++)
        {
            if (diceRolled[i].Result == color&& diceRolled[i].IsLocked)
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

        //List<Dice> diceToLock = new List<Dice>();

        foreach (Skill s in skillsByPriority)
        {
            List<Dice.diceFace> skillCost = s.skillData.Skill_colorCost;

            GetSingleSkillActivationsFromDiceManaCombo(s, diceRoll_byManaColor);
            int numberOfActivations = s.singleSkillActivationsDone;

            //maximum or target activations
            int maximumActivation = s.skill_maximumActivations;
            int maximumPrioritySkillActivations = firstSkill.skill_maximumActivations;

            int redDiceLocked = GetLockedDicesByColor(Dice.diceFace.red);
            int yellowDiceLocked = GetLockedDicesByColor(Dice.diceFace.yellow);
            int blueDiceLocked = GetLockedDicesByColor(Dice.diceFace.blue);

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

            //condition for Priority skill bonus max iterations for red mana to lock on dice:
            int maxYprioritySkill = s.singleSkillActivationsDone < maximumActivation ? s.skill_RedManaCost * (numberOfActivations + 1) - 1 : s.skill_RedManaCost * numberOfActivations;
            int maxY = isPrioritySkill ? maxYprioritySkill : s.skill_RedManaCost * numberOfActivations;

            //red mana check
            for (int y = 0; y < maxY; y++)
            {
                if (redDiceLocked == maxY)
                    break;
                for (int x = 0; x < diceRolled.Length; x++)
                {
                    if (diceRolled[x].Result == Dice.diceFace.red)
                    {
                        if (!diceRolled[x].IsLocked)
                        {
                            diceRolled[x].LockDice(true);
                            redDiceLocked++;
                        }
                        if (redDiceLocked == maxY)
                            break;
                    }
                }
            }

            //condition for Priority skill bonus max iterations for yellow mana to lock on dice:
            maxYprioritySkill = s.singleSkillActivationsDone < maximumActivation ? s.skill_YellowManaCost * (numberOfActivations + 1) - 1 : s.skill_YellowManaCost * numberOfActivations;
            maxY = isPrioritySkill ? maxYprioritySkill : s.skill_YellowManaCost * numberOfActivations;

            //yellow mana check
            for (int y = 0; y < maxY; y++)
            {
                if (yellowDiceLocked == maxY)
                    break;
                for (int x = 0; x < diceRolled.Length; x++)
                {
                    if (diceRolled[x].Result == Dice.diceFace.yellow)
                    {
                        if (!diceRolled[x].IsLocked)
                        {
                            diceRolled[x].LockDice(true);
                            yellowDiceLocked++;
                        }
                        if (yellowDiceLocked == maxY)
                            break;
                    }
                }
            }

            //eventual condition for Priority skill bonus max iterations for blue mana to lock on dice:
            //maxYprioritySkill = s.singleSkillActivationsDone < maximumActivation ? s.skill_BlueManaCost * (numberOfActivations + 1) - 1 : s.skill_BlueManaCost * numberOfActivations;
            maxY = /*isPrioritySkill ? maxYprioritySkill :*/ s.skill_BlueManaCost * numberOfActivations;

            //blue mana check
            for (int y = 0; y < maxY; y++)
            {
                if (blueDiceLocked == maxY)
                    break;
                for (int x = 0; x < diceRolled.Length; x++)
                {
                    if (diceRolled[x].Result == Dice.diceFace.blue)
                    {
                        if (!diceRolled[x].IsLocked)
                        {
                            diceRolled[x].LockDice(true);
                            blueDiceLocked++;
                        }
                        if (blueDiceLocked == maxY)
                            break;
                    }
                }
            }
        }
    }
    //OK

    List<Dice> diceToLock = new List<Dice>();

    public void SetDiceToLock()
    {
        for (int i=0; i < diceRolled.Length; i++)
        {
            for (int j = 0; j < diceToLock.Count; j++)
            {
                if (diceRolled[i].Result == diceToLock[j].Result)
                {
                    if (diceToLock[j].IsLocked == false)
                    {
                        diceToLock[j].LockDice(true);
                        diceRolled[i].LockDice(true);
                        return;
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

        //GetSingleSkillManaCost(skill); <---- now it's in Skill constructor

        GetRolledActivationsAndDamageFromEachSkill();

        //CHECK TO LOCK
        CheckDiceToLock();

        //PER OGNI MOSSA
        // SE POSSO FARLA, LOCKO I DADI NECESSARI PER FARLA : PROBABILITÁ 100%
        // RIPETERE LA STESSA MOSSA É SEMPRE PREFERIBILE A FARNE DUE DIVERSE
        // SE LA MOSSA CON PRIORITA' E'VICINA AD UN'ALTRA ATTIVAZIONE ALLORA LOCKA I DADI NECESSARI
        // (distanza massima: 1 mana rosso, 1 mana giallo)

        //WIP
        //get all possible diceCombination to lock(by mana type)
        DiceCombinationsCalculator();

        GetSkillActivationsSetsFromEachDiceCombination();

        //get all chances to do each diceCombination (by type)
        GetChanceOfEachDiceCombinationFromRoll();

        GetDamageAndEffectsFromEachActivationSet();
        //CheckMaxDamageBetweenSets(selectedCardData.ActivationsSetsData_List);

        //MaximizeSingleSkillCalculations();

        // // Qui ha deciso quali rerollare
        string debugText = "";
        for (int x = 0; x < diceRolled.Length; x++)
        {
            Dice d = diceRolled[x];
            debugText += $"Dice {x} - {d.Result} - {d.IsLocked}\n";
        }
        Debug.Log("EB - " + debugText);

    }
    #endregion

    #region < combinations chance calculations - WIP >

    public void DiceCombinationsCalculator()
    {
        selectedCardData.diceCombinationsSets_byManaType = new List<int[]>();
        selectedCardData.ActivationsSetsData_List = new List<TargetSkillsSetsData>();

        int i = 0;
        int j = 0;
        int k = 0;

        for(i = 0; i<=6; i++)
        {
            for(j=0; j<=6-i; j++)
            {
                k = 6 - j - i;

                int[] set = new int[3] {i,j,k};

                selectedCardData.diceCombinationsSets_byManaType.Add(set);
                TargetSkillsSetsData newSet = new TargetSkillsSetsData(set);
                selectedCardData.ActivationsSetsData_List.Add(newSet);
              
                //Debug.Log("Set probabilities after roll = " + set[0] + set[1] + set[2]);
            }

            j = 0;

        }
    }

    public void GetSkillActivationsSetsFromEachDiceCombination(/*List<TargetSkillsSetsData> _activationDataSets*/ )
    {
        //_activationDataSets = selectedCardData.ActivationsSetsData_List;
        //_skills = selectedCardSkills;

        selectedCardData.skillsActivationsSets_ofDiceCombinationSets = new List<int[]>();
        int debug = 0;

        foreach (TargetSkillsSetsData setData in selectedCardData.ActivationsSetsData_List)
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
            selectedCardData.skillsActivationsSets_ofDiceCombinationSets.Add(setData.skillSet_activationsArray);
        }
    }

    public void GetChanceOfEachDiceCombinationFromRoll(/*ActivationsSetsData_List _activationDataSets*/)
    {
        //_activationDataSets = selectedCardData.ActivationsSetsData_List;
        selectedCardData.setProbabilitiesAfterRoll_List = new List<float>();
        selectedCardData.totalDodgeInstances_Activated = new List<int>();

        string debugText = "";
        foreach (TargetSkillsSetsData setData in selectedCardData.ActivationsSetsData_List)
        {
            float _redDiceNeeded = setData.skillSetDiceCombination_byManaType[0] - diceRoll_RedMana;
            float _yellowDiceNeeded = setData.skillSetDiceCombination_byManaType[1] - diceRoll_YellowMana;
            float _blueDiceNeeded = setData.skillSetDiceCombination_byManaType[2] - diceRoll_BlueMana;

            float _probability_set_after_roll = (Mathf.Pow((1 / 2f), _redDiceNeeded > 0 ? _redDiceNeeded : 0))
                                                    * (Mathf.Pow((1 / 3f), _yellowDiceNeeded > 0 ? _yellowDiceNeeded : 0))
                                                    * (Mathf.Pow((1 / 6f), _blueDiceNeeded > 0 ? _blueDiceNeeded : 0));

            setData.skillSet_probabilityAfterRoll = _probability_set_after_roll;
            selectedCardData.setProbabilitiesAfterRoll_List.Add(_probability_set_after_roll);

            debugText += ("Set (" + setData.activationSetNumber + ") " +
                    "- skills (" + setData.skillSet_activationsArray[0]+ setData.skillSet_activationsArray[1] + setData.skillSet_activationsArray[2] +") " +
                    "- dice (" + setData.skillSetDiceCombination_byManaType[0] + setData.skillSetDiceCombination_byManaType[1] + setData.skillSetDiceCombination_byManaType[2]+") " +
                    "- probabilities after roll = " + setData.skillSet_probabilityAfterRoll) + "\n";

        }

        selectedCardData.ActivationsSetsData_List.Sort((y, x) => x.skillSet_probabilityAfterRoll.CompareTo(y.skillSet_probabilityAfterRoll));
        selectedCardData.setProbabilitiesAfterRoll_List.Reverse();

        //List<float> MaxChanceOfActivationSet = new List<float>();
        debugText += "\nTop 5 Sets with max probabilities after roll \n";
        for (int h = 0; h <=5; h++)
        {
            debugText +=
                "- Set Num(" + selectedCardData.ActivationsSetsData_List[h].activationSetNumber  + ") = " +
                "- skills (" + selectedCardData.ActivationsSetsData_List[h].skillSet_activationsArray[0] + selectedCardData.ActivationsSetsData_List[h].skillSet_activationsArray[1] + selectedCardData.ActivationsSetsData_List[h].skillSet_activationsArray[2] +") " +
                "- dice (" + selectedCardData.ActivationsSetsData_List[h].skillSetDiceCombination_byManaType[0] + selectedCardData.ActivationsSetsData_List[h].skillSetDiceCombination_byManaType[1] + selectedCardData.ActivationsSetsData_List[h].skillSetDiceCombination_byManaType[2] + ") " +
                "- probabilities after roll = " + selectedCardData.ActivationsSetsData_List[h].skillSet_probabilityAfterRoll + "\n";
        }

        Debug.Log("EB - " + debugText);

    }

    public void GetSetsWithMaxProbability()
    {


    }

    public void GetDamageAndEffectsFromEachActivationSet(/*ActivationsSetsData_List _activationDataSets*/)
    {
        //_activationDataSets = selectedCardData.ActivationsSetsData_List;
        selectedCardData.damageByEachSetOfActivations_List = new List<float>();
        selectedCardData.defencesByEachSetOfActivations_List = new List<float>();
        selectedCardData.preciseIstancesByEachSetOfActivation_List = new List<int[]>();
        selectedCardData.skillsDodgeIstancesValuesByEachSetOfActivation_List = new List<int[]>();

        string debugText = "";

        foreach (TargetSkillsSetsData setData in selectedCardData.ActivationsSetsData_List)
        {
            //new damageSEt con tutte attivazioni

            int skillsDamage = 0;
            int skillsDefence = 0;
            int preciseDamage = 0;
            int normalDamage = 0;
            //[0] = first skill istances, [1] = second skil istancesl, [2] = third skill istances
            int[] skillsPreciseIstances = new int[3] {0,0,0};

            //array dodge valido per intero set
            List<int> skillsDodge = new List<int>();

            for (int i = 0; i < setData.skillSet_activationsArray.Length; i++)
            {
                //get skill defence istances
                skillsDefence += (setData.skillSet_activationsArray[i] * selectedCardSkills[i].skillData.DefInstances);

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

            //compile BattlingCard Lists values
            selectedCardData.damageByEachSetOfActivations_List.Add(skillsDamage);
            selectedCardData.defencesByEachSetOfActivations_List.Add(skillsDefence);
            selectedCardData.preciseIstancesByEachSetOfActivation_List.Add(skillsPreciseIstances);
            selectedCardData.skillsDodgeIstancesValuesByEachSetOfActivation_List.Add(skillsDodge.ToArray());

            debugText += "\nACTIVATION SETS EFFECTS: \n";

            debugText += 
            " SetNum " + setData.activationSetNumber 
            + " - damage = " + setData.skillSet_totalDamage
            + " - defences = " + setData.skillSet_defenceInstances
            + " - dodge istances = " + setData.skillSet_dodgeInstances
            + " - precise istances = " + setData.skillSet_preciseInstances[0]+ setData.skillSet_preciseInstances[1]+ setData.skillSet_preciseInstances[2];

        }

        Debug.Log("EB - " + debugText);
    }

    public void CheckMaxDamageBetweenSets(List<TargetSkillsSetsData> _damageSets)
    {
        List<float> CheckMaxDamage = new List<float>();

        string debugText = "";
        foreach (var damageSet in _damageSets)
        {
            debugText += ("damage by set = " + damageSet.skillSet_totalDamage.ToString() + " - " + damageSet.skillSet_probabilityAfterRoll.ToString() + "\n");
            CheckMaxDamage.Add(damageSet.skillSet_totalDamage);
        }

        int MaxDamage = (int)(CheckMaxDamage.Max());
        Debug.Log("EB - " + debugText + "damage max between maxed sets = " + MaxDamage);
    }

    #endregion

    #region < maximize single skill calculations - OLD >

    public float GetMaximizedSingleSkillTargetActivationsChance(Skill _skill, int _targetActivations)
    {
        //targetActivations for maximized skills sets

        //DiceNeeded - DiceSpared =  skill_ManaCost_byType * targetActivations - diceRoll_byManaType;
        //DiceNeeded = skill_ManaCost_byType * (targetActivations - singleSkillActivationsDone);
        //DiceSpared = diceRoll_byManaType - singleSkillActivationsDone * skill_ManaCost_byType;

        float redDiceNeeded = _skill.skill_RedManaCost * _targetActivations - diceRoll_RedMana;

        float yellowDiceNeeded = _skill.skill_YellowManaCost * _targetActivations - diceRoll_YellowMana;

        float blueDiceNeeded = _skill.skill_BlueManaCost * _targetActivations - diceRoll_BlueMana;

        float probability_maxActivations_afterRoll = _skill.singleSkillActivationsDone < _targetActivations ?
                                                 ((Mathf.Pow((1 / 2f), _skill.skill_RedManaCost * _targetActivations > diceRoll_RedMana ? (redDiceNeeded) : 0))
                                                * (Mathf.Pow((1 / 3f), _skill.skill_YellowManaCost * _targetActivations > diceRoll_YellowMana ? (yellowDiceNeeded) : 0))
                                                * (Mathf.Pow((1 / 6f), _skill.skill_BlueManaCost * _targetActivations > diceRoll_BlueMana ? (blueDiceNeeded) : 0))) : 0;

        //probability of maximized single skill
        return probability_maxActivations_afterRoll;
    }
    [SerializeField] private TargetSkillsSetsData activatedSet_afterFirstRoll;
    public void MaximizeSingleSkillCalculations()
    { 
        #region skill data pre-roll

        //foreach(Skill skill in selectedCardSkills)

        //probabilità che escano dadi per ripetere la skill max_activations volte a partire da nessun roll
        float probability1_maxActivations = Mathf.Pow(((Mathf.Pow((1 / 2f), (firstSkill.skill_RedManaCost))) 
                                                     * (Mathf.Pow((1 / 3f), (firstSkill.skill_YellowManaCost))) 
                                                     * (Mathf.Pow((1 / 6f), (firstSkill.skill_BlueManaCost)))), firstSkill.skill_maximumActivations);

        float probability2_maxActivations = Mathf.Pow(((Mathf.Pow((1 / 2f), (secondSkill.skill_RedManaCost)))
                                                     * (Mathf.Pow((1 / 3f), (secondSkill.skill_YellowManaCost)))
                                                     * (Mathf.Pow((1 / 6f), (secondSkill.skill_BlueManaCost)))), secondSkill.skill_maximumActivations);

        float probability3_maxActivations = Mathf.Pow(((Mathf.Pow((1 / 2f), (thirdSkill.skill_RedManaCost)))
                                                     * (Mathf.Pow((1 / 3f), (thirdSkill.skill_YellowManaCost)))
                                                     * (Mathf.Pow((1 / 6f), (thirdSkill.skill_BlueManaCost)))), thirdSkill.skill_maximumActivations);

        //skill base damage without activations from roll (damage * base_attack_instances)
        float base_damage1 = firstSkill.skillData.AtkInstances * firstSkill.skillData.Damage;
        float base_damage2 = secondSkill.skillData.AtkInstances * secondSkill.skillData.Damage;
        float base_damage3 = thirdSkill.skillData.AtkInstances * thirdSkill.skillData.Damage;

        #endregion

        #region after roll calculations

        //DANNO DOPO PRIMO ROLL
        //skill base_damage multiplied by number of activations from first roll (activations * base_damage)
        float damage1 = firstSkill.singleSkillActivationsDone * firstSkill.skillData.AtkInstances * firstSkill.skillData.Damage;
        float damage2 = secondSkill.singleSkillActivationsDone * secondSkill.skillData.AtkInstances * secondSkill.skillData.Damage;
        float damage3 = thirdSkill.singleSkillActivationsDone * thirdSkill.skillData.AtkInstances * thirdSkill.skillData.Damage;

        selectedCardData.totalDamageDone_onRoll = (int)(damage1 + damage2 + damage3);
        //totalDamage_AfterFirstRoll = selectedCardData.totalDamageDone_onRoll;

        activatedSet_afterFirstRoll = new TargetSkillsSetsData(new int[] { diceRoll_RedMana, diceRoll_YellowMana, diceRoll_BlueMana }, selectedCardData.totalDamageDone_onRoll);

        //here get all atk def, dodge, ecc
        activatedSet_afterFirstRoll.skillSet_activationsArray = new int[] { firstSkill.singleSkillActivationsDone, secondSkill.singleSkillActivationsDone, thirdSkill.singleSkillActivationsDone };

        //CREATE SETS WITH RESPECTIVE DAMAGE
        List<TargetSkillsSetsData> singleSkillMaximized_TargetSkillSets = new List<TargetSkillsSetsData>();
        //ADD TO LIST TO CHECK MAX DAMAGE OF ROLLED COMBINATION
        singleSkillMaximized_TargetSkillSets.Add(activatedSet_afterFirstRoll);

        //maximize activations of single skill
        //
        List<float> maximizedSkills_activationsChances_List = new List<float>();

        string debugText = "";
        for (int i=0; i< selectedCardSkills.Length; i++)
        {
            float maximizedSkill_activationChance = GetMaximizedSingleSkillTargetActivationsChance(selectedCardSkills[i], selectedCardSkills[i].skill_maximumActivations);

            int baseDamage =  selectedCardSkills[i].skillData.AtkInstances * selectedCardSkills[i].skillData.Damage;

            int doneDamage = baseDamage * selectedCardSkills[i].singleSkillActivationsDone;
            int toRollDamage =  baseDamage *  (selectedCardSkills[i].skill_maximumActivations - selectedCardSkills[i].singleSkillActivationsDone) /**maximizedSkill_activationChance */;

            float damageToRollByChance = toRollDamage * maximizedSkill_activationChance;

            //peso del danno = danno iterazioni mancanti * chance + danno iterazioni fatte
            int damageTotalChanceFormulax = (int)(baseDamage * (selectedCardSkills[i].singleSkillActivationsDone + maximizedSkill_activationChance * (selectedCardSkills[i].skill_maximumActivations - selectedCardSkills[i].singleSkillActivationsDone)));
            int damageTotalChanceFormula = (int)(doneDamage + damageToRollByChance);

            //create activation set with dice set array
            //here i set skillSet_totalDamage as damage multiplied by missing activations chance + damage done
            TargetSkillsSetsData targetMaxSet_afterFirstRoll = new TargetSkillsSetsData(new int[] { selectedCardSkills[i].skill_maximumActivations * selectedCardSkills[i].skill_RedManaCost, selectedCardSkills[i].skill_maximumActivations * selectedCardSkills[i].skill_YellowManaCost, selectedCardSkills[i].skill_maximumActivations * selectedCardSkills[i].skill_BlueManaCost }, damageTotalChanceFormula);
            targetMaxSet_afterFirstRoll.skillSet_activationsArray[i] = (GetSingleSkillActivationsFromDiceManaCombo(selectedCardSkills[i], targetMaxSet_afterFirstRoll.skillSetDiceCombination_byManaType));
            targetMaxSet_afterFirstRoll.skillSet_probabilityAfterRoll = maximizedSkill_activationChance;           
            targetMaxSet_afterFirstRoll.skillSet_damageMultipliedByChance = damageToRollByChance;

            maximizedSkills_activationsChances_List.Add(maximizedSkill_activationChance);

            singleSkillMaximized_TargetSkillSets.Add(targetMaxSet_afterFirstRoll);

            debugText += "skill_" + (i + 1) + "_maxed chance = " + maximizedSkill_activationChance + "_maxed damage = " + damageTotalChanceFormula + " -- " + damageTotalChanceFormulax + "\n";
        }

        Debug.Log("EB - " + debugText + "damage with this roll = " + selectedCardData.totalDamageDone_onRoll);

        //CHECK MAX DAMAGE BETWEEN ROLLED AND MAXIMIZED ACTIVATIONS FOR EACH SKILL
        //
        CheckMaxDamageBetweenSets(singleSkillMaximized_TargetSkillSets);

        #endregion
    }

    #endregion

    private float behaviourLimit;

    public void CheckSkillBehaviour()
    {
        //check if total number of abilities activated is less than

        if(selectedCardData.totalDamageDone_onRoll < behaviourLimit)
        {
            if (selectedCardData.skillsActivatedSet_onRoll[0] != 0)
            {

            }
        }

        //check activations of first ability -> if behaviour is focused on first skill
        if (selectedCardData.skillsActivatedSet_onRoll[0] != 0)
        {

        }

        //check activations of second ability -> if behaviour is focused on second skill
        if (selectedCardData.skillsActivatedSet_onRoll[1] != 0)
        {

        }

        //check activations of third ability -> if behaviour is focused on third skill
        if (selectedCardData.skillsActivatedSet_onRoll[2] != 0)
        {

        }

        //check if defence instances of chosen skill are above behaviour limit
        if(selectedCardData.totalDefenceInstances_Activated < behaviourLimit)
        {

        }




    }

}
