using System;
using System.Reflection;
using UnityEngine;
using FantasyColony.Defs;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem; // New Input System support
#endif

/// <summary>
/// Handles in-world placement of buildings while a placement tool is active.
/// This version targets a top-down world on the XZ plane (y = up).
/// </summary>
public class BuildPlacementTool : MonoBehaviour
{
    private BuildTool _tool = BuildTool.None;
    private GameObject _ghost;
    private MeshRenderer _ghostMr;
    private GameObject _marker;
    private MeshRenderer _markerMr;
    private bool _canPlace;
    private Vector3 _snapWorldPos;
    private Vector2Int _snapGridPos;
    private Vector2Int _footSize = Vector2Int.one; // current tool footprint in tiles
    private Vector3 _anchor;
    private float _groundConst; // y for XZ, z for XY
    private GridPlane _plane = GridPlane.XY;

    // Grid snapshot via reflection to avoid tight coupling
    private float _tile = 1f; private int _w = 128; private int _h = 128;
    private Vector3 _gridMinWorld; // bottom-left world corner of tile (0,0)
    private bool _haveBounds;
    private int _gridLayer = 0; // render layer to use for ghost/marker/visuals
    private string _activeBuildingDefId = "core.Building.ConstructionBoard";
    private VisualDef _ghostVDef; // cached visual for the active building

    private Camera _cam;
    private static Sprite _whiteSprite;

    private void SyncToolFromController()
    {
        var bm = BuildModeController.Instance;
        if (bm == null) return;
        if (_tool != bm.CurrentTool)
        {
            _tool = bm.CurrentTool;
        }
        // Always refresh footprint (avoid stale 1x1)
        _footSize = GetFootprint();
    }

    public void SetTool(BuildTool tool)
    {
        _tool = tool;
        _footSize = GetFootprint();
        EnsureGhost();
        EnsureMarker();
        ResolveActiveVisual();
    }

    // --- Input helpers: support both legacy Input and the new Input System ---
    private Vector2 GetMousePosition()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }
#endif
        return Input.mousePosition;
    }

    private bool LeftClickDown()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) return true;
#endif
        return Input.GetMouseButtonDown(0);
    }

    private bool RightClickDown()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame) return true;
#endif
        return Input.GetMouseButtonDown(1);
    }

    private bool CancelPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) return true;
