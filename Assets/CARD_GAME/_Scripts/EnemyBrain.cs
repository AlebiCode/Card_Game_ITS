using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static EnemyBrain;

public class EnemyBrain : MonoBehaviour
{
    [Header("DICES ROLL OUTCOME")]
    [SerializeField] private Dice[] diceRolled;
    [SerializeField] private Dice.diceFace[] diceRolled_faces;

    [SerializeField] private int diceRoll_RedMana = 0;
    [SerializeField] private int diceRoll_YellowMana = 0;
    [SerializeField] private int diceRoll_BlueMana = 0;
    [SerializeField] private int[] diceRoll_byManaColor;

    //[SerializeField] private List<int[]> diceCombinationsSets_byManaType;
    //[SerializeField] private List<int[]> skillsActivationsSets_ofDiceCombinationSets;
    //[SerializeField] List<ActivationSetsData> ActivationsSetsDataList;

    //[SerializeField] private int[] skillsActivationsSet_afterRoll;

    [Header("DAMAGE")]
    [SerializeField] private float TotalDamageAfterFirstRoll;

    [Header("CARD")]
    //[SerializeField] private Card enemy_SelectedCard;
    [SerializeField] private Card[] enemy_cards;
    [SerializeField] private Card selected_card;
    [SerializeField] private CardSelected battlingCardData;

    [SerializeField] private List<TargetSet> targetSets;

    [Serializable()]
    public class CardSelected
    {
        public Card battlingCard;

        public CardSelected(Card _selected_card)
        {
            battlingCard = _selected_card;
        }

        public int[] NumberOfSkillsActivatedSet_afterFirstRoll;
        public int total_damageDone_byActivations ;
        public int total_defenceInstances_Activated ;
        public int[] total_preciseIstances_Activated ;
        public List<int> total_dodgeInstances_Activated;

        public List<ActivationSetsData> ActivationsSetsDataList;

        public List<int[]> diceCombinationsSets_byManaType;
        public List<int[]> skillsActivationsSets_ofDiceCombinationSets;

        public List<float> setProbabilitiesAfterRoll_List = new List<float>();
    }

    [Serializable()]
    private struct TargetSet
    {
        public int[] set;
    }

    //TO DO: metto skills dentro card
    [Header("SKILLS")]
    [SerializeField] private Skill First_Skill;
    [SerializeField] private Skill Second_Skill;
    [SerializeField] private Skill Third_Skill;
    [SerializeField] private Skill[] CurrentCard_Skills;

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
        public int total_manaCost = 0;
        public int skill_RedManaCost = 0;
        public int skill_YellowManaCost = 0;
        public int skill_BlueManaCost = 0;
        public int[] skill_ManaCost_byManaType;

        [Header("Activations")]
        public int skill_maximum_activations;
        public int redMana_activations_onRoll = 0;
        public int yellowMana_activationsonRoll = 0;
        public int blueMana_activations_onRoll = 0;
        public List<int> activations_onRoll_byManaType;

