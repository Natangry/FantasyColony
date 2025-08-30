using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;
using System.Text;

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
        DestroyIfExists("PauseMenuController");
        DestroyAllOfType<EventSystem>();
        DestroyAllOfType<BuildModeController>();
        DestroyAllOfType<BuildPlacementTool>();
        DestroyAllOfType<BuildPaletteHUD>();

        // Destroy any EventSystem we created
        var es = Object.FindFirstObjectByType<EventSystem>();
        if (es != null) Object.Destroy(es.gameObject);

        // Reset bootstrap singletons/flags
        var bb = typeof(BuildBootstrap);
        var flag = bb.GetField("_defsLoadedOnce", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        if (flag != null) flag.SetValue(null, false);

        // Log all scenes in build settings
        Debug.Log(GetBuildScenesLog());

        // Prefer explicit config if present
        var cfg = GameStartupConfig.Load();
        if (cfg != null && !string.IsNullOrEmpty(cfg.introScenePath))
        {
            Debug.Log("[HardRestart] Restart using configured intro scene path: " + cfg.introScenePath);
            LoadClean(cfg.introScenePath);
            return;
        }

        // Choose intro scene:
        // 1) The scene we launched into (BootSession)
        // 2) intro/title/menu heuristic
        // 3) index 0 fallback
        int target = (BootSession.InitialSceneIndex >= 0) ? BootSession.InitialSceneIndex : FindIntroSceneIndex();
        var path = SceneUtility.GetScenePathByBuildIndex(target);
        Debug.Log("[HardRestart] Restart using scene index " + target + " (" + path + ")");
        LoadClean(target);
    }

    static void LoadClean(int buildIndex)
    {
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
        SceneManager.LoadScene(buildIndex);
    }
    static void LoadClean(string scenePath)
    {
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
        SceneManager.LoadScene(scenePath);
    }

    static string GetBuildScenesLog()
    {
        int count = SceneManager.sceneCountInBuildSettings;
        var sb = new StringBuilder();
        sb.AppendLine("[HardRestart] Build Settings scenes:");
        for (int i = 0; i < count; i++)
            sb.AppendLine($"  [{i}] {SceneUtility.GetScenePathByBuildIndex(i)}");
        return sb.ToString();
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

