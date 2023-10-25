using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class Dice : MonoBehaviour
{
    public enum diceFace { notRolled, red, blue, yellow }

    public ParticleSystemActivator particle;

    private Image image;
    private diceFace result = diceFace.notRolled;
    private bool locked = false;

    public diceFace Result => result;
    public bool IsLocked => locked;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void RollDice()
    {
        int roll = Random.Range(0,6);
        if(roll <= 2)
            result = diceFace.red;
        else if(roll <= 4)
            result = diceFace.yellow;
        else
            result = diceFace.blue;
    }

    public void LockDice(bool value)
    {
        locked = value;

        AudioManager.PlayUiDiceLocked();

        // Attiva/Disattiva grafica o fx di selezione
        GetComponentInChildren<TMPro.TMP_Text>().text = locked ? "Locked" : "";
    }

    public static Color FaceToRGB(diceFace diceFace)
    {
        switch (diceFace)
        {
            case diceFace.red:
                return Color.red;
            case diceFace.blue:
                return Color.blue;
            case diceFace.yellow:
                return Color.yellow;
        }
        return Color.white;
    }
    public void ResetColor()
    {
        image.color = Color.white;
    }

    public void StartRollAnimation(float rotationSpeed, float fadeDuration)
    {
        StopAllCoroutines();
        StartCoroutine(RollAnimation(rotationSpeed, fadeDuration));
    }
    public void StopRollAnimation(float duration)
    {
        StopAllCoroutines();
        image.DOColor(FaceToRGB(result), duration);
        transform.DORotate(Vector3.zero, duration);
    }
    private IEnumerator RollAnimation(float rotationSpeed, float fadeDuration)
    {
        image.DOColor(Color.white, fadeDuration);
        while (true)
        {
            transform.Rotate(new Vector3(0, 0, rotationSpeed * Time.deltaTime));
            yield return null;
        }
    }

}