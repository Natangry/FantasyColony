using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace FantasyColony.Defs
{
    [XmlRoot("Defs")] public class BuildingDefSet { [XmlElement("Building")] public List<BuildingDef> Items = new(); }
    [XmlRoot("Defs")] public class VisualDefSet   { [XmlElement("Visual")]   public List<VisualDef>   Items = new(); }

    [Serializable]
    public class BuildingDef
    {
        [XmlAttribute("id")] public string id;
        [XmlElement("display_name")] public string display_name;
        [XmlElement("size_x")] public int size_x = 1;
        [XmlElement("size_y")] public int size_y = 1;
        [XmlElement("unique")] public bool unique;
        [XmlElement("visual_ref")] public string visual_ref;
        [XmlArray("job_slots"), XmlArrayItem("slot")] public List<JobSlot> job_slots = new();
        [XmlArray("cost"), XmlArrayItem("entry")] public List<CostEntry> cost = new();

        [Serializable] public class JobSlot { [XmlAttribute("job")] public string job; [XmlAttribute("count")] public int count = 1; }
        [Serializable] public class CostEntry { [XmlAttribute("res")] public string resource; [XmlAttribute("amt")] public int amount; }

        public Vector2Int Size => new Vector2Int(Mathf.Max(1,size_x), Mathf.Max(1,size_y));
    }

    [Serializable]
    public class VisualDef
    {
        [XmlAttribute("id")] public string id;
        [XmlElement("plane")] public string plane = "XZ"; // "XY" or "XZ"
        [XmlElement("render_layer")] public string render_layer = "Default"; // name or index
        [XmlElement("color_rgba")] public string color_rgba = "#F3D95AFF"; // 8-digit RGBA hex
        [XmlElement("shader_hint")] public string shader_hint = "URP/Unlit"; // URP/Unlit | Unlit/Color | StandardTransparent
        [XmlElement("z_lift")] public float z_lift = 0.05f;

        public Color Color => ColorUtility.TryParseHtmlString(color_rgba, out var c) ? c : new Color(0.95f,0.85f,0.35f,1f);
        public GridPlane Plane => string.Equals(plane, "XY", StringComparison.OrdinalIgnoreCase) ? GridPlane.XY : GridPlane.XZ;
    }
}

namespace FantasyColony.Defs
{
    public static class DefDatabase
    {
        public static readonly Dictionary<string, BuildingDef> Buildings = new();
        public static readonly Dictionary<string, VisualDef> Visuals = new();

        public static void LoadAll()
        {
            Buildings.Clear();
            Visuals.Clear();
            XmlDefLoader.LoadSet("Buildings", System.IO.Path.Combine(Application.streamingAssetsPath, "Defs/Buildings"), (BuildingDefSet set) =>
            {
                foreach (var b in set.Items) if (!string.IsNullOrEmpty(b.id)) Buildings[b.id] = b;
            });
            XmlDefLoader.LoadSet("Visuals", System.IO.Path.Combine(Application.streamingAssetsPath, "Defs/Visuals"), (VisualDefSet set) =>
            {
                foreach (var v in set.Items) if (!string.IsNullOrEmpty(v.id)) Visuals[v.id] = v;
            });
            Debug.Log($"[Defs] Loaded Buildings={Buildings.Count}, Visuals={Visuals.Count} from {Application.streamingAssetsPath}");
        }
    }
}

namespace FantasyColony.Defs
{
    public static class XmlDefLoader
    {
        public static void LoadSet<T>(string kind, string folderAbs, Action<T> onLoaded)
        {
            try
            {
                var path = folderAbs;
                if (!System.IO.Directory.Exists(path))
                {
                    Debug.LogWarning($"[Defs] {kind} folder missing: {path}");
                    return;
                }
                var files = System.IO.Directory.GetFiles(path, "*.xml", System.IO.SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    var doc = System.IO.File.ReadAllText(file);
                    var ser = new XmlSerializer(typeof(T));
                    using var sr = new System.IO.StringReader(doc);
                    var obj = (T)ser.Deserialize(sr);
                    onLoaded?.Invoke(obj);
                }
                Debug.Log($"[Defs] {kind} loaded: {files.Length} files");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Defs] Failed to load {kind}: {e.Message}\n{e}");
            }
        }
    }
}
