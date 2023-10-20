using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static EnemyBrain;

public class EnemyBrain : MonoBehaviour
{

    [SerializeField] private Dice[] diceRolled;
    [SerializeField] private Dice.diceFace[] diceRolled_faces;

    [SerializeField] private Card enemy_SelectedCard;

    [SerializeField] private int diceRoll_RedMana = 0;
    [SerializeField] private int diceRoll_BlueMana = 0;
    [SerializeField] private int diceRoll_YellowMana = 0;
    [SerializeField] private int[] diceRoll_byManaColor;

    [SerializeField] private List<int> skillsTotalActivations_bySkill;

    [SerializeField] private Card[] selected_cards;
    [SerializeField] private Card battling_card;

    public class Skill
    {
        public SkillData skillData_onCard;
        public List<Dice.diceFace> skill_ManaCost_list;

        public Skill(SkillData skillData)
        {
            skillData_onCard = skillData;
            skill_ManaCost_list = skillData.Skill_colorCost;
            total_manaCost = skill_ManaCost_list.Count;
        }

        public int skill_RedManaCost = 0;
        public int skill_YellowManaCost = 0;
        public int skill_BlueManaCost = 0;
        public int[] skill_ManaCost_byManaType;
        public int total_manaCost = 0;

        public int skill_maximum_activations;

        public int redMana_activations_onRoll = 0;
        public int yellowMana_activationsonRoll = 0;
        public int blueMana_activations_onRoll = 0;
        public List<int> activations_onRoll_by_ManaType;

        public int NumberOfSkillActivations;

    }

    private Skill First_Skill;
    private Skill Second_Skill;
    private Skill Third_Skill;
    private Skill[] CurrentCard_Skills;

    public void GetRolledDiceFaces()
    {
        diceRolled = BattleManager.instance.EnemyDices;

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

        diceRoll_byManaColor[0] = diceRoll_RedMana;
        diceRoll_byManaColor[1] = diceRoll_YellowMana;
        diceRoll_byManaColor[2] = diceRoll_BlueMana;

        Debug.Log(diceRoll_byManaColor);
    }

    public void GetCurrentCardSkills()
    {
        First_Skill = new Skill(battling_card.CardData.Skills[0]);
        Second_Skill = new Skill(battling_card.CardData.Skills[1]);
        Third_Skill = new Skill(battling_card.CardData.Skills[2]);

        CurrentCard_Skills = new Skill[3] { First_Skill, Second_Skill, Third_Skill };

    }

    public void GetSingleAbilityManaCost(Skill _skill)
    {
        _skill.skill_ManaCost_byManaType = new int[3];

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

        _skill.skill_ManaCost_byManaType[0] = _skill.skill_RedManaCost;
        _skill.skill_ManaCost_byManaType[1] = _skill.skill_YellowManaCost;
        _skill.skill_ManaCost_byManaType[2] = _skill.skill_BlueManaCost;

        _skill.skill_maximum_activations = (int) (6 / (_skill.total_manaCost));

        Debug.Log(_skill.skill_ManaCost_byManaType);
    }

    public void CheckAbilityIterations(Skill _skill)
    {
        for (int i = 0; i < 3; i++)
        {
            if (_skill.skill_ManaCost_byManaType[i] != 0)
            {
                _skill.activations_onRoll_by_ManaType.Add((int)(diceRoll_byManaColor[i] / _skill.skill_ManaCost_byManaType[i]));
            }
        }

    }

    public void CheckLesserNumberOfAbilityActivations(Skill _skill)
    {
        _skill.NumberOfSkillActivations = _skill.activations_onRoll_by_ManaType.Min();
        skillsTotalActivations_bySkill.Add(_skill.NumberOfSkillActivations);
    }


    public void TurnLoop()
    {
        GetRolledDiceFaces();

        GetCurrentCardSkills();

        foreach (Skill skill in CurrentCard_Skills)
        {
            GetSingleAbilityManaCost(skill);

            CheckAbilityIterations(skill);

            CheckLesserNumberOfAbilityActivations(skill);
        }

    }

    public struct DamageByActivationSet
    {
        public float total_damage;
        public int[] skill_activation_set;
    }

