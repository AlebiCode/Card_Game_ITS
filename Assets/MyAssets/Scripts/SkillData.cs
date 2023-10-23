using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillData", menuName = "ScriptableObjects/Create Skill", order = 1)]
public class SkillData : ScriptableObject
{
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

}
