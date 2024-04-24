using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class Save : MonoBehaviour
{
    [SerializeField] GameObject Player;
    [SerializeField] TMP_Text Score;
    string path = Application.dataPath + "/High-Score.txt";
    private void Start()
    {
        if (Score != null)
        {
            SetScore();
        }
        if (!File.Exists(path))
        {
            File.Create(path);
        }
    }
    public void SaveFile()
    {
        if (LoadFile() >= Player.transform.position.x)
            return;
        using (StreamWriter writer = new StreamWriter(path))
        {
            writer.Write(Mathf.Round(Player.transform.position.x));
        }
    }
    public int LoadFile()
    {
        int loadedScore;

        using (StreamReader reader = new StreamReader(path))
        {
            string score = reader.ReadToEnd();
            int.TryParse(score, out loadedScore);
        }
        return loadedScore;
    }
    public void SetScore()
    {
        Score.text = LoadFile().ToString();
    }
}