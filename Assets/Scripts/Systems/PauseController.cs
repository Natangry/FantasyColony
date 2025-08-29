using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
#endif

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

#if ENABLE_INPUT_SYSTEM
    private InputAction _pauseAction;
#endif

    private void Update()
    {
#if ENABLE_INPUT_SYSTEM
        // With the new Input System active, we use an InputAction (enabled in OnEnable).
#else
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SetPaused(!IsPaused);
        }
#endif
    }

    public static void SetPaused(bool pause)
    {
        if (IsPaused == pause) return;
        IsPaused = pause;
        Time.timeScale = IsPaused ? 0f : 1f;
#if UNITY_EDITOR
        Debug.Log($"Paused: {IsPaused}");
#endif
        try { OnPauseChanged?.Invoke(IsPaused); } catch { /* no-op */ }
    }

    private void TogglePause()
    {
        SetPaused(!IsPaused);
    }

#if ENABLE_INPUT_SYSTEM
    private void OnEnable()
    {
        if (_pauseAction == null)
        {
            // Bind keyboard Space; also allow gamepad Start as a convenience.
            _pauseAction = new InputAction("Pause", binding: "<Keyboard>/space");
            _pauseAction.AddBinding("<Gamepad>/start");
            _pauseAction.performed += OnPausePerformed;
        }
        _pauseAction.Enable();
    }

    private void OnDisable()
    {
        if (_pauseAction != null)
            _pauseAction.Disable();
    }

    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        TogglePause();
    }
#endif

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
