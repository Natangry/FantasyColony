using System;
using System.Collections.Generic;
using UnityEngine;
using FantasyColony.Core;

namespace FantasyColony.Core.Services
{
    /// <summary>
    /// Minimal localization service. Loads Resources/Localization/{lang}/strings.json (TextAsset).
    /// Falls back to English or the key itself.
    /// </summary>
    public class LocService
    {
        private static LocService _instance;
        public static LocService Instance => _instance ?? (_instance = new LocService());

        private readonly Dictionary<string, string> _map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public string Language { get; private set; } = "en";

        public void SetLanguage(string lang)
        {
            Language = string.IsNullOrEmpty(lang) ? "en" : lang;
            Load(Language);
        }

        public string Get(string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            return _map.TryGetValue(key, out var v) ? v : key;
        }

        private void Load(string lang)
        {
            _map.Clear();
            // Try target language
            if (!TryLoadInto(lang))
            {
                // Fallback to English
                if (!string.Equals(lang, "en", StringComparison.OrdinalIgnoreCase))
                {
                    TryLoadInto("en");
                }
            }
        }

        private bool TryLoadInto(string lang)
        {
            try
            {
                var path = $"Localization/{lang}/strings";
                TextAsset ta = null;
                // Prefer provider if available (mod- and backend-friendly)
                var host = AppHost.Instance;
                if (host != null && host.Services != null && host.Services.TryGet<IAssetProvider>(out var provider))
                {
                    ta = provider.LoadText(path);
                }
                // Fallback to Resources for safety
                if (ta == null)
                {
                    ta = Resources.Load<TextAsset>(path);
                }
                if (ta == null) return false;
                var bag = JsonUtility.FromJson<LocBag>(ta.text);
                if (bag?.entries != null)
                {
                    foreach (var e in bag.entries)
                    {
                        if (!string.IsNullOrEmpty(e.key)) _map[e.key] = e.value ?? string.Empty;
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"LocService load error: {e.Message}");
                return false;
            }
        }

        [Serializable]
        private class LocBag
        {
            public Entry[] entries;
        }

        [Serializable]
        private class Entry
        {
            public string key;
            public string value;
        }
    }
}
