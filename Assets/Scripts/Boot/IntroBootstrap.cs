using UnityEngine;

/// <summary>
/// Ensures the temporary IntroScreen (Start/Quit) exists when the game boots,
/// independent of scene setup or build order.
/// </summary>
public static class IntroBootstrap
{
    private static bool spawnedOnce;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureIntro()
    {
#if NO_INTRO
        return;
#endif
        if (spawnedOnce)
            return;

        // If an IntroScreen already exists in the scene (FindAnyObjectByType/FindObjectOfType), do nothing.
#if UNITY_2022_2_OR_NEWER
        var existing = Object.FindAnyObjectByType<IntroScreen>();
#else
        var existing = Object.FindObjectOfType<IntroScreen>();
#endif
        if (existing != null)
        {
            spawnedOnce = true;
            return;
        }

        // Otherwise, create one.
        var go = new GameObject("IntroScreen (Auto)");
        // Keep it alive across scene loads; the IntroScreen script can hide/clear itself on Start.
        Object.DontDestroyOnLoad(go);
        go.AddComponent<IntroScreen>();

        spawnedOnce = true;
    }
}
