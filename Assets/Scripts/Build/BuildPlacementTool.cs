using System;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Handles in-world placement of buildings while a placement tool is active.
/// This version targets a top-down world on the XZ plane (y = up).
/// </summary>
public class BuildPlacementTool : MonoBehaviour
{
    private BuildTool _tool = BuildTool.None;
    private GameObject _ghost;
    private SpriteRenderer _ghostSr;
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

    private Camera _cam;
    private static Sprite _whiteSprite;

    private void SyncToolFromController()
    {
        var bm = BuildModeController.Instance;
        if (bm == null) return;
        if (_tool != bm.CurrentTool)
        {
            _tool = bm.CurrentTool;
            _footSize = GetFootprint();
        }
    }

    private void LateUpdate()
    {
        if (_tool == BuildTool.None)
        {
            DestroyGhost();
            return;
        }

        ReadGridInfo();
        UpdateGhost();

        if (Input.GetMouseButtonDown(1)) // cancel tool
        {
            SetTool(BuildTool.None);
            return;
        }

        if (Input.GetMouseButtonDown(0)) // left-click to place
        {
            TryPlace();
        }
    }

    public void SetTool(BuildTool tool)
    {
        _tool = tool;
        if (_tool == BuildTool.None) DestroyGhost();
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
                    _haveBounds = true;
                }
                else
                {
                    // XZ world (thin Y)
                    _plane = GridPlane.XZ;
                    GridSpace.Plane = _plane;
                    _groundConst = b.center.y;
                    _gridMinWorld = new Vector3(b.min.x, _groundConst, b.min.z);
                    _haveBounds = true;
                }
            }
            else
            {
                // Fallback: assume XY with z=0
                _plane = GridPlane.XY;
                GridSpace.Plane = _plane;
                _groundConst = 0f;
            }
        }
        else
        {
            _plane = GridPlane.XY;
            GridSpace.Plane = _plane;
            _groundConst = 0f;
        }
    }

    private void UpdateGhost()
    {
        // Keep local tool state in sync with the controller every frame
        SyncToolFromController();

        if (_tool == BuildTool.None)
        {
            DestroyGhost();
            return;
        }
        var cam = GetCamera();
        if (cam == null) return; // can't place without a camera

        // Raycast from camera to the detected ground plane
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane ground = _plane == GridPlane.XZ
            ? new Plane(Vector3.up, new Vector3(0f, _groundConst, 0f))
            : new Plane(Vector3.forward, new Vector3(0f, 0f, _groundConst));
        if (!ground.Raycast(ray, out float enter)) return;
        Vector3 world = ray.GetPoint(enter);

        // Determine anchor for snapping: true bottom-left if we have bounds, else (0,0)
        _anchor = _haveBounds ? _gridMinWorld : GuessCenteredAnchor();
        _footSize = GetFootprint();

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

        // Color
        SetGhostColor(_canPlace ? new Color(0.2f, 0.9f, 0.2f, 0.35f) : new Color(0.9f, 0.2f, 0.2f, 0.35f));
        _ghost.transform.position = _snapWorldPos;
        _ghost.transform.localScale = new Vector3(_footSize.x * _tile, _footSize.y * _tile, 1f);
        _ghost.transform.rotation = _plane == GridPlane.XZ ? Quaternion.Euler(-90f, 0f, 0f) : Quaternion.identity;
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
                break;
            }
        }

        // After successful placement, clear tool
        SetTool(BuildTool.None);
    }

    private void EnsureGhost()
    {
        if (_ghost != null) return;
        _ghost = new GameObject("Build Ghost");
        _ghost.layer = 0;
        _ghostSr = _ghost.AddComponent<SpriteRenderer>();
        if (_whiteSprite == null)
        {
            var tex = new Texture2D(1,1, TextureFormat.RGBA32, false); tex.SetPixel(0,0, Color.white); tex.Apply();
            _whiteSprite = Sprite.Create(tex, new Rect(0,0,1,1), new Vector2(0.5f,0.5f), 1f);
        }
        _ghostSr.sprite = _whiteSprite;
        _ghostSr.sortingOrder = 1000;
        // Orient to active plane
        _ghost.transform.rotation = _plane == GridPlane.XZ ? Quaternion.Euler(-90f, 0f, 0f) : Quaternion.identity;
        SetGhostColor(new Color(0.2f, 0.9f, 0.2f, 0.35f));
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
        string text = $"Plane: {plane}\nTool: {_tool}\nGrid: {_snapGridPos.x},{_snapGridPos.y}\nFoot: {_footSize.x}x{_footSize.y}\nValid: {_canPlace}";
        UnityEngine.GUI.Label(new UnityEngine.Rect(8, 8, 260, 96), text, style);
    }

    private void SetGhostColor(Color c)
    {
        if (_ghostSr != null) _ghostSr.color = c;
    }

    private void DestroyGhost()
    {
        if (_ghost != null)
        {
            UnityEngine.Object.Destroy(_ghost);
            _ghost = null;
            _ghostSr = null;
        }
    }
}
