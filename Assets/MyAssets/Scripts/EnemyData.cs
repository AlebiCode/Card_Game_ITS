using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TEMPORANEO
public enum EnemyPlaystyle { Aggressive, Timid, Balanced }

[CreateAssetMenu(fileName = "EnemyData", menuName = "ScriptableObjects/")]
public class EnemyData : ScriptableObject
{
    [SerializeField] string enemyName;
    [SerializeField] Card[] enemyCards;
    [SerializeField] EnemyPlaystyle enemyPlaystyle;

    public string EnemyName => enemyName;
    public Card[] EnemyCards => enemyCards;
    public EnemyPlaystyle EnemyPlaystyle => enemyPlaystyle;

}
