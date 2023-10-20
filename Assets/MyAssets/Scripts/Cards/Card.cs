using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    
    [SerializeField] private CardData cardData;

    [SerializeField] private Image image;
    [SerializeField] private TMP_Text cardName;
    [SerializeField] private TMP_Text cardDescription;
    [SerializeField] private RectTransform rectTransform;
    private UnityEvent<Card> onClick = new UnityEvent<Card>();

    public AudioCardManager audioSelect;
    private bool selected = false;


    public Image Image => image;
    public CardData CardData => cardData;
    public RectTransform RectTransform => rectTransform;
    public UnityEvent<Card> OnClick => onClick;

    public void LoadData(CardData cardData)
    {
        this.cardData = cardData;
        LoadGraphics();
    }

    public void LoadGraphics()
    {
        image.sprite = cardData.Sprite;
        cardName.text = cardData.CardName;
        cardDescription.text = cardData.CardDescription;
    }

    public void FireOnClick()
    {
        if (selected) {
            audioSelect.PlayDeSelectAudio();
            selected = false;
        } else {
            audioSelect.PlaySelectAudio();
            selected = true;
        }
        onClick.Invoke(this);
    }

}
