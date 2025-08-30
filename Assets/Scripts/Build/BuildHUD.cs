using UnityEngine;

/// <summary>
/// Minimal header HUD that exposes a Build toggle and self-heals missing systems.
/// </summary>
public class BuildHUD : MonoBehaviour
{
    BuildModeController Ctrl => BuildModeController.Instance;

    Rect HeaderRect => new Rect(12, 12, 160, 32);

    void OnGUI()
    {
        var ctrl = Ctrl;
        if (ctrl == null)
        {
            BuildBootstrap.Ensure();
            ctrl = BuildModeController.Instance;
        }
        GUILayout.BeginArea(HeaderRect, GUI.skin.window);
        GUILayout.BeginHorizontal();
        GUILayout.Label(ctrl != null && ctrl.IsActive ? "●" : "○", GUILayout.Width(20));
        if (GUILayout.Button("Build [B]", GUILayout.Height(20)))
        {
            if (ctrl != null) ctrl.ToggleBuildMode();
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
}

