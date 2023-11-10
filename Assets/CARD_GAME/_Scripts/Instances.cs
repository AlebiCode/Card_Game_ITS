using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Instances : MonoBehaviour
{
    public static Instances instances;
    
    [SerializeField] private BattleManager battleManager;

    public static BattleManager BattleManager => instances.battleManager;

    private void Start()
    {
        instances = this;
    }

}
