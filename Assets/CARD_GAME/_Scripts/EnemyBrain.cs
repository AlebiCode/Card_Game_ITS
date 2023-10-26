using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
//using System.Diagnostics;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;

public class EnemyBrain : MonoBehaviour
{
    [Header("DICES ROLL OUTCOME")]
    [SerializeField] private Dice[] diceRolled;
    [SerializeField] private Dice.diceFace[] diceRolled_faces;

    [SerializeField] private int diceRoll_RedMana = 0;
    [SerializeField] private int diceRoll_YellowMana = 0;
    [SerializeField] private int diceRoll_BlueMana = 0;
    [SerializeField] private int[] diceRoll_byManaColor;

    [Header("CARD")]
    //[SerializeField] private Card enemy_SelectedCard;
    [SerializeField] private Card[] enemy_cards;
    [SerializeField] private Card selected_card;
    [SerializeField] private CardSelected battlingCardData;

    //TO DO: metto skills dentro card
    [Header("SKILLS")]
    [SerializeField] private Skill First_Skill;
    [SerializeField] private Skill Second_Skill;
    [SerializeField] private Skill Third_Skill;
    [SerializeField] private Skill[] CurrentCard_Skills;

    [Header("DAMAGE")]
    [SerializeField] private float TotalDamageAfterFirstRoll;

    [Serializable()]
    public class CardSelected
    {
        public Card battlingCard;
        public Skill[] currentCardSkills;

        public CardSelected(Card _selected_card)
        {
            battlingCard = _selected_card;

        }

        public int[] NumberOfSkillsActivatedSet_afterFirstRoll;
        public int total_DamageDone_afterRoll;
        public int total_PrecisionDamage_afterRoll;
        public int total_NormalDamage_afterRoll;
        public int total_defenceInstances_Activated ;
        public int[] total_preciseIstances_Activated ;
        public List<int> total_dodgeInstances_Activated;


        public List<ActivationSetsData> ActivationsSetsDataList;

        public List<float> setProbabilitiesAfterRoll_List;
        public List<int[]> diceCombinationsSets_byManaType;
        public List<int[]> skillsActivationsSets_ofDiceCombinationSets;
        public List<float> damageByEachSetOfActivations_List;
        public List<float> defencesByEachSetOfActivations_List ;
        public List<int[]> preciseIstances_List;
        public List<int[]> skillsDodgeIstancesValues_List;
    }
    
    [Serializable()]
    public class Skill
    {
        public SkillData skillData;
        public List<Dice.diceFace> skill_ManaCost_list;

        public Skill(SkillData _skillData)
        {
            skillData = _skillData;
            skill_ManaCost_list = _skillData.Skill_colorCost;
        }

        [Header("Skill Mana Cost")]
        public int skill_totalManaCost = 0;
        public int skill_RedManaCost = 0;
        public int skill_YellowManaCost = 0;
        public int skill_BlueManaCost = 0;
        public int[] skill_ManaCost_byManaType;

        [Header("Activations")]
        public int skill_maximum_activations;
        public List<int> activations_onRoll_byManaType;
        public int NumberOfSkillActivations = 0;

        public int attackInstances_Activated = 0;
        public int defenceInstances_Activated = 0;
        public bool isAttackPrecise=false;
        public int preciseIstances_Activated = 0;
        public List<int> dodgeInstances_Activated;

        public int skillDamageDone_byAttackInstancesActivated = 0;
    }

    [Serializable()]
    public class ActivationSetsData
    {
        public ActivationSetsData(int[] _diceComboSet)
        {
            diceCombinationSet_byManaType = _diceComboSet;
        }

        //questo per test maximized
        public ActivationSetsData(float _damage, int[] _diceComboSet)
        {
            diceCombinationSet_byManaType = _diceComboSet;
            //occhio qua era skillactiv_sets

            total_damage = _damage;
        }

        //[0] = first skill activations, [1] = second skill activations, [2] = third skill activations
        public int[] skill_activation_set;

        //[0] = red mana faces, [1] = yellow mana faces, [2] = blue mana faces
        public int[] diceCombinationSet_byManaType;
        public float set_probability_preRoll;
        public float set_probability_afterRoll;

