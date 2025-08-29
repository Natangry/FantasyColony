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
    public static float CurrentSpeed { get; private set; } = 1f; // 1x by default
    public static event Action<bool> OnPauseChanged;

    [Header("Overlay")]
    [SerializeField] private Vector2 indicatorPadding = new Vector2(12f, 8f);
    [SerializeField] private float indicatorScale = 0.022f; // % of screen height for font sizing

#if ENABLE_INPUT_SYSTEM
    private InputAction _pauseAction;
    private InputAction _speed1Action;
    private InputAction _speed2Action;
    private InputAction _speed3Action;
#endif

    private void Update()
    {
#if ENABLE_INPUT_SYSTEM
        // With the new Input System active, we use an InputAction (enabled in OnEnable).
#else
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TogglePause();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetSpeed(1f);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetSpeed(2f);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetSpeed(3f);
#endif
    }

    public static void SetPaused(bool pause)
    {
        if (IsPaused == pause) return;
        IsPaused = pause;
        ApplyTimeScale();
#if UNITY_EDITOR
        Debug.Log($"Paused: {IsPaused}");
#endif
        try { OnPauseChanged?.Invoke(IsPaused); } catch { /* no-op */ }
    }

    public static void SetSpeed(float s)
    {
        CurrentSpeed = Mathf.Clamp(s, 0.25f, 3f);
        if (!IsPaused) ApplyTimeScale();
    }

    private static void ApplyTimeScale()
    {
        Time.timeScale = IsPaused ? 0f : CurrentSpeed;
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
        if (_speed1Action == null)
        {
            _speed1Action = new InputAction("Speed1", binding: "<Keyboard>/1");
            _speed1Action.performed += ctx => SetSpeed(1f);
        }
        if (_speed2Action == null)
        {
            _speed2Action = new InputAction("Speed2", binding: "<Keyboard>/2");
            _speed2Action.performed += ctx => SetSpeed(2f);
        }
        if (_speed3Action == null)
        {
            _speed3Action = new InputAction("Speed3", binding: "<Keyboard>/3");
            _speed3Action.performed += ctx => SetSpeed(3f);
        }
        _pauseAction.Enable();
        _speed1Action.Enable();
        _speed2Action.Enable();
        _speed3Action.Enable();
    }

    private void OnDisable()
    {
        if (_pauseAction != null)
            _pauseAction.Disable();
        if (_speed1Action != null)
            _speed1Action.Disable();
        if (_speed2Action != null)
            _speed2Action.Disable();
        if (_speed3Action != null)
            _speed3Action.Disable();
    }

    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        TogglePause();
    }
#endif

    private void OnGUI()
    {
        // Top-right speed (and paused) indicator
        var sw = Screen.width;
        var sh = Screen.height;

        var label = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.UpperRight,
            wordWrap = false,
            fontSize = Mathf.Max(12, Mathf.RoundToInt(sh * indicatorScale))
        };

        string text = IsPaused ? $"Paused — Speed: {CurrentSpeed:0.##}×" : $"Speed: {CurrentSpeed:0.##}×";
        Vector2 size = label.CalcSize(new GUIContent(text));
        float x = sw - size.x - indicatorPadding.x;
        float y = indicatorPadding.y;
        GUI.Label(new Rect(x, y, size.x, size.y), text, label);
    }
}
