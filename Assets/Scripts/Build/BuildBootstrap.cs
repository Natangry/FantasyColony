using UnityEngine;
using FantasyColony.Defs;

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

        // Ensure single placement tool
        var tools = go.GetComponents<BuildPlacementTool>();
        for (int i = 1; i < tools.Length; i++) Object.Destroy(tools[i]);
        if (tools.Length == 0) go.AddComponent<BuildPlacementTool>();

        if (go.GetComponent<BuildModeController>() == null) go.AddComponent<BuildModeController>();
        if (go.GetComponent<BuildPaletteHUD>() == null) go.AddComponent<BuildPaletteHUD>();
        if (go.GetComponent<JobService>() == null) go.AddComponent<JobService>();
        if (go.GetComponent<BuildToggleHUD>() == null) go.AddComponent<BuildToggleHUD>();
        if (go.GetComponent<BuildHotkeyListener>() == null) go.AddComponent<BuildHotkeyListener>();

        DefDatabase.LoadAll();
        VisualRegistry.Build(Application.isEditor);
    }
}