        //somma del danno di ogni abilità per le rispettive istanze e attivazioni del set
        public float total_damage;
        public float damageMultipliedByChance;
        public int defence_instances;
        public int[] precise_instances;
        public int[] dodge_instances_values /*= new int[] {}*/;

        //public float damage_toDo;
        //public float damage_done_withActivs;
    }

    public void RollDiceButtonTest()
    {
        RollDices();

        GetRolledDiceFaces();
    }

    public void CalculateSkillActivationsButtonTest()
    {
        GetCardDataAndSkills();

        GetRolledActivationsAndDamageFromEachSkill();

        AfterRollDamageFormula();
    }

    public void CalculateChanceButtonTest()
    {
        DiceCombinationsCalculator();

        //get all chances to do each diceCombination (by type)
        GetChanceOfEachDiceCombinationFromRoll();

        GetSkillActivationsSetsFromEachDiceCombination(/*battlingCardData.ActivationsSetsDataList*/);

        GetDamageAndEffectsFromEachActivationSet(/*battlingCardData.ActivationsSetsDataList*/);

        CheckMaxDamageBetweenSets(battlingCardData.ActivationsSetsDataList);

    }

    public void RollDices()
    {
        foreach (Dice die in diceRolled)
        {
            die.RollDice();
        }
    }
   
    public void GetDice()
    {
        
        diceRolled = BattleManager.instance.EnemyDices;

    }

    public void GetCardDataAndSkills()
    {
        //get skills from card and reset values of roll
        battlingCardData = new CardSelected(BattleManager.instance.EnemySelectedCard);

        First_Skill = new Skill(battlingCardData.battlingCard.CardData.Skills[0]);
        Second_Skill = new Skill(battlingCardData.battlingCard.CardData.Skills[1]);
        Third_Skill = new Skill(battlingCardData.battlingCard.CardData.Skills[2]);
        CurrentCard_Skills =  new Skill[3] { First_Skill, Second_Skill, Third_Skill };
        battlingCardData.currentCardSkills = CurrentCard_Skills;


    }

