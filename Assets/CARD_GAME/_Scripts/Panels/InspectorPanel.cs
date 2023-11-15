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
        AudioManager.StopSourceGroupVolumeDecreaseMethod(2);
        gameObject.SetActive(false);
    }

    public void InspectCard(Card _card)
    {
        AudioManager.StopSourceGroup(2);
        gameObject.SetActive(true);
        SetCardToBeInspected(_card);
        AudioManager.PlayAudio(card.CardData.CardAudioProfile.DescriptionClip, 2);
    }

    private void SetCardToBeInspected(Card _card) {
        card.LoadData(_card.CardData);
        description.text = _card.CardData.CardDescription;
    }


}
