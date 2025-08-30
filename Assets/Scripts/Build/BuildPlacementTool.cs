using UnityEngine;
using FantasyColony.Defs;
using System.Linq;
using System;
using UnityObject = UnityEngine.Object;

/// <summary>
/// Minimal placement tool for the Construction Board bring-up:
///  - Shows a snapping ghost to the grid
///  - Left-click to place; ESC to cancel (handled by controller too)
///  - Enforces uniqueness for ConstructionBoard
/// </summary>
public class BuildPlacementTool : MonoBehaviour
{
    [Header("Grid")]
    public float tileSize = 1f;

    [Header("Ghost")]
    public float ghostAlpha = 0.5f;
    GameObject ghostGO;
    SpriteRenderer ghostSr;

    BuildModeController ctrl;
    Camera cam;
    BuildTool active = BuildTool.None;

    readonly Plane groundPlane = new Plane(Vector3.up, 0f); // XZ ground at y=0
    // Prevent click-through after arming tool from a UI button press
    bool suppressClickUntilMouseUp = false;

    void Start()
    {
        ctrl = GetComponent<BuildModeController>();
        cam  = Camera.main;
    }

    public void SetTool(BuildTool tool)
    {
        active = tool;
        EnsureGhostDestroyed();
        if (active == BuildTool.PlaceConstructionBoard && ctrl.SelectedBuildingDef != null)
        {
            suppressClickUntilMouseUp = true; // wait for left mouse to be released once
            EnsureGhost(ctrl.SelectedBuildingDef);
            // Position ghost immediately under cursor (no first-frame origin flicker)
            if (cam == null) cam = Camera.main;
            if (TryGetCursorHit(out var hit))
                ghostGO.transform.position = SnapToGridXZ(hit, ctrl.SelectedBuildingDef) + new Vector3(0f, 0.02f, 0f);
            Debug.Log("[Build] Tool armed for " + (ctrl.SelectedBuildingDef.defName ?? "Unknown"));
        }
    }

    void Update()
    {
        if (active == BuildTool.None) return;
        if (cam == null) cam = Camera.main;

        var def = ctrl.SelectedBuildingDef;
        if (def == null)
        {
            ClearTool();
            return;
        }

        // Move ghost to snapped mouse position
        if (!TryGetCursorHit(out var hit))
            return;
        var snapped = SnapToGridXZ(hit, def);
        if (ghostGO != null)
        {
            ghostGO.transform.position = snapped + new Vector3(0f, 0.02f, 0f);
        }

        // Click to place
        // Ignore the initial click that selected the tool (click-through from UI)
        if (suppressClickUntilMouseUp)
        {
            if (Input.GetMouseButtonUp(0)) suppressClickUntilMouseUp = false;
        }
        else if (Input.GetMouseButtonDown(0))
        {
            TryPlace(def, snapped);
        }
    }

    // Compute scale to ensure the sprite footprint matches def (in world units)
    Vector3 ComputeScaleForSprite(Sprite sprite, BuildingDef def)
    {
        if (sprite == null) return new Vector3(def.width * tileSize, def.height * tileSize, 1f);
        float spriteWorldWidth  = sprite.rect.width  / sprite.pixelsPerUnit;
        float spriteWorldHeight = sprite.rect.height / sprite.pixelsPerUnit;
        float sx = (def.width  * tileSize) / Mathf.Max(0.0001f, spriteWorldWidth);
        float sy = (def.height * tileSize) / Mathf.Max(0.0001f, spriteWorldHeight);
        return new Vector3(sx, sy, 1f);
    }

    Visual2DDef GetVisualDefFor(BuildingDef def)
    {
        if (def == null) return null;
        if (string.IsNullOrEmpty(def.visualRef)) return null;
        if (DefDatabase.Visuals == null) return null;
        return DefDatabase.Visuals.FirstOrDefault(v => v.defName == def.visualRef);
    }

    Sprite LoadSpriteFor(BuildingDef def)
    {
        var v = GetVisualDefFor(def);
        if (v != null && !string.IsNullOrEmpty(v.spritePath))
        {
            var s = Resources.Load<Sprite>(v.spritePath);
            if (s != null) return s;
        }
        return null;
    }

