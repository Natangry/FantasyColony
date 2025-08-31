using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FantasyColony.Core.Mods
{
    public class ModInfo
    {
        public string id;
        public string path;
        public override string ToString() => id;
    }

    public static class ModDiscovery
    {
        /// <summary>
        /// Discover mods in StreamingAssets/Mods and persistentDataPath/Mods. Non-fatal on errors.
        /// </summary>
        public static List<ModInfo> Discover()
        {
            var list = new List<ModInfo>();
            TryDiscover(Path.Combine(Application.streamingAssetsPath, "Mods"), list);
            TryDiscover(Path.Combine(Application.persistentDataPath, "Mods"), list);
            return list;
        }

        private static void TryDiscover(string root, List<ModInfo> into)
        {
            try
            {
                if (string.IsNullOrEmpty(root) || !Directory.Exists(root)) return;
                foreach (var dir in Directory.GetDirectories(root))
                {
                    var id = Path.GetFileName(dir);
                    into.Add(new ModInfo { id = id, path = dir });
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Mod discovery error at {root}: {e.Message}");
            }
        }
    }
}
