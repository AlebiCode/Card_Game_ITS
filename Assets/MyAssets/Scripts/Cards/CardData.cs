using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardData", menuName = "ScriptableObjects/Create Card", order = 1)]
public class CardData : ScriptableObject
{
    [SerializeField] private Sprite sprite;
    [SerializeField] private string cardName = "Default Name";
    [SerializeField] private string cardDescription = "Default Description";
    [SerializeField] private SkillData[] skills = new SkillData[3];

    //DA METTERE
    //razza
    //3 mosse

    public Sprite Sprite => sprite;
    public string CardName => cardName;
    public string CardDescription => cardDescription;
    public SkillData[] Skills => skills;

}
