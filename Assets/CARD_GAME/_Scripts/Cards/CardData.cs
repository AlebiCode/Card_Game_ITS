using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardData", menuName = "ScriptableObjects/Create Card", order = 1)]
public class CardData : ScriptableObject
{
    [SerializeField] private Sprite sprite;
    [SerializeField] private Vector2 framedPosition;
    [SerializeField] private Vector2 framedSize;
    [SerializeField] private Color backGround;
    //[SerializeField] private Sprite sprite;
    //[SerializeField] private Sprite spriteLarge;
    [SerializeField] private string cardName = "Default Name";
    [SerializeField] private string cardDescription = "Default Description";
    [SerializeField] private CardAudioProfile cardAudioProfile;
    [SerializeField] private SkillData[] skills = new SkillData[3];
    [SerializeField] private RACES race;


    public Sprite Sprite => sprite;
    public Vector2 FramedPosition => framedPosition;
    public Vector2 FramedSize => framedSize;
    public Color BackGroundColor => backGround;
    //public Sprite SpriteLarge => spriteLarge;
    public string CardName => cardName;
    public string CardDescription => cardDescription;
    public SkillData[] Skills => skills;
    public CardAudioProfile CardAudioProfile => cardAudioProfile;
    public RACES Race => race;
} 
