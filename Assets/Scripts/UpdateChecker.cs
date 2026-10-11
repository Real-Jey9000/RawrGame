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
    [SerializeField] private Button autoUpdateButton;   // Button 1: Downloads update directly in-game
    [SerializeField] private Button manualUpdateButton; // Button 2: Opens GitHub releases page in browser

    private string directDownloadUrl;
    private string latestReleaseUrl;

    void Start()
    {
        if (autoUpdateButton != null)
        {
            autoUpdateButton.gameObject.SetActive(false);
            autoUpdateButton.onClick.AddListener(StartDownloadUpdate);
        }

        if (manualUpdateButton != null)
        {
            manualUpdateButton.gameObject.SetActive(false);
            manualUpdateButton.onClick.AddListener(OpenDownloadPage);
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
                // Store webpage URL for manual download button
                latestReleaseUrl = latestRelease.html_url;

                // Search for direct download asset URL
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
            updateInfoText.text = $"New version {newVer} available! (Current: {currentVer})";
        }

        // Show both buttons when an update is found
        if (autoUpdateButton != null) autoUpdateButton.gameObject.SetActive(true);
        if (manualUpdateButton != null) manualUpdateButton.gameObject.SetActive(true);
    }

    private void StartDownloadUpdate()
    {
        if (string.IsNullOrEmpty(directDownloadUrl))
        {
            if (updateInfoText != null) updateInfoText.text = "No direct download file (RawrGame.zip) found in release!";
            return;
        }

        if (autoUpdateButton != null) autoUpdateButton.gameObject.SetActive(false);
        if (manualUpdateButton != null) manualUpdateButton.gameObject.SetActive(false);

        StartCoroutine(DownloadAndInstallRoutine());
    }

    private void OpenDownloadPage()
    {
        if (!string.IsNullOrEmpty(latestReleaseUrl))
        {
            Application.OpenURL(latestReleaseUrl);
        }
    }

    private IEnumerator DownloadAndInstallRoutine()
    {
        string zipPath = Path.Combine(Application.persistentDataPath, "RawrGame.zip");
        string extractPath = Path.Combine(Application.persistentDataPath, "UpdateExtract");

        if (Directory.Exists(extractPath)) Directory.Delete(extractPath, true);
        Directory.CreateDirectory(extractPath);

        if (updateInfoText != null) updateInfoText.text = "Downloading update...";

        using (UnityWebRequest request = UnityWebRequest.Get(directDownloadUrl))
        {
            request.downloadHandler = new DownloadHandlerFile(zipPath);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                if (updateInfoText != null) updateInfoText.text = "Download failed: " + request.error;

                // Re-enable buttons on error
                if (autoUpdateButton != null) autoUpdateButton.gameObject.SetActive(true);
                if (manualUpdateButton != null) manualUpdateButton.gameObject.SetActive(true);
                yield break;
            }
        }

        if (updateInfoText != null) updateInfoText.text = "Extracting update...";

        try
        {
            ZipFile.ExtractToDirectory(zipPath, extractPath);
            File.Delete(zipPath);
        }
        catch (Exception ex)
        {
            if (updateInfoText != null) updateInfoText.text = "Extraction error: " + ex.Message;

            if (autoUpdateButton != null) autoUpdateButton.gameObject.SetActive(true);
            if (manualUpdateButton != null) manualUpdateButton.gameObject.SetActive(true);
            yield break;
        }

        if (updateInfoText != null) updateInfoText.text = "Update ready! Restarting game...";

        CreateAndRunUpdaterScript(extractPath);
    }

    private void CreateAndRunUpdaterScript(string extractPath)
    {
#if UNITY_STANDALONE_WIN
        string gameExeDir = Directory.GetParent(Application.dataPath).FullName;
        string exeName = "RawrGame.exe";
        string fullExePath = Path.Combine(gameExeDir, exeName);
        string batPath = Path.Combine(Application.persistentDataPath, "update.bat");

        string batchContent = $@"
@echo off
chcp 65001 > nul
echo Waiting for the game to close...
timeout /t 3 /nobreak > nul

echo Copying update files to: {gameExeDir}
if exist ""{extractPath}\RawrGame\*"" (
    xcopy /s /y /e ""{extractPath}\RawrGame\*"" ""{gameExeDir}\""
) else (
    xcopy /s /y /e ""{extractPath}\*"" ""{gameExeDir}\""
)

echo Restarting game...
start """" ""{fullExePath}""

rmdir /s /q ""{extractPath}""
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
        public string html_url;
        public GitHubAsset[] assets;
    }

    [Serializable]
    private class GitHubAsset
    {
        public string name;
        public string browser_download_url;
    }
}