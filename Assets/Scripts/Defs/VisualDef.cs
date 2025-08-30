using FantasyColony.Defs;
using UnityEngine;

/// <summary>
/// Compatibility shim: legacy code references VisualDef. We now use Visual2DDef.
/// This shim keeps existing code compiling while we migrate call sites.
/// </summary>
[System.Serializable]
public class VisualDef : Visual2DDef
{
    // --- Legacy static keys (string) for code that uses string keys ---
    public static readonly string id_key = nameof(defName);
    public static readonly string render_layer_key = nameof(sortingLayer);
    public static readonly string color_rgba_key = "color_rgba";
    public static readonly string plane_key = "plane";
    public static readonly string shader_hint_key = "shader_hint";
    public static readonly string z_lift_key = "z_lift";

    // --- Instance properties expected by existing rendering code ---
    public string id => defName;
    public string render_layer => sortingLayer;
    public Color Color => ParseColor(color_rgba);
    public GridPlane Plane => ParsePlane(plane);

    private static Color ParseColor(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return Color.white;
        s = s.Trim();
        // Support hex like #RRGGBB or #RRGGBBAA
        if (s[0] == '#')
        {
            if (ColorUtility.TryParseHtmlString(s, out var cHex)) return cHex;
        }
        // Support comma floats: r,g,b[,a]
        var parts = s.Split(',');
        if (parts.Length >= 3)
        {
            float r = ParseFloat(parts[0], 1f);
            float g = ParseFloat(parts[1], 1f);
            float b = ParseFloat(parts[2], 1f);
            float a = parts.Length > 3 ? ParseFloat(parts[3], 1f) : 1f;
            return new Color(r, g, b, a);
        }
        return Color.white;
    }
    private static float ParseFloat(string s, float d)
    {
        return float.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : d;
    }
    private static GridPlane ParsePlane(string p)
    {
        if (string.IsNullOrEmpty(p)) return GridPlane.XY;
        return p.Equals("XZ", System.StringComparison.OrdinalIgnoreCase) ? GridPlane.XZ : GridPlane.XY;
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
