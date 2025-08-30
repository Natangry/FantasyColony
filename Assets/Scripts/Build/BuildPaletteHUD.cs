using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using FantasyColony.Defs;

public class BuildPaletteHUD : MonoBehaviour
{
    Vector2 _scroll;
    Rect _panelRect = new Rect(16, 64, 320, 420);

    void OnGUI()
    {
        if (BuildModeController.Instance == null || !BuildModeController.Instance.IsBuildModeEnabled) return;

        GUILayout.BeginArea(_panelRect, GUI.skin.window);
        GUILayout.Label("Build Palette");

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
        // If a DefDatabase exists, use it. Otherwise return a local fallback.
        var list = new List<BuildingDef>();
        try
        {
            var db = DefDatabase.Instance; // adjust if your singleton accessor differs
            if (db != null && db.Buildings != null && db.Buildings.Count > 0)
            {
                foreach (var b in db.Buildings)
                {
                    if (b.showInPalette) list.Add(b);
                }
                // If nothing was explicitly marked for palette, surface Construction Board if present
                if (list.Count == 0)
                {
                    var cb = db.Buildings.FirstOrDefault(x => x.defName == "ConstructionBoard");
                    if (cb != null) list.Add(cb);
                }
                if (list.Count > 0) return list;
            }
        }
        catch { /* fall through to fallback */ }

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
