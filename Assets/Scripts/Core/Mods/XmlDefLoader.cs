using System;
using System.Collections.Generic;
using UnityEngine;
using FantasyColony.Core.Services;

namespace FantasyColony.Core.Mods {
    public class DefError {
        public string Path;
        public string Message;
        public override string ToString() => $"{Path}: {Message}";
    }

    /// <summary>
    /// Lenient XML loader: scans *.xml under each mod and registers their root elements by name+id.
    /// </summary>
    public static class XmlDefLoader {
        // Existing signature used by boot pipeline
        public static void Load(List<ModInfo> mods, DefRegistry registry, List<DefError> errors) {
            // current: iterate XML files under each mod's Defs folder, register Type/Id → path
            // note: per-file schema metadata is now collected later by DefIndex for validation/migrations
            foreach (var mod in mods) {
                var dir = System.IO.Path.Combine(mod.path, "Defs");
                if (!System.IO.Directory.Exists(dir)) continue;
                foreach (var file in System.IO.Directory.EnumerateFiles(dir, "*.xml", System.IO.SearchOption.AllDirectories)) {
                    try {
                        using var xr = System.Xml.XmlReader.Create(file, new System.Xml.XmlReaderSettings { IgnoreComments = true, IgnoreWhitespace = true, DtdProcessing = System.Xml.DtdProcessing.Ignore });
                        xr.MoveToContent();
                        var type = xr.Name; // root element name is type
                        var id = xr.GetAttribute("id") ?? string.Empty;
                        if (string.IsNullOrEmpty(id)) {
                            errors?.Add(new DefError { Path = file, Message = $"Missing id for type '{type}'" });
                            continue;
                        }
                        registry.Register(type, id, file, mod.id);
                    } catch (Exception e) {
                        errors?.Add(new DefError { Path = file, Message = e.Message });
                    }
                }
            }
        }
    }
}
