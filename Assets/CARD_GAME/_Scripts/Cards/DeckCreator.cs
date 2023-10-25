using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DeckCreator : MonoBehaviour
{
    private const int races = 5;
    private const int cards_per_race = 2;
    private const int cards_per_deck = 3;

    [SerializeField] private string cardsResourcesPath;
    [SerializeField] private string enemyResourcesPath;
    //[SerializeField] private GameObject cardPrefab;
    [SerializeField] private RectTransform cardsParent;
    [SerializeField] private GameObject startMatchButton;
    [SerializeField] private float cardSelectAnimSpeed = 1;

    [SerializeField] private List<Card> selectedCards = new List<Card>();

    private Vector2 standardSmallCardSize;

    public bool SelectionCompleted => selectedCards.Count == cards_per_deck;

    private void Start()
    {
        SpawnAllCards();
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
                standardSmallCardSize = card.RectTransform.sizeDelta;
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

    private void UpdateButton()
    {
        startMatchButton.SetActive(SelectionCompleted);
    }

    private void SelectCard(Card card)
    {
        card.particleSystemList.ActivateAnimation(VFX_TYPE.SELECT);
        selectedCards.Add(card);
        card.StopAllCoroutines();
        card.StartCoroutine(ScaleCard(card.RectTransform, standardSmallCardSize * 1.05f));
    }
    private void DeselectCard(Card card)
    {
        card.particleSystemList.DeactivateAnimation(VFX_TYPE.SELECT);
        selectedCards.Remove(card);
        card.StopAllCoroutines();
        card.StartCoroutine(ScaleCard(card.RectTransform, standardSmallCardSize));
    }

    public void LoadCombatCardData()
    {
        CardData[] cardData = new CardData[3];
        for (int i = 0; i < 3; i++)
            cardData[i] = selectedCards[i].CardData;

        BattleManager.instance.LoadData(cardData, GetRandomEnemy());
    }

    private EnemyData GetRandomEnemy() {
        EnemyData[] enemies = Resources.LoadAll<EnemyData>(enemyResourcesPath);
        return enemies[Random.Range(0, enemies.Length)];
    }

    private IEnumerator ScaleCard(RectTransform targetTransform, Vector2 targetSize)
    {
        while (targetTransform.sizeDelta.magnitude != targetSize.magnitude)
        {
            targetTransform.sizeDelta = Vector2.MoveTowards(targetTransform.sizeDelta, targetSize, cardSelectAnimSpeed * Time.deltaTime);
            yield return null;
        }
    }

}