        public int NumberOfSkillActivations = 0;
        public int damageDone_byActivations = 0;
        public int defenceInstances_Activated = 0;
        public bool isAttackPrecise=false;
        public int preciseIstances_Activated = 0;
        public List<int> dodgeInstances_Activated;

    }

    public void RollDiceButtonTest()
    {
        RollDices();

        GetRolledDiceFaces();
    }

    public void CalculateSkillActivationsButtonTest()
    {
        GetCurrentCardSkills();

        GetRolledActivationsFromEachSkill();

        PreRerollDamageFormula();
    }

    public void GetCreateDice()
    {
        
        diceRolled = BattleManager.instance.EnemyDices;

        //remove comments after testing
        /*
        diceRolled = new Dice[6];

        for(int i = 0; i < diceRolled.Length; i++)
        {
            diceRolled[i] = new Dice();
        }
        */
    }
   
    public void GetCardData()
    {
        battlingCardData = new CardSelected(selected_card);
    }

    public void RollDices()
    {
        foreach (Dice die in diceRolled)
        {
            die.RollDice();
        }
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

        Debug.Log(diceRoll_byManaColor);
    }

    public void GetCurrentCardSkills()
    {
        //get skills from card and reset values of roll
        First_Skill = new Skill(selected_card.CardData.Skills[0]);
        Second_Skill = new Skill(selected_card.CardData.Skills[1]);
        Third_Skill = new Skill(selected_card.CardData.Skills[2]);

        CurrentCard_Skills = new Skill[3] { First_Skill, Second_Skill, Third_Skill };
    }

    public void GetSingleAbilityManaCost(Skill _skill)
    {
        _skill.total_manaCost = _skill.skill_ManaCost_list.Count;

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

        _skill.skill_maximum_activations = (int) (6 / (_skill.total_manaCost));

        Debug.Log(_skill.skill_ManaCost_byManaType);
    }

    public void GetSkillActivationsAfterFirstRoll(Skill _skill)
    {
        _skill.activations_onRoll_byManaType = new List<int>();
        int[] activsByMana= new int[3];

        for (int i = 0; i < 3; i++)
        {
            if (_skill.skill_ManaCost_byManaType[i] != 0)
            {
                int activs = (int)(diceRoll_byManaColor[i] / _skill.skill_ManaCost_byManaType[i]);

                _skill.activations_onRoll_byManaType.Add(activs);
                activsByMana[i] = activs;
            }
        }
        _skill.redMana_activations_onRoll = activsByMana[0];
        _skill.yellowMana_activationsonRoll = activsByMana[1];
        _skill.blueMana_activations_onRoll = activsByMana[2];

        //get skill final activations
        _skill.NumberOfSkillActivations = _skill.activations_onRoll_byManaType.Min();

        //add to battlingCard list of skill activated set --> confront with other sets probabilities
        battlingCardData.NumberOfSkillsActivatedSet_afterFirstRoll.Append(_skill.NumberOfSkillActivations);
    }

    public void GetDamageAndEffectsActivatedForSingleSkillAfterRoll(Skill _skill)
    {
        //get damage done by activs
        _skill.damageDone_byActivations = 0;
        _skill.damageDone_byActivations = _skill.skillData.Damage * _skill.skillData.AtkInstances * _skill.NumberOfSkillActivations;

        //get skill defence istances
        _skill.defenceInstances_Activated = 0;
        _skill.defenceInstances_Activated = _skill.skillData.DefInstances * _skill.NumberOfSkillActivations;

        //get precise istances
        _skill.preciseIstances_Activated = 0;
        if (_skill.isAttackPrecise)
        {
            _skill.preciseIstances_Activated = _skill.NumberOfSkillActivations;
        }

        //get skill dodge istances
        _skill.dodgeInstances_Activated = new List<int>();

        if (_skill.skillData.Dodge > 0)
        {
            for (int i = 0; i < _skill.NumberOfSkillActivations; i++)
            {
                _skill.dodgeInstances_Activated.Add(_skill.skillData.Dodge);
            }
        }

    }

    public void GetRolledActivationsFromEachSkill()
    {
        battlingCardData.NumberOfSkillsActivatedSet_afterFirstRoll = new int[3];
        int total_damage = 0;
        int total_defences = 0;
        int[] total_preciseIstances;
        List<int> total_dodgeInstances = new List<int>();

        for(int i = 0; i< CurrentCard_Skills.Length; i++)
        //foreach (Skill skill in CurrentCard_Skills)
        {
            GetSingleAbilityManaCost(CurrentCard_Skills[i]);

            GetSkillActivationsAfterFirstRoll(CurrentCard_Skills[i]);

            GetDamageAndEffectsActivatedForSingleSkillAfterRoll(CurrentCard_Skills[i]);
            total_damage += CurrentCard_Skills[i].damageDone_byActivations;
            total_defences += CurrentCard_Skills[i].defenceInstances_Activated;
            total_dodgeInstances.AddRange(CurrentCard_Skills[i].dodgeInstances_Activated);

        }

        battlingCardData.total_damageDone_byActivations = total_damage;
        battlingCardData.total_defenceInstances_Activated = total_defences;
        battlingCardData.total_dodgeInstances_Activated = total_dodgeInstances;
        battlingCardData.total_preciseIstances_Activated = new int[3] { CurrentCard_Skills[0].preciseIstances_Activated, CurrentCard_Skills[1].preciseIstances_Activated, CurrentCard_Skills[2].preciseIstances_Activated };

        Debug.Log("ROLLED EFFECTS - damage = " + battlingCardData.total_damageDone_byActivations.ToString() 
            + " - defences = " + battlingCardData.total_defenceInstances_Activated.ToString()
            + " - dodge istances = " + battlingCardData.total_dodgeInstances_Activated.ToString() 
            + " - precise istances = " + battlingCardData.total_preciseIstances_Activated[0].ToString()
            + battlingCardData.total_preciseIstances_Activated[1].ToString() 
            + battlingCardData.total_preciseIstances_Activated[2].ToString());
    
    }

    public void TurnLoop()
    {
        //START
        GetCardData();

        GetCreateDice();

        //ROLL DICE
        RollDices();

        GetRolledDiceFaces();

        GetCurrentCardSkills();

        //CHECK ROLLED EFFECTS
        //first check of damage with roll
        GetRolledActivationsFromEachSkill();

        //CHECK POSSIBLE COMBINATIONS FOR LOCK
        //get all possible diceCombination(by mana type)
        DiceCombinationsCalculator();

        //get all chances to do each diceCombination (by type)
        GetChanceOfEachDiceCombinationFromRoll();

        GetSkillActivationsSetsFromEachDiceCombination();

        GetDamageAndEffectsFromEachActivationSet(/*battlingCardData.ActivationsSetsDataList*/);

        CheckMaxDamageBetweenSets(battlingCardData.ActivationsSetsDataList);

        PreRerollDamageFormula();
    }


    public class ActivationSetsData
    {
        public ActivationSetsData(int[] _diceComboSet)
        {
            diceCombinationSet_byManaType = _diceComboSet;
        }

        //questo per test maximized
        public ActivationSetsData(float _damage, int[] _skillActivationSet)
        {
            skill_activation_set = _skillActivationSet;
            //occhio qua era skillactiv_sets

            total_damage = _damage;
        }

        //[0] = red mana faces, [1] = yellow mana faces, [2] = blue mana faces
        public int[] diceCombinationSet_byManaType;
        public float set_probability_preRoll;
        public float set_probability_afterRoll;

        //[0] = first skill activations, [1] = second skill activations, [2] = third skill activations
        public int[] skill_activation_set;

        public float total_damage =  0f;
        public int defence_instances = 0;
        public int[] precise_instances;
        public int[] dodge_instances_values /*= new int[] {}*/;

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

                battlingCardData.ActivationsSetsDataList.Add(new ActivationSetsData(set));

                Debug.Log("combs = " + set[0] + set[1] + set[2]);

            }

            j = 0;

        }
    }


    public void GetChanceOfEachDiceCombinationFromRoll(/*ActivationsSetsDataList _activationDataSets*/)
    {

        //_activationDataSets = battlingCardData.ActivationsSetsDataList;

        battlingCardData.setProbabilitiesAfterRoll_List = new List<float>();

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
        }

    }

    public void GetSkillActivationsSetsFromEachDiceCombination(/*ActivationsSetsDataList _activationDataSets, Skill[] _skills*/)
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
        List<float> damageByEachSetOfActivations = new List<float>();
        List<float> defencesByEachSetOfActivations = new List<float>();
        List<int[]> preciseIstances = new List<int[]>();
        List<int[]> skillsDodgeIstancesValues = new List<int[]>();

        foreach (ActivationSetsData setData in battlingCardData.ActivationsSetsDataList)
        {
            //new damageSEt con tutte attivazioni

            float skillsDamage = 0;
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

            //compile ActivationSetsData values
            setData.total_damage = skillsDamage;
            setData.defence_instances = skillsDefence;
            setData.precise_instances = skillsPreciseIstances;
            setData.dodge_instances_values = skillsDodge;

            //compile BattlingCard values
            damageByEachSetOfActivations.Add(skillsDamage);
            defencesByEachSetOfActivations.Add(skillsDefence);
            preciseIstances.Add(skillsPreciseIstances);
            skillsDodgeIstancesValues.Add(skillsDodge);
        }
    }

    public float GetMaximizedSingleSkillTargetActivationsChance(Skill _skill, int _targetActivations)
    {
        //target activs for maximized skills sets

        //redDiceNeeded1 - redDiceSpared1 =  First_Skill.skill_RedManaCost * targetActivations - diceRoll_RedMana;

        float redDiceNeeded1 = _skill.skill_RedManaCost * _targetActivations - diceRoll_RedMana;

        float yellowDiceNeeded1 = _skill.skill_YellowManaCost * _targetActivations - diceRoll_YellowMana;

        float blueDiceNeeded1 = _skill.skill_BlueManaCost * _targetActivations - diceRoll_BlueMana;

        float probability_maxActivations_after = _skill.NumberOfSkillActivations < _targetActivations ?
                                                 ((Mathf.Pow((1 / 2f), _skill.skill_RedManaCost * _targetActivations > diceRoll_RedMana ? (redDiceNeeded1) : 0))
                                                * (Mathf.Pow((1 / 3f), _skill.skill_YellowManaCost * _targetActivations > diceRoll_YellowMana ? (yellowDiceNeeded1) : 0))
                                                * (Mathf.Pow((1 / 6f), _skill.skill_BlueManaCost * _targetActivations > diceRoll_BlueMana ? (blueDiceNeeded1) : 0))) : 0;

        //probability of maximized single skill * damage of skill
        //float skill_damage_byChance = _skill.skillData.Damage * probability_maxActivations_after;
        return probability_maxActivations_after;
    }    

    public void PreRerollDamageFormula()
    { 
        #region skill data pre-roll

        //foreach(Skill skill in CurrentCard_Skills)
        //skill base probability
        float base_probability1 = Mathf.Pow((1 / 2f), (First_Skill.skill_RedManaCost)) * Mathf.Pow((1 / 3f), (First_Skill.skill_YellowManaCost)) * Mathf.Pow((1 / 6f), ( First_Skill.skill_BlueManaCost));
        float base_probability2 = Mathf.Pow((1 / 2f), (Second_Skill.skill_RedManaCost)) * Mathf.Pow((1 / 3f), (Second_Skill.skill_YellowManaCost)) * Mathf.Pow((1 / 6f), (Second_Skill.skill_BlueManaCost));
        float base_probability3 = Mathf.Pow((1 / 2f), (Third_Skill.skill_RedManaCost)) * Mathf.Pow((1 / 3f), (Third_Skill.skill_YellowManaCost)) * Mathf.Pow((1 / 6f), (Third_Skill.skill_BlueManaCost));

        //maximum activations with 6 dice
        int max_activations1 = First_Skill.skill_maximum_activations;
        int max_activations2 = Second_Skill.skill_maximum_activations;
        int max_activations3 = Third_Skill.skill_maximum_activations;

        //probabilità che escano dadi per ripetere la skill max_activations volte a partire da nessun roll
        float probability1_maxActivations = Mathf.Pow(((Mathf.Pow((1 / 2f), (First_Skill.skill_RedManaCost))) 
                                                     * (Mathf.Pow((1 / 3f), (First_Skill.skill_YellowManaCost))) 
                                                     * (Mathf.Pow((1 / 6f), (First_Skill.skill_BlueManaCost)))), max_activations1);

        float probability2_maxActivations = Mathf.Pow(((Mathf.Pow((1 / 2f), (Second_Skill.skill_RedManaCost)))
                                                     * (Mathf.Pow((1 / 3f), (Second_Skill.skill_YellowManaCost)))
                                                     * (Mathf.Pow((1 / 6f), (Second_Skill.skill_BlueManaCost)))), max_activations2);

        float probability3_maxActivations = Mathf.Pow(((Mathf.Pow((1 / 2f), (Third_Skill.skill_RedManaCost)))
                                                     * (Mathf.Pow((1 / 3f), (Third_Skill.skill_YellowManaCost)))
                                                     * (Mathf.Pow((1 / 6f), (Third_Skill.skill_BlueManaCost)))), max_activations3);

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

        TotalDamageAfterFirstRoll = damage1 + damage2 + damage3;

        //CREATE SETS WITH RESPECTIVE DAMAGE
        List<ActivationSetsData> singleSkillMaximized_ActivationDataSets = new List<ActivationSetsData>();

        ActivationSetsData activatedSet_afterFirstRoll = new ActivationSetsData(TotalDamageAfterFirstRoll, new int[] { diceRoll_RedMana, diceRoll_YellowMana, diceRoll_BlueMana });

        //here get all atk def, dodge, ecc
        activatedSet_afterFirstRoll.skill_activation_set = new int[] { First_Skill.NumberOfSkillActivations, Second_Skill.NumberOfSkillActivations, Third_Skill.NumberOfSkillActivations };

        //ADD TO CHECK MAX DAMAGE OF ROLLED COMBINATION
        singleSkillMaximized_ActivationDataSets.Add(activatedSet_afterFirstRoll);

        List<float> maximizedSkills_activationsChances_List = new List<float>();
        //targetSets = new List<TargetSet[]>();


        //foreach (TargetSet tSet in /*selected_card.*/targetSets)
        //{
            for (int i=0; i< CurrentCard_Skills.Length; i++)
            {
                float maximizedSkill_activationChance = GetMaximizedSingleSkillTargetActivationsChance(CurrentCard_Skills[i], CurrentCard_Skills[i].skill_maximum_activations);
                float damagezz = maximizedSkill_activationChance * CurrentCard_Skills[i].skillData.Damage * CurrentCard_Skills[i].skill_maximum_activations;
                maximizedSkills_activationsChances_List.Add(maximizedSkill_activationChance);
                Debug.Log("skill_damadge_by_chance = " + maximizedSkill_activationChance);
            }

        //}

        #region one skill maximize
        //ONLY ONE SKILL MAXIMIZED FROM ROLLED DICE
        //Probabilità che escano dadi per ripetere la skill max_activations volte a partire dai dadi rollati la prima volta
        //
        //Skill 1
        //(redDiceNeeded1 - redDiceSpared1 =  First_Skill.skill_RedManaCost * max_activations1 - diceRoll_RedMana;
        float redDiceNeeded1 = First_Skill.skill_RedManaCost * (max_activations1 - First_Skill.NumberOfSkillActivations);
        float redDiceSpared1 = diceRoll_RedMana - First_Skill.NumberOfSkillActivations * First_Skill.skill_RedManaCost;

        float yellowDiceNeeded1 = First_Skill.skill_YellowManaCost * (max_activations1 - First_Skill.NumberOfSkillActivations);
        float yellowDiceSpared1 = diceRoll_YellowMana - First_Skill.NumberOfSkillActivations * First_Skill.skill_YellowManaCost;

        float blueDiceNeeded1 = First_Skill.skill_BlueManaCost * (max_activations1 - First_Skill.NumberOfSkillActivations);
        float bluedDiceSpared1 = diceRoll_BlueMana - First_Skill.NumberOfSkillActivations * First_Skill.skill_BlueManaCost;

        float probability1_maxActivations_after = First_Skill.NumberOfSkillActivations < max_activations1 ?
                                                            ((Mathf.Pow((1 / 2f), redDiceNeeded1 > redDiceSpared1 ? (redDiceNeeded1 - redDiceSpared1) : 0))
                                                           * (Mathf.Pow((1 / 3f), yellowDiceNeeded1 > yellowDiceSpared1 ? (yellowDiceNeeded1 - yellowDiceSpared1) : 0))
                                                           * (Mathf.Pow((1 / 6f), blueDiceNeeded1 > bluedDiceSpared1 ? (blueDiceNeeded1 - bluedDiceSpared1) : 0))) : 0;

        float probability_totalDamage1_afterRoll = base_damage1 * (First_Skill.NumberOfSkillActivations + probability1_maxActivations_after*(max_activations1- First_Skill.NumberOfSkillActivations));

        //Skill 2
        float redDiceNeeded2 = Second_Skill.skill_RedManaCost * (max_activations2 - Second_Skill.NumberOfSkillActivations);
        float redDiceSpared2 = diceRoll_RedMana - Second_Skill.NumberOfSkillActivations * Second_Skill.skill_RedManaCost;

        float yellowDiceNeeded2 = Second_Skill.skill_YellowManaCost * (max_activations2 - Second_Skill.NumberOfSkillActivations);
        float yellowDiceSpared2 = diceRoll_YellowMana - Second_Skill.NumberOfSkillActivations * Second_Skill.skill_YellowManaCost;

        float blueDiceNeeded2 = Second_Skill.skill_BlueManaCost * (max_activations2 - Second_Skill.NumberOfSkillActivations);
        float bluedDiceSpared2 = diceRoll_BlueMana - Second_Skill.NumberOfSkillActivations * Second_Skill.skill_BlueManaCost;

        float probability2_maxActivations_after = Second_Skill.NumberOfSkillActivations < max_activations2 ?
                                                            ((Mathf.Pow((1 / 2f), redDiceNeeded2 > redDiceSpared2 ? (redDiceNeeded2 - redDiceSpared2) : 0))
                                                           * (Mathf.Pow((1 / 3f), yellowDiceNeeded2 > yellowDiceSpared2 ? (yellowDiceNeeded2 - yellowDiceSpared2) : 0))
                                                           * (Mathf.Pow((1 / 6f), blueDiceNeeded2 > bluedDiceSpared2 ? (blueDiceNeeded2 - bluedDiceSpared2) : 0 ))) : 0;

        float probability_totalDamage2_afterRoll = base_damage2 * (Second_Skill.NumberOfSkillActivations + probability2_maxActivations_after * (max_activations2 - Second_Skill.NumberOfSkillActivations));

        //Skill 3
        float redDiceNeeded3 = Third_Skill.skill_RedManaCost * (max_activations3 - Third_Skill.NumberOfSkillActivations);
        float redDiceSpared3 = diceRoll_RedMana - Third_Skill.NumberOfSkillActivations * Third_Skill.skill_RedManaCost;

        float yellowDiceNeeded3 = Third_Skill.skill_YellowManaCost * (max_activations3 - Third_Skill.NumberOfSkillActivations);
        float yellowDiceSpared3 = diceRoll_YellowMana - Third_Skill.NumberOfSkillActivations * Third_Skill.skill_YellowManaCost;

        float blueDiceNeeded3 = Third_Skill.skill_BlueManaCost * (max_activations3 - Third_Skill.NumberOfSkillActivations);
        float bluedDiceSpared3 = diceRoll_BlueMana - Third_Skill.NumberOfSkillActivations * Third_Skill.skill_BlueManaCost;

        float probability3_maxActivations_after = Third_Skill.NumberOfSkillActivations < max_activations3 ?
                                                            ((Mathf.Pow((1 / 2f), (redDiceNeeded3 > redDiceSpared3 ? (redDiceNeeded3 - redDiceSpared3) : 0)))
                                                           * (Mathf.Pow((1 / 3f), (yellowDiceNeeded3 > yellowDiceSpared3 ? (yellowDiceNeeded3 - yellowDiceSpared3) : 0)))
                                                           * (Mathf.Pow((1 / 6f), (blueDiceNeeded3 > bluedDiceSpared3 ? (blueDiceNeeded3 - bluedDiceSpared3) : 0)))) : 0;

        float probability_totalDamage3_afterRoll = base_damage3 * (Third_Skill.NumberOfSkillActivations + probability3_maxActivations_after * (max_activations3 - Third_Skill.NumberOfSkillActivations));

        #endregion

        //ADD TO CHECK MAX DAMAGE OF MAXIMIZED SET
        ActivationSetsData activatedSet1_afterFirstRoll = new ActivationSetsData(probability_totalDamage1_afterRoll, new int[] { max_activations1 * First_Skill.skill_RedManaCost, max_activations1 * First_Skill.skill_YellowManaCost, max_activations1 * First_Skill.skill_BlueManaCost });
        ActivationSetsData activatedSet2_afterFirstRoll = new ActivationSetsData(probability_totalDamage2_afterRoll, new int[] { max_activations2 * Second_Skill.skill_RedManaCost, max_activations2 * Second_Skill.skill_YellowManaCost, max_activations2 * Second_Skill.skill_BlueManaCost });
        ActivationSetsData activatedSet3_afterFirstRoll = new ActivationSetsData(probability_totalDamage3_afterRoll, new int[] { max_activations3 * Third_Skill.skill_RedManaCost, max_activations3 * Third_Skill.skill_YellowManaCost, max_activations3 * Third_Skill.skill_BlueManaCost });
        activatedSet1_afterFirstRoll.skill_activation_set = new int[] { max_activations1, 0, 0 };
        activatedSet2_afterFirstRoll.skill_activation_set = new int[] { 0, max_activations2, 0 };
        activatedSet3_afterFirstRoll.skill_activation_set = new int[] { 0, 0, max_activations3 };
        activatedSet1_afterFirstRoll.set_probability_afterRoll = probability1_maxActivations_after;
        activatedSet2_afterFirstRoll.set_probability_afterRoll = probability2_maxActivations_after;
        activatedSet3_afterFirstRoll.set_probability_afterRoll = probability3_maxActivations_after;

        singleSkillMaximized_ActivationDataSets.Add(activatedSet1_afterFirstRoll);
        singleSkillMaximized_ActivationDataSets.Add(activatedSet2_afterFirstRoll);
        singleSkillMaximized_ActivationDataSets.Add(activatedSet3_afterFirstRoll);

        //show damage
        Debug.Log("damage with this roll = " + TotalDamageAfterFirstRoll);
        Debug.Log("damage with 1 skill maximized = " + probability_totalDamage1_afterRoll.ToString());
        Debug.Log("damage with 2 skill maximized = " + probability_totalDamage2_afterRoll.ToString());
        Debug.Log("damage with 3 skill maximized = " + probability_totalDamage3_afterRoll.ToString());

        #endregion

        //CHECK MAX DAMAGE BETWEEN ROLLED AND MAXIMIZED ACTIVATIONS FOR EACH SKILL
        //
        CheckMaxDamageBetweenSets(singleSkillMaximized_ActivationDataSets);

    }

    public void CheckMaxDamageBetweenSets(List<ActivationSetsData> _damageSets)
    {
        List<float> CheckMaxDamage = new List<float>();

        foreach (var damageSet in _damageSets)
        {
            //Debug.Log("damage by set = " + damageSet.total_damage.ToString() + " - " + damageSet.skill_activation_set.ToString());
            CheckMaxDamage.Add(damageSet.total_damage);
        }

        int MaxDamage = (int)(CheckMaxDamage.Max());
        Debug.Log("damage max = " + MaxDamage);
    }



    private List<Dice.diceFace> diceFaces_toGetWithReroll;
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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
