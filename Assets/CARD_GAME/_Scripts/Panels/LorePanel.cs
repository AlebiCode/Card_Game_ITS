using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LorePanel : MonoBehaviour
{
    //per quanto tempo lo schermo è nero senza scritte (casi per prima e dopo scritte)
    private const float LORE_PRE_LINES_TIME = 1.5f;
    private const float LORE_POST_LINES_TIME = 1f;
    //velocità animazioni scritte
    private const float LORE_LINE_FADE_IN_SPEED = 2.5f;
    private const float LORE_LINES_FADE_OUT_SPEED = 2.5f;
    //pannello
    private const float LORE_PANEL_ENDING_FADE_OUT_SPEED = 2.5f;


    [Header("Lore Panel")]
    //[SerializeField] private CanvasGroup loreCanvasGroup;
    [SerializeField] private VerticalLayoutGroup verticalLayoutGroup;
    [SerializeField] private AudioClip loreClip;
    [SerializeField] private TMP_Text exampleTextGO;
    [SerializeField] private LorePage[] lorePages;
    List<TMP_Text> texts = new List<TMP_Text>();

    private CanvasGroup FadeCanvasGroup => Instances.TransitionManager.FadeCanvasGroup;

    public void StartLore()
    {
        FadeCanvasGroup.gameObject.SetActive(true);
        gameObject.SetActive(true);
        StartCoroutine(ShowLore());
    }
    public void SkipLore()
    {
        StopAllCoroutines();
        StartCoroutine(EndLore());
        AudioManager.StopSourceGroupVolumeDecreaseMethod(2);
    }
    private IEnumerator ShowLore()
    {
        FadeCanvasGroup.gameObject.SetActive(true);
        gameObject.SetActive(true);

        exampleTextGO.gameObject.SetActive(false);

        yield return new WaitForSeconds(LORE_PRE_LINES_TIME);
        AudioSource audioSource = AudioManager.PlayAudio(loreClip, 2);

        for (int page = 0; page < lorePages.Length; page++)
        {
            //Sistemo i TMP text necessari
            for (int i = 0; i < Mathf.Max(lorePages[page].loreLines.Length, texts.Count); i++)
            {
                if (i >= lorePages[page].loreLines.Length)
                {
                    texts[i].gameObject.SetActive(false);
                    continue;
                }
                if (i >= texts.Count)
                {
                    TMP_Text newText = Instantiate(exampleTextGO.gameObject, verticalLayoutGroup.transform).GetComponent<TMP_Text>();
                    newText.transform.SetAsLastSibling();
                    newText.alpha = 0;
                    texts.Add(newText);
                }
                texts[i].text = lorePages[page].loreLines[i].line;
                texts[i].gameObject.SetActive(true);
            }
            //ca**ata per updatare il vertical layout group...
            verticalLayoutGroup.enabled = false;
            for (int i = 0; i < lorePages[page].loreLines.Length; i++)
                texts[i].transform.SetParent(verticalLayoutGroup.transform.parent);
            yield return null;
            for (int i = 0; i < lorePages[page].loreLines.Length; i++)
                texts[i].transform.SetParent(verticalLayoutGroup.transform);
            verticalLayoutGroup.enabled = true;
            //animazioni di comparsa
            for (int i = 0; i < lorePages[page].loreLines.Length; i++)
            {
                while (texts[i].alpha != 1)
                {
                    texts[i].alpha = Mathf.MoveTowards(texts[i].alpha, 1, LORE_LINE_FADE_IN_SPEED * Time.deltaTime);
                    yield return null;
                }
                yield return new WaitForSeconds(lorePages[page].loreLines[i].readingTime);
            }

            audioSource.Pause();
            yield return new WaitForSeconds(lorePages[page].audioStopTime);
            audioSource.UnPause();

            float alpha = 1;
            while (alpha > 0)
            {
                alpha = Mathf.MoveTowards(alpha, 0, LORE_LINES_FADE_OUT_SPEED * Time.deltaTime);
                for (int i = 0; i < texts.Count; i++)
                {
                    texts[i].alpha = alpha;
                }
                yield return null;
            }
            for (int i = 0; i < texts.Count; i++)
            {
                texts[i].gameObject.SetActive(false);
            }
        }

        yield return new WaitForSeconds(LORE_POST_LINES_TIME);
        yield return EndLore();
    }
    private IEnumerator EndLore()
    {
        while (FadeCanvasGroup.alpha != 0)
        {
            FadeCanvasGroup.alpha = Mathf.MoveTowards(FadeCanvasGroup.alpha, 0, LORE_PANEL_ENDING_FADE_OUT_SPEED * Time.deltaTime);
            yield return null;
        }
        gameObject.SetActive(false);
        FadeCanvasGroup.gameObject.SetActive(false);

        for (int i = 0; i < texts.Count; i++)
            Destroy(texts[i].gameObject);
    }

    [System.Serializable]
    private struct LorePage
    {
        public float audioStopTime;
        public LoreLine[] loreLines;
    }
    [System.Serializable]
    private struct LoreLine
    {
        public float readingTime;
        public string line;
    }



}
