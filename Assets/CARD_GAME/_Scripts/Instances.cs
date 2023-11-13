using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Instances : MonoBehaviour
{
    public static Instances instances;
    
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private LorePanel lorePanel;
    [SerializeField] private TransitionManager transitionManager;

    public static BattleManager BattleManager => instances.battleManager;
    public static LorePanel LorePanel => instances.lorePanel;
    public static TransitionManager TransitionManager => instances.transitionManager;


    private void Awake()
    {
        instances = this;
    }

}
