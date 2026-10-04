using System.Collections;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(PhotonView))]
public class GameLobbyManager : MonoBehaviourPunCallbacks
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text statusText;

    [Header("Scene References")]
    [Tooltip("Optional: Spawner hier reinziehen. Wenn leer, sucht das Skript automatisch alle Spawner in der Szene!")]
    [SerializeField] private Spawner[] spawnerList;
    [SerializeField] private Transform ghostSpawnPoint;

    [Header("Prefab Settings")]
    [Tooltip("Exakter Name des Geist-Prefabs im Assets/Resources Ordner")]
    [SerializeField] private string ghostPrefabName = "GhostPrefab";

    [Header("Countdown")]
    [SerializeField] private int countdownSeconds = 10;

    private bool hasSpawnedGhost = false;
    private bool countdownStarted = false;

    private void Start()
    {
        movementGhost.isGameRunning = false;
        Time.timeScale = 1f;
        hasSpawnedGhost = false;
        countdownStarted = false;

        movementGhost.AllActiveRunners.Clear();

        if (spawnerList == null || spawnerList.Length == 0)
        {
            spawnerList = FindObjectsOfType<Spawner>();
        }

        StartConnectFlow();
    }

    private void StartConnectFlow()
    {
        // 1. Falls wir noch in einem Raum feststecken: Raum verlassen!
        // Photon wechselt danach automatisch zum Master-Server und ruft OnConnectedToMaster() auf.
        if (PhotonNetwork.InRoom)
        {
            UpdateStatus("Verlasse vorherigen Raum...");
            PhotonNetwork.LeaveRoom();
            return;
        }

        // 2. Sind wir schon auf dem Master-Server und bereit fürs Matchmaking?
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.Server == ServerConnection.MasterServer)
        {
            OnConnectedToMaster();
            return;
        }

        // 3. Wenn komplett getrennt oder noch nicht initialisiert:
        if (PhotonNetwork.NetworkClientState == ClientState.Disconnected ||
            PhotonNetwork.NetworkClientState == ClientState.PeerCreated)
        {
            UpdateStatus("Verbinde mit Server...");
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            UpdateStatus("Warte auf Master-Server...");
        }
    }

    // Feuert zuverlässig, sobald wir WIRKLICH auf dem Master Server sind (und Räume joinen dürfen)
    public override void OnConnectedToMaster()
    {
        UpdateStatus("Suche Raum...");
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnLeftRoom()
    {
        // Sobald der Game-Server verlassen wurde, schickt Photon uns zurück zum Master-Server.
        // Falls wir schon auf dem Master Server sind, starten wir direkt:
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.Server == ServerConnection.MasterServer)
        {
            OnConnectedToMaster();
        }
        else
        {
            UpdateStatus("Kehre zum Master-Server zurück...");
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        UpdateStatus("Erstelle neuen Raum...");
        RoomOptions options = new RoomOptions
        {
            MaxPlayers = 2,
            IsOpen = true,
            IsVisible = true
        };
        PhotonNetwork.CreateRoom(null, options);
    }

    public override void OnJoinedRoom()
    {
        if (!hasSpawnedGhost)
        {
            Vector3 pos = ghostSpawnPoint != null ? ghostSpawnPoint.position : Vector3.zero;
            PhotonNetwork.Instantiate(ghostPrefabName, pos, Quaternion.identity);
            hasSpawnedGhost = true;
        }

        if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            UpdateStatus("Warte auf Mitspieler...");
        }
        else
        {
            UpdateStatus("Mitspieler gefunden!");

            if (PhotonNetwork.IsMasterClient && !countdownStarted)
            {
                StartCoroutine(StartCountdownRoutine());
            }
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateStatus("Mitspieler beigetreten!");

        if (PhotonNetwork.CurrentRoom.PlayerCount >= 2 && PhotonNetwork.IsMasterClient && !countdownStarted)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            StartCoroutine(StartCountdownRoutine());
        }
    }

    private IEnumerator StartCountdownRoutine()
    {
        countdownStarted = true;

        for (int i = countdownSeconds; i > 0; i--)
        {
            photonView.RPC(nameof(RPC_UpdateCountdown), RpcTarget.All, i);
            yield return new WaitForSeconds(1f);
        }

        int mapSeed = Random.Range(1000, 99999);
        photonView.RPC(nameof(RPC_StartGame), RpcTarget.All, mapSeed);
    }

    [PunRPC]
    private void RPC_UpdateCountdown(int secondsLeft)
    {
        UpdateStatus($"Spiel startet in {secondsLeft}...");
    }

    [PunRPC]
    private void RPC_StartGame(int seed)
    {
        UpdateStatus("");

        if (spawnerList == null || spawnerList.Length == 0)
        {
            spawnerList = FindObjectsOfType<Spawner>();
        }

        for (int i = 0; i < spawnerList.Length; i++)
        {
            if (spawnerList[i] != null)
            {
                spawnerList[i].StartWithSeed(seed + (i * 31));
            }
        }

        movementGhost.isGameRunning = true;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        movementGhost.isGameRunning = false;
        UpdateStatus("Mitspieler hat das Spiel verlassen.");
    }

    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log($"[Lobby] {message}");
    }
}