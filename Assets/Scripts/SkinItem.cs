using UnityEngine;

[System.Serializable]
public class SkinItem
{
    public string skinName;
    public string uniqueId;
    public Sprite icon;
    public bool isUnlockedByDefault;

    public bool IsUnlocked
    {
        get
        {
            if (isUnlockedByDefault) return true;
            return PlayerPrefs.GetInt("Skin_Unlocked_" + uniqueId, 0) == 1;
        }
        set
        {
            PlayerPrefs.SetInt("Skin_Unlocked_" + uniqueId, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}