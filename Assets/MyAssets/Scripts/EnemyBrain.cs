using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyBrain : MonoBehaviour
{
    [Header("DICES ROLL OUTCOME")]
    [SerializeField] private Dice[] diceRolled;
    [SerializeField] private Dice.diceFace[] diceRolled_faces;

    [SerializeField] private int diceRoll_RedMana = 0;
    [SerializeField] private int diceRoll_YellowMana = 0;
    [SerializeField] private int diceRoll_BlueMana = 0;
    [SerializeField] private int[] diceRoll_byManaColor;

    [SerializeField] private List<int> skillsTotalActivations_bySkill;
    [Header("DAMAGE")]
    [SerializeField] private float TotalDamageAfterFirstRoll;

    [Header("CARD")]
    //[SerializeField] private Card enemy_SelectedCard;
    [SerializeField] private Card[] selected_cards;
    [SerializeField] private Card battling_card;

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

        public bool isAttackPrecise=false;
        public int defenceInstances_Activated=0;
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

        GetRolledActivations();

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
        First_Skill = new Skill(battling_card.CardData.Skills[0]);
        Second_Skill = new Skill(battling_card.CardData.Skills[1]);
        Third_Skill = new Skill(battling_card.CardData.Skills[2]);

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

    public void CheckSkillIterationsByManaType(Skill _skill)
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
    }

    public void GetNumberOfSkillActivations(Skill _skill)
    {
        //get skill activations
        _skill.NumberOfSkillActivations = _skill.activations_onRoll_byManaType.Min();

        //get skill defence istances
        if(_skill.skillData.DefInstances != 0)
        {
            _skill.defenceInstances_Activated = _skill.skillData.DefInstances * _skill.NumberOfSkillActivations;

        }

        //get skill dodge istances
        _skill.dodgeInstances_Activated = new List<int>();
        if (_skill.skillData.Dodge != 0)
        {
            for(int i=0; i < _skill.NumberOfSkillActivations; i++)
            {
                _skill.dodgeInstances_Activated.Add(_skill.skillData.Dodge);
            }
        }

        //add to list of every skill activations
        skillsTotalActivations_bySkill.Add(_skill.NumberOfSkillActivations);
    }

    public void GetRolledActivations()
    {
        skillsTotalActivations_bySkill = new List<int>();

        foreach (Skill skill in CurrentCard_Skills)
        {
            GetSingleAbilityManaCost(skill);

            CheckSkillIterationsByManaType(skill);

            GetNumberOfSkillActivations(skill);
        }
    }

    public void TurnLoop()
    {
        GetCreateDice();

        RollDices();

        GetRolledDiceFaces();

        GetCurrentCardSkills();

        GetRolledActivations();

        PreRerollDamageFormula();
    }


    public struct DamageByActivationSet
    {
        public float total_damage;
        public int[] skill_activation_set;
    }

    public void PreRerollDamageFormula()
    {
        #region skill data

        //foreach(Skill skill in CurrentCard_Skills)
        //skill base probability
        float base_probability1 = Mathf.Pow((1 / 2f), (First_Skill.skill_RedManaCost)) * Mathf.Pow((1 / 3f), (First_Skill.skill_YellowManaCost)) * Mathf.Pow((1 / 6f), ( First_Skill.skill_BlueManaCost));
        float base_probability2 = Mathf.Pow((1 / 2f), (Second_Skill.skill_RedManaCost)) * Mathf.Pow((1 / 3f), (Second_Skill.skill_YellowManaCost)) * Mathf.Pow((1 / 6f), (Second_Skill.skill_BlueManaCost));
        float base_probability3 = Mathf.Pow((1 / 2f), (Third_Skill.skill_RedManaCost)) * Mathf.Pow((1 / 3f), (Third_Skill.skill_YellowManaCost)) * Mathf.Pow((1 / 6f), (Third_Skill.skill_BlueManaCost));

        //maximum activations with 6 dice
        float max_activations1 = First_Skill.skill_maximum_activations;
        float max_activations2 = Second_Skill.skill_maximum_activations;
        float max_activations3 = Third_Skill.skill_maximum_activations;

        //skill base damage without activations from roll (damage * base_attack_instances)
        float base_damage1 = First_Skill.skillData.AtkInstances * First_Skill.skillData.Damage;
        float base_damage2 = Second_Skill.skillData.AtkInstances * Second_Skill.skillData.Damage;
        float base_damage3 = Third_Skill.skillData.AtkInstances * Third_Skill.skillData.Damage;

        //probabilità che escano dadi per ripetere la skill max_activations volte(tenendo conto del massimo di volte ripetibili)
        //--> max_activations qua vale il massimo possibile
        float probability1_maxActivations = Mathf.Pow(((Mathf.Pow((1 / 2f), (First_Skill.skill_RedManaCost))) 
                                                     * (Mathf.Pow((1 / 3f), (First_Skill.skill_YellowManaCost))) 
                                                     * (Mathf.Pow((1 / 6f), (First_Skill.skill_BlueManaCost)))), max_activations1 );

        #endregion

        //DANNO DOPO PRIMO ROLL
        //skill damage multiplied by number of activations from first roll (damage * base_attack_instances * roll_activations)
        float damage1 = First_Skill.NumberOfSkillActivations * First_Skill.skillData.AtkInstances * First_Skill.skillData.Damage;
        float damage2 = Second_Skill.NumberOfSkillActivations * Second_Skill.skillData.AtkInstances * Second_Skill.skillData.Damage;
        float damage3 = Third_Skill.NumberOfSkillActivations * Third_Skill.skillData.AtkInstances * Third_Skill.skillData.Damage;

        TotalDamageAfterFirstRoll = damage1 + damage2 + damage3;


        //probabilità che escano dadi per ripetere la skill max_activations volte a partire dai dadi rollati la prima volta
        //
        float probability1_maxActivations_after = First_Skill.NumberOfSkillActivations < max_activations1 ? 
                                                            ((Mathf.Pow((1 / 2f), ((First_Skill.skill_RedManaCost * (max_activations1 - First_Skill.NumberOfSkillActivations)) - (diceRoll_RedMana-First_Skill.NumberOfSkillActivations*First_Skill.skill_RedManaCost))))
                                                           * (Mathf.Pow((1 / 3f), ((First_Skill.skill_YellowManaCost * (max_activations1 - First_Skill.NumberOfSkillActivations)) - (diceRoll_YellowMana - First_Skill.NumberOfSkillActivations * First_Skill.skill_YellowManaCost))))
                                                           * (Mathf.Pow((1 / 6f), ((First_Skill.skill_BlueManaCost * (max_activations1 - First_Skill.NumberOfSkillActivations)) - (diceRoll_BlueMana - First_Skill.NumberOfSkillActivations * First_Skill.skill_BlueManaCost))))) : 0;

        float probability_totalDamage1_afterRoll = base_damage1 * (First_Skill.NumberOfSkillActivations + probability1_maxActivations_after);

        float probability2_maxActivations_after = Second_Skill.NumberOfSkillActivations < max_activations2 ?
                                                            ((Mathf.Pow((1 / 2f), ((Second_Skill.skill_RedManaCost * (max_activations2 - Second_Skill.NumberOfSkillActivations)) - (diceRoll_RedMana - Second_Skill.NumberOfSkillActivations * Second_Skill.skill_RedManaCost))))
                                                           * (Mathf.Pow((1 / 3f), ((Second_Skill.skill_YellowManaCost * (max_activations2 - Second_Skill.NumberOfSkillActivations)) - (diceRoll_YellowMana - Second_Skill.NumberOfSkillActivations * Second_Skill.skill_YellowManaCost))))
                                                           * (Mathf.Pow((1 / 6f), ((Second_Skill.skill_BlueManaCost * (max_activations2 - Second_Skill.NumberOfSkillActivations)) - (diceRoll_BlueMana - Second_Skill.NumberOfSkillActivations * Second_Skill.skill_BlueManaCost))))) : 0;

        float probability_totalDamage2_afterRoll = base_damage2 * (Second_Skill.NumberOfSkillActivations + probability2_maxActivations_after);

        float probability3_maxActivations_after = Third_Skill.NumberOfSkillActivations < max_activations3 ?
                                                            ((Mathf.Pow((1 / 2f), ((Third_Skill.skill_RedManaCost * (max_activations3 - Third_Skill.NumberOfSkillActivations)) - (diceRoll_RedMana - Third_Skill.NumberOfSkillActivations * Third_Skill.skill_RedManaCost))))
                                                           * (Mathf.Pow((1 / 3f), ((Third_Skill.skill_YellowManaCost * (max_activations3 - Third_Skill.NumberOfSkillActivations)) - (diceRoll_YellowMana - Third_Skill.NumberOfSkillActivations * Third_Skill.skill_YellowManaCost))))
                                                           * (Mathf.Pow((1 / 6f), ((Third_Skill.skill_BlueManaCost * (max_activations3 - Third_Skill.NumberOfSkillActivations)) - (diceRoll_BlueMana - Third_Skill.NumberOfSkillActivations * Third_Skill.skill_BlueManaCost))))) : 0;

        float probability_totalDamage3_afterRoll = base_damage3 * (Third_Skill.NumberOfSkillActivations + probability3_maxActivations_after);

        float totalDamage_afterRoll_maximized;

        /*
        float probability1_maxActivations_after_roll = (First_Skill.skill_RedManaCost    > 0 ? (Mathf.Pow((1 / 2f), (max_activations1 * First_Skill.skill_RedManaCost) - diceRoll_RedMana)):1)
                                                     * (First_Skill.skill_YellowManaCost > 0 ? (Mathf.Pow((1 / 3f), (max_activations1 * First_Skill.skill_YellowManaCost) -diceRoll_YellowMana)):1)
                                                     * (First_Skill.skill_BlueManaCost   > 0 ? (Mathf.Pow((1 / 6f), (max_activations1 * First_Skill.skill_BlueManaCost) - diceRoll_BlueMana)):1);
        */
        /*
        List<DamageByActivationSet> total_damage_bySetOfActivations = new List<DamageByActivationSet>();

        foreach (var damageSet in total_damage_bySetOfActivations)
        {
            Debug.Log("damage by set = " + damageSet.total_damage.ToString() + " - " + damageSet.skill_activation_set[0].ToString()+ damageSet.skill_activation_set[1].ToString()+ damageSet.skill_activation_set[2].ToString());
        }
        */

        Debug.Log("damage with this roll = " + TotalDamageAfterFirstRoll);
        Debug.Log("damage with 1 skill maximized = " + probability_totalDamage1_afterRoll.ToString());
        Debug.Log("damage with 2 skill maximized = " + probability_totalDamage2_afterRoll.ToString());
        Debug.Log("damage with 3 skill maximized = " + probability_totalDamage3_afterRoll.ToString());

    }

    private List<Dice.diceFace> diceFaces_toGetWithReroll;
    private float behaviourLimit;

    public void CheckSkillBehaviour()
    {
        //check if total number of abilities activated is less than
        if ((skillsTotalActivations_bySkill[0] + skillsTotalActivations_bySkill[1] + skillsTotalActivations_bySkill[2]) < behaviourLimit)
        {

        }

        if(TotalDamageAfterFirstRoll < behaviourLimit)
        {
            if (skillsTotalActivations_bySkill[0] != 0)
            {

            }
        }

        //check activations of first ability -> if behaviour is focused on first skill
        if (skillsTotalActivations_bySkill[0] != 0)
        {

        }

        //check activations of second ability -> if behaviour is focused on second skill
        if (skillsTotalActivations_bySkill[1] != 0)
        {

        }

        //check activations of third ability -> if behaviour is focused on third skill
        if (skillsTotalActivations_bySkill[2] != 0)
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
