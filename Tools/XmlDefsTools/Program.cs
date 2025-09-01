using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using XmlDefsTools.Emit;
using XmlDefsTools.Scan;

namespace XmlDefsTools
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            try
            {
                var repoRoot = GetRepoRoot();
                var defsDir = GetDefsDir(repoRoot);
                Console.WriteLine($"[XmlDefsTools] Repo root: {repoRoot}");
                Console.WriteLine($"[XmlDefsTools] Defs dir : {defsDir}");

                var scanner = new XmlDefScanner();
                var scan = scanner.Scan(defsDir);

                // Load order hints
                var hintsPath = Path.Combine(repoRoot, "Tools", "XmlDefsTools", "Config", "SchemaOrder.json");
                var orderHints = File.Exists(hintsPath)
                    ? JsonSerializer.Deserialize<Dictionary<string, IList<string>>>(File.ReadAllText(hintsPath))
                    : new Dictionary<string, IList<string>>(StringComparer.OrdinalIgnoreCase);
                orderHints ??= new Dictionary<string, IList<string>>(StringComparer.OrdinalIgnoreCase);

                // Write index
                var indexPath = Path.Combine(repoRoot, "XML_INDEX.md");
                XmlIndexWriter.Write(repoRoot, indexPath, scan);
                Console.WriteLine($"[XmlDefsTools] Wrote index: {indexPath}");

                // Write snapshot
                var snapshotPath = Path.Combine(repoRoot, "XML_SNAPSHOT.txt");
                var docs = scan.FileDocs.Select(kvp => (kvp.Key, kvp.Value));
                XmlSnapshotWriter.Write(repoRoot, snapshotPath, docs);
                Console.WriteLine($"[XmlDefsTools] Wrote snapshot: {snapshotPath}");

                // Write templates
                var templatesDir = Path.Combine(repoRoot, "Docs", "Templates", "Defs");
                TemplateSynthesizer.WriteTemplates(templatesDir, scan, orderHints);
                Console.WriteLine($"[XmlDefsTools] Wrote templates to: {templatesDir}");

                // Write consolidated schema catalog
                SchemaCatalogWriter.Write(templatesDir, scan, orderHints);
                Console.WriteLine($"[XmlDefsTools] Wrote consolidated catalog: {Path.Combine(templatesDir, "_AllSchemas.xml")}");

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                return 1;
            }
        }

        private static string GetRepoRoot()
        {
            var env = Environment.GetEnvironmentVariable("FC_REPO_ROOT");
            if (!string.IsNullOrWhiteSpace(env))
                return Path.GetFullPath(env);
            // assume current working directory is repo root (CI) or a subfolder (local)
            var cwd = Directory.GetCurrentDirectory();
            // try to find .git upwards
            var dir = new DirectoryInfo(cwd);
            while (dir != null)
            {
                if (Directory.Exists(Path.Combine(dir.FullName, ".git")))
                    return dir.FullName;
                dir = dir.Parent;
            }
            return cwd;
        }

        private static string GetDefsDir(string repoRoot)
        {
            var env = Environment.GetEnvironmentVariable("FC_DEFS_DIR");
            if (!string.IsNullOrWhiteSpace(env))
            {
                var p = Path.IsPathRooted(env) ? env : Path.Combine(repoRoot, env);
                return Directory.Exists(p) ? p : repoRoot;
            }
            var candidates = new[]
            {
                Path.Combine(repoRoot, "StreamingAssets", "Defs"),
                Path.Combine(repoRoot, "Assets", "StreamingAssets", "Defs"),
                Path.Combine(repoRoot, "GameData", "Defs")
            };
            foreach (var c in candidates)
                if (Directory.Exists(c)) return c;
            // fallback: repo root (will scan, find no files, still emit artifacts)
            return repoRoot;
        }
    }
}
