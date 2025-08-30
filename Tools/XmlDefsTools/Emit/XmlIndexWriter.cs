using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using XmlDefsTools.Scan;

namespace XmlDefsTools.Emit
{
    public static class XmlIndexWriter
    {
        public static void Write(string repoRoot, string outFile, ScanResult scan)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# XML Index (.xml)");
            sb.AppendLine();
            sb.AppendLine($"_Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC_");
            sb.AppendLine();
            sb.AppendLine($"**Total files scanned:** {scan.TotalFiles}  ");
            sb.AppendLine($"**Valid XML files:** {scan.ValidFiles}  ");
            sb.AppendLine($"**Defs discovered:** {scan.TotalDefs}  ");
            sb.AppendLine();

            if (scan.Errors.Any())
            {
                sb.AppendLine("## Parse Errors");
                foreach (var err in scan.Errors)
                {
                    sb.AppendLine($"- `{Rel(repoRoot, err.File)}` — {err.Message}");
                }
                sb.AppendLine();
            }

            if (scan.DuplicateIds.Any())
            {
                sb.AppendLine("## Duplicate IDs");
                foreach (var kvp in scan.DuplicateIds.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
                {
                    sb.AppendLine($"- **{kvp.Key}**");
                    foreach (var item in kvp.Value)
                    {
                        sb.AppendLine($"  - `{item.Schema}` in `{Rel(repoRoot, item.SourceFile)}`");
                    }
                }
                sb.AppendLine();
            }

            // Match Code Index style: bulleted list of files with View · Raw links
            var (repo, branch) = RepoMeta();
            var allXmlFiles = scan.FileDocs.Keys.OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToList();
            if (allXmlFiles.Count > 0)
            {
                foreach (var file in allXmlFiles)
                {
                    var rel = Rel(repoRoot, file);
                    var webPath = rel.Replace('\\','/'); // ensure URL form
                    var enc = EncodePathSegments(webPath);
                    var viewUrl = $"https://github.com/{repo}/blob/{branch}/{enc}";
                    var rawUrl  = $"https://raw.githubusercontent.com/{repo}/{branch}/{enc}";
                    sb.AppendLine($"- `{webPath}` — [View]({viewUrl}) · [Raw]({rawUrl})");
                }
                sb.AppendLine();
            }

            // Keep the schema summary (below the list) for quick counts
            sb.AppendLine("## Summary by Schema");
            sb.AppendLine();
            sb.AppendLine("| Schema | Def Count | Files |");
            sb.AppendLine("|---|---:|---|");
            foreach (var schema in scan.Schemas.OrderBy(s => s))
            {
                var defs = scan.DefsBySchema[schema];
                var files = defs.Select(d => Rel(repoRoot, d.SourceFile)).Distinct().OrderBy(s => s);
                sb.AppendLine($"| {schema} | {defs.Count} | {string.Join("<br/>", files)} |");
            }
            sb.AppendLine();

            // Per-schema sections unchanged below (list ids + rare fields)

            foreach (var schema in scan.Schemas.OrderBy(s => s))
            {
                var defs = scan.DefsBySchema[schema];
                sb.AppendLine($"### {schema}");
                sb.AppendLine();
                foreach (var d in defs.OrderBy(d => d.Id, StringComparer.OrdinalIgnoreCase))
                {
                    sb.AppendLine($"- `{d.Id}` — `{Rel(repoRoot, d.SourceFile)}`");
                }
                // Any newly observed/rare fields
                var fieldCounts = new Dictionary<string,int>(StringComparer.OrdinalIgnoreCase);
                foreach (var d in defs)
                {
                    foreach (var f in d.AttributeFields.Concat(d.ElementFields))
                        fieldCounts[f] = fieldCounts.TryGetValue(f, out var c) ? c + 1 : 1;
                }
                var rare = fieldCounts.Where(kvp => kvp.Value <= Math.Max(1, defs.Count/5)).Select(kvp => kvp.Key).OrderBy(s=>s, StringComparer.OrdinalIgnoreCase).ToList();
                if (rare.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("<details><summary>Less common fields</summary>");
                    sb.AppendLine();
                    foreach (var f in rare) sb.AppendLine($"- `{f}`");
                    sb.AppendLine();
                    sb.AppendLine("</details>");
                }
                sb.AppendLine();
            }

            Directory.CreateDirectory(Path.GetDirectoryName(outFile)!);
            File.WriteAllText(outFile, sb.ToString());
        }

        // Encode each path segment to avoid corrupting '/' and handle special characters safely.
        private static string EncodePathSegments(string path)
        {
            var parts = path.Replace('\\','/').Split(new[]{'/'}, StringSplitOptions.RemoveEmptyEntries);
            var encoded = parts.Select(Uri.EscapeDataString);
            return string.Join("/", encoded);
        }

        private static (string repo, string branch) RepoMeta()
        {
            var repo = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY");
            if (string.IsNullOrWhiteSpace(repo)) repo = "Natangry/FantasyColony";
            var branch = Environment.GetEnvironmentVariable("GITHUB_REF_NAME");
            if (string.IsNullOrWhiteSpace(branch)) branch = "main";
            // strip refs/heads/ if present
            branch = branch.Replace("refs/heads/", "");
            return (repo, branch);
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
