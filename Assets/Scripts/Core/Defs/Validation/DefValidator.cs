using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace FantasyColony.Core.Defs.Validation {
    public static class DefValidator {
        public sealed class Result { public string Path; public string Message; public override string ToString()=> $"{Path}: {Message}"; }

        public static List<Result> Run(Defs.DefIndex index) {
            var results = new List<Result>();

            // R0: Duplicates within (Type, Id) across all mods
            var seen = new HashSet<string>();
            foreach (var m in index.Items) {
                var key = m.Type + "." + m.Id;
                if (!seen.Add(key)) results.Add(new Result { Path = m.Path, Message = $"Duplicate id for {m.Type}: '{m.Id}'" });
            }

            // Spec-driven validation
            foreach (var m in index.Items) {
                // Basic ID shape
                if (string.IsNullOrEmpty(m.Id) || !Defs.DefId.TryParse(m.Id.Contains('.') ? m.Id : $"{m.Type}.{m.Id}", out _)) {
                    results.Add(new Result { Path = m.Path, Message = $"Id not well-formed: '{m.Id}' (expected modid.{m.Type}.Name or {m.Type}.Name)" });
                    continue; // other checks will be noisy without an id
                }

                // Pick spec: declared version or current known
                SchemaSpec spec = null;
                if (m.SchemaVersion > 0) {
                    SchemaCatalog.TryGet(m.Type, m.SchemaVersion, out spec);
                } else {
                    SchemaCatalog.TryGetCurrent(m.Type, out spec);
                }

                // Version sanity vs current
                var (curOk, curVer) = Defs.Migrations.SchemaRegistry.TryGetCurrentVersion(m.Type);
                if (m.SchemaVersion > 0 && curOk && m.SchemaVersion > curVer) {
                    results.Add(new Result { Path = m.Path, Message = $"Schema version {m.SchemaVersion} ahead of supported {curVer} for {m.Type}" });
                }

                // Read minimal XML into attr/elem maps (root + first-level children)
                var attrs = new Dictionary<string,string>();
                var elems = new Dictionary<string,string>();
                try {
                    using var xr = XmlReader.Create(m.Path, new XmlReaderSettings { IgnoreComments = true, IgnoreWhitespace = true, DtdProcessing = DtdProcessing.Ignore });
                    xr.MoveToContent();
                    if (xr.HasAttributes) {
                        while (xr.MoveToNextAttribute()) attrs[xr.Name] = xr.Value;
                        xr.MoveToElement();
                    }
                    if (!xr.IsEmptyElement) {
                        xr.ReadStartElement();
                        int depth0 = xr.Depth;
                        while (!xr.EOF && xr.Depth <= depth0 + 1) {
                            if (xr.NodeType == XmlNodeType.Element) {
                                var name = xr.Name;
                                string val = string.Empty;
                                if (!xr.IsEmptyElement) val = xr.ReadElementContentAsString(); else xr.Read();
                                if (!elems.ContainsKey(name)) elems[name] = val;
                                continue;
                            }
                            xr.Read();
                        }
                    }
                } catch { /* per-file parsing issues are tolerated in lenient mode */ }

                if (spec == null) {
                    // No spec found; warn once for this file, but don't block
                    results.Add(new Result { Path = m.Path, Message = $"No schema spec found for {m.Type}@{(m.SchemaVersion>0?m.SchemaVersion:0)}; skipping spec checks" });
                    continue;
                }

                // Required fields present
                if (spec.required != null && spec.required.Length > 0) {
                    for (int i = 0; i < spec.required.Length; i++) {
                        var reqName = spec.required[i];
                        var fs = FindField(spec, reqName);
                        bool present = false;
                        if (fs != null && fs.kind == "attr") present = attrs.ContainsKey(reqName);
                        else if (fs != null && fs.kind == "elem") present = elems.ContainsKey(reqName);
                        else
                            present = attrs.ContainsKey(reqName) || elems.ContainsKey(reqName);
                        if (!present) results.Add(new Result { Path = m.Path, Message = $"Missing required field '{reqName}'" });
                    }
                }

                // Field type checks where present
                if (spec.fields != null) {
                    for (int i = 0; i < spec.fields.Count; i++) {
                        var fs = spec.fields[i];
                        string v = null; bool has = false;
                        if (fs.kind == "attr") { has = attrs.TryGetValue(fs.name, out v); }
                        else { has = elems.TryGetValue(fs.name, out v); }
                        if (!has) continue; // not present; required handling above
                        var (ok, err) = TypeChecks.Check(fs, v, index);
                        if (!ok) results.Add(new Result { Path = m.Path, Message = $"Field '{fs.name}': {err}" });
                    }
                }
            }

            return results;
        }

        private static FieldSpec FindField(SchemaSpec spec, string name) {
            if (spec.fields == null) return null;
            for (int i=0;i<spec.fields.Count;i++) if (spec.fields[i].name == name) return spec.fields[i];
            return null;
        }
    }
}
