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
            v.defName      = Value(root, "defName", required: true);
            v.spritePath   = Value(root, "spritePath", required: true);

            // Sorting & transform
            v.sortingLayer = Value(root, "sortingLayer", defaultValue: "Default");
            v.sortingOrder = Int(root, "sortingOrder", 0);
            v.pivotX       = Float(root, "pivotX", 0.5f);
            v.pivotY       = Float(root, "pivotY", 0.0f);
            v.scale        = Float(root, "scale", 1.0f);

            // Plane & depth
            v.plane        = Value(root, "plane", defaultValue: "XY");
            v.z_lift       = Float(root, "z_lift", 0f);

            // Material & color
            v.shader_hint  = Value(root, "shader_hint", defaultValue: null);
            v.color_rgba   = Value(root, "color_rgba", defaultValue: "1,1,1,1");

            // Variants
            var variantsEl = root.Element("variants");
            if (variantsEl != null)
            {
                v.variants = new List<Visual2DDef.Variant>();
                foreach (var li in variantsEl.Elements("li"))
                {
                    var varEl = new Visual2DDef.Variant
                    {
                        spritePath   = li.Element("spritePath")?.Value?.Trim(),
                        weight       = Int(li, "weight", 1),
                        conditionTag = li.Element("conditionTag")?.Value?.Trim()
                    };
                    if (!string.IsNullOrEmpty(varEl.spritePath))
                        v.variants.Add(varEl);
                }
            }
            return v;
        }

        private static BuildingDef ParseBuilding(XElement root)
        {
            var b = new BuildingDef();
            // Identity & UI
            b.defName       = Value(root, "defName", required: true);
            b.label         = Value(root, "label", defaultValue: null);
            b.description   = Value(root, "description", defaultValue: null);
            b.category      = Value(root, "category", defaultValue: null);
            b.showInPalette = Bool(root, "showInPalette", true);
            b.unique        = Bool(root, "unique", false);

            // Footprint & orientation
            b.width            = Int(root, "width", 1);
            b.height           = Int(root, "height", 1);
            var ar = Value(root, "allowedRotations", defaultValue: null);
            if (!string.IsNullOrEmpty(ar))
                b.allowedRotations = new List<string>(ar.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries));
            b.defaultRotation  = Value(root, "defaultRotation", defaultValue: "N");

            // Placement rules
            b.plane = Value(root, "plane", defaultValue: null);

            // Pathing
            b.canPathThrough  = Bool(root, "canPathThrough", false);
            b.avoidanceRadius = Float(root, "avoidanceRadius", 0f);

            // Interactions
            var tagsEl = root.Element("interactTags");
            if (tagsEl != null)
            {
                b.interactTags = new List<string>();
                foreach (var li in tagsEl.Elements("li"))
                {
                    var t = li.Value?.Trim();
                    if (!string.IsNullOrEmpty(t)) b.interactTags.Add(t);
                }
            }

            // Costs/work
            b.workToBuild = Float(root, "workToBuild", 0f);

            // Visual link (camelCase and snake_case)
            b.visualRef = Value(root, "visualRef", defaultValue: null);
            if (string.IsNullOrEmpty(b.visualRef))
                b.visualRef = Value(root, "visual_ref", defaultValue: null);
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

