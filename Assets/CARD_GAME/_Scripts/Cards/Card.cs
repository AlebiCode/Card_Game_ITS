using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
    
    [SerializeField] private CardData cardData;

    [SerializeField] private Image image;
    [SerializeField] private TMP_Text cardName;
    [SerializeField] private TMP_Text cardDescription;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Skill[] skills;
    [SerializeField] private bool lmbInteractable;
    [SerializeField] private bool rmbInteractable;
    [SerializeField] private UnityEvent lmbClick = new UnityEvent();
    [SerializeField] private UnityEvent<Card> rmbClick = new UnityEvent<Card>();
    [SerializeField] private UnityEvent pointerEnter = new UnityEvent();

    public ParticleSystemActivator particleSystemList;

    public Image Image => image;
    public CardData CardData => cardData;
    public RectTransform RectTransform => rectTransform;

    public void LoadData(CardData cardData)
    {
        this.cardData = cardData;
        LoadGraphics();
    }
    public void LoadData(CardData cardData, bool lmbIneractable, bool rmbInteractable)
    {
        this.lmbInteractable = lmbIneractable;
        this.rmbInteractable = rmbInteractable;
        LoadData(cardData);
    }

    public void LoadGraphics()
    {
        image.sprite = cardData.Sprite;
        cardName.text = cardData.CardName;
        cardDescription.text = cardData.CardDescription;
        for (int i = 0; i < 3; i++)
        {
            int j = 0;
            while (j < 3)
            {
                if (j < cardData.Skills[i].Skill_colorCost.Count)
                {
                    var face = cardData.Skills[i].Skill_colorCost[j];
                    skills[i].SkillDices[j].gameObject.SetActive(true);
                    //public enum diceFace { notRolled, red, blue, yellow }
                    skills[i].SkillDices[j].color = Dice.FaceToRGB(face);
                }
                else
                {
                    skills[i].SkillDices[j].gameObject.SetActive(false);
                }
                j++;
            }
        }
    }

    public float EnterCombatSceneAnim() //retunrs total duration of animation
    {
        float duration = 0;
        duration = PlayEnteringAudio();
        return duration;
    }
    private float PlayEnteringAudio()
    {
        float offsetTime = 0.5f;
        AudioClip entranceClip = cardData.CardAudioProfile.GetRandomEntranceClip();
        AudioClip lineClip = cardData.CardAudioProfile.GetRandomLinesClip();
        AudioManager.PlayCardAudio(entranceClip);
        AudioManager.PlayCardAudio(lineClip, offsetTime);
        return Mathf.Max(0, entranceClip.length + offsetTime, lineClip.length);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && lmbInteractable)
        {
            //Debug.Log("Left click");
            lmbClick.Invoke();
        }
        else if (eventData.button == PointerEventData.InputButton.Right && rmbInteractable)
        {
            rmbClick.Invoke(this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerEnter.Invoke();
    }
}