using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using FantasyColony.Defs;

public class BuildPaletteHUD : MonoBehaviour
{
    Vector2 _scroll;

    BuildModeController Ctrl => BuildModeController.Instance;

    Rect GetPanelRect()
    {
        // Responsive panel sizing for high-DPI / large resolutions
        float w = Mathf.Clamp(Screen.width * 0.28f, 420f, 760f);
        float h = Mathf.Clamp(Screen.height * 0.62f, 540f, 980f);
        return new Rect(16, 64, w, h);
    }

    void OnGUI()
    {
        if (BuildModeController.Instance == null || !BuildModeController.Instance.IsActive) return;

        // Ensure systems exist (harmless if already present)
        BuildBootstrap.Ensure();

        var _panelRect = GetPanelRect();

        GUILayout.BeginArea(_panelRect, GUI.skin.window);
        var activeName = (Ctrl.SelectedBuildingDef != null) ? $" – Selected: {Ctrl.SelectedBuildingDef.label ?? Ctrl.SelectedBuildingDef.defName}" : "";
        GUILayout.Label("Build Palette" + activeName);

        // Try to enumerate building defs (fallback to a single Construction Board def if database not ready)
        var defs = GetPaletteDefs();
        _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.ExpandHeight(true));
        foreach (var def in defs)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(def.label ?? def.defName, GUILayout.Width(200));
            if (GUILayout.Button("Select", GUILayout.Width(80)))
            {
                // For now we only have a placement tool for Construction Board.
                // Down the road this can dispatch different tools per def.category/type.
                BuildModeController.Instance.SetPlacingBuilding(def);
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();

        GUILayout.EndArea();
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
            category = "Stations"
        });
        return list;
    }
}
