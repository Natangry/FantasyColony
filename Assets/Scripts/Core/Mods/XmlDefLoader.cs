using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEngine;

namespace Core.Mods
{
    public class DefError
    {
        public string path;
        public string message;
        public override string ToString() => $"{path}: {message}";
    }

    /// <summary>
    /// Lenient XML loader: scans *.xml under each mod and registers their root elements by name+id.
    /// </summary>
    public static class XmlDefLoader
    {
        public static void Load(List<ModInfo> mods, Core.Services.DefRegistry registry, List<DefError> errors)
        {
            if (mods == null || registry == null) return;
            foreach (var mod in mods)
            {
                var defDir = Path.Combine(mod.path, "Defs");
                if (!Directory.Exists(defDir)) continue;
                foreach (var file in Directory.GetFiles(defDir, "*.xml", SearchOption.AllDirectories))
                {
                    try
                    {
                        var doc = XDocument.Load(file);
                        var root = doc.Root;
                        if (root == null) continue;
                        var id = (string)root.Attribute("id") ?? Path.GetFileNameWithoutExtension(file);
                        var type = root.Name.LocalName;
                        registry.Add(type, id, file);
                    }
                    catch (Exception e)
                    {
                        errors?.Add(new DefError { path = file, message = e.Message });
                    }
                }
            }
        }
    }
}
