using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;


public class Play : MonoBehaviour
{
    [SerializeField] TMP_InputField nameTMP;
    AudioSource Error;

    private void Start()
    {
        Error = GetComponent<AudioSource>();
        nameTMP.text = PlayerPrefs.GetString("UserName");
    }

    public void PlayGame()
    {
        if (ChecknameTMP(nameTMP.text))
        {
            PlayerPrefs.SetString("UserName", nameTMP.text);
            Leaderboard.SetLeaderboardEntry(() =>
            {
                UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(1);
            });
        }
        else
        {
            Error.Play();
        }

    }

    public void PlayCoopGame()
    {
        if (ChecknameTMP(nameTMP.text))
        {
            PlayerPrefs.SetString("UserName", nameTMP.text);
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(2);
        }
        else
        {
            Error.Play();
        }

    }
    bool ChecknameTMP(string str)
    {
        if (string.IsNullOrEmpty(str))
            return false;

        foreach (char c in str)
        {
            if (c != ' ')
                return !ProfanityFilter.IsInappropriate(str);
        }
        return false;
    }

}
