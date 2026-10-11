using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UpdateChecker : MonoBehaviour
{
    private const string ApiUrl = "https://api.github.com/repos/Real-Jey9000/RawrGame/releases/latest";

    [SerializeField] private TMP_Text updateInfoText;
    [SerializeField] private Button updateButton;

    private string directDownloadUrl;

    void Start()
    {
        if (updateButton != null)
        {
            updateButton.gameObject.SetActive(false);
            updateButton.onClick.AddListener(StartDownloadUpdate);
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
                foreach (var asset in latestRelease.assets)
                {
                    if (asset.name == "RawrGame.zip")
                    {
                        directDownloadUrl = asset.browser_download_url;
                        break;
                    }
                }

                CheckVersions(Application.version, latestRelease.tag_name);
            }
        }
    }

    private void CheckVersions(string localRaw, string remoteRaw)
    {
        string cleanLocal = NormalizeVersionString(localRaw);
        string cleanRemote = NormalizeVersionString(remoteRaw);

        if (Version.TryParse(cleanLocal, out Version localVer) &&
            Version.TryParse(cleanRemote, out Version remoteVer))
        {
            if (remoteVer > localVer)
            {
                NotifyNewVersionAvailable(localRaw, remoteRaw);
            }
        }
        else if (cleanRemote != cleanLocal)
        {
            NotifyNewVersionAvailable(localRaw, remoteRaw);
        }
    }

    private string NormalizeVersionString(string rawVersion)
    {
        if (string.IsNullOrEmpty(rawVersion)) return "0.0.0";
        string cleaned = rawVersion.Replace(" ", "").TrimStart('v', 'V').TrimStart('.');
        return cleaned;
    }

    private void NotifyNewVersionAvailable(string currentVer, string newVer)
    {
        if (updateInfoText != null)
        {
            updateInfoText.text = $"Neue Version {newVer} verfügbar! (Aktuell: {currentVer})\nKlicke zum Aktualisieren.";
        }

        if (updateButton != null)
        {
            updateButton.gameObject.SetActive(true);
        }
    }

    private void StartDownloadUpdate()
    {
        if (string.IsNullOrEmpty(directDownloadUrl))
        {
            if (updateInfoText != null) updateInfoText.text = "Keine Download-Datei gefunden!";
            return;
        }

        if (updateButton != null) updateButton.gameObject.SetActive(false);
        StartCoroutine(DownloadAndInstallRoutine());
    }

    private IEnumerator DownloadAndInstallRoutine()
    {
        string zipPath = Path.Combine(Application.persistentDataPath, "RawrGame.zip");
        string extractPath = Path.Combine(Application.persistentDataPath, "UpdateExtract");

        if (Directory.Exists(extractPath)) Directory.Delete(extractPath, true);
        Directory.CreateDirectory(extractPath);

        if (updateInfoText != null) updateInfoText.text = "Lade Update herunter...";

        using (UnityWebRequest request = UnityWebRequest.Get(directDownloadUrl))
        {
            request.downloadHandler = new DownloadHandlerFile(zipPath);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                if (updateInfoText != null) updateInfoText.text = "Download fehlgeschlagen: " + request.error;
                yield break;
            }
        }

        if (updateInfoText != null) updateInfoText.text = "Entpacke Update...";

        try
        {
            ZipFile.ExtractToDirectory(zipPath, extractPath);
            File.Delete(zipPath);
        }
        catch (Exception ex)
        {
            if (updateInfoText != null) updateInfoText.text = "Fehler beim Entpacken: " + ex.Message;
            yield break;
        }

        if (updateInfoText != null) updateInfoText.text = "Update bereit! Spiel wird neu gestartet...";

        CreateAndRunUpdaterScript(extractPath);
    }

    private void CreateAndRunUpdaterScript(string extractPath)
    {
#if UNITY_STANDALONE_WIN
        string gameExeDir = Directory.GetParent(Application.dataPath).FullName;
        string batPath = Path.Combine(Application.persistentDataPath, "update.bat");

        string batchContent = $@"
@echo off
timeout /t 2 /nobreak > nul
xcopy /s /y ""{extractPath}\*"" ""{gameExeDir}\""
start """" ""{Application.dataPath.Replace("_Data", "")}.exe""
del ""%~f0""
";

        File.WriteAllText(batPath, batchContent);

        System.Diagnostics.Process.Start(batPath);
        Application.Quit();
#endif
    }

    [Serializable]
    private class GitHubRelease
    {
        public string tag_name;
        public GitHubAsset[] assets;
    }

    [Serializable]
    private class GitHubAsset
    {
        public string name;
        public string browser_download_url;
    }
}