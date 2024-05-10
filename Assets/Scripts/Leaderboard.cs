using Dan.Main;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Dan.Main;

public class Leaderboard : MonoBehaviour
{
    [SerializeField] private List<TMP_Text> names;
    [SerializeField] private List<TMP_Text> scores;
    [SerializeField] Color UserColor;
    public static string[] badwords = {"RAWR", "Sushicat"};

    public static string publicLeaderboardKey = "65808c598689e94f03eb4d0935e398184f16e720abc618f463a59635b93e2090";

    private void Start()
    {
        GetLeaderboard();
    }
    public void GetLeaderboard()
    {
        LeaderboardCreator.GetLeaderboard(publicLeaderboardKey, ((msg) =>{
            for(int i = 0; i < msg.Length; i++)
            {
                names[i].text = msg[i].Username;
                scores[i].text = msg[i].Score.ToString();
                if (msg[i].Username.Equals(PlayerPrefs.GetString("UserName")))
                {
                    names[i].color = UserColor;
                    scores[i].color = UserColor;
                }
            }
        }));
    }

    public static void SetLeaderboardEntry()
    {
        LeaderboardCreator.UploadNewEntry(publicLeaderboardKey, PlayerPrefs.GetString("UserName"),
            PlayerPrefs.GetInt("Highscore"), ((msg) => {
            }));
    }
    
}