    public void GetRolledDiceFaces()
    {
        #region test
        /*
        foreach (Dice.diceFace face in diceRolled_faces) //delete this line and remove comments below outside testing
        {
            switch (face)
            {
                case Dice.diceFace.notRolled:
                    Debug.Log("face not rolled :(");
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
        */

        #endregion

        //reset dice faces count
        diceRolled_faces = new Dice.diceFace[6];
        diceRoll_RedMana = 0;
        diceRoll_YellowMana = 0;
        diceRoll_BlueMana = 0;
        diceRoll_byManaColor = new int[3] { 0, 0, 0 };

        foreach (Dice die in diceRolled)
        {
            diceRolled_faces.Append(die.Result);

            switch (die.Result)
            {
                case Dice.diceFace.notRolled:
                    Debug.Log("face not rolled :(");
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

        Debug.Log(diceRoll_byManaColor.ToString());
    }

    public void GetSingleSkillManaCost(Skill _skill)
    {
        _skill.skill_totalManaCost = _skill.skill_ManaCost_list.Count;

        foreach (var face in _skill.skill_ManaCost_list)
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

        _skill.skill_ManaCost_byManaType = new int[3] { _skill.skill_RedManaCost , _skill.skill_YellowManaCost , _skill.skill_BlueManaCost };

        _skill.skill_maximum_activations = (int) (6 / (_skill.skill_totalManaCost));

        Debug.Log(_skill.skill_ManaCost_byManaType.ToString());
    }

    public void GetSingleSkillActivationsAfterFirstRoll(Skill _skill)
    {
        _skill.activations_onRoll_byManaType = new List<int>();

        for (int i = 0; i < 3; i++)
        {
            if (_skill.skill_ManaCost_byManaType[i] != 0)
            {
                int activs = (int)(diceRoll_byManaColor[i] / _skill.skill_ManaCost_byManaType[i]);

                _skill.activations_onRoll_byManaType.Add(activs);
            }
        }

        //get skill final activations
        _skill.NumberOfSkillActivations = _skill.activations_onRoll_byManaType.Min();

        //add to battlingCard list of skill activated set --> confront with other sets probabilities
        battlingCardData.NumberOfSkillsActivatedSet_afterFirstRoll.Append(_skill.NumberOfSkillActivations);
    }

    public void GetDamageAndEffectsActivatedForSingleSkillAfterRoll(Skill _skill)
    {
        //get attack instances 
        _skill.attackInstances_Activated = 0;
        _skill.attackInstances_Activated = _skill.skillData.AtkInstances * _skill.NumberOfSkillActivations;

        //get precise and normal damage istances
        _skill.preciseIstances_Activated = 0;
        if (_skill.isAttackPrecise)
        {
            _skill.preciseIstances_Activated = _skill.attackInstances_Activated;
            battlingCardData.total_PrecisionDamage_afterRoll += _skill.skillDamageDone_byAttackInstancesActivated;
        }
        else
        {
            battlingCardData.total_NormalDamage_afterRoll += _skill.skillDamageDone_byAttackInstancesActivated;
        }
        battlingCardData.total_preciseIstances_Activated.Append(_skill.preciseIstances_Activated);

        //get skill defence istances
        _skill.defenceInstances_Activated = 0;
        _skill.defenceInstances_Activated = _skill.skillData.DefInstances * _skill.NumberOfSkillActivations;
        battlingCardData.total_defenceInstances_Activated += _skill.defenceInstances_Activated;

        //get skill dodge istances
        _skill.dodgeInstances_Activated = new List<int>();
        if (_skill.skillData.Dodge > 0)
        {
            for (int i = 0; i < _skill.NumberOfSkillActivations; i++)
            {
                _skill.dodgeInstances_Activated.Add(_skill.skillData.Dodge);
            }
        }
        battlingCardData.total_dodgeInstances_Activated.AddRange(_skill.dodgeInstances_Activated);

        //get damage done by single skill activs
        _skill.skillDamageDone_byAttackInstancesActivated = 0;
        _skill.skillDamageDone_byAttackInstancesActivated = (_skill.skillData.Damage * _skill.attackInstances_Activated);

        //set total damage done by all skill activs
        battlingCardData.total_DamageDone_afterRoll = battlingCardData.total_PrecisionDamage_afterRoll + battlingCardData.total_NormalDamage_afterRoll;
    }

    public void GetRolledActivationsAndDamageFromEachSkill()
    {
        battlingCardData.total_preciseIstances_Activated = new int[3] ;
        battlingCardData.NumberOfSkillsActivatedSet_afterFirstRoll = new int[3];

        for (int i = 0; i < CurrentCard_Skills.Length; i++)
        //foreach (Skill skill in CurrentCard_Skills)
        {
            GetSingleSkillManaCost(CurrentCard_Skills[i]);

            GetSingleSkillActivationsAfterFirstRoll(CurrentCard_Skills[i]);

            GetDamageAndEffectsActivatedForSingleSkillAfterRoll(CurrentCard_Skills[i]);

        }

        Debug.Log($"ROLLED EFFECTS - damage =  {battlingCardData.total_DamageDone_afterRoll}" +
                                 $"- defences = {battlingCardData.total_defenceInstances_Activated}" +
                                 $"- dodge istances = {battlingCardData.total_dodgeInstances_Activated.Count}" +
                                 $"- precise istances = {battlingCardData.total_preciseIstances_Activated[0]}" +
                                 $"                       {battlingCardData.total_preciseIstances_Activated[1]}" +
                                 $"                      {battlingCardData.total_preciseIstances_Activated[2]}");
    
    }

    private int GetLockedDicesByColor(Dice.diceFace color)
    {
        int n = 0;
        for (int x=0; x<diceRolled.Length; x++)
        {
            if (diceRolled[x].Result == color&& diceRolled[x].IsLocked)
            {
                n++;
            }
        }
        return n;
    }

    public void CheckDiceToLock()
    {
        //PRIORITÁ DELLE MOSSE: ORDINAMENTO
        //different skill priority orders
        List<Skill> skillsByPriority = new List<Skill>();

        skillsByPriority.Add(First_Skill);
        skillsByPriority.Add(Second_Skill);
        skillsByPriority.Add(Third_Skill);

        List<Dice> diceToLock = new List<Dice>();

        foreach (Skill s in skillsByPriority)
        {
            List<Dice.diceFace> skillCost = s.skillData.Skill_colorCost;
            GetSingleSkillActivationsAfterFirstRoll(s);
            int numberOfActivations = s.NumberOfSkillActivations;
            if (numberOfActivations == 0)
                continue;

            int redDiceLocked = GetLockedDicesByColor(Dice.diceFace.red);
            for (int y = 0; y < s.skill_RedManaCost * numberOfActivations; y++)
            {
                if (redDiceLocked == s.skill_RedManaCost * numberOfActivations)
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
                        if (redDiceLocked == s.skill_RedManaCost * numberOfActivations)
                            break;
                    }
                }
            }

            int yellowDiceLocked = GetLockedDicesByColor(Dice.diceFace.yellow);
            for (int y = 0; y < s.skill_YellowManaCost * numberOfActivations; y++)
            {
                if (yellowDiceLocked == s.skill_YellowManaCost * numberOfActivations)
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
                        if (yellowDiceLocked == s.skill_YellowManaCost * numberOfActivations)
                            break;
                    }
                }
            }

