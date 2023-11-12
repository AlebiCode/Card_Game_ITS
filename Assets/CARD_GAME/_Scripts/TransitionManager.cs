using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TransitionManager : MonoBehaviour
{
    private static TransitionManager instance;

    //per quanto tempo lo schermo è nero senza scritte (casi per prima e dopo scritte)
    private const float LORE_PRE_LINES_TIME = 1.5f;
    private const float LORE_POST_LINES_TIME = 1f;
    //velocità animazioni scritte
    private const float LORE_LINE_FADE_IN_SPEED = 2.5f;
    private const float LORE_LINES_FADE_OUT_SPEED = 2.5f;
    //pannello
    private const float LORE_PANEL_ENDING_FADE_OUT_SPEED = 2.5f;

    [Header("Fade Panel")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private GameObject currentPanel;
    [SerializeField] private float fadeSpeed = 10;

    [Header("Lore Panel")]
    [SerializeField] private CanvasGroup loreCanvasGroup;
    [SerializeField] private VerticalLayoutGroup verticalLayoutGroup;
    [SerializeField] private AudioClip loreClip;
    [SerializeField] private TMP_Text textGO;
    [SerializeField] private LorePage[] lorePages;

    private void Awake()
    {
        if (instance)
        {
            Destroy(this);
        }
        else
        {
            DontDestroyOnLoad(this);
            StartCoroutine(ShowLore());
            instance = this;
        }
    }

    private void OnLevelWasLoaded(int level)
    {
        StopAllCoroutines();
    }

    public void FadeToPanel(GameObject newPanel)
    {
        fadeCanvasGroup.alpha = 0;
        fadeCanvasGroup.gameObject.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(MainCoroutine(newPanel));
    }

    private IEnumerator MainCoroutine(GameObject newPanel)
    {
        yield return FadeCoroutine(1);

        currentPanel?.SetActive(false);
        currentPanel = newPanel;
        newPanel.SetActive(true);

        yield return FadeCoroutine(0);
        fadeCanvasGroup.gameObject.SetActive(false);
    }
    private IEnumerator FadeCoroutine(float targetAlpha)
    {
        while (fadeCanvasGroup.alpha != targetAlpha)
        {
            fadeCanvasGroup.alpha = Mathf.MoveTowards(fadeCanvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private IEnumerator ShowLore()
    {
        Debug.Log("Show lore!");
        fadeCanvasGroup.gameObject.SetActive(true);
        loreCanvasGroup.gameObject.SetActive(true);
        loreCanvasGroup.alpha = 1;

        textGO.alpha = 0;
        List<TMP_Text> texts = new List<TMP_Text>() { textGO };

        yield return new WaitForSeconds(LORE_PRE_LINES_TIME);
        AudioSource audioSource = AudioManager.PlayAudio(loreClip, 2);
        for (int page = 0; page < lorePages.Length; page++)
        {
            for (int i = 0; i < lorePages[page].loreLines.Length; i++)
            {
                if (i >= texts.Count)
                {
                    TMP_Text newText = Instantiate(textGO.gameObject, loreCanvasGroup.transform).GetComponent<TMP_Text>();
                    newText.transform.SetAsLastSibling();
                    newText.alpha = 0;
                    texts.Add(newText);
                }
                texts[i].text = lorePages[page].loreLines[i].line;
                texts[i].gameObject.SetActive(true);

                //-------------------------------ca**ata per updatare il vertical layout group...------
                verticalLayoutGroup.enabled = false;
                texts[i].transform.SetParent(loreCanvasGroup.transform.parent);
                yield return null;
                texts[i].transform.SetParent(loreCanvasGroup.transform);
                verticalLayoutGroup.enabled = true;
                //--------------------------------------------------------------------------------------

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
            while(alpha > 0)
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
        while (fadeCanvasGroup.alpha != 0)
        {
            fadeCanvasGroup.alpha = Mathf.MoveTowards(fadeCanvasGroup.alpha, 0, LORE_PANEL_ENDING_FADE_OUT_SPEED * Time.deltaTime);
            yield return null;
        }
        loreCanvasGroup.gameObject.SetActive(false);
        fadeCanvasGroup.gameObject.SetActive(false);

        for (int i = 1; i < texts.Count; i++)
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
