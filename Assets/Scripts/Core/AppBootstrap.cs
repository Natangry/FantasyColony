using UnityEngine;

namespace FantasyColony.Core
{
    /// <summary>
    /// Single entry point. We don't rely on authored Unity scenes; everything is spawned at runtime.
    /// </summary>
    public static class AppBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            // Root host object that survives for the whole app lifetime.
            var root = new GameObject("AppRoot");
            Object.DontDestroyOnLoad(root);
            root.AddComponent<AppHost>();
        }
    }
}
