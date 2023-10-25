using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RotateObj : MonoBehaviour
{
    public void Rotate(GameObject obj, Vector3 targetRotation, float timeRotation)
    {
        obj.transform.DORotate(targetRotation, timeRotation);
    }   
    
}
