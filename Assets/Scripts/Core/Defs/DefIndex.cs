using System.Collections.Generic;
using System.Xml;
using FantasyColony.Core.Mods;
using FantasyColony.Core.Services;

namespace FantasyColony.Core.Defs {
    /// <summary>
    /// Builds a read-only index of defs from the registry + quick XML header reads.
    /// </summary>
    public sealed class DefIndex {
        public readonly List<DefMeta> Items = new();
        private readonly Dictionary<string, List<DefMeta>> _byType = new();
        private readonly Dictionary<string, DefMeta> _byKey = new(); // Type.Id (no mod prefix)

        public IEnumerable<DefMeta> OfType(string type) => _byType.TryGetValue(type, out var list) ? list : System.Array.Empty<DefMeta>();
        public DefMeta Find(string type, string id) => _byKey.TryGetValue(type + "." + id, out var m) ? m : null;

        public static DefIndex Build(List<ModInfo> mods, DefRegistry registry) {
            var index = new DefIndex();
            foreach (var entry in registry.All()) {
                // entry: (Type, Id, Path, ModId)
                var meta = new DefMeta {
                    Type = entry.Type,
                    Id = entry.Id,
                    Path = entry.Path,
                    ModId = entry.ModId,
                    Schema = null,
                    SchemaVersion = 0
                };

                // Quick read root attrs for schema/schema_version without loading entire doc
                try {
                    using var reader = XmlReader.Create(entry.Path, new XmlReaderSettings { IgnoreComments = true, IgnoreWhitespace = true, DtdProcessing = DtdProcessing.Ignore });
                    reader.MoveToContent();
                    if (reader.HasAttributes) {
                        var schemaAttr = reader.GetAttribute("schema");
                        if (!string.IsNullOrEmpty(schemaAttr)) meta.Schema = schemaAttr;
                        var svAttr = reader.GetAttribute("schema_version");
                        if (!string.IsNullOrEmpty(svAttr) && int.TryParse(svAttr, out var sv)) meta.SchemaVersion = sv;
                        else if (!string.IsNullOrEmpty(meta.Schema)) {
                            var at = meta.Schema.LastIndexOf('@');
                            if (at >= 0 && int.TryParse(meta.Schema.Substring(at + 1), out var sv2)) meta.SchemaVersion = sv2;
                        }
                    }
                } catch { /* ignore per-file errors here; validator will surface issues */ }

                index.Items.Add(meta);
                if (!index._byType.TryGetValue(meta.Type, out var list)) index._byType[meta.Type] = list = new List<DefMeta>(4);
                list.Add(meta);
                index._byKey[meta.Type + "." + meta.Id] = meta;
            }
            return index;
        }
    }
}
