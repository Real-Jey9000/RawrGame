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
        // 1. If currently in a room: leave it first!
        // Photon will automatically route back to MasterServer and invoke OnConnectedToMaster().
        if (PhotonNetwork.InRoom)
        {
            UpdateStatus("Leaving previous room...");
            PhotonNetwork.LeaveRoom();
            return;
        }

        // 2. Are we already on the MasterServer and ready for matchmaking?
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.Server == ServerConnection.MasterServer)
        {
            OnConnectedToMaster();
            return;
        }

        // 3. If disconnected or not initialized yet:
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

    // Fires reliably once we are actually on the MasterServer and permitted to join rooms
    public override void OnConnectedToMaster()
    {
        UpdateStatus("Searching for room...");
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnLeftRoom()
    {
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
        UpdateStatus("Creating new room...");
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
            UpdateStatus("Waiting for other player...");
        }
        else
        {
            UpdateStatus("Player found!");

            if (PhotonNetwork.IsMasterClient && !countdownStarted)
            {
                StartCoroutine(StartCountdownRoutine());
            }
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateStatus("Player joined!");

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
        UpdateStatus($"Game starts in {secondsLeft}...");
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
        UpdateStatus("Other player left the game.");
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