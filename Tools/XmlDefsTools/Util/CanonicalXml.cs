using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace XmlDefsTools.Util
{
    public static class CanonicalXml
    {
        private static readonly string[] AttributePrecedence =
        {
            "id","schema","name_key","tags","version","requires"
        };

        public static string Canonicalize(XDocument doc)
        {
            // Remove comments
            foreach (var c in doc.DescendantNodes().OfType<XComment>().ToList())
                c.Remove();

            // Canonicalize elements & attributes
            var root = CanonicalizeElement(doc.Root!);
            var newDoc = new XDocument(root);

            // Save with stable formatting
            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Indent = true,
                NewLineOnAttributes = false
            };
            var sb = new StringBuilder();
            using (var writer = XmlWriter.Create(sb, settings))
            {
                newDoc.Save(writer);
            }
            return sb.ToString().Trim() + Environment.NewLine;
        }

        public static XElement CanonicalizeElement(XElement el)
        {
            var orderedAttrs = el.Attributes()
                .Where(a => !a.IsNamespaceDeclaration)
                .OrderBy(a => OrderKey(a.Name.LocalName), StringComparer.OrdinalIgnoreCase)
                .ThenBy(a => a.Name.LocalName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var newEl = new XElement(el.Name);
            foreach (var a in orderedAttrs)
                newEl.SetAttributeValue(a.Name.LocalName, a.Value);

            foreach (var node in el.Nodes())
            {
                if (node is XElement child)
                {
                    newEl.Add(CanonicalizeElement(child));
                }
                else if (node is XText t)
                {
                    var v = t.Value;
                    if (!string.IsNullOrWhiteSpace(v))
                        newEl.Add(new XText(NormalizeWhitespace(v)));
                }
                // other node types (comments, processing instructions) are skipped
            }
            return newEl;
        }

        private static int OrderKey(string name)
        {
            for (int i = 0; i < AttributePrecedence.Length; i++)
                if (name.Equals(AttributePrecedence[i], StringComparison.OrdinalIgnoreCase))
                    return i - 1000; // bubble to front
            return 0;
        }

        private static string NormalizeWhitespace(string s)
        {
            // Collapse internal whitespace sequences
            var arr = s.ToCharArray();
            var sb = new StringBuilder(arr.Length);
            bool inWs = false;
            foreach (var ch in arr)
            {
                if (char.IsWhiteSpace(ch))
                {
                    if (!inWs)
                    {
                        sb.Append(' ');
                        inWs = true;
                    }
                }
                else
                {
                    sb.Append(ch);
                    inWs = false;
                }
            }
            return sb.ToString().Trim();
        }
    }
}
