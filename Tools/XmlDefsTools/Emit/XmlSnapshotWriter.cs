using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using XmlDefsTools.Util;

namespace XmlDefsTools.Emit
{
    public static class XmlSnapshotWriter
    {
        public static void Write(string repoRoot, string outFile, IEnumerable<(string path, XDocument? doc)> docs)
        {
            var sb = new StringBuilder();
            foreach (var (path, doc) in docs.OrderBy(t => t.path, StringComparer.OrdinalIgnoreCase))
            {
                sb.AppendLine($"// ===== FILE: {Rel(repoRoot, path)} =====");
                if (doc == null)
                {
                    sb.AppendLine("// (invalid XML)");
                    sb.AppendLine();
                    continue;
                }
                var normalized = CanonicalXml.Canonicalize(doc);
                sb.AppendLine(normalized);
                sb.AppendLine();
            }
            File.WriteAllText(outFile, sb.ToString());
        }

        private static string Rel(string root, string path)
        {
            try
            {
                var rp = Path.GetRelativePath(root, path).Replace('\\','/');
                return rp;
            }
            catch
            {
                return path.Replace('\\','/');
            }
        }
    }
}
