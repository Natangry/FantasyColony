using UnityEngine;

/// <summary>
/// Ensures a PawnInteractionManager exists at runtime to coordinate pawn-to-pawn interactions.
/// </summary>
public static class InteractionBootstrap
{
    private static bool spawned;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureInteractionManager()
    {
        if (spawned) return;
#if UNITY_2022_2_OR_NEWER
        var existing = Object.FindAnyObjectByType<PawnInteractionManager>();
#else
        var existing = Object.FindObjectOfType<PawnInteractionManager>();
#endif
        if (existing != null) { spawned = true; return; }

        var go = new GameObject("PawnInteractionManager (Auto)");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<PawnInteractionManager>();
        spawned = true;
    }
}
