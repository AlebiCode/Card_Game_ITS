using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class InspectorPanel : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Card card;
    [SerializeField] private TMP_Text description;

    public void OnPointerClick(PointerEventData eventData) {
        gameObject.SetActive(false);
    }

    public void SetCardToBeInspected(Card _card) {
        card.LoadData(_card.CardData);
        description.text = _card.CardData.CardDescription;
    }
}
