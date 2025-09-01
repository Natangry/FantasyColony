using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FantasyColony.Core.Defs.Validation {
    /// <summary>
    /// Loads and caches per-type schema files located in StreamingAssets and Mods.
    /// </summary>
    public static class SchemaCatalog {
        private static readonly Dictionary<string, Dictionary<int, SchemaSpec>> _map = new(); // type -> version -> spec
        private static bool _loaded;

        public static void EnsureLoadedFromIndex(Defs.DefIndex index) {
            if (_loaded) return;
            try {
                // Game-bundled schemas
                var gameDir = Path.Combine(Application.streamingAssetsPath, "Defs", "Schemas");
                LoadFromDir(gameDir);

                // Mod-provided schemas (infer mod root from def file paths in index)
                if (index != null) {
                    var roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var m in index.Items) {
                        var root = FindDefsRoot(Path.GetDirectoryName(m.Path));
                        if (!string.IsNullOrEmpty(root) && roots.Add(root)) {
                            var dir = Path.Combine(root, "Defs", "Schemas");
                            LoadFromDir(dir);
                        }
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

        private static string FindDefsRoot(string startDir) {
            try {
                var dir = startDir;
                while (!string.IsNullOrEmpty(dir)) {
                    var defsDir = Path.Combine(dir, "Defs");
                    if (Directory.Exists(defsDir)) return dir;
                    dir = Path.GetDirectoryName(dir);
                }
            } catch { }
            return null;
        }

        private static void LoadFile(string path) {
            try {
                var json = File.ReadAllText(path);
                var spec = JsonUtility.FromJson<SchemaSpec>(json);
                if (spec == null || string.IsNullOrEmpty(spec.type) || spec.version <= 0) return;

                // Mark presence of numeric bounds so zero is enforceable
                try {
                    if (spec.fields != null) {
                        foreach (var fs in spec.fields) {
                            if (string.IsNullOrEmpty(fs.name)) continue;
                            var nameEsc = System.Text.RegularExpressions.Regex.Escape(fs.name);
                            var rx = new System.Text.RegularExpressions.Regex("{[^{}]*\\\"name\\\"\\s*:\\s*\\\"" + nameEsc + "\\\"[^{}]*}", System.Text.RegularExpressions.RegexOptions.CultureInvariant);
                            var m = rx.Match(json);
                            if (m.Success) {
                                var slice = m.Value;
                                fs.hasMin = slice.Contains("\\\"min\\\"");
                                fs.hasMax = slice.Contains("\\\"max\\\"");
                            }
                        }
                    }
                } catch { /* best-effort; missing flags default to false */ }
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
