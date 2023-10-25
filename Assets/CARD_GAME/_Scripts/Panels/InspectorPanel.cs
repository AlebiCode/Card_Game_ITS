using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InspectorPanel : MonoBehaviour, IPointerClickHandler
{
   
    [SerializeField]Card card;

    public void OnPointerClick(PointerEventData eventData) {
        gameObject.SetActive(false);
    }

    public void SetCardToBeInspected(Card _card) {
        card.LoadData(_card.CardData);
        
    }
}
