using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private string musicTag = "Music";

    private const string PREF_MUSIC_MUTED = "Settings_Music_Muted";
    private readonly List<AudioSource> activeMusicSources = new List<AudioSource>();

    public bool IsMusicMuted { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        IsMusicMuted = PlayerPrefs.GetInt(PREF_MUSIC_MUTED, 0) == 1;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        RefreshAndApplyMusicSources();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshAndApplyMusicSources();
    }

    public void RefreshAndApplyMusicSources()
    {
        activeMusicSources.Clear();

        AudioSource[] allSources = Resources.FindObjectsOfTypeAll<AudioSource>();
        foreach (var src in allSources)
        {
            if (src == null || !src.gameObject.scene.isLoaded) continue;

            bool isMusic = false;

            try
            {
                if (src.gameObject.tag == musicTag)
                {
                    isMusic = true;
                }
            }
            catch
            {
            }

            if (!isMusic && src.gameObject.name.ToLower().Contains("music"))
            {
                isMusic = true;
            }

            if (isMusic)
            {
                activeMusicSources.Add(src);
                src.mute = IsMusicMuted;
            }
        }
    }

    public void SetMusicMute(bool mute)
    {
        IsMusicMuted = mute;
        PlayerPrefs.SetInt(PREF_MUSIC_MUTED, mute ? 1 : 0);
        PlayerPrefs.Save();

        for (int i = activeMusicSources.Count - 1; i >= 0; i--)
        {
            if (activeMusicSources[i] == null)
            {
                activeMusicSources.RemoveAt(i);
                continue;
            }

            activeMusicSources[i].mute = IsMusicMuted;
        }
    }
}