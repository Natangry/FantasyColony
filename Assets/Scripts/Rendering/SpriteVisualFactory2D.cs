using System.Collections.Generic;
using FantasyColony.Defs;
using UnityEngine;
using System.Globalization;

public static class SpriteVisualFactory2D
{
    private static readonly Dictionary<string, GameObject> _ghostPrefabs = new();
    private static readonly Dictionary<string, GameObject> _placedPrefabs = new();
    private static Sprite _white;
    private static string _sortingLayer;
    private static int _orderGround;

    public static string SortingLayerName => _sortingLayer;
    public static int GroundOrder => _orderGround;

    public static void Build()
    {
        _ghostPrefabs.Clear();
        _placedPrefabs.Clear();
        EnsureWhiteSprite();
        DetectSortingLayer();

        foreach (var v in DefDatabase.Visuals)
        {
            // DefDatabase.Visuals is a list; build prefabs keyed by defName
            _ghostPrefabs[v.defName] = MakeSpritePrefab(v, translucent: true);
            _placedPrefabs[v.defName] = MakeSpritePrefab(v, translucent: false);
        }
        if (_ghostPrefabs.Count == 0)
        {
            // synthesize a default visual so we see something
            var v = new Visual2DDef { defName = "core.Visual.Board_Default", color_rgba = "#F3D95AFF", plane = "XY" };
            _ghostPrefabs[v.defName] = MakeSpritePrefab(v, translucent:true);
            _placedPrefabs[v.defName] = MakeSpritePrefab(v, translucent:false);
        }
        Debug.Log($"[SpriteVisualFactory2D] Ready. SortingLayer='{_sortingLayer}', GroundOrder={_orderGround}");
    }

    public static GameObject SpawnGhost(string visualId, Vector2Int foot, float tile, Transform parent)
    {
        if (!_ghostPrefabs.TryGetValue(visualId, out var pf)) return null;
        var inst = Object.Instantiate(pf, parent);
        SizeAndPlace(inst.transform, foot, tile, true);
        return inst;
    }

    public static GameObject SpawnPlaced(string visualId, Vector2Int foot, float tile, Transform parent)
    {
        if (!_placedPrefabs.TryGetValue(visualId, out var pf)) return null;
        var inst = Object.Instantiate(pf, parent);
        SizeAndPlace(inst.transform, foot, tile, false);
        return inst;
    }

    private static GameObject MakeSpritePrefab(Visual2DDef vdef, bool translucent)
    {
        var go = new GameObject((vdef.defName ?? "Unknown") + (translucent?".Ghost":".Placed"));
        var sr = go.AddComponent<SpriteRenderer>();
        Sprite sprite = _white;
        if (!string.IsNullOrEmpty(vdef.spritePath))
        {
            var loaded = Resources.Load<Sprite>(vdef.spritePath);
            if (loaded != null) sprite = loaded;
        }
        sr.sprite = sprite;
        sr.sortingLayerName = _sortingLayer;
        sr.sortingOrder = _orderGround + (translucent?5:3);
        var c = ParseColor(vdef.color_rgba); if (translucent) c.a *= 0.4f; sr.color = c;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity; // XY plane
        return go;
    }

    private static Color ParseColor(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return Color.white;
        s = s.Trim();
        if (s.StartsWith("#") && ColorUtility.TryParseHtmlString(s, out var cHex)) return cHex;
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
        return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : d;
    }

    private static void SizeAndPlace(Transform t, Vector2Int foot, float tile, bool ghost)
    {
        // Scale sprite in XY to desired world size: default white is 1 unit per side already
        t.localScale = new Vector3(Mathf.Max(0.1f, foot.x * tile), Mathf.Max(0.1f, foot.y * tile), 1f);
        // offset half size from parent (which is bottom-left)
        t.localPosition = new Vector3((foot.x * tile) * 0.5f, (foot.y * tile) * 0.5f, ghost ? -0.01f : 0f);
    }

    private static void EnsureWhiteSprite()
    {
        if (_white != null) return;
        var tex = new Texture2D(1,1, TextureFormat.RGBA32, false);
        tex.SetPixel(0,0,Color.white); tex.Apply();
        _white = Sprite.Create(tex, new Rect(0,0,1,1), new Vector2(0.5f,0.5f), 1f);
    }

    private static void DetectSortingLayer()
    {
        // Prefer a Pawn/Unit sprite if available, else any SpriteRenderer, else Default
        _sortingLayer = "Default";
        _orderGround = 0;
        SpriteRenderer picked = null;
        foreach (var sr in Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
        {
            var n = sr.gameObject.name.ToLower();
            if (n.Contains("pawn") || n.Contains("unit") || n.Contains("colonist")) { picked = sr; break; }
            if (picked == null) picked = sr; // fallback to first seen
        }
        if (picked != null)
        {
            _sortingLayer = picked.sortingLayerName;
            _orderGround = picked.sortingOrder;
        }
    }
}
