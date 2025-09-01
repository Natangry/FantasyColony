using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FantasyColony.Core.Services
{
    /// <summary>
    /// Minimal JSON-backed config service with string Get/Set to avoid API churn.
    /// </summary>
    public class JsonConfigService : IConfigService
    {
        private static JsonConfigService _instance;
        public static JsonConfigService Instance => _instance ?? (_instance = new JsonConfigService());

        private readonly Dictionary<string, string> _map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private string Path => System.IO.Path.Combine(Application.persistentDataPath, "config.json");

        public void Load()
        {
            try
            {
                if (!File.Exists(Path)) return;
                var json = File.ReadAllText(Path);
                var data = JsonUtility.FromJson<Bag>(json);
                _map.Clear();
                if (data != null && data.keys != null)
                {
                    for (int i = 0; i < data.keys.Length; i++)
                    {
                        var k = data.keys[i];
                        var v = (data.values != null && i < data.values.Length) ? data.values[i] : string.Empty;
                        if (!string.IsNullOrEmpty(k)) _map[k] = v ?? string.Empty;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"JsonConfigService load error: {e.Message}");
            }
        }

        public void Save()
        {
            try
            {
                var bag = new Bag
                {
                    keys = new string[_map.Count],
                    values = new string[_map.Count]
                };
                int idx = 0;
                foreach (var kv in _map)
                {
                    bag.keys[idx] = kv.Key;
                    bag.values[idx] = kv.Value;
                    idx++;
                }
                var json = JsonUtility.ToJson(bag, true);
                var dir = System.IO.Path.GetDirectoryName(Path);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                var tmp = Path + ".tmp";
                try
                {
                    File.WriteAllText(tmp, json);
                    // Atomic replace when available (PC platforms)
#if UNITY_2021_2_OR_NEWER
                    var bak = Path + ".bak";
                    File.Replace(tmp, Path, bak, true);
#else
                    // Fallback: best-effort replace
                    if (File.Exists(Path)) File.Delete(Path);
                    File.Move(tmp, Path);
#endif
                }
                finally { if (File.Exists(tmp)) { try { File.Delete(tmp); } catch { } } }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"JsonConfigService save error: {e.Message}");
            }
        }

        public string Get(string key, string fallback = "")
        {
            if (string.IsNullOrEmpty(key)) return fallback;
            return _map.TryGetValue(key, out var v) ? v : fallback;
        }

        public FantasyColony.Core.Defs.Validation.ValidationMode GetValidationMode()
        {
            var s = Get("validation_mode", "lenient").ToLowerInvariant();
            return (s == "strict") ? FantasyColony.Core.Defs.Validation.ValidationMode.Strict : FantasyColony.Core.Defs.Validation.ValidationMode.Lenient;
        }

        public void Set(string key, string value) {
            _map[key] = value ?? string.Empty;
            // Optional: persist to disk here if desired.
        }

        [Serializable]
        private class Bag
        {
            public string[] keys;
            public string[] values;
        }
    }
}
