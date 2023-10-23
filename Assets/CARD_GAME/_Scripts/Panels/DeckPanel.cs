using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckPanel : MonoBehaviour
{
    public List<GameObject> cardList;
    public GameObject readyButton;

    public void SelectCard(GameObject cardToAdd)
    {
        // Se la carta era già stata selezionata la deseleziono
        if (cardList.Contains(cardToAdd))
        {
            DeselectCard(cardToAdd);
            return;
        }

        // Se erano già state selezionate tre carte non posso selezionarne altre
        if (cardList.Count == 3)
        {
            // Pop up di errore
            return;
        }

        // Aggiungiamo la carta alla lista delle selezionate
        cardList.Add(cardToAdd);

        // TODO: Aggiungere fx vari di selezione
        // TODO: Attiviamo effetto di selezione (sprite o fx)
        cardToAdd.GetComponent<Image>().color = Color.white / 2;

        if (cardList.Count == 3)
        {
            readyButton.SetActive(true);
        }
    }

    void DeselectCard(GameObject cardToRemove)
    {
        // TODO: Aggiungere fx vari di deselezione
        // TODO: Disattiviamo effetto di selezione (sprite o fx)
        cardToRemove.GetComponent<Image>().color = Color.white;

        cardList.Remove(cardToRemove);

        readyButton.SetActive(false);
    }
}
