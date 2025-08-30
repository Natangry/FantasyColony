using UnityEngine;
using FantasyColony.Defs;
using System.Linq;

/// <summary>
/// Guarantees build systems exist at runtime and that defs are loaded.
/// This runs automatically after scene load.
/// </summary>
public static class BuildBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void EnsureOnLoad()
    {
        Ensure();
    }

    public static GameObject Ensure()
    {
        var go = GameObject.Find("BuildSystems (Auto)");
        if (go == null)
        {
            go = new GameObject("BuildSystems (Auto)");
            Object.DontDestroyOnLoad(go);
        }

        if (go.GetComponent<BuildModeController>() == null)
            go.AddComponent<BuildModeController>();
        if (go.GetComponent<BuildPlacementTool>() == null)
            go.AddComponent<BuildPlacementTool>();

        // Make sure defs are available
        try
        {
            if (DefDatabase.Buildings == null || DefDatabase.Visuals == null ||
                DefDatabase.Buildings.Count == 0 || DefDatabase.Visuals.Count == 0)
            {
                DefDatabase.LoadAll();
            }
        }
        catch
        {
            // ignore – bring-up should still run with fallbacks
        }

        // Ensure there is a palette HUD
        var pal = GameObject.Find("BuildPalette (Auto)");
        if (pal == null)
        {
            pal = new GameObject("BuildPalette (Auto)");
            pal.AddComponent<BuildPaletteHUD>();
            Object.DontDestroyOnLoad(pal);
            Debug.Log("[BuildBootstrap] Spawned BuildPalette");
        }

        return go;
    }
}

