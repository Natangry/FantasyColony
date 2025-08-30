using FantasyColony.Defs;
using UnityEngine;

/// <summary>
/// Compatibility shim: legacy code references VisualDef. We now use Visual2DDef.
/// This shim keeps existing code compiling while we migrate call sites.
/// </summary>
[System.Serializable]
public class VisualDef : Visual2DDef
{
    // Intentionally mostly empty; below are legacy helper keys & parsers expected by old code.
    // --- Legacy static keys (string) ---
    public static readonly string id = nameof(defName);
    public static readonly string render_layer = nameof(sortingLayer);
    public static readonly string color_rgba_key = "color_rgba"; // avoid name clash
    public static readonly string plane_key = "plane";
    public static readonly string shader_hint_key = "shader_hint";
    public static readonly string z_lift_key = "z_lift";

    // Legacy helper methods (parsers)
    public static Color Color(Visual2DDef def, string key, Color fallback)
    {
        var src = (def as VisualDef)?.color_rgba ?? def.color_rgba;
        if (string.IsNullOrEmpty(src)) return fallback;
        var parts = src.Split(',');
        if (parts.Length < 3) return fallback;
        float r = Parse(parts[0], 1f), g = Parse(parts[1], 1f), b = Parse(parts[2], 1f);
        float a = parts.Length > 3 ? Parse(parts[3], 1f) : 1f;
        return new Color(r, g, b, a);
    }
    public static int Plane(Visual2DDef def, string key, int fallback)
    {
        var p = (def as VisualDef)?.plane ?? def.plane ?? "XY";
        return p.Equals("XZ", System.StringComparison.OrdinalIgnoreCase) ? 1 : 0;
    }
    public static string Plane(Visual2DDef def, string key, string fallback)
    {
        var p = (def as VisualDef)?.plane ?? def.plane;
        return string.IsNullOrEmpty(p) ? fallback : p;
    }
    private static float Parse(string s, float d)
    {
        return float.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : d;
    }
}
/// <summary>Legacy "Value" access pattern used by rendering code: allow key-based lookups from Visual2DDef.</summary>
public static class VisualDefExtensions
{
    public static string Value(this Visual2DDef d, string key, string fallback = null)
    {
        switch (key)
        {
            case "id": return d.defName ?? fallback;
            case "render_layer": return d.sortingLayer ?? fallback;
            case "plane": return d.plane ?? fallback;
            case "shader_hint": return (d as VisualDef)?.shader_hint ?? d.shader_hint ?? fallback;
            case "color_rgba": return (d as VisualDef)?.color_rgba ?? d.color_rgba ?? fallback;
            default: return fallback;
        }
    }
    public static int Value(this Visual2DDef d, string key, int fallback)
    {
        switch (key)
        {
            case "sortingOrder": return d.sortingOrder;
            default: return fallback;
        }
    }
    public static float Value(this Visual2DDef d, string key, float fallback)
    {
        switch (key)
        {
            case "z_lift": return d.z_lift;
            default: return fallback;
        }
    }
}
