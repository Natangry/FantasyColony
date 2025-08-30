using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Records the scene the app launched into so we can "full reboot" back to it.
/// </summary>
public static class BootSession
{
    public static int InitialSceneIndex { get; private set; } = -1;
    public static string InitialScenePath { get; private set; } = null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Capture()
    {
        var active = SceneManager.GetActiveScene();
        InitialSceneIndex = active.buildIndex;
        InitialScenePath = active.path;
        Debug.Log($"[BootSession] Captured initial scene index={InitialSceneIndex} path={InitialScenePath}");
    }
}
