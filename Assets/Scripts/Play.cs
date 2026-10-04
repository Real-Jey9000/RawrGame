using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class Play : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameTMP;
    private AudioSource Error;

    private void Start()
    {
        Error = GetComponent<AudioSource>();
        if (nameTMP != null)
        {
            nameTMP.text = PlayerPrefs.GetString("UserName", "");
        }
    }

    public void PlayGame()
    {
        if (ChecknameTMP(nameTMP.text))
        {
            string chosenName = nameTMP.text.Trim();
            PlayerPrefs.SetString("UserName", chosenName);
            PlayerPrefs.Save();

            // Direkt in die Singleplayer-Szene wechseln
            SceneManager.LoadSceneAsync(1);
        }
        else
        {
            if (Error != null) Error.Play();
        }
    }

    public void PlayCoopGame()
    {
        if (ChecknameTMP(nameTMP.text))
        {
            string chosenName = nameTMP.text.Trim();
            PlayerPrefs.SetString("UserName", chosenName);
            PlayerPrefs.Save();

            // WICHTIG: Photon-NickName sofort zuweisen, bevor die Multiplayer-Szene geladen wird!
            PhotonNetwork.NickName = chosenName;

            // In die Multiplayer-Szene wechseln
            SceneManager.LoadSceneAsync(2);
        }
        else
        {
            if (Error != null) Error.Play();
        }
    }

    private bool ChecknameTMP(string str)
    {
        if (string.IsNullOrEmpty(str) || str.Trim().Length == 0 || str.Length > 15)
            return false;

        return !ProfanityFilter.IsInappropriate(str);
    }
}