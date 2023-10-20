using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardData", menuName = "ScriptableObjects/Create Card", order = 1)]
public class CardData : ScriptableObject
{
    [SerializeField] private Sprite sprite;
    [SerializeField] private Sprite spriteLarge;
    [SerializeField] private string cardName = "Default Name";
    [SerializeField] private string cardDescription = "Default Description";
    [SerializeField] private CardAudioProfile cardAudioProfile;
    [SerializeField] private SkillData[] skills = new SkillData[3];

    public Sprite Sprite => sprite;
    public Sprite SpriteLarge => spriteLarge;
    public string CardName => cardName;
    public string CardDescription => cardDescription;
    public SkillData[] Skills => skills;
    public CardAudioProfile CardAudioProfile => cardAudioProfile;

}
