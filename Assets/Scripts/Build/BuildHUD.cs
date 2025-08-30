using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Minimal header HUD that exposes a Build toggle and self-heals missing systems.
/// </summary>
public class BuildHUD : MonoBehaviour
{
    BuildModeController Ctrl => BuildModeController.Instance;

    Rect HeaderRect => new Rect(12, 12, 160, 32);

    void OnGUI()
    {
        // Hide on intro/menu/title scenes
        var sn = SceneManager.GetActiveScene().name;
        if (!string.IsNullOrEmpty(sn))
        {
            var s = sn.ToLowerInvariant();
            if (s.Contains("intro") || s.Contains("menu") || s.Contains("title")) return;
        }

        var ctrl = Ctrl;
        if (ctrl == null)
        {
            BuildBootstrap.Ensure();
            ctrl = BuildModeController.Instance;
        }

        // Scale UI for high DPI
        float scale = Mathf.Clamp(Mathf.Min(Screen.width / 1920f, Screen.height / 1080f), 1.0f, 2.5f);
        var prevMatrix = GUI.matrix;
        GUIUtility.ScaleAroundPivot(new Vector2(scale, scale), Vector2.zero);
        var rect = new Rect(HeaderRect.x / scale, HeaderRect.y / scale, HeaderRect.width / scale, HeaderRect.height / scale);

        var skin = GUI.skin;
        int oldButton = skin.button.fontSize;
        skin.button.fontSize = Mathf.RoundToInt(14 * scale);
        GUILayout.BeginArea(rect, GUI.skin.window);
        GUILayout.BeginHorizontal();
        GUILayout.Label(ctrl != null && ctrl.IsActive ? "●" : "○", GUILayout.Width(20));
        if (GUILayout.Button("Build [B]", GUILayout.Height(20)))
        {
            if (ctrl != null) ctrl.ToggleBuildMode();
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
        skin.button.fontSize = oldButton;
        GUI.matrix = prevMatrix;
    }
}
