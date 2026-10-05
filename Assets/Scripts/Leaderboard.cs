using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LootLocker.Requests;
using Photon.Pun;

public class Leaderboard : MonoBehaviour
{
    [Header("LootLocker Keys")]
    [SerializeField] private string singleplayerKey = "36941";
    [SerializeField] private string multiplayerKey = "36956";

    [Header("Tab Buttons & Texts")]
    [SerializeField] private Button singleplayerBtn;
    [SerializeField] private TMP_Text singleplayerTxt;

    [SerializeField] private Button multiplayerBtn;
    [SerializeField] private TMP_Text multiplayerTxt;

    [Header("Colors")]
    [SerializeField] private Color activeBtnColor = new Color32(10, 20, 56, 255);
    [SerializeField] private Color activeTxtColor = new Color32(252, 209, 22, 255);
    [SerializeField] private Color inactiveBtnColor = new Color32(194, 184, 126, 255);
    [SerializeField] private Color inactiveTxtColor = Color.black;

    [Header("UI Elements")]
    [SerializeField] private List<TMP_Text> names;
    [SerializeField] private List<TMP_Text> scores;
    [SerializeField] private Color userColor = Color.yellow;
    [SerializeField] private Color defaultColor = Color.white;

    private string activeLeaderboardKey;
    private static int loggedInPlayerId = -1;

    private void Awake()
    {
        if (!string.IsNullOrEmpty(singleplayerKey))
            PlayerPrefs.SetString("LL_Cached_SingleKey", singleplayerKey);

        if (!string.IsNullOrEmpty(multiplayerKey))
            PlayerPrefs.SetString("LL_Cached_MultiKey", multiplayerKey);

        PlayerPrefs.Save();

        activeLeaderboardKey = singleplayerKey;
    }

    private void Start()
    {
        if (singleplayerBtn != null) singleplayerBtn.onClick.AddListener(SelectSingleplayer);
        if (multiplayerBtn != null) multiplayerBtn.onClick.AddListener(SelectMultiplayer);

        SelectSingleplayer();
        StartSession();
    }

    public void SelectSingleplayer()
    {
        activeLeaderboardKey = !string.IsNullOrEmpty(singleplayerKey)
            ? singleplayerKey
            : PlayerPrefs.GetString("LL_Cached_SingleKey", "36941");

        SetButtonColors(singleplayerBtn, singleplayerTxt, activeBtnColor, activeTxtColor);
        SetButtonColors(multiplayerBtn, multiplayerTxt, inactiveBtnColor, inactiveTxtColor);

        if (loggedInPlayerId != -1)
        {
            GetLeaderboard();
        }
    }

    public void SelectMultiplayer()
    {
        activeLeaderboardKey = !string.IsNullOrEmpty(multiplayerKey)
            ? multiplayerKey
            : PlayerPrefs.GetString("LL_Cached_MultiKey", "36956");

        SetButtonColors(singleplayerBtn, singleplayerTxt, inactiveBtnColor, inactiveTxtColor);
        SetButtonColors(multiplayerBtn, multiplayerTxt, activeBtnColor, activeTxtColor);

        if (loggedInPlayerId != -1)
        {
            GetLeaderboard();
        }
    }

    private void SetButtonColors(Button btn, TMP_Text txt, Color btnColor, Color txtColor)
    {
        if (btn != null && btn.targetGraphic != null)
        {
            btn.targetGraphic.color = btnColor;
        }

        if (txt != null)
        {
            txt.color = txtColor;
        }
    }

    public static string GetOrCreatePlayerId()
    {
        string uniqueId = PlayerPrefs.GetString("Persistent_PlayerID", string.Empty);
        if (string.IsNullOrEmpty(uniqueId))
        {
            uniqueId = Guid.NewGuid().ToString();
            PlayerPrefs.SetString("Persistent_PlayerID", uniqueId);
            PlayerPrefs.Save();
        }
        return uniqueId;
    }

    private void StartSession()
    {
        string persistentId = GetOrCreatePlayerId();

        LootLockerSDKManager.StartGuestSession(persistentId, (response) =>
        {
            if (response.success)
            {
                loggedInPlayerId = response.player_id;
                GetLeaderboard();
            }
            else
            {
                Debug.LogError($"[LootLocker] Session start failed: {response.errorData?.message}");
            }
        });
    }

