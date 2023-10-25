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
    [SerializeField]private GameObject enemySelectedCard;
    
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
        selectedCard.particleSystemList.ActivateAnimation(VFX_TYPE.SELECT);
        selectedCard.tag = "SelectedCard";

        SyncReadyButton();
    }

    void DeselectCard(Card card)
    {
        selectedCard.particleSystemList.DeactivateAnimation(VFX_TYPE.SELECT);
        //selectedCard.Image.GetComponent<Image>().color = Color.white;
        selectedCard.tag = "Untagged";
        selectedCard = null;
        SyncReadyButton();
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
        readyButton.SetActive(false);
        pickYourCardText.SetActive(false);

        mySelectedCard = GameObject.FindGameObjectWithTag("SelectedCard");
        
        myCards.transform.DOLocalMoveY(-cardsMovement, cardsMoveDuration);
        enemyCards.transform.DOLocalMoveY(cardsMovement, cardsMoveDuration);

        foreach(GameObject c in enemyCardsList)
        {
            c.transform.DORotate(new Vector3(0, 0, 180), cardsMoveDuration / 2);
        }
       

    }

    public void CombatPanelToTablePanelAnimation()
    {
        pickYourCardText.SetActive(true);

        myCards.transform.DOLocalMoveY(myCardsInitialPosition.y, cardsMoveDuration / 2);
        enemyCards.transform.DOLocalMoveY(enemyCardsInitialPosition.y, cardsMoveDuration / 2);

        foreach (GameObject c in enemyCardsList)
        {
            c.transform.DORotate(new Vector3(0, 0, 0), cardsMoveDuration / 2);
        }
    }

    // CALL THIS METHOD IN THE BATTLE MANAGER
    public void ScaleAndMoveMyCard(GameObject myCombatCard)
    {
        StartCoroutine(WaitAndDeactivate(cardsMoveDuration, mySelectedCard));

        myCombatCard.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 400);
        myCombatCard.SetActive(false);

        StartCoroutine(ScaleAndMove(myCombatCard, 1.84f, 1.4f, mySelectedCard.transform, myCombatCardTargetPosition.transform.position, 1.4f, cardsMoveDuration));
    }

    // CALL THIS METHOD IN THE BATTLE MANAGER
    public void ScaleAndMoveEnemyCard(GameObject enemyCombatCard)
    {
        StartCoroutine(WaitAndDeactivate(cardsMoveDuration, enemySelectedCard));

        enemyCombatCard.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 400);
        enemyCombatCard.SetActive(false);

        StartCoroutine(ScaleAndMove(enemyCombatCard, 1.84f, 1.4f, enemySelectedCard.transform, enemyCombatCardTargetPosition.transform.position, 1.4f, cardsMoveDuration));
    }

    IEnumerator StartMatch(float time) { yield return new WaitForSeconds(time); BattleManager.instance.StartMatch(); }
    IEnumerator WaitAndDeactivate(float time, GameObject obj) { yield return new WaitForSeconds(time); obj.SetActive(false); }

    IEnumerator ScaleAndMove(GameObject combatCard, float endScaleValue, float scaleTime, Transform selectedCard, Vector3 endPosition, float moveTime, float startTimeAnimation)
    {
        yield return new WaitForSeconds(startTimeAnimation);
        combatCard.transform.localScale = new Vector3(0.55f, 0.55f, 1);
        combatCard.SetActive(true);
        combatCard.transform.DOScale(endScaleValue, scaleTime);
        combatCard.transform.position = selectedCard.transform.position;
        combatCard.transform.DOMove(endPosition, moveTime);
    }

    
}
