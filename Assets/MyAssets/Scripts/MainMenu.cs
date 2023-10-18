using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void PlayButton()
    {
        Debug.Log("Play");
    }
    public void QuitButton()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
}