#endif
        return Input.GetKeyDown(KeyCode.Escape);
    }

    private void Update()
    {
        // Drive placement every frame
        SyncToolFromController();
        ReadGridInfo(); // keep bounds/plane fresh

        if (_tool == BuildTool.None)
        {
            DestroyGhost();
            DestroyMarker();
            return;
        }

        // Update ghost & validity continuously
        UpdateGhost();

        // Handle input
        if (LeftClickDown())
        {
            TryPlace();
        }
        else if (RightClickDown() || CancelPressed())
        {
            CancelPlacement();
        }
    }

    private void ReadGridInfo()
    {
        var grid = FindObjectOfTypeByName("SimpleGridMap");
        _tile = 1f; _w = 128; _h = 128;
        _haveBounds = false;
        if (grid != null)
        {
            TryGetField(grid, "tileSize", ref _tile);
            TryGetField(grid, "width", ref _w);
            TryGetField(grid, "height", ref _h);

            // Try to find a Renderer to get true world bounds and deduce the plane
            var rend = (grid as Component).GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                var b = rend.bounds;
                // Decide plane by which axis is thinnest
                if (b.size.z < b.size.y && b.size.z < b.size.x)
                {
                    // XY world (thin Z)
                    _plane = GridPlane.XY;
                    GridSpace.Plane = _plane;
                    _groundConst = b.center.z;
                    _gridMinWorld = new Vector3(b.min.x, b.min.y, _groundConst);
                    _gridLayer = rend.gameObject.layer;
                    _haveBounds = true;
                }
                else
                {
                    // XZ world (thin Y)
                    _plane = GridPlane.XZ;
                    GridSpace.Plane = _plane;
                    _groundConst = b.center.y;
                    _gridMinWorld = new Vector3(b.min.x, _groundConst, b.min.z);
                    _gridLayer = rend.gameObject.layer;
                    _haveBounds = true;
                }
            }
            else
            {
                // Fallback: assume XY with z=0
                _plane = GridPlane.XY;
                GridSpace.Plane = _plane;
                _groundConst = 0f;
                _gridLayer = 0; // Default
            }
        }
        else
        {
            _plane = GridPlane.XY;
            GridSpace.Plane = _plane;
            _groundConst = 0f;
            _gridLayer = 0;
        }
    }

    private void UpdateGhost()
    {
        var cam = GetCamera();
        if (cam == null) return; // can't place without a camera

        if (!TryGetMouseOnGround(cam, out var world)) return;
        // show marker even before we know if placement is valid
        if (_marker == null) EnsureMarker();
        // Determine anchor for snapping: true bottom-left if we have bounds, else (0,0)
        _anchor = _haveBounds ? _gridMinWorld : GuessCenteredAnchor();
        _footSize = GetFootprint();
        if (_tool == BuildTool.PlaceConstructionBoard) _footSize = new Vector2Int(3, 1); // belt & suspenders
        if (_ghostVDef == null) ResolveActiveVisual();

        // Snap to grid anchored at bottom-left on the active plane
        int gx, gy;
        if (_plane == GridPlane.XZ)
        {
            gx = Mathf.RoundToInt((world.x - _anchor.x) / _tile);
            gy = Mathf.RoundToInt((world.z - _anchor.z) / _tile);
        }
        else
        {
            gx = Mathf.RoundToInt((world.x - _anchor.x) / _tile);
            gy = Mathf.RoundToInt((world.y - _anchor.y) / _tile);
        }
        gx = Mathf.Clamp(gx, -_w, _w * 2);
        gy = Mathf.Clamp(gy, -_h, _h * 2);
        // bottom-left of footprint
        _snapGridPos = new Vector2Int(gx, gy);
        float cx = _anchor.x + ((gx + _footSize.x * 0.5f) * _tile);
        if (_plane == GridPlane.XZ)
        {
            float cz = _anchor.z + ((gy + _footSize.y * 0.5f) * _tile);
            _snapWorldPos = new Vector3(cx, _groundConst + 0.02f, cz);
        }
        else
        {
            float cy = _anchor.y + ((gy + _footSize.y * 0.5f) * _tile);
            _snapWorldPos = new Vector3(cx, cy, _groundConst + 0.0f);
        }

        _canPlace = IsInsideGrid(_snapGridPos, _footSize) && IsPlacementAllowedHere();

        EnsureGhost();

        // Color + transforms
        SetGhostColor(_canPlace ? new Color(0.2f, 0.9f, 0.2f, 0.35f) : new Color(0.9f, 0.2f, 0.2f, 0.35f));
        _ghost.transform.position = _snapWorldPos;
        _ghost.transform.localScale = new Vector3(_footSize.x * _tile, _footSize.y * _tile, 1f);
        _ghost.transform.rotation = _plane == GridPlane.XZ ? Quaternion.Euler(-90f, 0f, 0f) : Quaternion.identity;

        // Raw hit marker (tiny) at mouse world
        if (_marker != null)
        {
            _marker.transform.position = world + (_plane == GridPlane.XZ ? Vector3.up * 0.03f : Vector3.forward * 0.01f);
            _marker.transform.localScale = Vector3.one * _tile * 0.2f;
            _marker.transform.rotation = _plane == GridPlane.XZ ? Quaternion.Euler(-90f, 0f, 0f) : Quaternion.identity;
        }
    }

    private bool IsInsideGrid(Vector2Int p, Vector2Int size)
    {
        if (!_haveBounds) // be permissive if we don't know exact bounds
            return p.x >= -_w && p.y >= -_h && p.x + size.x <= _w * 2 && p.y + size.y <= _h * 2;
        return p.x >= 0 && p.y >= 0 && p.x + size.x <= _w && p.y + size.y <= _h;
    }

    private bool IsPlacementAllowedHere()
    {
        if (_tool == BuildTool.PlaceConstructionBoard && BuildModeController.UniqueBuildingExists<ConstructionBoard>())
            return false; // unique per map

        // Check for any existing Building overlapping this footprint
        Building[] all;
#if UNITY_2023_1_OR_NEWER
        all = UnityEngine.Object.FindObjectsByType<Building>(FindObjectsSortMode.None);
#else
        all = FindObjectsOfType<Building>();
#endif
        foreach (var b in all)
        {
            if (RectOverlaps(b.GridPos, b.size, _snapGridPos, _footSize)) return false;
        }
        return true;
    }

    private static bool RectOverlaps(Vector2Int aPos, Vector2Int aSize, Vector2Int bPos, Vector2Int bSize)
    {
        return aPos.x < bPos.x + bSize.x && aPos.x + aSize.x > bPos.x &&
               aPos.y < bPos.y + bSize.y && aPos.y + aSize.y > bPos.y;
    }

    private void TryPlace()
    {
        // Ensure we're using the latest tool info
        SyncToolFromController();
        if (_tool == BuildTool.PlaceConstructionBoard) _footSize = new Vector2Int(3, 1);

        if (!_canPlace) return;

        Transform parent = EnsureBuildingsParent();

        switch (_tool)
        {
            case BuildTool.PlaceConstructionBoard:
            {
                var go = new GameObject("Construction Board");
                go.transform.SetParent(parent, worldPositionStays: true);
                // Align to bottom-left tile of the footprint
                float wx = _anchor.x + (_snapGridPos.x * _tile);
                if (_plane == GridPlane.XZ)
                {
                    float wz = _anchor.z + (_snapGridPos.y * _tile);
                    go.transform.position = new Vector3(wx, _groundConst, wz);
                }
                else
                {
                    float wy = _anchor.y + (_snapGridPos.y * _tile);
                    go.transform.position = new Vector3(wx, wy, _groundConst);
                }

                var board = go.AddComponent<ConstructionBoard>();
                board.displayName = "Construction Board";
                board.uniquePerMap = true;
                board.size = new Vector2Int(3, 1);
                board.OnPlaced(_snapGridPos, _tile);

                // Attach visual via defs
                var vdef = _ghostVDef ?? new VisualDef();
                VisualFactory.CreatePlaced(vdef, board.size, _tile, go.transform, _gridLayer, _plane, GetCamera());
                break;
            }
        }

        // After successful placement, clear tool
        SetTool(BuildTool.None);
    }

    private void EnsureGhost()
    {
        if (_ghost != null) return;
        var vdef = _ghostVDef ?? new VisualDef();
        _ghost = VisualFactory.CreateGhost(vdef, _footSize, _tile, this.transform, _gridLayer, _plane, GetCamera());
        _ghostMr = _ghost.GetComponent<MeshRenderer>();
    }

    private void EnsureMarker()
    {
        if (_marker != null) return;
        var vdef = _ghostVDef ?? new VisualDef();
        var markerColor = vdef.Color; markerColor.a = 0.85f;
        _marker = VisualFactory.CreateGhost(new VisualDef{ color_rgba = ColorUtility.ToHtmlStringRGBA(markerColor), plane = vdef.plane, z_lift = 0.1f }, new Vector2Int(1,1), _tile, this.transform, _gridLayer, _plane, GetCamera());
        _markerMr = _marker.GetComponent<MeshRenderer>();
    }

    private Transform EnsureBuildingsParent()
    {
        var world = GameObject.Find("World");
        if (world == null)
        {
            world = new GameObject("World");
        }
        var trans = world.transform.Find("Buildings");
        if (trans == null)
        {
            var go = new GameObject("Buildings");
            trans = go.transform;
            trans.SetParent(world.transform, worldPositionStays: false);
            trans.localPosition = Vector3.zero;
        }
        return trans;
    }

    private static Component FindObjectOfTypeByName(string typeName)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            var t = asm.GetType(typeName);
            if (t == null) continue;
