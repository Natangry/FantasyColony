using UnityEngine;

namespace FantasyColony.Core
{
    /// <summary>
    /// Single entry point. We don't rely on authored Unity scenes; everything is spawned at runtime.
    /// </summary>
    public static class AppBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
            // Guard against duplicates (domain reloads, additive loads)
#if UNITY_2023_1_OR_NEWER
            if (Object.FindFirstObjectByType<AppHost>() != null) return;
#else
            if (Object.FindObjectOfType<AppHost>() != null) return;
#endif
            // Root host object that survives for the whole app lifetime.
            var root = new GameObject("AppRoot");
            Object.DontDestroyOnLoad(root);
            root.AddComponent<AppHost>();
        }
    }
}
