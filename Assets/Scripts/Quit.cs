using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Quit : MonoBehaviour
{
    public void OnApplicationQuit()
    {
        if (SceneManager.GetActiveScene().name.Equals("Main Menu"))
            Application.Quit();
        else
            SceneManager.LoadScene(0);
    }
}