            int blueDiceLocked = GetLockedDicesByColor(Dice.diceFace.blue);
            for (int y = 0; y < s.skill_BlueManaCost * numberOfActivations; y++)
            {
                if (blueDiceLocked == s.skill_BlueManaCost * numberOfActivations)
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
                        if (blueDiceLocked == s.skill_BlueManaCost * numberOfActivations)
                            break;
                    }
                }
            }
        }
    }

    public void TurnLoop()
    {
        //START
        //ROLL DICE
        //RollDices();

        GetDice();

        GetRolledDiceFaces();

        GetCardDataAndSkills();

        GetRolledActivationsAndDamageFromEachSkill();

        //check and lock dice
        CheckDiceToLock();

        //PER OGNI MOSSA
        // SE POSSO FARLA, LOCKO I DADI NECESSARI PER FARLA : PROBABILITÁ 100%
        // RIPETERE LA STESSA MOSSA É SEMPRE PREFERIBILE A FARNE DUE DIVERSE

        //REROLLO TUTTI I DADI CHE NON SONO LOCKATI

        // da qui inizia a decidere quali lockare

        // //CHECK ROLLED EFFECTS
        // //first check of damage with roll
        // GetRolledActivationsAndDamageFromEachSkill();

        // //CALCULATE POSSIBLE COMBINATIONS FOR LOCK
        // //get all possible diceCombination(by mana type)
        // DiceCombinationsCalculator();

        // //get all chances to do each diceCombination (by type)
        // GetChanceOfEachDiceCombinationFromRoll();

        // GetSkillActivationsSetsFromEachDiceCombination(/*battlingCardData.ActivationsSetsDataList*/);

        // GetDamageAndEffectsFromEachActivationSet(/*battlingCardData.ActivationsSetsDataList*/);

        // CheckMaxDamageBetweenSets(battlingCardData.ActivationsSetsDataList);

        // // Qui ha deciso quali rerollare

        for (int x = 0; x < diceRolled.Length; x++)
        {
            Dice d = diceRolled[x];
            Debug.Log($"Dice {x} - {d.Result} - {d.IsLocked}");
        }


        AfterRollDamageFormula();
    }

    public void DiceCombinationsCalculator()
    {
        battlingCardData.diceCombinationsSets_byManaType = new List<int[]>();
        battlingCardData.ActivationsSetsDataList = new List<ActivationSetsData>();

        int i = 0;
        int j = 0;
        int k = 0;

        for(i = 0; i<=6; i++)
        {
            for(j=0; j<=6-i; j++)
            {
                k = 6 - j - i;

                int[] set = new int[3] {i,j,k};

                battlingCardData.diceCombinationsSets_byManaType.Add(set);
                ActivationSetsData newSet = new ActivationSetsData(set);
                battlingCardData.ActivationsSetsDataList.Add(newSet);
              
                Debug.Log("Set probabilities after roll = " + set[0] + set[1] + set[2]);
            }

            j = 0;

        }
    }

    public void GetChanceOfEachDiceCombinationFromRoll(/*ActivationsSetsDataList _activationDataSets*/)
    {

        //_activationDataSets = battlingCardData.ActivationsSetsDataList;
        battlingCardData.setProbabilitiesAfterRoll_List = new List<float>();
        battlingCardData.total_dodgeInstances_Activated = new List<int>();
        int debug = 0;

        foreach (ActivationSetsData setData in battlingCardData.ActivationsSetsDataList)
        {
            float _redDiceNeeded = setData.diceCombinationSet_byManaType[0] - diceRoll_RedMana;

            float _yellowDiceNeeded = setData.diceCombinationSet_byManaType[1] - diceRoll_YellowMana;

            float _blueDiceNeeded = setData.diceCombinationSet_byManaType[2] - diceRoll_BlueMana;

            float _probability_set_after_roll = (Mathf.Pow((1 / 2f), _redDiceNeeded > 0 ? _redDiceNeeded : 0))
                                                    * (Mathf.Pow((1 / 3f), _yellowDiceNeeded > 0 ? _yellowDiceNeeded : 0))
                                                    * (Mathf.Pow((1 / 6f), _blueDiceNeeded > 0 ? _blueDiceNeeded : 0));

            setData.set_probability_afterRoll = _probability_set_after_roll;
            battlingCardData.setProbabilitiesAfterRoll_List.Add(_probability_set_after_roll);

            debug++;
            Debug.Log("Set (" + debug + ") " +
                    "- skills (" + setData.skill_activation_set + ") " +
                    "- dice (" + setData.diceCombinationSet_byManaType +") " +
                    "- probabilities after roll = " + setData.set_probability_afterRoll);

        }

        battlingCardData.setProbabilitiesAfterRoll_List.Sort();
        //List<float> MaxChanceOfActivationSet = new List<float>();

        for (int d = 5; d >= 1; d--)
        {
            Debug.Log("max 5 probabilities after roll " + battlingCardData.setProbabilitiesAfterRoll_List[battlingCardData.setProbabilitiesAfterRoll_List.Count-d]);
        }
    }

    public void GetSkillActivationsSetsFromEachDiceCombination(/*List<ActivationSetsData> _activationDataSets , Skill[] _skills*/)
    {
        //_activationDataSets = battlingCardData.ActivationsSetsDataList;
        //_skills = CurrentCard_Skills;

        battlingCardData.skillsActivationsSets_ofDiceCombinationSets = new List<int[]>();

        foreach (ActivationSetsData setData in battlingCardData.ActivationsSetsDataList)
        {
            int[] skillsActivationsSets = new int[3];

            foreach (Skill skill in CurrentCard_Skills)
            {
                List<int> activationsByMana = new List<int>();

                for (int i = 0; i < setData.diceCombinationSet_byManaType.Length; i++)
                {
                    if (skill.skill_ManaCost_byManaType[i] != 0)
                    {
                        int activs = (int)(setData.diceCombinationSet_byManaType[i] / skill.skill_ManaCost_byManaType[i]);

                        activationsByMana.Add(activs);
                        //activsByMana[i] = activs;
                    }
                }

                int singleSkillActivationsForSet = activationsByMana.Min();

                skillsActivationsSets.Append(singleSkillActivationsForSet);
            }

            setData.skill_activation_set = skillsActivationsSets;
            battlingCardData.skillsActivationsSets_ofDiceCombinationSets.Add(skillsActivationsSets);
        }
    }

    public void GetDamageAndEffectsFromEachActivationSet(/*ActivationsSetsDataList _activationDataSets*/)
    {
        //_activationDataSets = battlingCardData.ActivationsSetsDataList;
        battlingCardData.damageByEachSetOfActivations_List = new List<float>();
        battlingCardData.defencesByEachSetOfActivations_List = new List<float>();
        battlingCardData.preciseIstances_List = new List<int[]>();
        battlingCardData.skillsDodgeIstancesValues_List = new List<int[]>();

        int debug = 0;

        foreach (ActivationSetsData setData in battlingCardData.ActivationsSetsDataList)
        {
            //new damageSEt con tutte attivazioni

            float skillsDamage = 0f;
            int skillsDefence = 0;

            //[0] = first skill istances, [1] = second skil istancesl, [2] = third skill istances
            int[] skillsPreciseIstances = new int[3] {0,0,0};

            //array dodge valido per intero set
            int[] skillsDodge = new int[] {};

            for (int i = 0; i < setData.skill_activation_set.Length; i++)
            {

                skillsDamage += (setData.skill_activation_set[i] * CurrentCard_Skills[i].skillData.Damage * CurrentCard_Skills[i].skillData.AtkInstances);

                //get skill defence istances
                skillsDefence += (setData.skill_activation_set[i] * CurrentCard_Skills[i].skillData.DefInstances);

                if (CurrentCard_Skills[i].isAttackPrecise)
                {
                    skillsPreciseIstances[i] += setData.skill_activation_set[i];
                }

                //get skill dodge istances
                if (CurrentCard_Skills[i].skillData.Dodge > 0)
                {
                    //aggiungi un valore di dodge per ogni attivazione della skill
                    for (int j = 0; j < setData.skill_activation_set[i]; j++)
                    {
                        skillsDodge.Append(CurrentCard_Skills[i].skillData.Dodge);
                    }
                }
            }

            //compile each ActivationSetsData with own values
            setData.total_damage = skillsDamage;
            setData.defence_instances = skillsDefence;
            setData.precise_instances = skillsPreciseIstances;
            setData.dodge_instances_values = skillsDodge;

            //compile BattlingCard Lists values
            battlingCardData.damageByEachSetOfActivations_List.Add(skillsDamage);
            battlingCardData.defencesByEachSetOfActivations_List.Add(skillsDefence);
            battlingCardData.preciseIstances_List.Add(skillsPreciseIstances);
            battlingCardData.skillsDodgeIstancesValues_List.Add(skillsDodge);

            debug++;
            Debug.Log("ACTIVATIONSETS EFFECTS " + debug 
            + " - damage = " + setData.total_damage.ToString()
            + " - defences = " + setData.defence_instances.ToString()
            + " - dodge istances = " + setData.dodge_instances_values.ToString()
            + " - precise istances = " + setData.precise_instances[0].ToString()
            + setData.precise_instances[1].ToString()
            + setData.precise_instances[2].ToString());
        }
    }

    //maxmized skills only
    public float GetMaximizedSingleSkillTargetActivationsChance(Skill _skill, int _targetActivations)
    {
        //targetActivations for maximized skills sets

        //DiceNeeded - DiceSpared =  skill_ManaCost_byType * targetActivations - diceRoll_byManaType;
        //DiceNeeded = skill_ManaCost_byType * (targetActivations - NumberOfSkillActivations);
        //DiceSpared = diceRoll_byManaType - NumberOfSkillActivations * skill_ManaCost_byType;

        float redDiceNeeded = _skill.skill_RedManaCost * _targetActivations - diceRoll_RedMana;

        float yellowDiceNeeded = _skill.skill_YellowManaCost * _targetActivations - diceRoll_YellowMana;

        float blueDiceNeeded = _skill.skill_BlueManaCost * _targetActivations - diceRoll_BlueMana;

        float probability_maxActivations_afterRoll = _skill.NumberOfSkillActivations < _targetActivations ?
                                                 ((Mathf.Pow((1 / 2f), _skill.skill_RedManaCost * _targetActivations > diceRoll_RedMana ? (redDiceNeeded) : 0))
                                                * (Mathf.Pow((1 / 3f), _skill.skill_YellowManaCost * _targetActivations > diceRoll_YellowMana ? (yellowDiceNeeded) : 0))
                                                * (Mathf.Pow((1 / 6f), _skill.skill_BlueManaCost * _targetActivations > diceRoll_BlueMana ? (blueDiceNeeded) : 0))) : 0;

        //probability of maximized single skill
        return probability_maxActivations_afterRoll;
    }    

    public void AfterRollDamageFormula()
    { 
        #region skill data pre-roll

        //foreach(Skill skill in CurrentCard_Skills)

        //probabilità che escano dadi per ripetere la skill max_activations volte a partire da nessun roll
        float probability1_maxActivations = Mathf.Pow(((Mathf.Pow((1 / 2f), (First_Skill.skill_RedManaCost))) 
                                                     * (Mathf.Pow((1 / 3f), (First_Skill.skill_YellowManaCost))) 
                                                     * (Mathf.Pow((1 / 6f), (First_Skill.skill_BlueManaCost)))), First_Skill.skill_maximum_activations);

        float probability2_maxActivations = Mathf.Pow(((Mathf.Pow((1 / 2f), (Second_Skill.skill_RedManaCost)))
                                                     * (Mathf.Pow((1 / 3f), (Second_Skill.skill_YellowManaCost)))
                                                     * (Mathf.Pow((1 / 6f), (Second_Skill.skill_BlueManaCost)))), Second_Skill.skill_maximum_activations);

        float probability3_maxActivations = Mathf.Pow(((Mathf.Pow((1 / 2f), (Third_Skill.skill_RedManaCost)))
                                                     * (Mathf.Pow((1 / 3f), (Third_Skill.skill_YellowManaCost)))
                                                     * (Mathf.Pow((1 / 6f), (Third_Skill.skill_BlueManaCost)))), Third_Skill.skill_maximum_activations);

        //skill base damage without activations from roll (damage * base_attack_instances)
        float base_damage1 = First_Skill.skillData.AtkInstances * First_Skill.skillData.Damage;
        float base_damage2 = Second_Skill.skillData.AtkInstances * Second_Skill.skillData.Damage;
        float base_damage3 = Third_Skill.skillData.AtkInstances * Third_Skill.skillData.Damage;

        #endregion

        #region after roll calculations

        //DANNO DOPO PRIMO ROLL
        //skill base_damage multiplied by number of activations from first roll (activations * base_damage)
        float damage1 = First_Skill.NumberOfSkillActivations * First_Skill.skillData.AtkInstances * First_Skill.skillData.Damage;
        float damage2 = Second_Skill.NumberOfSkillActivations * Second_Skill.skillData.AtkInstances * Second_Skill.skillData.Damage;
        float damage3 = Third_Skill.NumberOfSkillActivations * Third_Skill.skillData.AtkInstances * Third_Skill.skillData.Damage;

        battlingCardData.total_DamageDone_afterRoll = (int)(damage1 + damage2 + damage3);
        TotalDamageAfterFirstRoll = battlingCardData.total_DamageDone_afterRoll;

        //CREATE SETS WITH RESPECTIVE DAMAGE
        List<ActivationSetsData> singleSkillMaximized_ActivationDataSets = new List<ActivationSetsData>();

        ActivationSetsData activatedSet_afterFirstRoll = new ActivationSetsData(battlingCardData.total_DamageDone_afterRoll, new int[] { diceRoll_RedMana, diceRoll_YellowMana, diceRoll_BlueMana });

        //here get all atk def, dodge, ecc
        activatedSet_afterFirstRoll.skill_activation_set = new int[] { First_Skill.NumberOfSkillActivations, Second_Skill.NumberOfSkillActivations, Third_Skill.NumberOfSkillActivations };

        //ADD TO LIST TO CHECK MAX DAMAGE OF ROLLED COMBINATION
        singleSkillMaximized_ActivationDataSets.Add(activatedSet_afterFirstRoll);

        //maximize activations of single skill
        //
        List<float> maximizedSkills_activationsChances_List = new List<float>();

        for (int i=0; i< CurrentCard_Skills.Length; i++)
        {
            float maximizedSkill_activationChance = GetMaximizedSingleSkillTargetActivationsChance(CurrentCard_Skills[i], CurrentCard_Skills[i].skill_maximum_activations);

            float baseDamage =  CurrentCard_Skills[i].skillData.AtkInstances * CurrentCard_Skills[i].skillData.Damage;

            float doneDamage = baseDamage * CurrentCard_Skills[i].NumberOfSkillActivations;
            float toRollDamage =  baseDamage *  (CurrentCard_Skills[i].skill_maximum_activations - CurrentCard_Skills[i].NumberOfSkillActivations) /**maximizedSkill_activationChance */;

            float damageToRollByChance = toRollDamage * maximizedSkill_activationChance;

            //peso del danno = danno iterazioni mancanti * chance + danno iterazioni fatte
            float damageTotalChanceFormulax = baseDamage * (CurrentCard_Skills[i].NumberOfSkillActivations + maximizedSkill_activationChance * (CurrentCard_Skills[i].skill_maximum_activations - CurrentCard_Skills[i].NumberOfSkillActivations));
            float damageTotalChanceFormula = doneDamage + damageToRollByChance;

            //create activation set with dice set array
            //here i set total_damage as damage multiplied by missing activations chance + damage done
            ActivationSetsData activation_Set_afterFirstRoll = new ActivationSetsData(damageTotalChanceFormula, new int[] { CurrentCard_Skills[i].skill_maximum_activations * CurrentCard_Skills[i].skill_RedManaCost, CurrentCard_Skills[i].skill_maximum_activations * CurrentCard_Skills[i].skill_YellowManaCost, CurrentCard_Skills[i].skill_maximum_activations * CurrentCard_Skills[i].skill_BlueManaCost });
            
            activation_Set_afterFirstRoll.set_probability_afterRoll = maximizedSkill_activationChance;

            activation_Set_afterFirstRoll.damageMultipliedByChance = damageToRollByChance;

            maximizedSkills_activationsChances_List.Add(maximizedSkill_activationChance);

            singleSkillMaximized_ActivationDataSets.Add(activation_Set_afterFirstRoll);

            Debug.Log("skill_"+(i+1)+ "_maxed chance = " + maximizedSkill_activationChance);

            Debug.Log("skill_" + (i + 1) + "_maxed damage = " + damageTotalChanceFormula + " -- "+ damageTotalChanceFormulax);
        }

        Debug.Log("damage with this roll = " + TotalDamageAfterFirstRoll);

        //CHECK MAX DAMAGE BETWEEN ROLLED AND MAXIMIZED ACTIVATIONS FOR EACH SKILL
        //
        CheckMaxDamageBetweenSets(singleSkillMaximized_ActivationDataSets);

        //ADD TO CHECK MAX DAMAGE OF MAXIMIZED SET

        //activatedSet1_afterFirstRoll.skill_activation_set = new int[] { max_activations1, 0, 0 };
        //activatedSet2_afterFirstRoll.skill_activation_set = new int[] { 0, max_activations2, 0 };
        //activatedSet3_afterFirstRoll.skill_activation_set = new int[] { 0, 0, max_activations3 };

        //show damage

        #endregion

    }

    public void CheckMaxDamageBetweenSets(List<ActivationSetsData> _damageSets)
    {
        List<float> CheckMaxDamage = new List<float>();

        foreach (var damageSet in _damageSets)
        {
            Debug.Log("damage by set = " + damageSet.total_damage.ToString() + " - " + damageSet.set_probability_afterRoll.ToString());
            CheckMaxDamage.Add(damageSet.total_damage);
        }

        int MaxDamage = (int)(CheckMaxDamage.Max());
        Debug.Log("damage max between maxed sets = " + MaxDamage);
    }

    private float behaviourLimit;

    public void CheckSkillBehaviour()
    {
        //check if total number of abilities activated is less than
        if ((battlingCardData.NumberOfSkillsActivatedSet_afterFirstRoll[0] + battlingCardData.NumberOfSkillsActivatedSet_afterFirstRoll[1] + battlingCardData.NumberOfSkillsActivatedSet_afterFirstRoll[2]) < behaviourLimit)
        {

        }

        if(TotalDamageAfterFirstRoll < behaviourLimit)
        {
            if (battlingCardData.NumberOfSkillsActivatedSet_afterFirstRoll[0] != 0)
            {

            }
        }

        //check activations of first ability -> if behaviour is focused on first skill
        if (battlingCardData.NumberOfSkillsActivatedSet_afterFirstRoll[0] != 0)
        {

        }

        //check activations of second ability -> if behaviour is focused on second skill
        if (battlingCardData.NumberOfSkillsActivatedSet_afterFirstRoll[1] != 0)
        {

        }

        //check activations of third ability -> if behaviour is focused on third skill
        if (battlingCardData.NumberOfSkillsActivatedSet_afterFirstRoll[2] != 0)
        {

        }

        //check if defence instances of chosen skill are above behaviour limit
        //if(defenceInstances_Activated < behaviourLimit)
        {

        }




    }

}
