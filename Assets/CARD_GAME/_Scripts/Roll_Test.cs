using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roll_Test : MonoBehaviour
{
    [System.Serializable]
    private struct DiceSprites
    {
        public Sprite[] rollSprites;
        public Sprite endingSprite;
    }

    [SerializeField] private SpriteRenderer mainSpriteRenderer;
    [SerializeField] private SpriteRenderer ghostSpriteRenderer;
    [SerializeField] private float rollInterval;
    [SerializeField] private bool randomizeSet;
    [SerializeField] private int currentSet;
    [SerializeField] private DiceSprites[] rollSets;

    private int currentSprite;
    private float rollTimeCounter;


    // Update is called once per frame
    void Update()
    {
        if (rollTimeCounter >= rollInterval)
        {
            if(++currentSprite >= rollSets[currentSet].rollSprites.Length)
                currentSprite = 0;
            ghostSpriteRenderer.sprite = mainSpriteRenderer.sprite;
            mainSpriteRenderer.sprite = rollSets[currentSet].rollSprites[currentSprite];
            rollTimeCounter = 0;
        }
        rollTimeCounter += Time.deltaTime;
    }

    private void OnEnable()
    {
        if(randomizeSet)
            currentSet = Random.Range(0, rollSets.Length);
        rollTimeCounter = currentSprite = 0;
        mainSpriteRenderer.sprite = rollSets[currentSet].rollSprites[currentSprite];
        ghostSpriteRenderer.enabled = true;
        ghostSpriteRenderer.sprite = mainSpriteRenderer.sprite;
        Color color = mainSpriteRenderer.color;
        color.a = 0.4f;
        ghostSpriteRenderer.color = color;
    }

    private void OnDisable()
    {
        mainSpriteRenderer.sprite = rollSets[currentSet].endingSprite;
        ghostSpriteRenderer.enabled = false;
    }

}
