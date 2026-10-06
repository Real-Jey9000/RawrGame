using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class MusicMuteToggle : MonoBehaviour
{
    private Toggle toggle;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
    }

    private void Start()
    {
        bool isMuted = PlayerPrefs.GetInt("Settings_Music_Muted", 0) == 1;
        if (AudioManager.Instance != null)
        {
            isMuted = AudioManager.Instance.IsMusicMuted;
        }

        toggle.onValueChanged.RemoveListener(OnToggleChanged);
        toggle.isOn = isMuted;
        toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void OnDestroy()
    {
        if (toggle != null)
        {
            toggle.onValueChanged.RemoveListener(OnToggleChanged);
        }
    }

    private void OnToggleChanged(bool isOn)
    {
        bool mute = isOn;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicMute(mute);
        }
        else
        {
            PlayerPrefs.SetInt("Settings_Music_Muted", mute ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}