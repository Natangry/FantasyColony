using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FantasyColony.Core.Services
{
    public struct SaveSlotMeta
    {
        public string slotId;
        public string displayName;
        public DateTime lastPlayedUtc;
        public string version;
    }

    /// <summary>
    /// Skeleton save service: only reads metadata from disk so the Main Menu can react.
    /// </summary>
    public class JsonSaveService
    {
        private static JsonSaveService _instance;
        public static JsonSaveService Instance => _instance ?? (_instance = new JsonSaveService());

        private readonly List<SaveSlotMeta> _cache = new List<SaveSlotMeta>();
        public IReadOnlyList<SaveSlotMeta> Slots => _cache;

        private string Root => Path.Combine(Application.persistentDataPath, "saves");

        public void RefreshCache()
        {
            _cache.Clear();
            try
            {
                if (!Directory.Exists(Root)) return;
                foreach (var dir in Directory.GetDirectories(Root))
                {
                    var slotId = Path.GetFileName(dir);
                    var metaPath = Path.Combine(dir, "meta.json");
                    if (!File.Exists(metaPath)) continue;
                    try
                    {
                        var json = File.ReadAllText(metaPath);
                        var meta = JsonUtility.FromJson<SlotBag>(json) ?? new SlotBag();
                        var when = DateTime.TryParse(meta.lastPlayedUtc, out var dt) ? dt : DateTime.UtcNow;
                        _cache.Add(new SaveSlotMeta
                        {
                            slotId = slotId,
                            displayName = string.IsNullOrEmpty(meta.displayName) ? slotId : meta.displayName,
                            lastPlayedUtc = when,
                            version = meta.version ?? "unknown"
                        });
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"Bad save meta in {dir}: {e.Message}");
                    }
                }
                _cache.Sort((a, b) => b.lastPlayedUtc.CompareTo(a.lastPlayedUtc));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Save discovery error: {e.Message}");
            }
        }

        public bool HasAnySaves() => _cache.Count > 0;

        [Serializable]
        private class SlotBag
        {
            public string displayName;
            public string lastPlayedUtc;
            public string version;
        }
    }
}
