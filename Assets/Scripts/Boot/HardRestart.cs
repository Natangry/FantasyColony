using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public static class HardRestart
{
    public static void RebootToFirstScene()
    {
        Time.timeScale = 1f;

        // Destroy our known persistent autos if present
        DestroyIfExists("BuildSystems (Auto)");
        DestroyIfExists("BuildPalette (Auto)");
        DestroyIfExists("BuildCanvas (Auto)");

        // Destroy any EventSystem we created
        var es = Object.FindFirstObjectByType<EventSystem>();
        if (es != null) Object.Destroy(es.gameObject);

        // Reset bootstrap singletons/flags
        var bb = typeof(BuildBootstrap);
        var flag = bb.GetField("_defsLoadedOnce", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        if (flag != null) flag.SetValue(null, false);

        // Reload first scene (intro)
        SceneManager.LoadScene(0);
    }

    static void DestroyIfExists(string name)
    {
        var go = GameObject.Find(name);
        if (go != null) Object.Destroy(go);
    }
}

