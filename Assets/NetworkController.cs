using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkController : MonoBehaviourPunCallbacks
{
    [SerializeField] private string roomName = "RawrLobby";

    void Start()
    {
        Debug.Log("[Photon] Verbinde mit Master Server...");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("[Photon] Verbunden! Trete Lobby/Raum bei...");
        RoomOptions options = new RoomOptions { MaxPlayers = 2 };
        PhotonNetwork.JoinOrCreateRoom(roomName, options, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"[Photon] Raum '{PhotonNetwork.CurrentRoom.Name}' erfolgreich beigetreten!");

        // Spawnt den eigenen Ghost/Netzwerk-Spieler
        // WICHTIG: Das Prefab muss im Ordner "Assets/Resources/" liegen!
        PhotonNetwork.Instantiate("NetworkGhostPlayer", Vector3.zero, Quaternion.identity);
    }
}