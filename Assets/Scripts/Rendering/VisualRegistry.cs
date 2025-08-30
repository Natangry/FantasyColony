using System.Collections.Generic;
using FantasyColony.Defs;
using UnityEngine;

public static class VisualRegistry
{
    private static readonly Dictionary<string, GameObject> _ghostPrefabs = new();
    private static readonly Dictionary<string, GameObject> _placedPrefabs = new();
    private static int _visibleLayer = 0;

    public static void Build(bool inEditor)
    {
        _ghostPrefabs.Clear();
        _placedPrefabs.Clear();
        var cam = Camera.main ?? Object.FindAnyObjectByType<Camera>();
        _visibleLayer = PickVisibleLayer(0, cam);

        // Build from current defs
        foreach (var v in DefDatabase.Visuals)
        {
            var vd = v as VisualDef ?? new VisualDef
            {
                defName = v.defName,
                sortingLayer = v.sortingLayer,
                sortingOrder = v.sortingOrder,
                pivotX = v.pivotX,
                pivotY = v.pivotY,
                scale = v.scale,
                plane = v.plane,
                z_lift = v.z_lift,
                shader_hint = v.shader_hint,
                color_rgba = v.color_rgba
            };
            var ghost = MakeQuadPrefab(vd, translucent: true, cam);
            var placed = MakeQuadPrefab(vd, translucent: false, cam);
            _ghostPrefabs[vd.id] = ghost;
            _placedPrefabs[vd.id] = placed;
        }
        Debug.Log($"[VisualRegistry] Built prefabs: Ghost={_ghostPrefabs.Count}, Placed={_placedPrefabs.Count}, Layer={_visibleLayer}");
    }

    public static GameObject SpawnGhost(string visualId, Vector2Int foot, float tile, Transform parent)
    {
        if (!_ghostPrefabs.TryGetValue(visualId, out var pf)) return null;
        var inst = Object.Instantiate(pf, parent);
        if (DefDatabase.VisualsByName != null && DefDatabase.VisualsByName.TryGetValue(visualId, out var def))
        {
            var vd = def as VisualDef ?? new VisualDef
            {
                defName = def.defName,
                sortingLayer = def.sortingLayer,
                sortingOrder = def.sortingOrder,
                pivotX = def.pivotX,
                pivotY = def.pivotY,
                scale = def.scale,
                plane = def.plane,
                z_lift = def.z_lift,
                shader_hint = def.shader_hint,
                color_rgba = def.color_rgba
            };
            Orient(inst.transform, vd, foot, tile, true);
        }
        return inst;
    }

    public static GameObject SpawnPlaced(string visualId, Vector2Int foot, float tile, Transform parent)
    {
        if (!_placedPrefabs.TryGetValue(visualId, out var pf)) return null;
        var inst = Object.Instantiate(pf, parent);
        if (DefDatabase.VisualsByName != null && DefDatabase.VisualsByName.TryGetValue(visualId, out var def))
        {
            var vd = def as VisualDef ?? new VisualDef
            {
                defName = def.defName,
                sortingLayer = def.sortingLayer,
                sortingOrder = def.sortingOrder,
                pivotX = def.pivotX,
                pivotY = def.pivotY,
                scale = def.scale,
                plane = def.plane,
                z_lift = def.z_lift,
                shader_hint = def.shader_hint,
                color_rgba = def.color_rgba
            };
            Orient(inst.transform, vd, foot, tile, false);
        }
        return inst;
    }

    private static GameObject MakeQuadPrefab(VisualDef vdef, bool translucent, Camera cam)
    {
        var root = new GameObject(vdef.id + (translucent ? ".Ghost" : ".Placed"));
        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.transform.SetParent(root.transform, false);
        var mr = quad.GetComponent<MeshRenderer>();
        mr.sharedMaterial = MakeMaterial(vdef, translucent);
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;
        var col = quad.GetComponent<Collider>(); if (col != null) Object.DestroyImmediate(col);
        root.layer = quad.layer = PickVisibleLayer(LayerFromNameOrIndex(vdef.render_layer), cam);
        return root;
    }

    private static Material MakeMaterial(VisualDef vdef, bool translucent)
    {
        Shader s = null;
        if (!string.IsNullOrEmpty(vdef.shader_hint))
        {
            if (vdef.shader_hint.Contains("URP")) s = Shader.Find("Universal Render Pipeline/Unlit");
            if (s == null && vdef.shader_hint.Contains("Unlit")) s = Shader.Find("Unlit/Color");
        }
        if (s == null) s = Shader.Find("Standard");
        var m = new Material(s);
        if (s.name.Contains("Standard"))
        {
            m.SetFloat("_Mode", translucent ? 3 : 0);
            m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetInt("_ZWrite", translucent ? 0 : 1);
            if (translucent) { m.EnableKeyword("_ALPHABLEND_ON"); m.renderQueue = 3001; } else { m.DisableKeyword("_ALPHABLEND_ON"); m.renderQueue = 2450; }
        }
        else
        {
            m.renderQueue = translucent ? 3001 : 2450;
        }
        var c = vdef.Color; if (translucent) c.a *= 0.4f; m.color = c;
        return m;
    }

    private static void Orient(Transform t, VisualDef vdef, Vector2Int foot, float tile, bool ghost)
    {
        if (vdef.Plane == GridPlane.XZ)
        {
            t.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            var lift = vdef.z_lift <= 0f ? (ghost ? 0.08f : 0.05f) : vdef.z_lift;
            t.localPosition = new Vector3((foot.x * tile) * 0.5f, lift, (foot.y * tile) * 0.5f);
            t.localScale = new Vector3(foot.x * tile, foot.y * tile, 1f);
        }
        else
        {
            t.localRotation = Quaternion.identity;
            var lift = vdef.z_lift <= 0f ? (ghost ? 0.02f : 0.0f) : vdef.z_lift;
            t.localPosition = new Vector3((foot.x * tile) * 0.5f, (foot.y * tile) * 0.5f, lift);
            t.localScale = new Vector3(foot.x * tile, foot.y * tile, 1f);
        }
    }

    private static int LayerFromNameOrIndex(string layer)
    {
        if (int.TryParse(layer, out var idx)) return idx;
        int n = LayerMask.NameToLayer(layer);
        if (n >= 0) return n;
        return 0;
    }

    private static int PickVisibleLayer(int preferred, Camera cam)
    {
        if (cam == null) return preferred;
        int mask = cam.cullingMask;
        if ((mask & (1 << preferred)) != 0) return preferred;
        for (int i = 0; i < 32; i++) if ((mask & (1 << i)) != 0) return i;
        return 0;
    }
}