    public void PreRerollDamageFormula()
    {
        //skill base damage without activations from roll (damage * base_attack_instances)
        var base_damage1 = First_Skill.skillData_onCard.AtkInstances * First_Skill.skillData_onCard.Damage;
        var base_damage2 = Second_Skill.skillData_onCard.AtkInstances * Second_Skill.skillData_onCard.Damage;
        var base_damage3 = Third_Skill.skillData_onCard.AtkInstances * Third_Skill.skillData_onCard.Damage;

        var base_probability1 = (Mathf.Pow((1 / 2), (First_Skill.skill_RedManaCost))) * (Mathf.Pow((1 / 3), (First_Skill.skill_YellowManaCost))) * (Mathf.Pow((1 / 6), ( First_Skill.skill_BlueManaCost)));
        var base_probability2 = (Mathf.Pow((1 / 2), (Second_Skill.skill_RedManaCost))) * (Mathf.Pow((1 / 3), (Second_Skill.skill_YellowManaCost))) * (Mathf.Pow((1 / 6), (Second_Skill.skill_BlueManaCost)));
        var base_probability3 = (Mathf.Pow((1 / 2), (Third_Skill.skill_RedManaCost))) * (Mathf.Pow((1 / 3), (Third_Skill.skill_YellowManaCost))) * (Mathf.Pow((1 / 6), (Third_Skill.skill_BlueManaCost)));

        //skill damage multiplied by number of activations from roll (damage * base_attack_instances * roll_activations)
        var damage1 = First_Skill.NumberOfSkillActivations * First_Skill.skillData_onCard.AtkInstances * First_Skill.skillData_onCard.Damage;
        var damage2 = Second_Skill.NumberOfSkillActivations * Second_Skill.skillData_onCard.AtkInstances * Second_Skill.skillData_onCard.Damage;
        var damage3 = Third_Skill.NumberOfSkillActivations * Third_Skill.skillData_onCard.AtkInstances * Third_Skill.skillData_onCard.Damage;

        //maximum activations with 6 dice
        var target_activations1 = First_Skill.skill_maximum_activations;
        var target_activations2 = Second_Skill.skill_maximum_activations;
        var target_activations3 = Third_Skill.skill_maximum_activations;

        //probabilità che escano dadi per ripetere la skill target_activations volte(tenendo conto del massimo di volte ripetibili) --> target_activations qua vale il massimo possibile
        var damage1_probability_maxActivations = base_damage1 * (Mathf.Pow((1 / 2), (target_activations1 * First_Skill.skill_RedManaCost))) * (Mathf.Pow((1 / 3), (target_activations1 * First_Skill.skill_YellowManaCost))) * (Mathf.Pow((1 / 6), (target_activations1 * First_Skill.skill_BlueManaCost)));

        //probabilità che escano dadi per ripetere la skill target_activations volte a partire dai dadi rollati la prima volta
        var probability1_maxActivations_after_roll = (Mathf.Pow((1 / 2), (target_activations1 * First_Skill.skill_RedManaCost) - diceRoll_RedMana)) * (Mathf.Pow((1 / 3), (target_activations1 * First_Skill.skill_YellowManaCost) -diceRoll_YellowMana)) * (Mathf.Pow((1 / 6), (target_activations1 * First_Skill.skill_BlueManaCost) - diceRoll_BlueMana));

        List<DamageByActivationSet> total_damage_by_activations = new List<DamageByActivationSet>();
      
        int i = 0;
        int j = 0;
        int k = 0;

        for (i=0; i <= 6 - (j * Second_Skill.total_manaCost) - (k * Third_Skill.total_manaCost) ; i++)
        {
            var damage1_probability_maximize_after_roll = (Mathf.Pow((1 / 2), (i * First_Skill.skill_RedManaCost) - diceRoll_RedMana)) * (Mathf.Pow((1 / 3), (i * First_Skill.skill_YellowManaCost) - diceRoll_YellowMana)) * (Mathf.Pow((1 / 6), (i * First_Skill.skill_BlueManaCost) - diceRoll_BlueMana));
            var damage1_by_chance = base_damage1 * damage1_probability_maximize_after_roll;

            for (j=0; j <= 6 - (i * First_Skill.total_manaCost) - (k * Third_Skill.total_manaCost); j++)
            {
                var damage2_probability_maximize_after_roll = (Mathf.Pow((1 / 2), (j * Second_Skill.skill_RedManaCost) - diceRoll_RedMana)) * (Mathf.Pow((1 / 3), (j * Second_Skill.skill_YellowManaCost) - diceRoll_YellowMana)) * (Mathf.Pow((1 / 6), (j * Second_Skill.skill_BlueManaCost) - diceRoll_BlueMana));
                var damage2_by_chance = base_damage2 * damage2_probability_maximize_after_roll;

                for (k=0; k <= 6 - (i * First_Skill.total_manaCost) - (j * Second_Skill.total_manaCost); k++)
                {             
                    var damage3_probability_maximize_after_roll = (Mathf.Pow((1 / 2), (k * Third_Skill.skill_RedManaCost) - diceRoll_RedMana)) * (Mathf.Pow((1 / 3), (k * Third_Skill.skill_YellowManaCost) - diceRoll_YellowMana)) * (Mathf.Pow((1 / 6), (k * Third_Skill.skill_BlueManaCost) - diceRoll_BlueMana));
                    var damage3_by_chance = base_damage3 * damage3_probability_maximize_after_roll;

                    DamageByActivationSet newset = new DamageByActivationSet();

                    total_damage_by_activations.Add(newset);
                    newset.total_damage = (damage1_probability_maximize_after_roll + damage2_probability_maximize_after_roll + damage3_probability_maximize_after_roll);

                    int[] activationSet = new int[] {i, j, k };
                    newset.skill_activation_set = activationSet;
                    Debug.Log(total_damage_by_activations);
                }
            }
        }
    }

    public void ChanceFormula()
    {


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
