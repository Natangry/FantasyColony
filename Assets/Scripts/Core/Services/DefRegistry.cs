using System;
using System.Collections.Generic;
using UnityEngine;

namespace FantasyColony.Core.Services {
    /// <summary>
    /// Definition registry keyed by type then id, storing file path and mod id.
    /// </summary>
    public class DefRegistry {
        private static DefRegistry _instance;
        public static DefRegistry Instance => _instance ?? (_instance = new DefRegistry());

        public struct Entry {
            public string Type;
            public string Id;
            public string Path;
            public string ModId;
        }

        private readonly Dictionary<string, Dictionary<string, Entry>> _map = new(StringComparer.OrdinalIgnoreCase);

        public int Count { get; private set; }
        public int ConflictCount { get; private set; }

        public void Register(string type, string id, string filePath, string modId) {
            if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(id)) return;
            if (!_map.TryGetValue(type, out var inner)) {
                inner = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);
                _map[type] = inner;
            }
            if (inner.TryGetValue(id, out var existing)) {
                ConflictCount++;
                Debug.LogWarning($"[Defs] Conflict for {type}/{id}: {existing.ModId} -> {modId} (file: {filePath})");
            }
            else { Count++; }
            inner[id] = new Entry { Type = type, Id = id, Path = filePath ?? string.Empty, ModId = modId };
        }

        // Legacy Add signature for backward compatibility
        public void Add(string type, string id, string filePath) => Register(type, id, filePath, string.Empty);

        public bool TryGetPath(string type, string id, out string path) {
            path = null;
            if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(id)) return false;
            if (_map.TryGetValue(type, out var inner) && inner.TryGetValue(id, out var e)) {
                path = e.Path;
                return true;
            }
            return false;
        }

        public IEnumerable<Entry> All() {
            foreach (var inner in _map.Values)
                foreach (var e in inner.Values)
                    yield return e;
        }

        public IEnumerable<string> Types() => _map.Keys;

        public IEnumerable<string> Ids(string type) {
            if (_map.TryGetValue(type, out var inner)) return inner.Keys;
            return Array.Empty<string>();
        }
    }
}
