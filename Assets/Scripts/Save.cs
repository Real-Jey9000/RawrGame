using UnityEngine;
using TMPro;
using Photon.Pun;

public class Save : MonoBehaviour
{
    [Header("Mode")]
    [Tooltip("Haken AN = Multiplayer-Highscore (HighscoreMulti). Haken AUS = Solo-Highscore (Highscore).")]
    [SerializeField] private bool isMultiplayer = false;

    [Header("References")]
    [SerializeField] private GameObject Player;
    [SerializeField] private TMP_Text HighScore;

    private const string KEY_SINGLE = "Highscore";
    private const string KEY_MULTI = "HighscoreMulti";

    private string CurrentKey => isMultiplayer ? KEY_MULTI : KEY_SINGLE;

    private void Awake()
    {
        // Wenn wir im Solo-Modus sind, aber noch als "InRoom" markiert sind: Raum sofort verlassen!
        if (!isMultiplayer && PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }

        // Sicherstellen, dass das Spiel nicht pausiert startet
        Time.timeScale = 1f;
    }

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

            // Nur Solo ins Leaderboard eintragen
            if (!isMultiplayer)
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
        if (HighScore != null)
        {
            HighScore.text = "High-Score: " + LoadScore().ToString();
        }
    }

    private int GetCurrentDistance()
    {
        if (isMultiplayer && GameRunnerAnchor.Instance != null)
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