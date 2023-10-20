using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TEMPORANEO
public enum EnemyPlaystyle { Aggressive, Timid, Balanced }

[CreateAssetMenu(fileName = "EnemyData", menuName = "ScriptableObjects/Create Enemy")]
public class EnemyData : ScriptableObject
{
    [SerializeField] string enemyName;
    [SerializeField] CardData[] enemyCardDatas = new CardData[3];
    [SerializeField] EnemyPlaystyle enemyPlaystyle;

    public string EnemyName => enemyName;
    public CardData[] EnemyCardDatas => enemyCardDatas;
    public EnemyPlaystyle EnemyPlaystyle => enemyPlaystyle;

}
