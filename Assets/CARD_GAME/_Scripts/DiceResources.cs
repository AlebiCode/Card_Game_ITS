using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceResources : MonoBehaviour
{
    public static DiceResources instance;

    [System.Serializable] public struct DiceSprites
    {
        public Sprite[] rollSprites;
        public Sprite endingSprite;
    }

    [SerializeField] private float rollInterval;
    [SerializeField] private int defaultSet;
    [SerializeField] private bool randomizeSet;
    [SerializeField] private DiceSprites[] rollSets;

    public static DiceSprites[] RollSets => instance.rollSets;
    public static float RollInterval => instance.rollInterval;
    public static int GetSetIndex => instance.randomizeSet ? Random.Range(0, RollSets.Length) : instance.defaultSet;
    //public static bool RandomizeSet => instance.randomizeSet;


    private void Awake()
    {
        if (instance)
            Destroy(this);
        else
            instance = this;
    }

}
