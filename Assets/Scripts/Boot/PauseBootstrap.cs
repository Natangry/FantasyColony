using UnityEngine;

public static class PauseBootstrap
{
    private static bool spawned;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsurePauseController()
    {
#if NO_INTRO
        // Even if NO_INTRO is defined, we still want pause in gameplay.
#endif
        if (spawned) return;
#if UNITY_2022_2_OR_NEWER
        var existing = Object.FindAnyObjectByType<PauseController>();
#else
        var existing = Object.FindObjectOfType<PauseController>();
#endif
        GameObject go;
        if (existing != null)
        {
            go = existing.gameObject;
        }
        else
        {
            go = new GameObject("PauseController (Auto)");
            Object.DontDestroyOnLoad(go);
            go.AddComponent<PauseController>();
        }

        if (go.GetComponent<GameClock>() == null) go.AddComponent<GameClock>();
        if (go.GetComponent<ClockHUD>() == null) go.AddComponent<ClockHUD>();
        spawned = true;
    }
}
