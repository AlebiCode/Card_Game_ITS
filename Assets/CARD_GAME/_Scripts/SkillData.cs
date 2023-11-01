using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillData", menuName = "ScriptableObjects/Create Skill", order = 1)]
public class SkillData : ScriptableObject
{
    private const string ATTACK_NAME = "Colpo";
    private const string PARRY_NAME = "Parata";
    private const string DODGE_NAME = "Schivata";

    [SerializeField] private string skillName;
    [SerializeField] private List<Dice.diceFace> skill_colorCost = new List<Dice.diceFace>();
    [SerializeField] private int damage;
    [SerializeField] private int atkInstances;
    [SerializeField] private int defInstances;
    [SerializeField] private int dodge;
    [SerializeField] private bool precise;  //cant be dodged
    [SerializeField] private string animation = "ANIM STUFF HERE";

    public List<Dice.diceFace> Skill_colorCost => skill_colorCost;
    public string SkillName => skillName;
    public int Damage => damage;
    public int AtkInstances => atkInstances;
    public int DefInstances => defInstances;
    public int Dodge => dodge;
    public bool Precise => precise;

    public string GetSkillText() {
        bool hasAttack = Damage > 0;
        bool hasParry = DefInstances > 0;
        bool hasDodge = dodge > 0;
        string skillText = "";

        if (!hasAttack && !hasDodge && !hasParry)
            return skillText;



        //costruisco la stringa al contrario
        if (hasDodge) {
            skillText += DODGE_NAME + " " + Dodge + "%";
        }

        if (hasParry) {

            if (skillText != "") 
                skillText = ", " + skillText;

            skillText = PARRY_NAME + " " + DefInstances + skillText;
        }

        if(hasAttack) {
            
            if (skillText != "")
                skillText = ", " + skillText;

            skillText = ATTACK_NAME + " " + Damage + " x" + AtkInstances + skillText;
        }

        return skillText;

    }
}
