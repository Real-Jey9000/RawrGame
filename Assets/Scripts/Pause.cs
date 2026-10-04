using UnityEngine;
using Photon.Pun;

public class Pause : MonoBehaviour
{
    [SerializeField] private GameObject Canvas;
    [SerializeField] private GameObject CanvasDeath;

    private void Start()
    {
        Unpause();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !CanvasDeath.activeSelf)
        {
            if (!Canvas.activeSelf)
            {
                PauseGame();
            }
            else
            {
                Unpause();
            }
        }
    }

    public void PauseGame()
    {
        Canvas.SetActive(true);

        // Nur im Singleplayer einfrieren
        if (!PhotonNetwork.InRoom)
        {
            Time.timeScale = 0f;
        }
    }

    public void Unpause()
    {
        Canvas.SetActive(false);

        // Nur im Singleplayer die Zeit wieder starten
        if (!PhotonNetwork.InRoom)
        {
            Time.timeScale = 1f;
        }
    }
}