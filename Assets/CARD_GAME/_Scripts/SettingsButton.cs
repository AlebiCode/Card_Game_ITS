using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsButton : MonoBehaviour
{
    [SerializeField] GameObject pausePanel;

    public void TogglePausePanelOnAndOff()
    {
        if (!pausePanel.activeInHierarchy) 
        {
            pausePanel.SetActive(true);
        }
        else
        {
            pausePanel.SetActive(false);
        }
             
    }
}
