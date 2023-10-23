using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitlePanel : MonoBehaviour
{
    public GameObject deckPanel;
    public AudioClip audioClip;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown)
        {
            deckPanel.SetActive(true);
            if(audioClip)
                AudioManager.PlayUiAudio(audioClip);
            gameObject.SetActive(false);
        }
    }
}