    void ApplyScaleForSpriteOrFallbackXZ(GameObject go, BuildingDef def, Sprite spriteOrNull)
    {
        var scale = ComputeScaleForSprite(spriteOrNull, def);
        go.transform.localScale = scale;
        // Lay the sprite flat on XZ ground (face camera looking down -Y)
        go.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        // keep y slightly above ground to avoid z-fighting
    }

    void TryPlace(BuildingDef def, Vector3 pos)
    {
        // For MVP: explicitly handle uniqueness for ConstructionBoard
        if (def.unique
#if UNITY_2023_1_OR_NEWER
            && UnityObject.FindAnyObjectByType<ConstructionBoard>() != null
#elif UNITY_2022_2_OR_NEWER
            && UnityObject.FindFirstObjectByType<ConstructionBoard>() != null
#else
#pragma warning disable 618
            && FindObjectOfType<ConstructionBoard>() != null
#pragma warning restore 618
#endif
            )
        {
            // silently ignore for now; could beep/flash UI
            return;
        }

        // Instantiate placed object
        GameObject go = new GameObject(def.label ?? def.defName ?? "Building");
        // Attach the specific behaviour for Construction Board MVP
        var board = go.AddComponent<ConstructionBoard>();
        // If your ConstructionBoard has an OnPlaced API, you can call it here:
        // board.OnPlaced(new Vector2Int(def.width, def.height), tileSize);

        // Visuals: prefer real sprite from VisualDef.spritePath; fall back to white unit
        var sr = go.AddComponent<SpriteRenderer>();
        var spr = LoadSpriteFor(def);
        if (spr != null)
        {
            sr.sprite = spr;
        }
        else
        {
            Debug.LogWarning("[Build] Sprite not found at Resources/" + (GetVisualDefFor(def)?.spritePath ?? "(null)") + ".png — using white fallback.");
            sr.sprite = MakeUnitSprite();
        }
        ApplyScaleForSpriteOrFallbackXZ(go, def, spr);
        sr.sortingOrder = 100;
        // Set final position on ground with slight epsilon to avoid z-fighting
        go.transform.position = new Vector3(pos.x, 0.03f, pos.z); // slight epsilon

        ClearTool(); // place once for MVP
    }

    void ClearTool()
    {
        active = BuildTool.None;
        EnsureGhostDestroyed();
    }

    Vector3 SnapToGridXZ(Vector3 world, BuildingDef def)
    {
        float w = def.width * tileSize;
        float d = def.height * tileSize;
        // Snap to tile grid (XZ), keep pivot at center of the footprint
        float x = Mathf.Floor(world.x / tileSize) * tileSize + (w * 0.5f) - (tileSize * 0.5f);
        float z = Mathf.Floor(world.z / tileSize) * tileSize + (d * 0.5f) - (tileSize * 0.5f);
        return new Vector3(x, 0f, z);
    }

    bool TryGetCursorHit(out Vector3 hit)
    {
        hit = default;
        if (cam == null) cam = Camera.main;
        if (cam == null) return false;
        var ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!groundPlane.Raycast(ray, out var enter)) return false;
        hit = ray.GetPoint(enter); // y == 0 plane
        return true;
    }

    void EnsureGhost(BuildingDef def)
    {
        EnsureGhostDestroyed();
        ghostGO = new GameObject("Ghost_" + (def.label ?? def.defName));
        ghostSr = ghostGO.AddComponent<SpriteRenderer>();
        var spr = LoadSpriteFor(def);
        if (spr != null)
        {
            ghostSr.sprite = spr;
        }
        else
        {
            ghostSr.sprite = MakeUnitSprite();
        }
        var c = ghostSr.color;
        c.a = ghostAlpha;
        ghostSr.color = c;
        ApplyScaleForSpriteOrFallbackXZ(ghostGO, def, spr);
        ghostSr.sortingOrder = 200;
    }

    void EnsureGhostDestroyed()
    {
        if (ghostGO != null) Destroy(ghostGO);
        ghostGO = null;
        ghostSr = null;
    }

    // Generates/returns a 1x1 white sprite for simple visuals
    static Sprite _unitSprite;
    static Sprite MakeUnitSprite()
    {
        if (_unitSprite != null) return _unitSprite;
        var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply(false, false);
        _unitSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        return _unitSprite;
    }
}
