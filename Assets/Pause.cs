using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause : MonoBehaviour
{
    [SerializeField] GameObject Canvas;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!Canvas.activeSelf)
            {
                Canvas.SetActive(true);
                Time.timeScale = 0;
            }
            else
            {
                Unpause();
            }
        }
    }
    private void Start()
    {
        Canvas.SetActive(false);
    }

    public void Unpause ()
    {
        Canvas.SetActive(false);
        Time.timeScale = 1;
    }
}
