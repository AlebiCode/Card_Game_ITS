using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MyUtils
{

    public class Debugger : MonoBehaviour
    {

        void Update()
        {

            if (Input.GetKeyDown(KeyCode.P))
            {
                SceneManager.LoadScene(0);
            }

        }
    }

}
