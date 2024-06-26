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
    
    //[SerializeField] private GameObject myCombatCard;
    [SerializeField] private Transform myCombatCardTargetPosition;
    //[SerializeField] private GameObject enemyCombatCard;
    [SerializeField] private Transform enemyCombatCardTargetPosition;
    
    [SerializeField] private GameObject pickYourCardText;
    [SerializeField] private GameObject readyButton;
    [SerializeField] private GameObject myCards;
    [SerializeField] private GameObject enemyCards;
    [SerializeField] private GameObject[] enemyCardsList;
    [SerializeField] private float cardsMovement;
    [SerializeField] private float cardsMoveDuration;
    [SerializeField] private GameObject protectionPanel;
    
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
        Debug.Log("!!!" + myCards.transform.position.y);
        //select enemy card entering table panel for the first time 
        Instances.BattleManager.SelectEnemyCard();

    }

    public void RenterPanel()
    {
        //Se la carta era gi� selezionata la deseleziono
        if (selectedCard)
        {
            DeselectCard(selectedCard);
            protectionPanel.SetActive(false);
            SyncReadyButton();
        }

        //select enemy card re-entering table panel subsequent times
        Instances.BattleManager.SelectEnemyCard();

        //secondo round sceglie prima la CPU
        if (Instances.BattleManager.CurrentRound == 2) {
            Instances.BattleManager.EnemySelectedCard.particleSystemList.StartSelectionAnim(true);
        }
    }

    public void OnPlayerCardClick(Card card)
    {
        // Se la carta era gi� selezionata la deseleziono
        if (card == selectedCard)
        {
            DeselectCard(card);
            return;
        }

        // Se sto cambiando selezione deseleziono la carta scelta in precedenza
        if (selectedCard)
            DeselectCard(selectedCard);

        // Seleziono la nuova carta
        SelectCard(card);

    }

    public void OnEnemyCardClick(Card card )
    { }

    void SelectCard(Card card)
    {
        selectedCard = card;
        //selectedCard.Image.GetComponent<Image>().color = Color.yellow;
        selectedCard.particleSystemList.StartSelectionAnim(false);
        selectedCard.tag = "SelectedCard";

        SyncReadyButton();
    }

    void DeselectCard(Card card)
    {
        selectedCard.particleSystemList.StopSelectionAnim();
        //selectedCard.Image.GetComponent<Image>().color = Color.white;
        selectedCard.tag = "Untagged";
        selectedCard = null;
        SyncReadyButton();
    }

    void SyncReadyButton()
    {
        readyButton.SetActive(selectedCard);
        pickYourCardText.SetActive(!selectedCard);
    }

    private void OnDisable()
    {
        if (selectedCard)
            DeselectCard(selectedCard);
    }

    public void TablePanelToCombatPanelAnimation()
    {
        readyButton.SetActive(false);
        pickYourCardText.SetActive(false);

        mySelectedCard = GameObject.FindGameObjectWithTag("SelectedCard");
        
        myCards.transform.DOMoveY(-cardsMovement, cardsMoveDuration);
        enemyCards.transform.DOMoveY(cardsMovement, cardsMoveDuration);

        foreach(GameObject c in enemyCardsList)
        {
            c.transform.DORotate(new Vector3(0, 0, 180), cardsMoveDuration / 2);
        }

        protectionPanel.SetActive(true);
        StartCoroutine(StartMatch(cardsMoveDuration));
    }

    public void CombatPanelToTablePanelAnimation()
    {
        pickYourCardText.SetActive(true);

        myCards.transform.DOMoveY(myCardsInitialPosition.y, cardsMoveDuration / 2);
        enemyCards.transform.DOMoveY(enemyCardsInitialPosition.y, cardsMoveDuration / 2);

        foreach (GameObject c in enemyCardsList)
        {
            c.transform.DORotate(new Vector3(0, 0, 0), cardsMoveDuration / 2);
        }
    }

    // CALL THIS METHOD IN THE BATTLE MANAGER
    public IEnumerator ScaleAndMoveMyCard(GameObject myCombatCard)
    {
        //myCombatCard.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 400);

        yield return ScaleAndMove(myCombatCard, 1.84f, 1.4f, mySelectedCard.transform, myCombatCardTargetPosition.transform.position, 1.4f, cardsMoveDuration);
    }

    // CALL THIS METHOD IN THE BATTLE MANAGER
    public IEnumerator ScaleAndMoveEnemyCard(GameObject enemyCombatCard)
    {
        //enemyCombatCard.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 400);

        yield return ScaleAndMove(enemyCombatCard, 1.84f, 1.4f, Instances.BattleManager.EnemySelectedCard.transform, enemyCombatCardTargetPosition.transform.position, 1.4f, cardsMoveDuration);
    }

    private IEnumerator StartMatch(float time) { yield return new WaitForSeconds(time); Instances.BattleManager.StartMatch(); }

    private IEnumerator ScaleAndMove(GameObject combatCard, float endScaleValue, float scaleTime, Transform selectedCard, Vector3 endPosition, float moveTime, float startTimeAnimation)
    {
        //yield return new WaitForSeconds(startTimeAnimation);
        selectedCard.gameObject.SetActive(false);
        combatCard.SetActive(true);
        combatCard.transform.position = selectedCard.transform.position;
        ((RectTransform)combatCard.transform).sizeDelta = ((RectTransform)selectedCard.transform).sizeDelta;
        combatCard.transform.DOMove(endPosition, moveTime);
        combatCard.transform.DOScale(endScaleValue, scaleTime);
        yield return new WaitForSeconds(Mathf.Max(0, scaleTime, moveTime));
    }

    
}
