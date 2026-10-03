using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public static class ProfanityFilter
{
    private static readonly HashSet<string> ExactBlockedWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private static readonly List<string> SubstringBlockedWords = new List<string>();
    private static bool isInitialized = false;

    private static readonly Dictionary<char, char> LeetMap = new Dictionary<char, char>
    {
        { '@', 'a' }, { '4', 'a' },
        { '8', 'b' },
        { '(', 'c' }, { '<', 'c' },
        { '3', 'e' }, { '€', 'e' },
        { '1', 'i' }, { '!', 'i' }, { '|', 'i' }, { 'l', 'i' },
        { '0', 'o' },
        { '$', 's' }, { '5', 's' },
        { '7', 't' }, { '+', 't' },
        { 'v', 'u' },
        { 'w', 'v' }
    };

    public static void Initialize()
    {
        if (isInitialized) return;

        // 1. Substrings: Allways blocked, no matter where in name
        AddSubstringPatterns(new[]
        {
            "nigg", "n1gg", "niga", "nigga", "nigger", "kike", "chink", "spic", "fag", "faggot",
            "schwuchtel", "tranny",

            "hitler", "nazi", "swastika", "hakenkreuz", "holocaust", "auschwitz",

            "pedophil", "paedophil", "rape", "vergewaltig", "childporn", "cp",

            "hure", "hurensohn", "fotze", "misstgeburt", "missgeburt", "bastard", "wanker",
            "motherfucker", "whore", "slut", "cunt", "arschloch", "spast", "spasti"
        });

        // 2. Exact words: Blocked,  if name is exactly...
        AddExactWords(new[]
        {
            // Deutsch
            "nutte", "schlampe", "wichser", "wichs", "penner", "idiot", "spacko", "vollidiot", "ss-",
            "arsch", "scheisse", "scheiße", "kacke", "fick", "ficken", "sau", "depp", "trottel",
            "pimmel", "schwanz", "muschi", "titten", "fresse", "nega", "neger", "-",

            // English
            "fuck", "fucking", "fucked", "fucker", "bitch", "shit", "ass", "asshole",
            "dick", "cock", "penis", "vagina", "boobs", "tits", "porn", "porno", "sex",
            "anal", "clit", "pussy", "dildo", "jerk", "moron", "retard", "dumbass"
        });

        // 3. Future optional exact txt containing more badwords from Resources/badwords.txt
        TextAsset file = Resources.Load<TextAsset>("badwords");
        if (file != null)
        {
            string[] lines = file.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                string trimmed = line.Trim();
                if (!string.IsNullOrEmpty(trimmed) && !trimmed.StartsWith("#"))
                {
                    ExactBlockedWords.Add(Normalize(trimmed));
                }
            }
        }

        isInitialized = true;
    }

    private static void AddExactWords(IEnumerable<string> words)
    {
        foreach (var w in words)
        {
            ExactBlockedWords.Add(Normalize(w));
        }
    }

    private static void AddSubstringPatterns(IEnumerable<string> patterns)
    {
        foreach (var p in patterns)
        {
            SubstringBlockedWords.Add(Normalize(p));
        }
    }

    public static bool IsInappropriate(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;
        if (!isInitialized) Initialize();

        string cleaned = Normalize(input);

        foreach (var badSub in SubstringBlockedWords)
        {
            if (cleaned.Contains(badSub))
            {
                return true;
            }
        }

        if (ExactBlockedWords.Contains(cleaned))
        {
            return true;
        }

        string[] parts = input.Split(new[] { ' ', '_', '-', '.', ',', '/', '|' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            string partCleaned = Normalize(part);
            if (ExactBlockedWords.Contains(partCleaned))
            {
                return true;
            }
        }

        return false;
    }

    private static string Normalize(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return string.Empty;

        raw = raw.ToLowerInvariant();
        StringBuilder sb = new StringBuilder();

        foreach (char c in raw)
        {
            if (LeetMap.TryGetValue(c, out char mapped))
            {
                sb.Append(mapped);
            }
            else if (char.IsLetterOrDigit(c))
            {
                sb.Append(c);
            }
        }

        string intermediate = sb.ToString();

        return Regex.Replace(intermediate, @"(.)\1+", "$1");
    }
}