#if UNITY_2023_1_OR_NEWER
            var comp = UnityEngine.Object.FindAnyObjectByType(t) as Component;
#else
            var comp = UnityEngine.Object.FindObjectOfType(t) as Component;
#endif
            if (comp != null) return comp;
        }
        return null;
    }

    private static void TryGetField<T>(Component c, string field, ref T value)
    {
        if (c == null) return;
        var f = c.GetType().GetField(field, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (f != null)
        {
            object v = f.GetValue(c);
            if (v is T tv) value = tv;
            else
            {
                try { value = (T)System.Convert.ChangeType(v, typeof(T)); } catch { }
            }
        }
    }

    private Camera GetCamera()
    {
        if (_cam != null) return _cam;
        _cam = Camera.main;
        if (_cam == null)
        {
            if (Camera.allCamerasCount > 0)
            {
                var arr = Camera.allCameras;
                if (arr != null && arr.Length > 0) _cam = arr[0];
            }
        }
        return _cam;
    }

    private Vector3 GuessCenteredAnchor()
    {
        if (_plane == GridPlane.XZ)
            return new Vector3(-_w * _tile * 0.5f, _groundConst, -_h * _tile * 0.5f);
        else
            return new Vector3(-_w * _tile * 0.5f, -_h * _tile * 0.5f, _groundConst);
    }

    private Vector2Int GetFootprint()
    {
        switch (_tool)
        {
            case BuildTool.PlaceConstructionBoard: return new Vector2Int(3, 1);
            default: return Vector2Int.one;
        }
    }

    // Optional tiny debug overlay to help diagnose placement in the wild
    private void OnGUI()
    {
        SyncToolFromController();
        if (_tool == BuildTool.None) return;
        var style = new UnityEngine.GUIStyle(UnityEngine.GUI.skin.box);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = 14;
        string plane = _plane.ToString();
        string reason = _canPlace ? "" : InvalidReason();
        string worldStr = _plane == GridPlane.XZ ? $"{_snapWorldPos.x:F2},{_snapWorldPos.y:F2},{_snapWorldPos.z:F2}" : $"{_snapWorldPos.x:F2},{_snapWorldPos.y:F2},{_snapWorldPos.z:F2}";
        string anchorStr = _plane == GridPlane.XZ ? $"{_anchor.x:F2},{_groundConst:F2},{_anchor.z:F2}" : $"{_anchor.x:F2},{_anchor.y:F2},{_groundConst:F2}";
        string text = $"Plane: {plane}\nTool: {_tool}\nDef: {_activeBuildingDefId} -> GhostVisual: {(_ghostVDef!=null ? _ghostVDef.id : "(default)")}\nGrid: {_snapGridPos.x},{_snapGridPos.y}\nWorld: {worldStr}\nAnchor: {anchorStr}\nHaveBounds: {_haveBounds}  Tile: {_tile:F2}\nGridLayer: {_gridLayer}  CamHasLayer: {CameraHasLayer(_gridLayer)}\nGhostActive: {(_ghost!=null)}\nFoot: {_footSize.x}x{_footSize.y}\nValid: {_canPlace} {reason}";
        UnityEngine.GUI.Label(new UnityEngine.Rect(8, 8, 620, 190), text, style);
    }

    private bool CameraHasLayer(int layer)
    {
        var cam = GetCamera(); if (cam == null) return true;
        return (cam.cullingMask & (1 << layer)) != 0;
    }

    private bool TryGetMouseOnGround(Camera cam, out Vector3 world)
    {
        // 1) Try raycast to plane (works in both perspective and ortho)
        Vector2 mp = GetMousePosition();
        Ray ray = cam.ScreenPointToRay(new Vector3(mp.x, mp.y, 0f));
        Plane ground = _plane == GridPlane.XZ
            ? new Plane(Vector3.up, new Vector3(0f, _groundConst, 0f))
            : new Plane(Vector3.forward, new Vector3(0f, 0f, _groundConst));
        if (ground.Raycast(ray, out float enter))
        {
            world = ray.GetPoint(enter);
            return true;
        }

        // 2) Fallback via depth from ground reference (covers edge camera setups)
        Vector3 groundRef = _plane == GridPlane.XZ ? new Vector3(0f, _groundConst, 0f) : new Vector3(0f, 0f, _groundConst);
        float depth = cam.WorldToScreenPoint(groundRef).z;
        world = cam.ScreenToWorldPoint(new Vector3(mp.x, mp.y, depth));
        if (!float.IsNaN(world.x)) return true;

        // 3) Final fallback: project using camera basis onto plane
        Vector3 origin = cam.transform.position;
        Vector3 dir = (cam.transform.forward.sqrMagnitude < 0.0001f) ? Vector3.forward : cam.transform.forward;
        Ray ray2 = new Ray(origin, dir);
        if (ground.Raycast(ray2, out float enter2)) { world = ray2.GetPoint(enter2); return true; }
        return true;
    }

    private void ResolveActiveVisual()
    {
        // Ensure defs are loaded once
        if (DefDatabase.Visuals.Count == 0) DefDatabase.LoadAll();

        // For now, only Construction Board is placed by this tool
        // This maps to core.Building.ConstructionBoard → core.Visual.Board_Default
        BuildingDef bdef = null;
        DefDatabase.Buildings.TryGetValue("core.Building.ConstructionBoard", out bdef);
        if (bdef == null)
        {
            // synthesize a default
            _ghostVDef = new VisualDef();
            return;
        }
        VisualDef vdef = null;
        if (!string.IsNullOrEmpty(bdef.visual_ref)) DefDatabase.Visuals.TryGetValue(bdef.visual_ref, out vdef);
        _ghostVDef = vdef ?? new VisualDef();
    }

    private void SetGhostColor(Color c)
    {
        if (_ghostMr != null)
        {
            // Ensure we're not editing a shared material instance in play mode
            if (Application.isPlaying)
            {
                if (_ghostMr.material != null) _ghostMr.material.color = c;
            }
            else
            {
                if (_ghostMr.sharedMaterial != null) _ghostMr.sharedMaterial.color = c;
            }
        }
    }

    private void DestroyMarker()
    {
        if (_marker != null)
        {
            Destroy(_marker);
            _marker = null;
            _markerMr = null;
        }
    }

    private string InvalidReason()
    {
        if (!IsInsideGrid(_snapGridPos, _footSize)) return "(out of bounds)";
        if (_tool == BuildTool.PlaceConstructionBoard && BuildModeController.UniqueBuildingExists<ConstructionBoard>())
            return "(unique-per-map already placed)";
        var all = UnityEngine.Object.FindObjectsByType<Building>(FindObjectsSortMode.None);
        foreach (var b in all)
            if (RectOverlaps(b.GridPos, b.size, _snapGridPos, _footSize)) return $"(overlap with {b.displayName})";
        return "";
    }

    private void CancelPlacement()
    {
        var bm = BuildModeController.Instance;
        if (bm != null)
        {
            bm.SetTool(BuildTool.None);
        }
        else
        {
            SetTool(BuildTool.None);
        }
        DestroyGhost();
    }

    private void DestroyGhost()
    {
        if (_ghost != null)
        {
            UnityEngine.Object.Destroy(_ghost);
            _ghost = null;
            _ghostMr = null;
        }
    }
}
