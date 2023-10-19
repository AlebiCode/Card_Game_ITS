using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static EnemyBrain;

public class EnemyBrain : MonoBehaviour
{

    [SerializeField] private Dice[]             dice_rolled;
    [SerializeField] private Dice.diceFace[]    dice_rolled_faces;

    [SerializeField] private int                diceRoll_RedMana=0;
    [SerializeField] private int                diceRoll_BlueMana=0;
    [SerializeField] private int                diceRoll_YellowMana=0;
    [SerializeField] private int[]              diceRoll_byManaColor;

    [SerializeField] private Card[]             selected_cards;
    [SerializeField] private Card               battling_card;  

    public class Skill
    {
        public SkillData            skillData_onCard;
        public List<Dice.diceFace>  skill_ManaCost;

        public Skill(SkillData skillData)
        {
            skillData_onCard =  skillData;
            skill_ManaCost =    skillData.Skill_colorCost;
        }

        public int          ability_RedManaCost=0;
        public int          ability_BlueManaCost=0;
        public int          ability_YellowManaCost = 0;
        public int[]        ability_ManaCost_byManaType;

        public int          redMana_activations_onRoll = 0;
        public int          yellowMana_activationsonRoll = 0;
        public int          blueMana_activations_onRoll = 0;
        public List<int>    activations_on_roll_by_ManaType;

        public int          NumberOfAbilityActivations;

    }

    private Skill       First_Skill;
    private Skill       Second_Skill;
    private Skill       Third_Skill;
    private Skill[]     CurrentCard_Skills;

    public void GetRolledDiceFaces()
    {
        dice_rolled = BattleManager.instance.EnemyDices;

        foreach(Dice die in dice_rolled)
        {
            dice_rolled_faces.Append(die.Result);

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
        _skill.ability_ManaCost_byManaType = new int[3];

        foreach (var face in _skill.skill_ManaCost)
        {
            switch(face)
            {
                case Dice.diceFace.notRolled:
                    Debug.Log("face missing :(");
                    break;

                case Dice.diceFace.red:
                    _skill.ability_RedManaCost++;
                    break;

                case Dice.diceFace.yellow:
                    _skill.ability_YellowManaCost++;
                    break;

                case Dice.diceFace.blue:
                    _skill.ability_BlueManaCost++;
                    break;
            }
        }

        _skill.ability_ManaCost_byManaType[0] =  _skill.ability_RedManaCost;
        _skill.ability_ManaCost_byManaType[1] =  _skill.ability_YellowManaCost ;
        _skill.ability_ManaCost_byManaType[2] =  _skill.ability_BlueManaCost ;

        Debug.Log(_skill.ability_ManaCost_byManaType);
    }

    public void CheckAbilityIterations(Skill _skill)
    {
        for(int i = 0; i < 3; i++)
        {
            if (_skill.ability_ManaCost_byManaType[i] != 0)
            {
                _skill.activations_on_roll_by_ManaType.Add((int)(diceRoll_byManaColor[i]/_skill.ability_ManaCost_byManaType[i]));
            }
        }

    }

    public void CheckLesserNumberOfAbilityActivations(Skill _skill)
    {
        _skill.NumberOfAbilityActivations = _skill.activations_on_roll_by_ManaType.Min();
    }


    public void TurnLoop()
    {
        GetRolledDiceFaces();

        GetCurrentCardSkills();

        foreach (Skill skill in CurrentCard_Skills)
        {
            GetSingleAbilityManaCost(skill);

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
