using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using XmlDefsTools.Scan;
using XmlDefsTools.Util;

namespace XmlDefsTools.Emit
{
    /// <summary>
    /// Writes a consolidated catalog file listing all schemas,
    /// their union of attributes and elements, and a canonical template
    /// sample for each schema.
    /// </summary>
    public static class SchemaCatalogWriter
    {
        public static void Write(string outputDir, ScanResult scan,
            IReadOnlyDictionary<string, IList<string>> orderHints)
        {
            Directory.CreateDirectory(outputDir);
            var outPath = Path.Combine(outputDir, "_AllSchemas.xml");

            var configuredSchemas = orderHints.Keys
                .Where(k => !string.Equals(k, "_common", StringComparison.OrdinalIgnoreCase)
                    && !k.StartsWith("//"))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var allSchemas = new HashSet<string>(scan.Schemas, StringComparer.OrdinalIgnoreCase);
            foreach (var s in configuredSchemas) allSchemas.Add(s);

            var doc = new XDocument(
                new XElement("DefSchemas",
                    new XAttribute("generated",
                        DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")))
            );

            var common = orderHints.TryGetValue("_common", out var commonOrder)
                ? commonOrder
                : Array.Empty<string>();

            foreach (var schema in allSchemas.OrderBy(s => s, StringComparer.OrdinalIgnoreCase))
            {
                var perSchema = orderHints.TryGetValue(schema, out var per)
                    ? per : Array.Empty<string>();
                var precedence = common.Concat(perSchema).ToList();

                // Collect union of fields from discovered defs
                var attrFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var elemFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (scan.DefsBySchema.TryGetValue(schema, out var defs))
                {
                    foreach (var d in defs)
                    {
                        foreach (var a in d.AttributeFields) attrFields.Add(a);
                        foreach (var e in d.ElementFields) elemFields.Add(e);
                    }
                }

                // Seed with configured fields
                foreach (var hinted in perSchema) elemFields.Add(hinted);

                // Attributes for root + ordered element list
                var rootAttrs = NormalizeOrder(attrFields, precedence,
                    keepOnly: new[] { "id", "schema", "name_key", "tags", "version", "requires" });
                var elementNames = NormalizeOrder(
                    elemFields.Except(rootAttrs, StringComparer.OrdinalIgnoreCase), precedence);

                // Build a canonical template element (same rules as TemplateSynthesizer)
                var root = new XElement(schema);
                foreach (var a in rootAttrs)
                {
                    var placeholder = a.Equals("id", StringComparison.OrdinalIgnoreCase) ? "your_id_here"
                        : a.Equals("name_key", StringComparison.OrdinalIgnoreCase) ? "ui.your.key.here"
                        : a.Equals("tags", StringComparison.OrdinalIgnoreCase) ? string.Empty
                        : a.Equals("version", StringComparison.OrdinalIgnoreCase) ? "1"
                        : string.Empty;
                    root.SetAttributeValue(a, placeholder);
                }

                bool HasElem(string name) =>
                    elementNames.Any(n => n.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (HasElem("version") && root.Attribute("version") == null)
                    root.Add(new XElement("version", "1"));
                if (HasElem("requires") && root.Attribute("requires") == null)
                    root.Add(new XElement("requires"));
                if (HasElem("components") ||
                    perSchema.Contains("components", StringComparer.OrdinalIgnoreCase))
                {
                    root.Add(new XElement("components",
                        new XComment(" Add component entries like <Component type=\"...\"/> ")));
                }
                foreach (var el in elementNames)
                {
                    if (string.Equals(el, "components", StringComparison.OrdinalIgnoreCase))
                        continue;
                    if (string.Equals(el, "version", StringComparison.OrdinalIgnoreCase)
                        && root.Elements("version").Any())
                        continue;
                    if (string.Equals(el, "requires", StringComparison.OrdinalIgnoreCase)
                        && root.Elements("requires").Any())
                        continue;
                    root.Add(new XElement(el));
                }
                var canonicalElement = CanonicalXml.CanonicalizeElement(root);

                // Compose catalog node
                var schemaNode = new XElement("Schema",
                    new XAttribute("name", schema),
                    new XElement("Attributes",
                        rootAttrs.Select(a => new XElement("Attr", new XAttribute("name", a)))),
                    new XElement("Elements",
                        elementNames.Select(e => new XElement("El", new XAttribute("name", e)))),
                    new XElement("Template", canonicalElement));

                doc.Root!.Add(schemaNode);
            }

            doc.Save(outPath);
        }

        private static List<string> NormalizeOrder(IEnumerable<string> names,
            IList<string> precedence, IEnumerable<string>? keepOnly = null)
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

