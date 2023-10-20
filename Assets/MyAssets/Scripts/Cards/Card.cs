using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour, IPointerClickHandler
{
    
    [SerializeField] private CardData cardData;

    [SerializeField] private Image image;
    [SerializeField] private TMP_Text cardName;
    [SerializeField] private TMP_Text cardDescription;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private bool lmbIneractable;
    [SerializeField] private bool rmbInteractable;
    [SerializeField] private UnityEvent<Card> lmbClick = new UnityEvent<Card>();

    public AudioCardManager audioSelect;
    private bool selected = false;


    public Image Image => image;
    public CardData CardData => cardData;
    public RectTransform RectTransform => rectTransform;
    public UnityEvent<Card> OnClick => lmbClick;

    public void LoadData(CardData cardData)
    {
        this.cardData = cardData;
        LoadGraphics();
    }
    public void LoadData(CardData cardData, bool lmbIneractable, bool rmbInteractable)
    {
        this.lmbIneractable = lmbIneractable;
        this.rmbInteractable = rmbInteractable;
        LoadData(cardData);
    }

    public void LoadGraphics()
    {
        image.sprite = cardData.Sprite;
        cardName.text = cardData.CardName;
        cardDescription.text = cardData.CardDescription;
    }

    public void FireOnClick()
    {
<<<<<<< Updated upstream
        if (selected) {
            audioSelect.PlayDeSelectAudio();
            selected = false;
        } else {
            audioSelect.PlaySelectAudio();
            selected = true;
        }
        onClick.Invoke(this);
=======
        lmbClick.Invoke(this);
>>>>>>> Stashed changes
    }

    public float EnterCombatSceneAnim() //retunrs total duration of animation
    {
        float duration = 0;
        duration = PlayEnteringAudio();
        return duration;
    }
    private float PlayEnteringAudio()
    {
        AudioClip clip = cardData.CardAudioProfile.GetRandomClip();
        AudioManager.PlayCardAudio(clip); 
        return clip.length;
    }

    public void OnPointerEnter()
    {
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && lmbIneractable)
        {
            lmbClick.Invoke(this);
        }
        else if (eventData.button == PointerEventData.InputButton.Right && lmbIneractable)
        {
            Debug.Log("Right click");
        }
    }
}
