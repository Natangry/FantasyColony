using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace FantasyColony.Mods
{
    /// <summary>
    /// Finds Mods and their Defs directories under StreamingAssets.
    /// Load order: Core -> (other mods alphabetically)
    /// </summary>
    public static class ModDiscovery
    {
        public static IEnumerable<string> EnumerateDefXmlFiles()
        {
            var modsRoot = Path.Combine(Application.streamingAssetsPath, "Mods");
            if (!Directory.Exists(modsRoot))
                yield break;

            // Core first if present
            var core = Path.Combine(modsRoot, "Core");
            if (Directory.Exists(core))
            {
                foreach (var f in EnumerateDefFiles(core))
                    yield return f;
            }

            // Then other mods alphabetically
            foreach (var modDir in Directory.GetDirectories(modsRoot).OrderBy(p => p))
            {
                if (Path.GetFileName(modDir) == "Core")
                    continue;
                foreach (var f in EnumerateDefFiles(modDir))
                    yield return f;
            }
        }

        private static IEnumerable<string> EnumerateDefFiles(string modDir)
        {
            var defsDir = Path.Combine(modDir, "Defs");
            if (!Directory.Exists(defsDir))
                yield break;
            foreach (var f in Directory.GetFiles(defsDir, "*.xml", SearchOption.AllDirectories))
                yield return f;
        }
    }
}

