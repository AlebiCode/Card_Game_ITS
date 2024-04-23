using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class DeckCreator : MonoBehaviour
{
    private const int races = 5;
    private const int cards_per_race = 2;
    private const int cards_per_deck = 3;

    [SerializeField] private string cardsResourcesPath;
    [SerializeField] private string enemyResourcesPath;
    [SerializeField] private AudioClip music;
    //[SerializeField] private GameObject cardPrefab;
    [SerializeField] private RectTransform cardsParent;
    [SerializeField] private GameObject startMatchButton;
    [SerializeField] private GameObject cardSelectionText;
    [SerializeField] private float cardSelectAnimSpeed = 1;

    [SerializeField] private List<Card> selectedCards = new List<Card>();

    private Vector3 standardSmallCardSize;

    [SerializeField] private Card easterEggCard;
    [SerializeField] private Sprite easterSprite;
    [SerializeField] private AudioClip easterAudioClip;
    private int easterEggClicks;

    public bool SelectionCompleted => selectedCards.Count == cards_per_deck;

    private void Start()
    {
        SpawnAllCards();
    }
    private void OnEnable()
    {
        StartCoroutine(AudioCoroutine());
    }
    private IEnumerator AudioCoroutine()
    {
        yield return null;
        if (music)
            AudioManager.PlayAudio(music, 3, loop: true);
    }
    private void OnDisable()
    {
        if(!startMatchButton.IsDestroyed())
            startMatchButton?.SetActive(false);
    }

    private void SpawnAllCards()
    {
        CardData[] cardDatas = Resources.LoadAll<CardData>(cardsResourcesPath);
        int spawnedCards = 0;
        //foreach (var data in cardDatas)
        for(int i = 0; i < cardsParent.childCount; i++)
        {
            if (spawnedCards < cardDatas.Length)
            {
                Card card = cardsParent.GetChild(spawnedCards++).GetComponentInChildren<Card>();
                card.LoadData(cardDatas[i]);
                //card.OnClick.AddListener(OnCardClick);
                card.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                standardSmallCardSize = card.transform.localScale;
            }
            else
            {
                cardsParent.GetChild(spawnedCards++).gameObject.SetActive(false);
            }
        }
    }

    public void OnCardClick(Card card)
    {
        if (selectedCards.Contains(card))
        {
            DeselectCard(card);
        }
        else if (selectedCards.Count < 3)
        {
            SelectCard(card);
        }
        UpdateButton();
    }
    public void EasterEggClick()
    {
        Debug.Log("???");
        easterEggClicks++;
        if (easterEggClicks == 9)
        {
            easterEggCard.gameObject.SetActive(true);
            easterEggCard.LoadGraphics();
            AudioManager.PlayAudio(easterAudioClip, 4);
        }
        if (easterEggClicks == 20)
        {
            Image[] images = cardsParent.GetComponentsInChildren<Image>();
            foreach (Image image in images) { image.sprite = easterSprite; }
        }
    }

    private void SelectCard(Card card)
    {
        card.particleSystemList.StartSelectionAnim(false);
        selectedCards.Add(card);
        card.StopAllCoroutines();
        card.StartCoroutine(ScaleCard(card.transform, standardSmallCardSize * 1.05f));
    }
    private void DeselectCard(Card card)
    {
        card.particleSystemList.StopSelectionAnim();
        selectedCards.Remove(card);
        card.StopAllCoroutines();
        card.StartCoroutine(ScaleCard(card.RectTransform, standardSmallCardSize));
    }
    private void UpdateButton()
    {
        cardSelectionText.SetActive(!SelectionCompleted);
        startMatchButton.SetActive(SelectionCompleted);
        
    }

    public void LoadCombatCardData()
    {
        CardData[] cardData = new CardData[3];
        for (int i = 0; i < 3; i++)
            cardData[i] = selectedCards[i].CardData;

        Instances.BattleManager.LoadData(cardData, GetRandomEnemy());
    }

    private EnemyData GetRandomEnemy() {
        EnemyData[] enemies = Resources.LoadAll<EnemyData>(enemyResourcesPath);
        return enemies[Random.Range(0, enemies.Length)];
    }

    private IEnumerator ScaleCard(Transform transform, Vector3 targetSize)
    {
        while (transform.localScale != targetSize)
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, targetSize, cardSelectAnimSpeed * Time.deltaTime);
            yield return null;
        }
    }

}