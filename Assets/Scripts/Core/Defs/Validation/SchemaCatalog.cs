using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using FantasyColony.Core.Mods;

namespace FantasyColony.Core.Defs.Validation {
    /// <summary>
    /// Loads and caches per-type schema files located in StreamingAssets and Mods.
    /// </summary>
    public static class SchemaCatalog {
        private static readonly Dictionary<string, Dictionary<int, SchemaSpec>> _map = new(); // type -> version -> spec
        private static bool _loaded;

        public static void EnsureLoaded(List<ModInfo> mods) {
            if (_loaded) return;
            try {
                // Game-bundled schemas
                var gameDir = Path.Combine(Application.streamingAssetsPath, "Defs", "Schemas");
                LoadFromDir(gameDir);

                // Mod-provided schemas
                if (mods != null) {
                    for (int i = 0; i < mods.Count; i++) {
                        var dir = Path.Combine(mods[i].RootPath, "Defs", "Schemas");
                        LoadFromDir(dir);
                    }
                }
            } catch (Exception e) {
                Debug.LogWarning($"SchemaCatalog load issue: {e.Message}");
            }
            _loaded = true;
        }

        private static void LoadFromDir(string dir) {
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return;
            var files = Directory.GetFiles(dir, "*.schema.json", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++) LoadFile(files[i]);
        }

        private static void LoadFile(string path) {
            try {
                var json = File.ReadAllText(path);
                var spec = JsonUtility.FromJson<SchemaSpec>(json);
                if (spec == null || string.IsNullOrEmpty(spec.type) || spec.version <= 0) return;
                var tkey = spec.type;
                if (!_map.TryGetValue(tkey, out var byVer)) _map[tkey] = byVer = new Dictionary<int, SchemaSpec>();
                // First-in wins; later duplicates are ignored but warned
                if (byVer.ContainsKey(spec.version)) {
                    Debug.LogWarning($"Duplicate schema {spec.type}@{spec.version} at {path}; keeping first loaded.");
                    return;
                }
                byVer[spec.version] = spec;
            } catch (Exception e) {
                Debug.LogWarning($"Failed to load schema '{path}': {e.Message}");
            }
        }

        public static bool TryGet(string defType, int version, out SchemaSpec spec) {
            spec = null;
            if (string.IsNullOrEmpty(defType) || version <= 0) return false;
            if (_map.TryGetValue(defType, out var byVer) && byVer.TryGetValue(version, out spec)) return true;
            return false;
        }

        public static bool TryGetCurrent(string defType, out SchemaSpec spec) {
            spec = null;
            var (ok, v) = FantasyColony.Core.Defs.Migrations.SchemaRegistry.TryGetCurrentVersion(defType);
            if (!ok) return false;
            return TryGet(defType, v, out spec);
        }
    }
}
