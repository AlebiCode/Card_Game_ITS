using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MyUtils
{

    public class Permanent : MonoBehaviour
    {
        public static Permanent instance;

        private void Awake()
        {
            if (instance)
            {
                Destroy(this);
            }
            else
            {
                instance = this;
                DontDestroyOnLoad(this);
            }
        }

        private void Start()
        {
            Instances.LorePanel.StartLore();
        }

        void Update()
        {

            if (Input.GetKeyDown(KeyCode.P))
            {
                SceneManager.LoadScene(0);
            }

        }
    }

}
