using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkinManager : MonoBehaviour
{
    [Header("Skin Listen")]
    [SerializeField] private List<SkinItem> hatSkins = new List<SkinItem>();
    [SerializeField] private List<SkinItem> jacketSkins = new List<SkinItem>();

    [Header("UI Image Preview")]
    [SerializeField] private Image hatPreviewImage;
    [SerializeField] private Image jacketPreviewImage;

    private int currentHatIndex = 0;
    private int currentJacketIndex = 0;

    private const string PREF_HAT_ID = "Selected_Hat_ID";
    private const string PREF_JACKET_ID = "Selected_Jacket_ID";
    public static SkinManager Instance;

    private void Awake()
    {
        // Duplikat-Check: Existiert bereits eine Instanz?
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        LoadSelectedSkins();
        UpdateUI();
    }

    // ==========================================
    // BUTTON ACTIONS (Plus / Minus)
    // ==========================================

    public void NextHat() => CycleHat(1);
    public void PreviousHat() => CycleHat(-1);

    public void NextJacket() => CycleJacket(1);
    public void PreviousJacket() => CycleJacket(-1);

    private void CycleHat(int direction)
    {
        if (hatSkins.Count == 0) return;

        int nextIdx = FindNextUnlockedIndex(hatSkins, currentHatIndex, direction);
        if (nextIdx != -1)
        {
            currentHatIndex = nextIdx;
            SaveSelection(PREF_HAT_ID, hatSkins[currentHatIndex].uniqueId);
            UpdateUI();
        }
    }
    public Sprite GetHatSpriteById(string id)
    {
        var item = hatSkins.Find(s => s.uniqueId == id);
        return item != null ? item.icon : null;
    }

    public Sprite GetJacketSpriteById(string id)
    {
        var item = jacketSkins.Find(s => s.uniqueId == id);
        return item != null ? item.icon : null;
    }

    private void CycleJacket(int direction)
    {
        if (jacketSkins.Count == 0) return;

        int nextIdx = FindNextUnlockedIndex(jacketSkins, currentJacketIndex, direction);
        if (nextIdx != -1)
        {
            currentJacketIndex = nextIdx;
            SaveSelection(PREF_JACKET_ID, jacketSkins[currentJacketIndex].uniqueId);
            UpdateUI();
        }
    }

    /// <summary>
    /// Sucht ausgehend vom aktuellen Index in angegebener Richtung den nächsten freigeschalteten Skin.
    /// Gibt -1 zurück, wenn gar kein Skin freigeschaltet ist.
    /// </summary>
    private int FindNextUnlockedIndex(List<SkinItem> list, int startIndex, int direction)
    {
        int count = list.Count;
        for (int i = 1; i <= count; i++)
        {
            int checkIndex = (startIndex + (i * direction) % count + count) % count;
            if (list[checkIndex].IsUnlocked)
            {
                return checkIndex;
            }
        }
        return -1; // Keiner freigeschaltet
    }

    // ==========================================
    // GETTER FÜR SPRITES & ITEMS
    // ==========================================

    public Sprite GetCurrentHatSprite()
    {
        if (hatSkins.Count == 0 || !hatSkins[currentHatIndex].IsUnlocked) return null;
        return hatSkins[currentHatIndex].icon;
    }

    public Sprite GetCurrentJacketSprite()
    {
        if (jacketSkins.Count == 0 || !jacketSkins[currentJacketIndex].IsUnlocked) return null;
        return jacketSkins[currentJacketIndex].icon;
    }

    public SkinItem GetCurrentHat() => (hatSkins.Count > 0 && hatSkins[currentHatIndex].IsUnlocked) ? hatSkins[currentHatIndex] : null;
    public SkinItem GetCurrentJacket() => (jacketSkins.Count > 0 && jacketSkins[currentJacketIndex].IsUnlocked) ? jacketSkins[currentJacketIndex] : null;

    // ==========================================
    // FREISCHALTUNG
    // ==========================================

    public void UnlockSkin(string skinId)
    {
        SkinItem skin = hatSkins.Find(s => s.uniqueId == skinId) ?? jacketSkins.Find(s => s.uniqueId == skinId);
        if (skin != null)
        {
            skin.IsUnlocked = true;
        }
    }
    public bool IsSkinUnlocked(string skinId)
    {
        // Sucht das Item in den Listen
        SkinItem skin = hatSkins.Find(s => s.uniqueId == skinId) ?? jacketSkins.Find(s => s.uniqueId == skinId);

        if (skin != null)
        {
            return skin.IsUnlocked;
        }

        // Fallback: Direkt über PlayerPrefs prüfen
        return PlayerPrefs.GetInt("Skin_Unlocked_" + skinId, 0) == 1;
    }

    // ==========================================
    // INTERNE LOGIK (Save / Load / UI)
    // ==========================================

    private void UpdateUI()
    {
        if (hatPreviewImage != null && hatSkins.Count > 0 && hatSkins[currentHatIndex].IsUnlocked)
        {
            hatPreviewImage.sprite = hatSkins[currentHatIndex].icon;
        }

        if (jacketPreviewImage != null && jacketSkins.Count > 0 && jacketSkins[currentJacketIndex].IsUnlocked)
        {
            jacketPreviewImage.sprite = jacketSkins[currentJacketIndex].icon;
        }
    }

    private void SaveSelection(string prefKey, string uniqueId)
    {
        PlayerPrefs.SetString(prefKey, uniqueId);
        PlayerPrefs.Save();
    }

    private void LoadSelectedSkins()
    {
        // 1. Hat laden und absichern
        if (hatSkins.Count > 0)
        {
            string savedHatId = PlayerPrefs.GetString(PREF_HAT_ID, "");
            int hatIdx = hatSkins.FindIndex(s => s.uniqueId == savedHatId && s.IsUnlocked);

            // Falls gespeicherter Skin nicht existiert oder nicht freigeschaltet ist: ersten freigeschalteten nehmen
            if (hatIdx == -1)
            {
                hatIdx = hatSkins.FindIndex(s => s.IsUnlocked);
            }

            currentHatIndex = (hatIdx != -1) ? hatIdx : 0;
            if (hatSkins[currentHatIndex].IsUnlocked)
            {
                SaveSelection(PREF_HAT_ID, hatSkins[currentHatIndex].uniqueId);
            }
        }

        // 2. Jacket laden und absichern
        if (jacketSkins.Count > 0)
        {
            string savedJacketId = PlayerPrefs.GetString(PREF_JACKET_ID, "");
            int jacketIdx = jacketSkins.FindIndex(s => s.uniqueId == savedJacketId && s.IsUnlocked);

            if (jacketIdx == -1)
            {
                jacketIdx = jacketSkins.FindIndex(s => s.IsUnlocked);
            }

            currentJacketIndex = (jacketIdx != -1) ? jacketIdx : 0;
            if (jacketSkins[currentJacketIndex].IsUnlocked)
            {
                SaveSelection(PREF_JACKET_ID, jacketSkins[currentJacketIndex].uniqueId);
            }
        }
    }
}