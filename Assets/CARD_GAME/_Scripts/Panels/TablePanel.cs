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
    private GameObject mySelectedCard;
    [SerializeField] private GameObject myCombatCard;
    [SerializeField] private Transform myCombatCardTargetPosition; 
    [SerializeField] private GameObject readyButton;
    [SerializeField] private GameObject pickCardText;
    [SerializeField] private GameObject myCards;
    [SerializeField] private GameObject enemyCards;
    [SerializeField] private float cardsMovement;
    [SerializeField] private float cardsMoveDuration;
    private Vector3 myCardsInitialPosition;
    private Vector3 enemyCardsInitialPosition;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        myCardsInitialPosition = myCards.transform.position;
        enemyCardsInitialPosition = enemyCards.transform.position;
    }

    public void OnEnable()
    {
        // Se la carta era già selezionata la deseleziono
        if (!selectedCard)
        {
            DeselectCard(selectedCard);
            SyncReadyButton();
            return;
        }
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
        selectedCard.tag = "SelectedCard";
    }

    void DeselectCard(Card card)
    {
        selectedCard.Image.GetComponent<Image>().color = Color.white;
        selectedCard.tag = "Untagged";
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
        mySelectedCard = GameObject.FindGameObjectWithTag("SelectedCard");
        
        myCards.transform.DOLocalMoveY(-cardsMovement, cardsMoveDuration);
        enemyCards.transform.DOLocalMoveY(cardsMovement, cardsMoveDuration);
        
        StartCoroutine(WaitAndDeactivate(cardsMoveDuration, mySelectedCard));

        myCombatCard.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 400);
        //myCombatCard.transform.position = mySelectedCard.transform.position;

        StartCoroutine(ScaleAndMove(myCombatCard, 1.84f, 1.4f, mySelectedCard.transform, myCombatCardTargetPosition.transform.position, 1.4f, cardsMoveDuration));

        //myCombatCard.transform.DOScale(1.84f, 1.84f);
        //myCombatCard.transform.DOMove(myCombatCardTargetPosition, 1);

    }

    public void CombatPanelToTablePanelAnimation()
    {
        myCards.transform.DOLocalMoveY(myCardsInitialPosition.y, cardsMoveDuration / 2);
        enemyCards.transform.DOLocalMoveY(enemyCardsInitialPosition.y, cardsMoveDuration / 2);
    }

    IEnumerator WaitAndDeactivate(float time, GameObject obj) { yield return new WaitForSeconds(time); obj.SetActive(false); }

    IEnumerator ScaleAndMove(GameObject obj, float endScaleValue, float scaleTime, Transform selectedCard, Vector3 endPosition, float moveTime, float startTimeAnimation)
    {
        yield return new WaitForSeconds(startTimeAnimation);
        obj.transform.DOScale(endScaleValue, scaleTime);
        obj.transform.position = selectedCard.transform.position;
        obj.transform.DOMove(endPosition, moveTime);
    }

    
}
