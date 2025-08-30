using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;

namespace FantasyColony.Defs.Xml
{
    /// <summary>
    /// Minimal XML loader for v0 bring-up. One def per file, root element name is the type.
    /// </summary>
    public static class XmlDefLoader
    {
        public static void LoadAll(List<Visual2DDef> visuals, List<BuildingDef> buildings)
        {
            foreach (var file in Mods.ModDiscovery.EnumerateDefXmlFiles())
            {
                try
                {
                    string modId = new DirectoryInfo(Directory.GetParent(Directory.GetParent(file).FullName).FullName).Name; // Mods/<modId>/Defs/<...>/<file>
                    using (var reader = CreateSafeXmlReader(file))
                    {
                        var doc = XDocument.Load(reader, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
                        var root = doc.Root;
                        if (root == null)
                            continue;

                        switch (root.Name.LocalName)
                        {
                            case Visual2DDefKinds.Root:
                                var v = ParseVisual2D(root);
                                v.modId = modId;
                                visuals.Add(v);
                                break;
                            // Back-compat: allow <VisualDef> as an alias for <Visual2DDef>
                            case "VisualDef":
                                var v2 = ParseVisual2D(root);
                                v2.modId = modId;
                                visuals.Add(v2);
                                break;
                            case BuildingDefKinds.Root:
                                var b = ParseBuilding(root);
                                b.modId = modId;
                                buildings.Add(b);
                                break;
                            default:
                                Debug.LogWarning($"[Defs] Unknown root '{root.Name.LocalName}' in {Short(file)} – skipping.");
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[Defs] Failed to parse '{Short(file)}': {ex.Message}");
                }
            }
        }

        private static XmlReader CreateSafeXmlReader(string path)
        {
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                IgnoreComments = true,
                IgnoreProcessingInstructions = true
            };
            return XmlReader.Create(path, settings);
        }

        private static string Short(string path)
        {
            return path.Replace(Application.dataPath, "Assets");
        }

        private static Visual2DDef ParseVisual2D(XElement root)
        {
            var v = new Visual2DDef();
            v.defName = Value(root, nameof(Visual2DDef.defName), required: true);
            v.spritePath = Value(root, nameof(Visual2DDef.spritePath), required: true);
            v.sortingLayer = Value(root, nameof(Visual2DDef.sortingLayer), defaultValue: "Default");
            v.sortingOrder = Int(root, nameof(Visual2DDef.sortingOrder), 0);
            v.pivotX = Float(root, nameof(Visual2DDef.pivotX), 0.5f);
            v.pivotY = Float(root, nameof(Visual2DDef.pivotY), 0.0f);
            return v;
        }

        private static BuildingDef ParseBuilding(XElement root)
        {
            var b = new BuildingDef();
            b.defName = Value(root, nameof(BuildingDef.defName), required: true);
            b.width = Int(root, nameof(BuildingDef.width), 1);
            b.height = Int(root, nameof(BuildingDef.height), 1);
            b.unique = Bool(root, nameof(BuildingDef.unique), false);
            b.showInPalette = Bool(root, nameof(BuildingDef.showInPalette), true);
            b.visualRef = Value(root, nameof(BuildingDef.visualRef), required: false);
            return b;
        }

        private static string Value(XElement root, string name, string defaultValue = null, bool required = false)
        {
            var e = root.Element(name);
            if (e == null)
            {
                if (required)
                    throw new Exception($"Missing <{name}> element.");
                return defaultValue;
            }
            return e.Value?.Trim();
        }

        private static int Int(XElement root, string name, int defaultValue)
        {
            var s = Value(root, name, defaultValue.ToString(CultureInfo.InvariantCulture), required: false);
            return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : defaultValue;
        }

        private static float Float(XElement root, string name, float defaultValue)
        {
            var s = Value(root, name, defaultValue.ToString(CultureInfo.InvariantCulture), required: false);
            return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : defaultValue;
        }

        private static bool Bool(XElement root, string name, bool defaultValue)
        {
            var s = Value(root, name, defaultValue ? "true" : "false", required: false);
            if (string.Equals(s, "true", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(s, "false", StringComparison.OrdinalIgnoreCase)) return false;
            return defaultValue;
        }
    }
}

