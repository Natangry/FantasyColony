using UnityEngine;

/// <summary>
/// Ensures a SelectionController exists at runtime in any scene.
/// (Updated for marquee selection support)
/// </summary>
public static class SelectionBootstrap
{
    private static bool spawned;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureSelectionController()
    {
        if (spawned) return;
#if UNITY_2022_2_OR_NEWER
        var existing = Object.FindAnyObjectByType<SelectionController>();
#else
        var existing = Object.FindObjectOfType<SelectionController>();
#endif
        if (existing != null) { spawned = true; return; }

        var go = new GameObject("SelectionController (Auto)");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<SelectionController>();
        // HUD that draws the bottom-left info panel and right-side gizmos
        go.AddComponent<SelectionHUD>();
        spawned = true;
    }
}

