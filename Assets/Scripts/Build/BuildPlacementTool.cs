using System;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Handles in-world placement of buildings while a placement tool is active.
/// </summary>
public class BuildPlacementTool : MonoBehaviour
{
    private BuildTool _tool = BuildTool.None;
    private GameObject _ghost;
    private SpriteRenderer _ghostSr;
    private bool _canPlace;
    private Vector3 _snapWorldPos;
    private Vector2Int _snapGridPos;

    // Grid snapshot via reflection to avoid tight coupling
    private float _tile = 1f; private int _w = 128; private int _h = 128;
    private Vector3 _gridMinWorld; // bottom-left world corner of tile (0,0)
    private bool _haveBounds;
    private bool _assumeCentered;

    private Camera _cam;
    private static Sprite _whiteSprite;

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

            // Try to find a Renderer to get true world bounds
            var rend = (grid as Component).GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                var b = rend.bounds;
                _gridMinWorld = new Vector3(b.min.x, b.min.y, 0f);
                _haveBounds = true;
            }
        }
    }

    private void UpdateGhost()
    {
        var cam = GetCamera();
        var m = Input.mousePosition;
        if (cam == null) return; // can't place without a camera
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(m.x, m.y, Mathf.Abs(cam.transform.position.z)));

        // Determine anchor for snapping: true bottom-left if we have bounds, else (0,0)
        Vector3 anchor = _haveBounds ? _gridMinWorld : GuessCenteredAnchor();

        // Snap to grid anchored at bottom-left
        float tx = Mathf.Round((world.x - anchor.x) / _tile) * _tile + anchor.x;
        float ty = Mathf.Round((world.y - anchor.y) / _tile) * _tile + anchor.y;
        _snapWorldPos = new Vector3(tx, ty, 0f);

        int gx = Mathf.RoundToInt((tx - anchor.x) / _tile);
        int gy = Mathf.RoundToInt((ty - anchor.y) / _tile);
        _snapGridPos = new Vector2Int(gx, gy);

        EnsureGhost();

        // Validate
        _canPlace = IsInsideGrid(_snapGridPos) && IsPlacementAllowedHere();

        // Color
        SetGhostColor(_canPlace ? new Color(0.2f, 0.9f, 0.2f, 0.7f) : new Color(0.9f, 0.2f, 0.2f, 0.7f));
        _ghost.transform.position = _snapWorldPos;
        _ghost.transform.localScale = Vector3.one * _tile;
    }

    private bool IsInsideGrid(Vector2Int p)
    {
        if (!_haveBounds) // be permissive if we don't know exact bounds
            return p.x >= -_w && p.y >= -_h && p.x < _w * 2 && p.y < _h * 2;
        return p.x >= 0 && p.y >= 0 && p.x < _w && p.y < _h;
    }

    private bool IsPlacementAllowedHere()
    {
        if (_tool == BuildTool.PlaceConstructionBoard && BuildModeController.UniqueBuildingExists<ConstructionBoard>())
            return false; // unique per map

        // Check for any existing Building occupying this tile (1x1 for now)
        Building[] all;
#if UNITY_2023_1_OR_NEWER
        all = UnityEngine.Object.FindObjectsByType<Building>(FindObjectsSortMode.None);
#else
        all = FindObjectsOfType<Building>();
#endif
        foreach (var b in all)
        {
            if (b.Occupies(_snapGridPos)) return false;
        }
        return true;
    }

    private void TryPlace()
    {
        if (!_canPlace) return;

        Transform parent = EnsureBuildingsParent();

        switch (_tool)
        {
            case BuildTool.PlaceConstructionBoard:
            {
                var go = new GameObject("Construction Board");
                go.transform.SetParent(parent, worldPositionStays: true);
                go.transform.position = _snapWorldPos;

                var board = go.AddComponent<ConstructionBoard>();
                board.displayName = "Construction Board";
                board.uniquePerMap = true;
                board.size = new Vector2Int(1, 1);
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
#if UNITY_2023_1_OR_NEWER
        _cam = Camera.main != null ? Camera.main : (Camera.GetAllCamerasCount() > 0 ? Camera.allCameras[0] : null);
#else
        _cam = Camera.main != null ? Camera.main : (Camera.allCamerasCount > 0 ? Camera.allCameras[0] : null);
#endif
        return _cam;
    }

    private Vector3 GuessCenteredAnchor()
    {
        // Assume grid is centered around (0,0) if no renderer bounds are found
        _assumeCentered = true;
        return new Vector3(-_w * _tile * 0.5f, -_h * _tile * 0.5f, 0f);
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
