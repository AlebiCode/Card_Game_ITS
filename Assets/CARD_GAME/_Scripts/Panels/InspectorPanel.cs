using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InspectorPanel : MonoBehaviour, IPointerClickHandler
{
   
    Card card;

    public void OnPointerClick(PointerEventData eventData) {
        gameObject.SetActive(false);
    }

    public void SetCardToBeInspected(Card card) {
        this.card = card;
    }
}