    public void GetLeaderboard()
    {
        if (names == null || scores == null || names.Count == 0) return;

        int countToFetch = Mathf.Min(names.Count, scores.Count);
        string myPlayerId = GetOrCreatePlayerId();

        LootLockerSDKManager.GetScoreList(activeLeaderboardKey, countToFetch, 0, (response) =>
        {
            if (!response.success)
            {
                Debug.LogError($"[LootLocker] Failed to fetch leaderboard: {response.errorData?.message}");
                return;
            }

            LootLockerLeaderboardMember[] items = response.items ?? new LootLockerLeaderboardMember[0];

            for (int i = 0; i < countToFetch; i++)
            {
                if (names[i] == null || scores[i] == null) continue;

                if (i < items.Length && items[i] != null)
                {
                    var member = items[i];
                    string displayName = ParseDisplayNameFromMetadata(member.metadata);

                    if (string.IsNullOrEmpty(displayName))
                    {
                        displayName = member.player != null && !string.IsNullOrEmpty(member.player.name)
                            ? member.player.name
                            : $"Player #{member.rank}";
                    }

                    // --- HIGHLIGHT CHECK ---
                    bool isMine = false;

                    // 1. Session ID Match
                    if (loggedInPlayerId != -1 && member.player != null && member.player.id == loggedInPlayerId)
                    {
                        isMine = true;
                    }
                    // 2. Member ID Match (Team String oder direkte ID)
                    else if (!string.IsNullOrEmpty(member.member_id) &&
                            (member.member_id == myPlayerId || member.member_id.Contains(myPlayerId) || member.member_id == loggedInPlayerId.ToString()))
                    {
                        isMine = true;
                    }
                    // 3. Fallback auf Namen im Singleplayer
                    else if (activeLeaderboardKey == singleplayerKey)
                    {
                        string savedName = PlayerPrefs.GetString("UserName", "Player");
                        if (!string.IsNullOrEmpty(savedName) && displayName.Equals(savedName, StringComparison.OrdinalIgnoreCase))
                        {
                            isMine = true;
                        }
                    }

                    names[i].text = displayName;
                    scores[i].text = member.score.ToString();

                    Color targetColor = isMine ? userColor : defaultColor;
                    names[i].color = targetColor;
                    scores[i].color = targetColor;
                }
                else
                {
                    names[i].text = "-";
                    scores[i].text = "-";
                    names[i].color = defaultColor;
                    scores[i].color = defaultColor;
                }
            }
        });
    }

    private static string ParseDisplayNameFromMetadata(string rawMetadata)
    {
        if (string.IsNullOrEmpty(rawMetadata)) return string.Empty;

        try
        {
            LeaderboardMetadata data = JsonUtility.FromJson<LeaderboardMetadata>(rawMetadata);
            if (data != null && !string.IsNullOrEmpty(data.name))
            {
                return data.name;
            }
        }
        catch { }

        return rawMetadata;
    }

    public static void SubmitRunScore(int roundScore, bool isMultiplayer, Action onComplete = null)
    {
        if (isMultiplayer && PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient)
        {
            Debug.Log("[LootLocker] Client ist nicht MasterClient – überspringe Upload.");
            onComplete?.Invoke();
            return;
        }

        string targetKey = isMultiplayer
            ? PlayerPrefs.GetString("LL_Cached_MultiKey", "36956")
            : PlayerPrefs.GetString("LL_Cached_SingleKey", "36941");

        string persistentId = GetOrCreatePlayerId();
        string myName = PlayerPrefs.GetString("UserName", "Player");

        string submitMemberId = persistentId;
        string finalDisplayName = myName;

        if (isMultiplayer)
        {
            string partnerId = "UnknownPartner";
            string partnerName = "Partner";

            if (PhotonNetwork.PlayerListOthers != null && PhotonNetwork.PlayerListOthers.Length > 0)
            {
                var other = PhotonNetwork.PlayerListOthers[0];
                if (!string.IsNullOrEmpty(other.NickName))
                {
                    partnerName = other.NickName;
                }

                if (other.CustomProperties.TryGetValue("LL_ID", out object val) && val != null)
                {
                    partnerId = val.ToString();
                }
            }

            submitMemberId = string.Compare(persistentId, partnerId, StringComparison.Ordinal) < 0
                ? $"team_{persistentId}_{partnerId}"
                : $"team_{partnerId}_{persistentId}";

            finalDisplayName = string.Compare(myName, partnerName, StringComparison.OrdinalIgnoreCase) < 0
                ? $"{myName} & {partnerName}"
                : $"{partnerName} & {myName}";
        }

        LeaderboardMetadata metadataObject = new LeaderboardMetadata
        {
            name = finalDisplayName,
            version = Application.version,
            date = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };

        string metadataJson = JsonUtility.ToJson(metadataObject);

        Debug.Log($"[LootLocker] Sende Score: Score={roundScore}, Key={targetKey}, MemberID={submitMemberId}, Team={finalDisplayName}");

        LootLockerSDKManager.SubmitScore(submitMemberId, roundScore, targetKey, metadataJson, (response) =>
        {
            if (response.success)
            {
                Debug.Log($"[LootLocker] Upload ERFOLGREICH für {finalDisplayName}!");
            }
            else
            {
                Debug.LogError($"[LootLocker] Upload FEHLGESCHLAGEN: {response.errorData?.message}");
            }

            onComplete?.Invoke();
        });
    }

    [Serializable]
    private class LeaderboardMetadata
    {
        public string name;
        public string version;
        public string date;
    }
}