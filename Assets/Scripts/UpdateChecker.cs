using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UpdateChecker : MonoBehaviour
{
    private const string ApiUrl = "https://api.github.com/repos/Real-Jey9000/RawrGame/releases/latest";

    [SerializeField] private TMP_Text updateInfoText;
    [SerializeField] private Button updateButton;

    private string latestReleaseUrl;

    void Start()
    {
        if (updateButton != null)
        {
            updateButton.gameObject.SetActive(false);
            updateButton.onClick.AddListener(OpenDownloadPage);
        }

        if (updateInfoText != null)
        {
            updateInfoText.text = "";
        }

        StartCoroutine(CheckForUpdates());
    }

    private IEnumerator CheckForUpdates()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(ApiUrl))
        {
            request.SetRequestHeader("User-Agent", "Unity-RawrGame-Client");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                yield break;
            }

            GitHubRelease latestRelease = JsonUtility.FromJson<GitHubRelease>(request.downloadHandler.text);

            if (latestRelease != null && !string.IsNullOrEmpty(latestRelease.tag_name))
            {
                CheckVersions(Application.version, latestRelease.tag_name, latestRelease.html_url);
            }
        }
    }

    private void CheckVersions(string localRaw, string remoteRaw, string downloadUrl)
    {
        string cleanLocal = NormalizeVersionString(localRaw);
        string cleanRemote = NormalizeVersionString(remoteRaw);

        if (Version.TryParse(cleanLocal, out Version localVer) &&
            Version.TryParse(cleanRemote, out Version remoteVer))
        {
            if (remoteVer > localVer)
            {
                NotifyNewVersionAvailable(localRaw, remoteRaw, downloadUrl);
            }
        }
        else if (cleanRemote != cleanLocal)
        {
            NotifyNewVersionAvailable(localRaw, remoteRaw, downloadUrl);
        }
    }

    private string NormalizeVersionString(string rawVersion)
    {
        if (string.IsNullOrEmpty(rawVersion)) return "0.0.0";

        string cleaned = rawVersion.Replace(" ", "");
        cleaned = cleaned.TrimStart('v', 'V');
        cleaned = cleaned.TrimStart('.');

        return cleaned;
    }

    private void NotifyNewVersionAvailable(string currentVer, string newVer, string url)
    {
        latestReleaseUrl = url;

        if (updateInfoText != null)
        {
            updateInfoText.text = $"New version {newVer} available! (Current: {currentVer})";
        }

        if (updateButton != null)
        {
            updateButton.gameObject.SetActive(true);
        }
    }

    private void OpenDownloadPage()
    {
        if (!string.IsNullOrEmpty(latestReleaseUrl))
        {
            Application.OpenURL(latestReleaseUrl);
        }
    }

    [Serializable]
    private class GitHubRelease
    {
        public string tag_name;
        public string html_url;
    }
}