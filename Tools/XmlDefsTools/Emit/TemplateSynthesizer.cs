using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using XmlDefsTools.Scan;
using XmlDefsTools.Util;

namespace XmlDefsTools.Emit
{
    public static class TemplateSynthesizer
    {
        public static void WriteTemplates(string outputDir, ScanResult scan, IReadOnlyDictionary<string, IList<string>> orderHints)
        {
            Directory.CreateDirectory(outputDir);
            foreach (var schema in scan.Schemas.OrderBy(s => s))
            {
                var defs = scan.DefsBySchema[schema];

                // union of observed fields (attributes + elements)
                var attrFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var elemFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var d in defs)
                {
                    foreach (var f in d.AttributeFields) attrFields.Add(f);
                    foreach (var f in d.ElementFields) elemFields.Add(f);
                }

                // known/common order
                var common = orderHints.TryGetValue("_common", out var c) ? c : Array.Empty<string>();
                var perSchema = orderHints.TryGetValue(schema, out var s) ? s : Array.Empty<string>();
                var precedence = common.Concat(perSchema).ToList();

                // Attributes to place on root element
                var rootAttrs = NormalizeOrder(attrFields, precedence, keepOnly: new[] { "id","schema","name_key","tags","version","requires" });
                // Remaining element fields
                var elementNames = NormalizeOrder(elemFields, precedence);

                var sb = new StringBuilder();
                sb.AppendLine($"<!-- Auto-generated default template for {schema}. Edit your copies; this file is regenerated. -->");

                var root = new XElement(schema);
                // Put attributes with placeholders
                foreach (var a in rootAttrs)
                {
                    var placeholder = a.Equals("id", StringComparison.OrdinalIgnoreCase) ? "your_id_here"
                                   : a.Equals("name_key", StringComparison.OrdinalIgnoreCase) ? "ui.your.key.here"
                                   : a.Equals("tags", StringComparison.OrdinalIgnoreCase) ? ""
                                   : a.Equals("version", StringComparison.OrdinalIgnoreCase) ? "1"
                                   : "";
                    root.SetAttributeValue(a, placeholder);
                }

                // Insert version/requires as elements too if heavily used as elements
                if (elementNames.Contains("version", StringComparer.OrdinalIgnoreCase) && root.Attribute("version") == null)
                    root.Add(new XElement("version", "1"));
                if (elementNames.Contains("requires", StringComparer.OrdinalIgnoreCase) && root.Attribute("requires") == null)
                    root.Add(new XElement("requires"));

                // Components block if observed
                if (elementNames.Contains("components", StringComparer.OrdinalIgnoreCase))
                {
                    root.Add(new XElement("components",
                        new XComment(" Add component entries like <Component type=\"...\"/> ")));
                }

                // Other fields as empty elements
                foreach (var el in elementNames)
                {
                    if (string.Equals(el, "components", StringComparison.OrdinalIgnoreCase)) continue;
                    if (string.Equals(el, "version", StringComparison.OrdinalIgnoreCase) && root.Elements("version").Any()) continue;
                    if (string.Equals(el, "requires", StringComparison.OrdinalIgnoreCase) && root.Elements("requires").Any()) continue;
                    root.Add(new XElement(el));
                }

                // Canonicalize attribute order
                var canonical = CanonicalXml.CanonicalizeElement(root);
                sb.AppendLine(canonical.ToString(SaveOptions.None));

                var outPath = Path.Combine(outputDir, $"{schema}.xml");
                File.WriteAllText(outPath, sb.ToString());
            }
        }

        private static List<string> NormalizeOrder(IEnumerable<string> names, IList<string> precedence, IEnumerable<string>? keepOnly = null)
        {
            var all = new HashSet<string>(names, StringComparer.OrdinalIgnoreCase);
            if (keepOnly != null)
            {
                var keep = new HashSet<string>(keepOnly, StringComparer.OrdinalIgnoreCase);
                all.RemoveWhere(n => !keep.Contains(n));
            }
            var ordered = new List<string>();
            foreach (var p in precedence)
            {
                if (all.Remove(p)) ordered.Add(p);
            }
            ordered.AddRange(all.OrderBy(n => n, StringComparer.OrdinalIgnoreCase));
            return ordered;
        }
    }
}
