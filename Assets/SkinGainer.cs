using System;
using System.Collections.Generic;
using UnityEngine;

public class SkinGainer : MonoBehaviour
{
    public enum UnlockConditionType
    {
        ScoreReached,       // Wird anhand der Player-X-Position / Score freigeschaltet
        AnnualDateRange     // Jährliches Event (Monat + Tag, z.B. 24.12. bis 26.12.)
    }

    [System.Serializable]
    public class SkinUnlockRule
    {
        [Header("Skin Info")]
        public string skinId;
        public UnlockConditionType conditionType;

        [Header("Bedingung: Score / Distanz")]
        [Tooltip("Mindest-Score / X-Position, die erreicht werden muss")]
        public float requiredScore = 5000f;

        [Header("Bedingung: Jährliches Datum (Tag & Monat)")]
        [Tooltip("Start-Tag (1-31)")]
        [Range(1, 31)] public int startDay = 24;
        [Tooltip("Start-Monat (1-12)")]
        [Range(1, 12)] public int startMonth = 12;

        [Space(5)]
        [Tooltip("End-Tag (1-31)")]
        [Range(1, 31)] public int endDay = 26;
        [Tooltip("End-Monat (1-12)")]
        [Range(1, 12)] public int endMonth = 12;

        [HideInInspector]
        public bool isChecked = false; // Verhindert mehrfaches Prüfen im selben Run
    }

    [Header("Referenzen")]
    [Tooltip("Transform des lokalen Spielers (zur Distanz-/Score-Ermittlung). Falls leer, wird LocalScenePlayerInstance genutzt.")]
    [SerializeField] private Transform playerTransform;

    [Header("Herausforderungen")]
    [SerializeField] private List<SkinUnlockRule> unlockRules = new List<SkinUnlockRule>();

    private void Start()
    {
        // 1. Jährliche Datums-Events beim Starten prüfen
        CheckAnnualDateUnlocks();
    }

    private void Update()
    {
        // 2. Score-basierte Belohnungen während des Laufs prüfen
        CheckScoreBasedUnlocks();
    }

    private void CheckAnnualDateUnlocks()
    {
        DateTime now = DateTime.Now;

        foreach (var rule in unlockRules)
        {
            if (rule.conditionType == UnlockConditionType.AnnualDateRange)
            {
                if (IsSkinAlreadyUnlocked(rule.skinId)) continue;

                if (IsDateInRange(now, rule.startDay, rule.startMonth, rule.endDay, rule.endMonth))
                {
                    GrantSkin(rule.skinId, $"Saisonales Event aktiv ({rule.startDay}.{rule.startMonth}. bis {rule.endDay}.{rule.endMonth}.)");
                }
            }
        }
    }

    /// <summary>
    /// Prüft, ob ein Datum unabhängig vom Jahr im Bereich liegt (auch über Silvester hinaus).
    /// </summary>
    private bool IsDateInRange(DateTime now, int startDay, int startMonth, int endDay, int endMonth)
    {
        // Vergleichswert als Zahl formatieren: Monat * 100 + Tag (z.B. 24. Dezember = 1224)
        int currentVal = now.Month * 100 + now.Day;
        int startVal = startMonth * 100 + startDay;
        int endVal = endMonth * 100 + endDay;

        // Normaler Bereich innerhalb eines Kalenderjahres (z.B. 24.12. bis 26.12.)
        if (startVal <= endVal)
        {
            return currentVal >= startVal && currentVal <= endVal;
        }
        else
        {
            // Bereich geht über Neujahr (z.B. 28.12. bis 05.01.)
            return currentVal >= startVal || currentVal <= endVal;
        }
    }

    private void CheckScoreBasedUnlocks()
    {
        if (playerTransform == null)
        {
            if (movementGhost.LocalScenePlayerInstance != null)
            {
                playerTransform = movementGhost.LocalScenePlayerInstance.transform;
            }
            else
            {
                return;
            }
        }

        float currentScore = playerTransform.position.x;

        foreach (var rule in unlockRules)
        {
            if (rule.conditionType == UnlockConditionType.ScoreReached && !rule.isChecked)
            {
                if (currentScore >= rule.requiredScore)
                {
                    rule.isChecked = true;

                    if (!IsSkinAlreadyUnlocked(rule.skinId))
                    {
                        GrantSkin(rule.skinId, $"Score von {rule.requiredScore} erreicht!");
                    }
                }
            }
        }
    }

    private bool IsSkinAlreadyUnlocked(string skinId)
    {
        if (SkinManager.Instance != null)
        {
            return SkinManager.Instance.IsSkinUnlocked(skinId);
        }
        return PlayerPrefs.GetInt("Skin_Unlocked_" + skinId, 0) == 1;
    }

    private void GrantSkin(string skinId, string reason)
    {
        if (SkinManager.Instance != null)
        {
            SkinManager.Instance.UnlockSkin(skinId);
        }
        else
        {
            PlayerPrefs.SetInt("Skin_Unlocked_" + skinId, 1);
            PlayerPrefs.Save();
        }

        Debug.Log($"<color=green>[SkinGainer] Skin freigeschaltet: {skinId}!</color> Grund: {reason}");
    }
}