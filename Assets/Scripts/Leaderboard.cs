using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using LootLocker.Requests;

public class Leaderboard : MonoBehaviour
{
    [Header("LootLocker Config")]
    [Tooltip("Can be the leaderboard key string or integer ID from your dashboard")]
    [SerializeField] private string leaderboardKey = "your_leaderboard_key_here";

    [Header("UI Elements")]
    [SerializeField] private List<TMP_Text> names;
    [SerializeField] private List<TMP_Text> scores;
    [SerializeField] private Color userColor = Color.yellow;
    [SerializeField] private Color defaultColor = Color.white;

    private static string activeLeaderboardKey;

    private void Awake()
    {
        activeLeaderboardKey = leaderboardKey;
    }

    private void Start()
    {
        StartSession();
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
                if (names[i] == null || scores[i] == null)
                {
                    continue;
                }

                if (i < items.Length && items[i] != null)
                {
                    var member = items[i];

                    string displayName = ParseDisplayNameFromMetadata(member.metadata);

                    if (string.IsNullOrEmpty(displayName))
                    {
                        if (member.player != null && !string.IsNullOrEmpty(member.player.name))
                        {
                            displayName = member.player.name;
                        }
                        else
                        {
                            displayName = $"Player #{member.rank}";
                        }
                    }

                    bool isMine = (member.member_id == myPlayerId);

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
            // Versucht JSON auszulesen: {"name":"...","version":"...","date":"..."}
            LeaderboardMetadata data = JsonUtility.FromJson<LeaderboardMetadata>(rawMetadata);
            if (data != null && !string.IsNullOrEmpty(data.name))
            {
                return data.name;
            }
        }
        catch
        {
            // Falls alte Einträge reiner Klartext ohne JSON waren
        }

        return rawMetadata;
    }

    public static void SetLeaderboardEntry(System.Action onComplete = null)
    {
        int score = PlayerPrefs.GetInt("Highscore", 0);
        string persistentId = GetOrCreatePlayerId();
        string currentName = PlayerPrefs.GetString("UserName", "Player");

        LeaderboardMetadata metadataObject = new LeaderboardMetadata
        {
            name = currentName,
            version = Application.version,
            date = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };

        string metadataJson = JsonUtility.ToJson(metadataObject);

        LootLockerSDKManager.SubmitScore(persistentId, score, activeLeaderboardKey, metadataJson, (response) =>
        {
            if (response.success)
            {
                Debug.Log("Score and metadata successfully submitted.");
            }
            else
            {
                Debug.LogError($"Failed to submit score: {response.errorData?.message}");
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