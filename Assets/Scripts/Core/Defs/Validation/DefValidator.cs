using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace FantasyColony.Core.Defs.Validation {
    public static class DefValidator {
        public sealed class Result { public string Path; public string Message; public override string ToString()=> $"{Path}: {Message}"; }

        public static List<Result> Run(Defs.DefIndex index) {
            var results = new List<Result>();

            // R1: ID well-formed and present
            foreach (var m in index.Items) {
                if (string.IsNullOrEmpty(m.Id)) {
                    results.Add(new Result { Path = m.Path, Message = $"Missing id for type {m.Type}" });
                    continue;
                }
                if (!Defs.DefId.TryParse(m.Id.Contains('.') ? m.Id : $"{m.Type}.{m.Id}", out _)) {
                    results.Add(new Result { Path = m.Path, Message = $"Id not well-formed: '{m.Id}' (expected modid.{m.Type}.Name or {m.Type}.Name)" });
                }
            }

            // R2: Duplicates within (Type, Id) across all mods
            var seen = new HashSet<string>();
            foreach (var m in index.Items) {
                var key = m.Type + "." + m.Id;
                if (!seen.Add(key)) results.Add(new Result { Path = m.Path, Message = $"Duplicate id for {m.Type}: '{m.Id}'" });
            }

            // R3: Cross-ref scan (heuristic) for common ref attributes
            foreach (var m in index.Items) {
                try {
                    using var xr = XmlReader.Create(m.Path, new XmlReaderSettings { IgnoreComments = true, IgnoreWhitespace = true, DtdProcessing = DtdProcessing.Ignore });
                    xr.MoveToContent();
                    // Scan attributes on root and first level children for ref-like names
                    ScanAttrs(xr, m, index, results);
                    if (!xr.IsEmptyElement) {
                        xr.ReadStartElement();
                        int depth0 = xr.Depth;
                        while (!xr.EOF && xr.Depth <= depth0 + 1) {
                            if (xr.NodeType == XmlNodeType.Element) {
                                ScanAttrs(xr, m, index, results);
                            }
                            xr.Read();
                        }
                    }
                } catch { /* per-file issues already handled elsewhere */ }
            }

            // R4: Schema presence and version
            foreach (var m in index.Items) {
                var (curOk, curVer) = Defs.Migrations.SchemaRegistry.TryGetCurrentVersion(m.Type);
                if (!string.IsNullOrEmpty(m.Schema) || m.SchemaVersion > 0) {
                    if (curOk && m.SchemaVersion > curVer) {
                        results.Add(new Result { Path = m.Path, Message = $"Schema version {m.SchemaVersion} ahead of game-supported {curVer} for {m.Type}" });
                    }
                } else {
                    results.Add(new Result { Path = m.Path, Message = $"Missing schema/schema_version on {m.Type}" });
                }
            }

            return results;
        }

        private static readonly string[] RefLike = new[] { "def", "defref", "target", "source", "*_def", "*_ref" };
        private static void ScanAttrs(XmlReader xr, Defs.DefMeta m, Defs.DefIndex index, List<Result> results) {
            if (!xr.HasAttributes) return;
            while (xr.MoveToNextAttribute()) {
                var an = xr.Name.ToLowerInvariant();
                if (!LooksLikeRef(an)) continue;
                var v = xr.Value;
                if (string.IsNullOrEmpty(v)) continue;
                if (!TryResolveRef(index, m, an, v)) {
                    results.Add(new Result { Path = m.Path, Message = $"Missing reference '{an}={v}'" });
                }
            }
            xr.MoveToElement();
        }

        private static bool LooksLikeRef(string attrLower) {
            if (attrLower.EndsWith("_def") || attrLower.EndsWith("_ref")) return true;
            for (int i=0;i<RefLike.Length;i++) if (attrLower == RefLike[i]) return true;
            return false;
        }

        private static bool TryResolveRef(Defs.DefIndex index, Defs.DefMeta m, string attrName, string value) {
            // Accept Type.Id or modid.Type.Name or just Id if Type can be inferred (not supported yet)
            if (value.Contains('.')) {
                // normalize to Type.Id without mod prefix if present
                string type, id;
                var parts = value.Split('.');
                if (parts.Length == 2) { type = parts[0]; id = parts[1]; }
                else if (parts.Length == 3) { type = parts[1]; id = parts[2]; }
                else return false;
                return index.Find(type, id) != null;
            }
            // Could be plain Id; we don't infer yet → treat as unresolved
            return false;
        }
    }
}
