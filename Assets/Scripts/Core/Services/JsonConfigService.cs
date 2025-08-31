using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Core.Services
{
    /// <summary>
    /// Minimal JSON-backed config service with string Get/Set to avoid API churn.
    /// </summary>
    public class JsonConfigService
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
                        var v = i < data.values.Length ? data.values[i] : string.Empty;
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
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
                File.WriteAllText(Path, json);
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

        public void Set(string key, string value)
        {
            if (string.IsNullOrEmpty(key)) return;
            _map[key] = value ?? string.Empty;
        }

        [Serializable]
        private class Bag
        {
            public string[] keys;
            public string[] values;
        }
    }
}
