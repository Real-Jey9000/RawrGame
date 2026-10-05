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
    [Tooltip("Optional: Drag spawners here. If empty, the script automatically finds all spawners in the scene!")]
    [SerializeField] private Spawner[] spawnerList;
    [SerializeField] private Transform ghostSpawnPoint;

    [Header("Prefab Settings")]
    [Tooltip("Exact name of the ghost prefab inside the Assets/Resources folder")]
    [SerializeField] private string ghostPrefabName = "GhostPrefab";

    [Header("Countdown")]
    [SerializeField] private int countdownSeconds = 10;

    [Header("Matchmaking Settings")]
    [Tooltip("Feste Server-Region (z.B. eu, us, asia). Verhindert, dass Clients auf verschiedenen Servern landen.")]
    [SerializeField] private string fixedRegion = "eu";

    // Öffentliches Flag, um im Leaderboard-Upload-Skript zu prüfen, ob der Run gewertet werden darf
    public static bool isCoopRunValid = false;

    private bool hasSpawnedGhost = false;
    private bool countdownStarted = false;
    private Coroutine soloRoomTimeoutCoroutine;
    private Coroutine countdownCoroutine;

    private void Start()
    {
        movementGhost.isGameRunning = false;
        isCoopRunValid = false;
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
        PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = Application.version;
        if (!string.IsNullOrEmpty(fixedRegion))
        {
            PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = fixedRegion;
        }

        if (PhotonNetwork.InRoom)
        {
            UpdateStatus("Leaving previous room...");
            PhotonNetwork.LeaveRoom();
            return;
        }

        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.Server == ServerConnection.MasterServer)
        {
            OnConnectedToMaster();
            return;
        }

        if (PhotonNetwork.NetworkClientState == ClientState.Disconnected ||
            PhotonNetwork.NetworkClientState == ClientState.PeerCreated)
        {
            UpdateStatus("Connecting to server...");
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            UpdateStatus("Waiting for Master Server...");
        }
    }

    public override void OnConnectedToMaster()
    {
        UpdateStatus("Searching for room...");
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnLeftRoom()
    {
        hasSpawnedGhost = false;
        countdownStarted = false;
        isCoopRunValid = false;
        movementGhost.AllActiveRunners.Clear();

        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }

        if (soloRoomTimeoutCoroutine != null)
        {
            StopCoroutine(soloRoomTimeoutCoroutine);
            soloRoomTimeoutCoroutine = null;
        }

        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.Server == ServerConnection.MasterServer)
        {
            OnConnectedToMaster();
        }
        else
        {
            UpdateStatus("Returning to Master Server...");
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        UpdateStatus("Creating room...");

        RoomOptions options = new RoomOptions
        {
            MaxPlayers = 2,
            IsOpen = true,
            IsVisible = true,
            EmptyRoomTtl = 0,
            PlayerTtl = 0
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
            UpdateStatus("Waiting for other player...");

            if (soloRoomTimeoutCoroutine != null) StopCoroutine(soloRoomTimeoutCoroutine);
            soloRoomTimeoutCoroutine = StartCoroutine(SoloRoomTimeoutRoutine());
        }
        else
        {
            if (soloRoomTimeoutCoroutine != null) StopCoroutine(soloRoomTimeoutCoroutine);
            UpdateStatus("Player found!");

            if (PhotonNetwork.IsMasterClient && !countdownStarted)
            {
                countdownCoroutine = StartCoroutine(StartCountdownRoutine());
            }
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateStatus("Player joined!");

        if (soloRoomTimeoutCoroutine != null)
        {
            StopCoroutine(soloRoomTimeoutCoroutine);
            soloRoomTimeoutCoroutine = null;
        }

        if (PhotonNetwork.CurrentRoom.PlayerCount >= 2 && PhotonNetwork.IsMasterClient && !countdownStarted)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            countdownCoroutine = StartCoroutine(StartCountdownRoutine());
        }
    }

    private IEnumerator SoloRoomTimeoutRoutine()
    {
        float delay = 4f + Random.Range(0.2f, 1.0f);
        yield return new WaitForSeconds(delay);

        if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.PlayerCount < 2 && !countdownStarted)
        {
            Debug.Log("[Lobby] Alleine im Raum festgesteckt. Re-Matchmaking...");
            PhotonNetwork.LeaveRoom();
        }
    }

    private IEnumerator StartCountdownRoutine()
    {
        countdownStarted = true;

        for (int i = countdownSeconds; i > 0; i--)
        {
            // Sicherheitsprüfung während des Countdowns: Ist der Partner noch da?
            if (PhotonNetwork.CurrentRoom == null || PhotonNetwork.CurrentRoom.PlayerCount < 2)
            {
                countdownStarted = false;
                yield break;
            }

            photonView.RPC(nameof(RPC_UpdateCountdown), RpcTarget.All, i);
            yield return new WaitForSeconds(1f);
        }

        // Finale Prüfung vor Spielstart
        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.PlayerCount >= 2)
        {
            int mapSeed = Random.Range(1000, 99999);
            photonView.RPC(nameof(RPC_StartGame), RpcTarget.All, mapSeed);
        }
        else
        {
            countdownStarted = false;
        }
    }

    [PunRPC]
    private void RPC_UpdateCountdown(int secondsLeft)
    {
        UpdateStatus($"Game starts in {secondsLeft}...");
    }

    [PunRPC]
    private void RPC_StartGame(int seed)
    {
        UpdateStatus("");
        isCoopRunValid = true;

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
        // 1. Ungültig machen, damit kein Score hochgeladen wird
        isCoopRunValid = false;
        movementGhost.isGameRunning = false;

        // 2. Countdown stoppen, falls er gerade lief
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }
        countdownStarted = false;

        // 3. Status setzen & Raum wieder öffnen oder verlassen
        UpdateStatus("Partner left the game.");

        if (PhotonNetwork.InRoom)
        {
            // Raum wieder für andere Spieler freigeben und Timeout starten
            PhotonNetwork.CurrentRoom.IsOpen = true;

            if (soloRoomTimeoutCoroutine != null) StopCoroutine(soloRoomTimeoutCoroutine);
            soloRoomTimeoutCoroutine = StartCoroutine(SoloRoomTimeoutRoutine());
        }
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