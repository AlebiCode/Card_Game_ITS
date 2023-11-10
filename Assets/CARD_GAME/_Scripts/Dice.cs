using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class Dice : MonoBehaviour
{
    public enum diceFace { notRolled, red, blue, yellow }

    [SerializeField] private Image mainImage;
    [SerializeField] private Image ghostImage;
    public ParticleSystemActivator particle;

    private diceFace result = diceFace.notRolled;
    private bool locked = false;

    public diceFace Result => result;
    public bool IsLocked => locked;

    private void Start()
    {
        mainImage.sprite = DiceResources.RollSets[DiceResources.GetSetIndex].endingSprite;
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
    public void LockDiceDebug(bool value)
    {
        locked = value;

    }
    public void SetResult(Dice.diceFace _result)
    {
        result = _result;
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
        mainImage.color = Color.white;
    }

    public void StartRollAnimation(float fadeDuration)
    {
        StopAllCoroutines();
        StartCoroutine(RollAnimation(fadeDuration));
    }
    public void StopRollAnimation()
    {
        StopAllCoroutines();
        mainImage.DOColor(FaceToRGB(result), 1);
        mainImage.sprite = DiceResources.RollSets[DiceResources.GetSetIndex].endingSprite;
        ghostImage.gameObject.SetActive(false);
        //transform.DORotate(Vector3.zero, duration);
    }
    private IEnumerator RollAnimation(float fadeDuration)
    {
        ghostImage.gameObject.SetActive(true);
        mainImage.DOColor(Color.white, fadeDuration);
        int rollSet = DiceResources.GetSetIndex;
        int face = 0;
        while (true)
        {
            ghostImage.sprite = mainImage.sprite;
            mainImage.sprite = DiceResources.RollSets[rollSet].rollSprites[face++];
            ghostImage.color = new Color(mainImage.color.r, mainImage.color.g, mainImage.color.b, 0.4f);
            if (face >= DiceResources.RollSets[rollSet].rollSprites.Length)
                face = 0;
            //transform.Rotate(new Vector3(0, 0, rotationSpeed * Time.deltaTime));
            yield return new WaitForSeconds(DiceResources.RollInterval);
        }
    }

}