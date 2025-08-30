using UnityEngine;
using FantasyColony.Defs;
using System.Linq;
using UnityEngine.SceneManagement;
using System;
using UnityObject = UnityEngine.Object;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine.EventSystems;

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

    Plane groundPlane = new Plane(Vector3.up, 0f); // XZ ground at y=0 (adjustable)
    // Prevent click-through after arming tool from a UI button press
    bool suppressClickUntilMouseUp = false;
    float armBlockUntilTime = 0f;
    static bool _warnedSpriteMissing = false;
    // cached cell size set when arming/first hit
    float _cachedCellSize = -1f;

    void Start()
    {
        ctrl = GetComponent<BuildModeController>();
        cam  = GetActiveCamera();
    }

    float GetCellSize()
    {
        // Try to find a grid/map object with a cell size property/field.
        // This is tolerant: it will use reflection to read common names.
        float cell = 1f;
        try
        {
            var any = FindAnyObjectByType<MonoBehaviour>();
            if (any != null)
            {
                var type = any.GetType();
                var csField = type.GetField("CellSize") ?? type.GetField("cellSize");
                var csProp  = type.GetProperty("CellSize") ?? type.GetProperty("cellSize");
                if (csField != null && csField.FieldType == typeof(float)) cell = (float)csField.GetValue(any);
                else if (csProp != null && csProp.PropertyType == typeof(float)) cell = (float)csProp.GetValue(any);
            }
        }
        catch { /* best-effort */ }
        if (cell <= 0f || float.IsNaN(cell) || float.IsInfinity(cell)) cell = 1f;
        return cell;
    }

    public void SetTool(BuildTool tool)
    {
        active = tool;
        EnsureGhostDestroyed();
        if (active == BuildTool.PlaceConstructionBoard && ctrl.SelectedBuildingDef != null)
        {
            suppressClickUntilMouseUp = true; // wait for left mouse to be released once
            armBlockUntilTime = Time.realtimeSinceStartup + 0.12f;
            EnsureGhost(ctrl.SelectedBuildingDef);
            // Position ghost immediately under cursor (no first-frame origin flicker)
            cam = GetActiveCamera();
            if (_cachedCellSize <= 0f) _cachedCellSize = GetCellSize();
            groundPlane = new Plane(Vector3.up, GetGroundY());
            var mp = GetMouseScreenPos();
            if (TryGetCursorHitRaw(mp, out var hit))
            {
                var world = SnapToGridXZ(hit, ctrl.SelectedBuildingDef);
                ghostGO.transform.position = world + new Vector3(0f, 0.02f, 0f);
                Debug.Log($"[Build] Arm: mouse {mp} -> world {world}");
                _cachedCellSize = GetCellSize();
            }
            else if (TryGetCenterHit(out var centerHit))
                ghostGO.transform.position = SnapToGridXZ(centerHit, ctrl.SelectedBuildingDef) + new Vector3(0f, 0.02f, 0f);
            else
                ghostGO.transform.position = new Vector3(0f, 0.02f, 0f); // absolute worst-case fallback
            Debug.Log("[Build] Tool armed for " + (ctrl.SelectedBuildingDef.defName ?? "Unknown"));
        }
    }

    void Update()
    {
        if (active == BuildTool.None) return;
        if (cam == null || !cam.isActiveAndEnabled) cam = GetActiveCamera();

        var def = ctrl.SelectedBuildingDef;
        if (def == null)
        {
            ClearTool();
            return;
        }
        if (_cachedCellSize <= 0f) _cachedCellSize = GetCellSize();
        groundPlane = new Plane(Vector3.up, GetGroundY());

        // Move ghost to snapped mouse position
        if (!TryGetCursorHit(out var hit))
            return;
        var snapped = SnapToGridXZ(hit, def);
        if (ghostGO != null)
        {
            // Keep previous valid location if we cannot get a new hit (e.g., cursor off-screen)
            Vector3 pos = snapped;
            if (float.IsNaN(snapped.x) || float.IsNaN(snapped.z))
                pos = lastValidWorld;
            ghostGO.transform.position = pos + new Vector3(0f, 0.02f, 0f);
        }

        // Click to place
        // Ignore the initial click that selected the tool (click-through from UI)
        if (suppressClickUntilMouseUp)
        {
            if (Input.GetMouseButtonUp(0) && Time.realtimeSinceStartup >= armBlockUntilTime)
                suppressClickUntilMouseUp = false;
        }
        else
        {
            bool clickDown = false;
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null) clickDown |= Mouse.current.leftButton.wasPressedThisFrame;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER || !ENABLE_INPUT_SYSTEM
            clickDown |= Input.GetMouseButtonDown(0);
#endif
            // Avoid placing when pointer over UI
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                clickDown = false;

            if (clickDown)
            {
                TryPlace(def, snapped);
            }
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

    Vector3 lastValidWorld;

    Vector3 GetMouseScreenPos()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null) return (Vector3)Mouse.current.position.ReadValue();
