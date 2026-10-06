using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkinManager : MonoBehaviour
{
    public static SkinManager Instance { get; private set; }

    [Header("Skin Listen")]
    [SerializeField] private List<SkinItem> hatSkins = new List<SkinItem>();
    [SerializeField] private List<SkinItem> jacketSkins = new List<SkinItem>();

    [Header("Audio")]
    [SerializeField] private AudioSource hatChangeSound;
    [SerializeField] private AudioSource jacketChangeSound;

    [Header("UI Toggle Buttons")]
    [Tooltip("Objekte/Buttons, die nur aktiv sind, wenn man mehr als 1 Hat besitzt")]
    [SerializeField] private List<GameObject> hatSelectionButtons = new List<GameObject>();

    [Tooltip("Objekte/Buttons, die nur aktiv sind, wenn man mehr als 1 Jacket besitzt")]
    [SerializeField] private List<GameObject> jacketSelectionButtons = new List<GameObject>();

    private Image hatImage;
    private Image jacketImage;

    private int currentHatIndex = 0;
    private int currentJacketIndex = 0;

    private const string PREF_HAT_ID = "Selected_Hat_ID";
    private const string PREF_JACKET_ID = "Selected_Jacket_ID";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSelectedSkins();
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 144;
    }

    private void Start()
    {
        FindImagesAndApply();
        UpdateSelectionButtonsVisibility();
    }

    private void FindImagesAndApply()
    {
        GameObject hatObj = GameObject.Find("Hat");
        if (hatObj != null)
        {
            hatImage = hatObj.GetComponent<Image>();
            if (hatImage != null)
            {
                hatImage.sprite = GetCurrentHatSprite();
            }
        }

        GameObject jacketObj = GameObject.Find("Jacket");
        if (jacketObj != null)
        {
            jacketImage = jacketObj.GetComponent<Image>();
            if (jacketImage != null)
            {
                jacketImage.sprite = GetCurrentJacketSprite();
            }
        }
    }

    public void UpdateSelectionButtonsVisibility()
    {
        bool hasMultipleHats = GetUnlockedHatCount() > 1;
        if (hatSelectionButtons != null)
        {
            foreach (var btn in hatSelectionButtons)
            {
                if (btn != null)
                {
                    btn.SetActive(hasMultipleHats);
                }
            }
        }

        bool hasMultipleJackets = GetUnlockedJacketCount() > 1;
        if (jacketSelectionButtons != null)
        {
            foreach (var btn in jacketSelectionButtons)
            {
                if (btn != null)
                {
                    btn.SetActive(hasMultipleJackets);
                }
            }
        }
    }

    public int GetUnlockedHatCount()
    {
        int count = 0;
        foreach (var h in hatSkins)
        {
            if (h != null && h.IsUnlocked) count++;
        }
        return count;
    }

    public int GetUnlockedJacketCount()
    {
        int count = 0;
        foreach (var j in jacketSkins)
        {
            if (j != null && j.IsUnlocked) count++;
        }
        return count;
    }

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

            if (hatChangeSound != null)
            {
                hatChangeSound.Play();
            }

            if (hatImage == null) FindImagesAndApply();
            else hatImage.sprite = GetCurrentHatSprite();
        }
    }

    private void CycleJacket(int direction)
    {
        if (jacketSkins.Count == 0) return;

        int nextIdx = FindNextUnlockedIndex(jacketSkins, currentJacketIndex, direction);
        if (nextIdx != -1)
        {
            currentJacketIndex = nextIdx;
            SaveSelection(PREF_JACKET_ID, jacketSkins[currentJacketIndex].uniqueId);

            if (jacketChangeSound != null)
            {
                jacketChangeSound.Play();
            }

            if (jacketImage == null) FindImagesAndApply();
            else jacketImage.sprite = GetCurrentJacketSprite();
        }
    }

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
        return -1;
    }

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

    public SkinItem GetCurrentHat() => (hatSkins.Count > 0 && hatSkins[currentHatIndex].IsUnlocked) ? hatSkins[currentHatIndex] : null;
    public SkinItem GetCurrentJacket() => (jacketSkins.Count > 0 && jacketSkins[currentJacketIndex].IsUnlocked) ? jacketSkins[currentJacketIndex] : null;

    public void UnlockSkin(string skinId)
    {
        SkinItem skin = hatSkins.Find(s => s.uniqueId == skinId) ?? jacketSkins.Find(s => s.uniqueId == skinId);
        if (skin != null)
        {
            skin.IsUnlocked = true;
            UpdateSelectionButtonsVisibility();
        }
    }

    public bool IsSkinUnlocked(string skinId)
    {
        var skin = hatSkins.Find(s => s.uniqueId == skinId) ?? jacketSkins.Find(s => s.uniqueId == skinId);
        if (skin != null && skin.isUnlockedByDefault) return true;

        return PlayerPrefs.GetInt("Skin_Unlocked_" + skinId, 0) == 1;
    }

    private void SaveSelection(string prefKey, string uniqueId)
    {
        PlayerPrefs.SetString(prefKey, uniqueId);
        PlayerPrefs.Save();
    }

    private void LoadSelectedSkins()
    {
        if (hatSkins.Count > 0)
        {
            string savedHatId = PlayerPrefs.GetString(PREF_HAT_ID, "");
            int hatIdx = hatSkins.FindIndex(s => s.uniqueId == savedHatId && s.IsUnlocked);

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