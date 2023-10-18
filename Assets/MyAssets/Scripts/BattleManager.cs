using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;
    [SerializeField]private Card[] carteNemiche = new Card[3];
    [SerializeField]private Card[] carteAmiche = new Card[3];
    [SerializeField] private GameObject rerollButton;
    [SerializeField] private GameObject FightButton;
    [SerializeField] private TMP_Text[] diceButtons;
    [SerializeField] private TablePanel tablePanel;

    [SerializeField] private (Dice.ManaTpye, bool)[] facceUsciteAmiche = new (Dice.ManaTpye, bool)[6];
    [SerializeField] private (Dice.ManaTpye, bool)[] facceUsciteNemiche = new (Dice.ManaTpye, bool)[6];
    //[SerializeField] private List<int> diceToLock = new List<int>();
    [SerializeField] private int cartaSelAmicaIndex;
    [SerializeField] private int cartaSelNemicaIndex;

    bool firstRoll = true;

    private Card CartaSelAmica => carteAmiche[cartaSelAmicaIndex];
    private Card CartaSelNemica => carteNemiche[cartaSelNemicaIndex];

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        //DiceRoll();
    }

    public void StartMatch(CardData[] carteProprie, CardData[] carteAvversario) 
    { 
        
        for (int i = 0; i < 3; i++)
        {
            carteAmiche[i].LoadData(carteProprie[i]);
            carteNemiche[i].LoadData(carteAvversario[i]);
            carteAmiche[i].OnClick.AddListener(SelectCard);
        }
        firstRoll = true;

    }
    private void RollUnlockedDice((Dice.ManaTpye, bool)[] facceUscite) 
    { 
        for(int i = 0; i < facceUscite.Length; i++) 
        {
            if (facceUscite[i].Item2 == false) 
            { 
                facceUscite[i].Item1 = Dice.RollDice();
                diceButtons[i].text = facceUscite[i].Item1.ToString();
                Debug.Log("dado: " + i + ", è uscito: " + facceUscite[i].Item1);
            }
        }
    }

    public void LockDice(int i) 
    {
        facceUsciteAmiche[i].Item2 = !facceUsciteAmiche[i].Item2;
        bool canFight = CanEnableFight();
        rerollButton.SetActive(!canFight);
        FightButton.SetActive(canFight);

    }

    private void SelectCard(Card card) 
    { 
        for(int i = 0; i<3; i++) 
        { 
            if(card == carteAmiche[i]) 
            {
                cartaSelAmicaIndex = i;
                break;
            }
        }

        cartaSelNemicaIndex = Random.Range(0, 3);
    }

    private bool CanEnableFight() 
    { 
        for (int i = 0; i < 6; i++)
        {
            if (facceUsciteAmiche[i].Item2 == false)
                return false;
        }
        return true;
    }

    public void OnRollClick()
    {
        RollUnlockedDice(facceUsciteAmiche);
        RollUnlockedDice(facceUsciteNemiche);

        if (firstRoll)
        {
            EnemyRollLock();
            firstRoll = false;
        }
        else
        {
            for (int i = 0; i < 6; i++)
            {
                facceUsciteNemiche[i].Item2 = true;
                facceUsciteAmiche[i].Item2 = true;
                diceButtons[i].color = Color.red;
            }
            rerollButton.SetActive(false);
            FightButton.SetActive(true);
        }
    }

    private void EnemyRollLock()
    {

    }

}
