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
    private static int loggedInPlayerId = -1;

    private void Awake()
    {
        activeLeaderboardKey = leaderboardKey;
    }

    private void Start()
    {
        // LootLocker requires an active authenticated session
        StartSession();

    }

    private void StartSession()
    {
        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if (response.success)
            {
                loggedInPlayerId = response.player_id;

                // Set the player's display name if saved in PlayerPrefs
                string savedName = PlayerPrefs.GetString("UserName", "");
                if (!string.IsNullOrEmpty(savedName))
                {
                    LootLockerSDKManager.SetPlayerName(savedName, (nameResponse) =>
                    {
                        GetLeaderboard();
                    });
                }
                else
                {
                    GetLeaderboard();
                }
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

        LootLockerSDKManager.GetScoreList(activeLeaderboardKey, countToFetch, 0, (response) =>
        {
            if (!response.success)
            {
                Debug.LogError($"[LootLocker] Failed to fetch leaderboard: {response.errorData?.message}");
                return;
            }

            // 1. Guard against null items array (empty leaderboard)
            LootLockerLeaderboardMember[] items = response.items ?? new LootLockerLeaderboardMember[0];

            for (int i = 0; i < countToFetch; i++)
            {
                // Skip if the TMP_Text component in the Inspector is unassigned
                if (names[i] == null || scores[i] == null)
                {
                    Debug.LogWarning($"[Leaderboard] Slot index {i} in names or scores list is not assigned in the Inspector.");
                    continue;
                }

                if (i < items.Length && items[i] != null)
                {
                    var member = items[i];

                    // 2. Safe check on player object and name
                    string displayName;
                    bool isMine = false;

                    if (member.player != null)
                    {
                        displayName = !string.IsNullOrEmpty(member.player.name)
                            ? member.player.name
                            : $"Player {member.player.id}";

                        isMine = (member.player.id == loggedInPlayerId);
                    }
                    else
                    {
                        // Fall back to member rank/score if player info is absent
                        displayName = $"Player #{member.rank}";
                    }

                    names[i].text = displayName;
                    scores[i].text = member.score.ToString();

                    Color targetColor = isMine ? userColor : defaultColor;
                    names[i].color = targetColor;
                    scores[i].color = targetColor;
                }
                else
                {
                    // Clear unused slots
                    names[i].text = "-";
                    scores[i].text = "-";
                    names[i].color = defaultColor;
                    scores[i].color = defaultColor;
                }
            }
        });
    }

    public static void SetLeaderboardEntry(System.Action onComplete = null)
    {
        int score = PlayerPrefs.GetInt("Highscore", 0);

        LootLockerSDKManager.SubmitScore("", score, activeLeaderboardKey, (response) =>
        {
            if (response.success)
            {
                Debug.Log("[LootLocker] Score successfully submitted.");
            }
            else
            {
                Debug.LogError($"[LootLocker] Failed to submit score: {response.errorData?.message}");
            }

            // Proceed to next action/scene only after LootLocker answers
            onComplete?.Invoke();
        });
    }
}