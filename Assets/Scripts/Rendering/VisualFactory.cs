using FantasyColony.Defs;
using UnityEngine;

public static class VisualFactory
{
    public static GameObject CreateGhost(VisualDef vdef, Vector2Int foot, float tile, Transform parent, int preferredLayer, GridPlane plane, Camera cam)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.name = "Build Ghost";
        go.transform.SetParent(parent, false);
        var mr = go.GetComponent<MeshRenderer>();
        mr.sharedMaterial = MakeMaterial(vdef, true);
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;
        var col = go.GetComponent<Collider>(); if (col != null) Object.Destroy(col);
        go.layer = PickVisibleLayer(preferredLayer, cam);
        Orient(go.transform, vdef, foot, tile, true);
        return go;
    }

    public static GameObject CreatePlaced(VisualDef vdef, Vector2Int foot, float tile, Transform parent, int preferredLayer, GridPlane plane, Camera cam)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.name = "BoardVisual";
        go.transform.SetParent(parent, false);
        var mr = go.GetComponent<MeshRenderer>();
        mr.sharedMaterial = MakeMaterial(vdef, false);
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;
        var col = go.GetComponent<Collider>(); if (col != null) Object.Destroy(col);
        go.layer = PickVisibleLayer(preferredLayer, cam);
        Orient(go.transform, vdef, foot, tile, false);
        return go;
    }

    private static Material MakeMaterial(VisualDef vdef, bool translucent)
    {
        Shader s = null;
        if (vdef.shader_hint.Contains("URP")) s = Shader.Find("Universal Render Pipeline/Unlit");
        if (s == null && vdef.shader_hint.Contains("Unlit")) s = Shader.Find("Unlit/Color");
        if (s == null) s = Shader.Find("Standard");
        var m = new Material(s);
        if (s.name.Contains("Standard"))
        {
            // standard transparent setup
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

    private static int PickVisibleLayer(int preferred, Camera cam)
    {
        if (cam == null) return preferred;
        int mask = cam.cullingMask;
        if ((mask & (1 << preferred)) != 0) return preferred;
        for (int i = 0; i < 32; i++) if ((mask & (1 << i)) != 0) return i;
        return 0;
    }
}
