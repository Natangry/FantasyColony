using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// One-per-map station that offers the Builder job via slots.
/// Footprint: 3x1 tiles (horizontal), anchored at bottom-left.
/// </summary>
public class ConstructionBoard : Building
{
    [SerializeField, Min(0)] private int builderSlots = 1;
    private bool _showInspector;

    private void Start()
    {
        id = "construction_board";
        displayName = string.IsNullOrEmpty(displayName) ? "Construction Board" : displayName;
        uniquePerMap = true;
        size = new Vector2Int(3, 1);
    }

    private void OnMouseUpAsButton()
    {
        _showInspector = !_showInspector;
    }

    public override void OnPlaced(Vector2Int grid, float tile)
    {
        base.OnPlaced(grid, tile);
        JobService js;
#if UNITY_2023_1_OR_NEWER
        js = UnityEngine.Object.FindFirstObjectByType<JobService>();
#else
        js = FindObjectOfType<JobService>();
#endif
        if (js != null)
        {
            js.SetSlots(this, JobType.Builder, builderSlots);
        }

        // Visual now handled by SpriteVisualFactory2D from the placement tool.
        // Keep logic & collider only.
    }

    /*private int DetectGridLayer()
    {
        // Try to find the grid's renderer & inherit its layer
        var grid = FindAnyGridRenderer();
        if (grid != null) return grid.gameObject.layer;
        return 0; // Default
    }

    private static Renderer FindAnyGridRenderer()
    {
        var grid = Object.FindAnyObjectByType<Component>();
        // Try a few common names first
        var go = GameObject.Find("Grid");
        if (go != null)
        {
            var r = go.GetComponentInChildren<Renderer>();
            if (r != null) return r;
        }
        // Fallback: find any renderer tagged as ground or in root
        foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            if (r.gameObject.name.ToLower().Contains("grid") || r.gameObject.name.ToLower().Contains("ground")) return r;
        }
        return null;
    }*/

    private void OnGUI()
    {
        if (!_showInspector) return;

        JobService js;
#if UNITY_2023_1_OR_NEWER
        js = UnityEngine.Object.FindFirstObjectByType<JobService>();
#else
        js = FindObjectOfType<JobService>();
#endif
        if (js == null) return;

        Rect r = new Rect(20f, Screen.height * 0.6f, Mathf.Max(260f, Screen.width * 0.22f), Screen.height * 0.35f);
        GUILayout.BeginArea(r, displayName, GUI.skin.window);

        GUILayout.Label("Builder job", GUI.skin.label);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("-", GUILayout.Width(32f)))
        {
            builderSlots = Mathf.Max(0, builderSlots - 1);
            js.SetSlots(this, JobType.Builder, builderSlots);
        }
        GUILayout.Label($"Slots: {builderSlots}", GUILayout.Width(120f));
        if (GUILayout.Button("+", GUILayout.Width(32f)))
        {
            builderSlots = Mathf.Min(99, builderSlots + 1);
            js.SetSlots(this, JobType.Builder, builderSlots);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(6f);
        var assigned = js.AssignedFor(this, JobType.Builder);
        GUILayout.Label($"Assigned ({assigned.Count})");
        int show = Mathf.Min(assigned.Count, 5);
        for (int i = 0; i < show; i++)
        {
            GUILayout.Label($"- {assigned[i].name}");
        }
        if (assigned.Count > show)
        {
            GUILayout.Label($"...and {assigned.Count - show} more");
        }

        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Close")) _showInspector = false;
        GUILayout.EndArea();
    }
}
