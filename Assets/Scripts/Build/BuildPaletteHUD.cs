using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using FantasyColony.Defs;

public class BuildPaletteHUD : MonoBehaviour
{
    Vector2 _scroll;

    BuildModeController Ctrl => BuildModeController.Instance;

    Rect GetPanelRect()
    {
        // More aggressive sizing for high-DPI / large resolutions
        float w = Mathf.Max(520f, Screen.width * 0.38f);
        float h = Mathf.Max(600f, Screen.height * 0.72f);
        return new Rect(16, 64, w, h);
    }

    static bool IsIntroScene()
    {
        var n = SceneManager.GetActiveScene().name;
        if (string.IsNullOrEmpty(n)) return false;
        n = n.ToLowerInvariant();
        return n.Contains("intro") || n.Contains("menu") || n.Contains("title");
    }

    void OnGUI()
    {
        if (IsIntroScene()) return;
        if (BuildModeController.Instance == null || !BuildModeController.Instance.IsActive) return;

        // Self-heal to guarantee systems are present
        BuildBootstrap.Ensure();

        var _panelRect = GetPanelRect();

        // --- DPI/UI scaling ---
        float scale = Mathf.Clamp(Mathf.Min(Screen.width / 1920f, Screen.height / 1080f), 1.0f, 2.5f);
        var prevMatrix = GUI.matrix;
        // Avoid empty tab look by drawing without window chrome at tiny scales
        GUIUtility.ScaleAroundPivot(new Vector2(scale, scale), Vector2.zero);
        var drawRect = new Rect(_panelRect.x / scale, _panelRect.y / scale, _panelRect.width / scale, _panelRect.height / scale);

        // Temporarily bump font sizes
        var skin = GUI.skin;
        int oldLabel = skin.label.fontSize;
        int oldButton = skin.button.fontSize;
        int oldWindow = skin.window.fontSize;
        skin.label.fontSize = Mathf.RoundToInt(14 * scale);
        skin.button.fontSize = Mathf.RoundToInt(14 * scale);
        skin.window.fontSize = Mathf.RoundToInt(16 * scale);

        GUILayout.BeginArea(drawRect); // no window chrome to prevent stray tab
        var activeName = (Ctrl.SelectedBuildingDef != null) ? $" – Selected: {Ctrl.SelectedBuildingDef.label ?? Ctrl.SelectedBuildingDef.defName}" : "";
        GUILayout.Label("Build Palette" + activeName);
        GUILayout.Label("Click a def to arm the tool, then left-click ground to place. Esc cancels.");

        // Try to enumerate building defs (fallback to a single Construction Board def if database not ready)
        var defs = GetPaletteDefs();
        _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.ExpandHeight(true));
        foreach (var def in defs)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(def.label ?? def.defName, GUILayout.Width(260));
            if (GUILayout.Button("Select", GUILayout.Width(Mathf.RoundToInt(160 * scale))))
            {
                // For now we only have a placement tool for Construction Board.
                // Down the road this can dispatch different tools per def.category/type.
                BuildModeController.Instance.SetPlacingBuilding(def);
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();

        GUILayout.EndArea();

        // Restore GUI state
        skin.label.fontSize = oldLabel;
        skin.button.fontSize = oldButton;
        skin.window.fontSize = oldWindow;
        GUI.matrix = prevMatrix;
    }

    static List<BuildingDef> GetPaletteDefs()
    {
        // Use static DefDatabase (no singleton). Otherwise return a local fallback.
        var list = new List<BuildingDef>();
        if (DefDatabase.Buildings != null && DefDatabase.Buildings.Count > 0)
        {
            foreach (var b in DefDatabase.Buildings)
            {
                if (b.showInPalette) list.Add(b);
            }
            // If nothing was explicitly marked for palette, surface Construction Board if present
            if (list.Count == 0)
            {
                var cb = DefDatabase.Buildings.FirstOrDefault(x => x.defName == "ConstructionBoard");
                if (cb != null) list.Add(cb);
            }
            if (list.Count > 0) return list;
        }

        // Fallback minimal def for bring-up
        list.Add(new BuildingDef
        {
            defName = "ConstructionBoard",
            label = "Construction Board",
            width = 3,
            height = 1,
            unique = true,
            showInPalette = true,
            visualRef = "ConstructionBoardVisual",
            category = "Stations",
            allowedRotations = new List<string> { "N" }
        });
        return list;
    }
}
