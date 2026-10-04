using UnityEngine;
using TMPro;
using Photon.Pun;
using ExitGames.Client.Photon;

public class Save : MonoBehaviour
{
    [Header("Mode")]
    [Tooltip("Haken AN = Multiplayer. Haken AUS = Solo.")]
    [SerializeField] private bool isMultiplayer = false;

    [Header("References")]
    [SerializeField] private GameObject Player;
    [SerializeField] private TMP_Text HighScore;

    private const string KEY_SINGLE = "Highscore";
    private const string KEY_MULTI = "HighscoreMulti";

    private string CurrentKey => isMultiplayer ? KEY_MULTI : KEY_SINGLE;

    private void Awake()
    {
        // 1. Lokalen Namen an Photon übergeben
        string localName = PlayerPrefs.GetString("UserName", "Player");
        PhotonNetwork.NickName = localName;

        // 2. Eigene LootLocker-PlayerID in Photons CustomProperties teilen
        string myLootLockerId = Leaderboard.GetOrCreatePlayerId();
        Hashtable props = new Hashtable { { "LL_ID", myLootLockerId } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        // 3. Falls Solo: sicherstellen, dass kein alter Raum aktiv ist
        if (!isMultiplayer && PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }

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
        }

        Leaderboard.SubmitRunScore(currentScore, isMultiplayer, () =>
        {
            Debug.Log("[Save] Upload abgeschlossen. Jetzt darf die Szene gewechselt werden!");
            // UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(0);
        });
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