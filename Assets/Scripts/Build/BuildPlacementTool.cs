using UnityEngine;
using FantasyColony.Defs;
using System.Linq;
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
            EnsureGhost(ctrl.SelectedBuildingDef);
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
        var mpos = Input.mousePosition;
        var world = cam.ScreenToWorldPoint(new Vector3(mpos.x, mpos.y, -cam.transform.position.z));
        world.z = 0f; // 2D XY plane
        var snapped = SnapToGrid(world, def);
        if (ghostGO != null)
        {
            ghostGO.transform.position = snapped;
        }

        // Click to place
        if (Input.GetMouseButtonDown(0))
        {
            TryPlace(def, snapped);
        }
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

    void ApplyScaleForSpriteOrFallback(GameObject go, BuildingDef def, Sprite spriteOrNull)
    {
        if (spriteOrNull != null)
            go.transform.localScale = Vector3.one; // rely on sprite PPU (hi-res 64 PPU)
        else
            go.transform.localScale = new Vector3(def.width * tileSize, def.height * tileSize, 1f); // fallback white unit sprite
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
        go.transform.position = pos;

        // Attach the specific behaviour for Construction Board MVP
        var board = go.AddComponent<ConstructionBoard>();
        // If your ConstructionBoard has an OnPlaced API, you can call it here:
        // board.OnPlaced(new Vector2Int(def.width, def.height), tileSize);

        // Visuals: prefer real sprite from VisualDef.spritePath; fall back to white unit
        var sr = go.AddComponent<SpriteRenderer>();
        var spr = LoadSpriteFor(def);
        if (spr != null) sr.sprite = spr; else sr.sprite = MakeUnitSprite();
        ApplyScaleForSpriteOrFallback(go, def, spr);

        ClearTool(); // place once for MVP
    }

    void ClearTool()
    {
        active = BuildTool.None;
        EnsureGhostDestroyed();
    }

    Vector3 SnapToGrid(Vector3 world, BuildingDef def)
    {
        float w = def.width * tileSize;
        float h = def.height * tileSize;
        // Snap to tile grid, keep pivot at center of the footprint
        float x = Mathf.Floor(world.x / tileSize) * tileSize + (w * 0.5f) - (tileSize * 0.5f);
        float y = Mathf.Floor(world.y / tileSize) * tileSize + (h * 0.5f) - (tileSize * 0.5f);
        return new Vector3(x, y, 0f);
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
        ApplyScaleForSpriteOrFallback(ghostGO, def, spr);
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
