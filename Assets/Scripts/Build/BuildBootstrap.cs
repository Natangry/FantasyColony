using UnityEngine;

public static class BuildBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void Ensure()
    {
        var go = GameObject.Find("BuildSystems (Auto)");
        if (go == null)
        {
            go = new GameObject("BuildSystems (Auto)");
        }

        if (go.GetComponent<BuildModeController>() == null) go.AddComponent<BuildModeController>();
        if (go.GetComponent<BuildPaletteHUD>() == null) go.AddComponent<BuildPaletteHUD>();
        if (go.GetComponent<JobService>() == null) go.AddComponent<JobService>();
        if (go.GetComponent<BuildToggleHUD>() == null) go.AddComponent<BuildToggleHUD>();
        if (go.GetComponent<BuildHotkeyListener>() == null) go.AddComponent<BuildHotkeyListener>();
    }
}
