using Dan.Main;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class Save : MonoBehaviour
{
    [SerializeField] GameObject Player;
    [SerializeField] TMP_Text HighScore;
    private void Start()
    {
        if (HighScore != null)
        {
            SetScore();
        }
    }

    public void SaveScore()
    {
        if((int)Mathf.Round(Player.transform.position.x) > LoadScore())
        {
            PlayerPrefs.SetInt("Highscore", (int)Mathf.Round(Player.transform.position.x));
            Leaderboard.SetLeaderboardEntry();
        }
    }
    public int LoadScore()
    {
        return PlayerPrefs.GetInt("Highscore");
    }

    public void SetScore()
    {
        HighScore.text = "High-Score: " + LoadScore().ToString();
    }
}