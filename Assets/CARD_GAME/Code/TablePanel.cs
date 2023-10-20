using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TablePanel : MonoBehaviour
{
    public static TablePanel instance;

    public AudioSource genericUiAudioSource;
    public Card selectedCard;
    [SerializeField] private GameObject readyButton;
    [SerializeField] private GameObject pickCardText;
    [SerializeField] private GameObject myCards;
    [SerializeField] private GameObject emenyCards;
    [SerializeField] private Vector2 cardsMovement;
    [SerializeField] private float cardsMoveDuration;

    private void Awake()
    {
        instance = this;
    }

    public void OnPlayerCardClick(Card card )
    {
        // Se la carta era già selezionata la deseleziono
        if (card == selectedCard)
        {
            DeselectCard(card);
            SyncReadyButton();
            return;
        }

        // Se sto cambiando selezione deseleziono la carta scelta in precedenza
        if (selectedCard)
            DeselectCard(selectedCard);

        // Seleziono la nuova carta
        SelectCard(card);

        SyncReadyButton();
    }

    public void OnEnemyCardClick(Card card )
    { }

    void SelectCard(Card card)
    {
        selectedCard = card;
        selectedCard.Image.GetComponent<Image>().color = Color.yellow;
    }

    void DeselectCard(Card card)
    {
        selectedCard.Image.GetComponent<Image>().color = Color.white;
        selectedCard = null;
    }

    void SyncReadyButton()
    {
        readyButton.SetActive(selectedCard);
    }

    private void OnDisable()
    {
        if (selectedCard)
            DeselectCard(selectedCard);
    }

    public void TablePanelToCombatPanelAnimation()
    {
        myCards.transform.DOLocalMove(-cardsMovement, cardsMoveDuration);
        emenyCards.transform.DOLocalMove(cardsMovement, cardsMoveDuration);
    }

    public void CombatPanelToTablePanelAnimation()
    {
        myCards.transform.DOLocalMove(cardsMovement, cardsMoveDuration / 2);
        emenyCards.transform.DOLocalMove(-cardsMovement, cardsMoveDuration / 2);
    }

}
