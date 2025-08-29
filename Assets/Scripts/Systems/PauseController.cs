using System;
using UnityEngine;

/// <summary>
/// Global pause toggle (Space). Pauses via Time.timeScale and shows a tiny overlay while paused.
/// </summary>
[AddComponentMenu("Systems/Pause Controller")]
public class PauseController : MonoBehaviour
{
    public static bool IsPaused { get; private set; }
    public static event Action<bool> OnPauseChanged;

    [Header("Overlay")]
    [SerializeField] private string pausedText = "PAUSED — press Space to resume";

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SetPaused(!IsPaused);
        }
    }

    public static void SetPaused(bool pause)
    {
        if (IsPaused == pause) return;
        IsPaused = pause;
        Time.timeScale = IsPaused ? 0f : 1f;
        try { OnPauseChanged?.Invoke(IsPaused); } catch { /* no-op */ }
    }

    private void OnGUI()
    {
        if (!IsPaused) return;

        var sw = Screen.width;
        var sh = Screen.height;

        float panelW = Mathf.Clamp(sw * 0.45f, 360f, 720f);
        float panelH = Mathf.Clamp(sh * 0.18f, 120f, 240f);
        var rect = new Rect((sw - panelW) * 0.5f, (sh - panelH) * 0.5f, panelW, panelH);

        var box = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(20, 20, 20, 20)
        };
        var label = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true,
            fontSize = Mathf.Max(16, Mathf.RoundToInt(sh * 0.03f))
        };

        GUILayout.BeginArea(rect, GUIContent.none, box);
        GUILayout.FlexibleSpace();
        GUILayout.Label(pausedText, label);
        GUILayout.FlexibleSpace();
        GUILayout.EndArea();
    }
}
