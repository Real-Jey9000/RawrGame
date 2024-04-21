using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause : MonoBehaviour
{
    [SerializeField] GameObject Canvas;
    [SerializeField] GameObject CanvasDeath;


    private void Update()
    {
            if (Input.GetKeyDown(KeyCode.Escape)&&!CanvasDeath.activeSelf)
            {
            
                if (!Canvas.activeSelf)
                {
                    Canvas.SetActive(true);
                    Time.timeScale = 0;
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
