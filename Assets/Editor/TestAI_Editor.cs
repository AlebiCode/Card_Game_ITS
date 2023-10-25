using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Runtime.CompilerServices;

[CustomEditor(typeof(EnemyBrain))]
public class TestAI_Editor : Editor
{

    public override void OnInspectorGUI()
    {
        EnemyBrain enemyBrain = (EnemyBrain)target;

        if(GUILayout.Button("Roll Dices"))
        {
            enemyBrain.RollDiceButtonTest();
        }

        if(GUILayout.Button("Calculate Activations"))
        {
            enemyBrain.CalculateSkillActivationsButtonTest();
        }

        if (GUILayout.Button("Calculate Chance"))
        {
            enemyBrain.CalculateChanceButtonTest();
        }



        base.OnInspectorGUI();
    }




}
