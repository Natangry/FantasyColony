using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace XmlDefsTools.Scan
{
    public sealed class XmlDefScanner
    {
        public ScanResult Scan(string rootDir)
        {
            var res = new ScanResult();
            if (!Directory.Exists(rootDir))
                return res;

            var files = Directory.EnumerateFiles(rootDir, "*.xml", SearchOption.AllDirectories).ToList();
            res.TotalFiles = files.Count;
            foreach (var file in files)
            {
                XDocument? doc = null;
                try
                {
                    var text = File.ReadAllText(file);
                    doc = XDocument.Parse(text, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
                    res.ValidFiles++;
                    res.FileDocs[file] = doc;
                }
                catch (Exception ex)
                {
                    res.Errors.Add(new ScanError { File = file, Message = ex.GetBaseException().Message });
                    res.FileDocs[file] = null;
                    continue;
                }

                // Collect defs: any element with an 'id' attribute
                var defs = doc.Descendants()
                    .Where(e => e.NodeType == System.Xml.XmlNodeType.Element)
                    .OfType<XElement>()
                    .Where(e => e.Attribute("id") != null)
                    .ToList();

                foreach (var el in defs)
                {
                    var schema = el.Name.LocalName;
                    var id = (string?)el.Attribute("id") ?? "(missing)";
                    var info = new DefInfo
                    {
                        Schema = schema,
                        Id = id,
                        SourceFile = file
                    };

                    // Attribute fields (names only)
                    foreach (var a in el.Attributes())
                    {
                        if (a.IsNamespaceDeclaration) continue;
                        info.AttributeFields.Add(a.Name.LocalName);
                    }

                    // Element fields (direct child element names only)
                    foreach (var child in el.Elements())
                    {
                        info.ElementFields.Add(child.Name.LocalName);
                    }

                    res.TotalDefs++;
                    if (!res.DefsBySchema.TryGetValue(schema, out var list))
                    {
                        list = new List<DefInfo>();
                        res.DefsBySchema[schema] = list;
                        res.Schemas.Add(schema);
                    }
                    list.Add(info);

                    // Track duplicates
                    var dupKey = $"{schema}:{id}";
                    if (!res.DuplicateIds.TryGetValue(dupKey, out var dupList))
                    {
                        dupList = new List<DefInfo>();
                        res.DuplicateIds[dupKey] = dupList;
                    }
                    dupList.Add(info);
                }
            }

            // Remove entries that are not actually duplicates (only one occurrence)
            var toPrune = res.DuplicateIds.Where(kvp => kvp.Value.Count <= 1)
                                          .Select(kvp => kvp.Key).ToList();
            foreach (var k in toPrune) res.DuplicateIds.Remove(k);

            return res;
        }
    }

    public sealed class ScanResult
    {
        public int TotalFiles { get; set; }
        public int ValidFiles { get; set; }
        public int TotalDefs { get; set; }
        public HashSet<string> Schemas { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, List<DefInfo>> DefsBySchema { get; } = new Dictionary<string, List<DefInfo>>(StringComparer.OrdinalIgnoreCase);
        public List<ScanError> Errors { get; } = new List<ScanError>();
        public Dictionary<string, XDocument?> FileDocs { get; } = new Dictionary<string, XDocument?>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, List<DefInfo>> DuplicateIds { get; } = new Dictionary<string, List<DefInfo>>(StringComparer.OrdinalIgnoreCase);
    }

    public sealed class DefInfo
    {
        public string Schema { get; set; } = "";
        public string Id { get; set; } = "";
        public string SourceFile { get; set; } = "";
        public HashSet<string> AttributeFields { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> ElementFields { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    public sealed class ScanError
    {
        public string File { get; set; } = "";
        public string Message { get; set; } = "";
    }
}
