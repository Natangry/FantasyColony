using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public static class HardRestart
{
    public static void RebootToIntro()
    {
        Time.timeScale = 1f;

        // Destroy our known persistent autos if present
        DestroyIfExists("BuildSystems (Auto)");
        DestroyIfExists("BuildPalette (Auto)");
        DestroyIfExists("BuildCanvas (Auto)");
        DestroyIfExists("PauseCanvas (Auto)");

        // Destroy any EventSystem we created
        var es = Object.FindFirstObjectByType<EventSystem>();
        if (es != null) Object.Destroy(es.gameObject);

        // Reset bootstrap singletons/flags
        var bb = typeof(BuildBootstrap);
        var flag = bb.GetField("_defsLoadedOnce", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        if (flag != null) flag.SetValue(null, false);

        // Choose intro-like scene by name/path; fallback to index 0
        int target = FindIntroSceneIndex();
        Debug.Log("[HardRestart] Rebooting to scene index " + target + " (" + SceneUtility.GetScenePathByBuildIndex(target) + ")");
        SceneManager.LoadScene(target);
    }

    static int FindIntroSceneIndex()
    {
        int count = SceneManager.sceneCountInBuildSettings;
        if (count <= 0) return 0;
        for (int i = 0; i < count; i++)
        {
            var path = SceneUtility.GetScenePathByBuildIndex(i);
            if (string.IsNullOrEmpty(path)) continue;
            var low = path.ToLowerInvariant();
            if (low.Contains("intro") || low.Contains("title") || low.Contains("menu") || low.Contains("start"))
                return i;
        }
        return 0;
    }

    static void DestroyIfExists(string name)
    {
        var go = GameObject.Find(name);
        if (go != null) Object.Destroy(go);
    }
}

