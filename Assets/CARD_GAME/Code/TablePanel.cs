using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TablePanel : MonoBehaviour
{
    public Card selectedCard;

    public GameObject readyButton;


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

}
