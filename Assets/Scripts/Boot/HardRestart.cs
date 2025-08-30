using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Text;

public static class HardRestart
{
    public static void RebootToIntro()
    {
        Time.timeScale = 1f;

        // Cancel/close gameplay overlays and systems
        DestroyIfExists("BuildSystems (Auto)");
        DestroyIfExists("BuildPalette (Auto)");
        DestroyIfExists("BuildCanvas (Auto)");
        DestroyIfExists("PauseCanvas (Auto)");
        DestroyIfExists("PauseMenuController");
        DestroyIfExists("BuildPalette (Auto)");
        DestroyAllOfType<EventSystem>();
        DestroyAllOfType<BuildModeController>();
        DestroyAllOfType<BuildPlacementTool>();
        DestroyAllOfType<BuildPaletteHUD>();
        DestroyAllOfType<SimpleGridMap>();

        // Reset bootstrap singletons/flags
        var bb = typeof(BuildBootstrap);
        var flag = bb.GetField("_defsLoadedOnce", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        if (flag != null) flag.SetValue(null, false);

        // Clean up assets to mimic a fresh boot
        Resources.UnloadUnusedAssets();
        System.GC.Collect();

        // Show the intro overlay (scene-less), real if present or fallback otherwise
        AppBootstrap.ShowIntroOverlay();

        Debug.Log("[HardRestart] Returned to Intro overlay (scene-less).");
    }

    // --- helpers ---

    static void DestroyAllOfType<T>() where T : Component
    {
        var arr = Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var c in arr)
        {
            Object.Destroy(c.gameObject);
        }
    }

    static void DestroyIfExists(string name)
    {
        var go = GameObject.Find(name);
        if (go != null) Object.Destroy(go);
    }
}

