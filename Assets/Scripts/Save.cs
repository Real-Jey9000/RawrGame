using UnityEngine;
using TMPro;
using Photon.Pun;

public class Save : MonoBehaviour
{
    [SerializeField] private GameObject Player;
    [SerializeField] private TMP_Text HighScore;

    private const string KEY_SINGLE = "Highscore";
    private const string KEY_MULTI = "HighscoreMulti";

    private string CurrentKey => PhotonNetwork.InRoom ? KEY_MULTI : KEY_SINGLE;

    private void Start()
    {
        if (HighScore != null)
        {
            SetScore();
        }
    }

    public void SaveScore()
    {
        int currentScore = GetCurrentDistance();

        if (currentScore > LoadScore())
        {
            PlayerPrefs.SetInt(CurrentKey, currentScore);
            PlayerPrefs.Save();

            if (!PhotonNetwork.InRoom)
            {
                Leaderboard.SetLeaderboardEntry(() =>
                {
                    UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(1);
                });
            }
        }
    }

    public int LoadScore()
    {
        return PlayerPrefs.GetInt(CurrentKey, 0);
    }

    public void SetScore()
    {
        HighScore.text = "High-Score: " + LoadScore().ToString();
    }

    private int GetCurrentDistance()
    {
        if (PhotonNetwork.InRoom && GameRunnerAnchor.Instance != null)
        {
            return (int)Mathf.Round(GameRunnerAnchor.Instance.transform.position.x);
        }

        if (Player != null)
        {
            return (int)Mathf.Round(Player.transform.position.x);
        }

        return 0;
    }
}