#endif
        return Input.mousePosition;
    }

    bool TryGetCursorHit(out Vector3 hit)
    {
        hit = default;
        if (cam == null) cam = GetActiveCamera();
        if (cam == null) return false;
        var mp = GetMouseScreenPos();
        var ray = cam.ScreenPointToRay(mp);
        if (!groundPlane.Raycast(ray, out var enter)) return false;
        hit = ray.GetPoint(enter); // y == plane height
        lastValidWorld = hit;
        return true;
    }

    bool TryGetCursorHitRaw(Vector3 screenPos, out Vector3 hit)
    {
        hit = default;
        if (cam == null) cam = GetActiveCamera();
        if (cam == null) return false;
        var ray = cam.ScreenPointToRay(screenPos);
        if (!groundPlane.Raycast(ray, out var enter)) return false;
        hit = ray.GetPoint(enter); // y == plane height
        lastValidWorld = hit;
        return true;
    }

    bool TryGetCenterHit(out Vector3 hit)
    {
        hit = default;
        var center = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        var camRef = GetActiveCamera();
        if (camRef == null) return false;
        var ray = camRef.ScreenPointToRay(center);
        if (ray.direction == Vector3.zero) return false;
        if (!groundPlane.Raycast(ray, out var enter)) return false;
        hit = ray.GetPoint(enter); lastValidWorld = hit; return true;
    }

    Camera GetActiveCamera()
    {
        var m = Camera.main;
        if (m != null && m.isActiveAndEnabled) return m;
        var any = FindAnyObjectByType<Camera>();
        if (any != null && any.isActiveAndEnabled) return any;
        return Camera.current;
    }

    float GetGroundY()
    {
        // If your world ground is not at y=0, put a better heuristic here.
        return 0f;
    }

    Visual2DDef GetVisualDefFor(BuildingDef def)
    {
        // Forgive missing visualRef for construction board in fallback scenarios
        if (def != null && string.IsNullOrEmpty(def.visualRef) &&
            !string.IsNullOrEmpty(def.defName) &&
            def.defName.ToLowerInvariant().Contains("constructionboard"))
        {
            Debug.Log("[Build] Using fallback visualRef=ConstructionBoardVisual");
            def.visualRef = "ConstructionBoardVisual";
        }

        if (def == null) return null;
        if (string.IsNullOrEmpty(def.visualRef)) return null;
        if (DefDatabase.Visuals == null) return null;
        return DefDatabase.Visuals.FirstOrDefault(v => v.defName == def.visualRef);
    }

    Sprite LoadSpriteFor(BuildingDef def)
    {
        // Primary path: via visual def
        var v = GetVisualDefFor(def);
        if (v != null && !string.IsNullOrEmpty(v.spritePath))
        {
            var spr = Resources.Load<Sprite>(v.spritePath);
            if (spr != null)
            {
                Debug.Log("[Build] Loaded sprite: " + spr.name + " (PPU=" + spr.pixelsPerUnit + ", path=" + v.spritePath + ")");
                return spr;
            }
            if (!_warnedSpriteMissing)
            {
                Debug.LogWarning("[Build] Sprite NOT FOUND at Resources/" + v.spritePath + ".png – ensure it's Sprite(2D & UI), under Assets/Resources, no extension in path.");
                _warnedSpriteMissing = true;
            }
        }

        // Direct fallback for Construction Board bring-up even when visual DB is missing
        if (def != null && !string.IsNullOrEmpty(def.defName) &&
            def.defName.ToLowerInvariant().Contains("constructionboard"))
        {
            const string directPath = "Sprites/ConstructionBoard";
            var spr = Resources.Load<Sprite>(directPath);
            if (spr != null)
            {
                Debug.Log("[Build] Loaded sprite via direct path fallback: " + spr.name + " (PPU=" + spr.pixelsPerUnit + ", path=" + directPath + ")");
                return spr;
            }
            if (!_warnedSpriteMissing)
                Debug.LogWarning("[Build] Sprite NOT FOUND at Resources/" + directPath + ".png – check asset path/type/import.");
        }
        return null;
    }

    void ApplyScaleForSpriteOrFallbackXZ(GameObject go, BuildingDef def, Sprite spriteOrNull)
    {
        if (_cachedCellSize <= 0f) _cachedCellSize = GetCellSize();
        var scale = ComputeScaleForSprite(spriteOrNull, def);
        // Ensure scale reflects grid squares instead of raw world units
        scale.x *= _cachedCellSize;
        scale.y *= _cachedCellSize;
        // Optional visual-upsize: double the board if desired
        // if (def.defName.Contains("ConstructionBoard")) { scale *= 2f; }
        go.transform.localScale = scale;
        // Lay the sprite flat on XZ ground (face camera looking down -Y)
        go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
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
            Debug.Log("[Build] Loaded sprite: " + spr.name + " (PPU=" + spr.pixelsPerUnit + ")");
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
        Debug.Log($"[Build] Placed {def.defName} at world {pos} (cellSize={_cachedCellSize})");

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
