using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
            Leaderboard.SetLeaderboardEntry();
            SceneManager.LoadSceneAsync(1);
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
                return true;
        }
        return false;
    }
}
