using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using UnityEngine;
//#if is allowed to safely reference the new Input System only when present
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Lightweight, robust in-game developer log overlay.
/// - Captures all Unity logs via Application.logMessageReceivedThreaded
/// - Thread-safe queue -> main-thread list with ring buffer cap
/// - IMGUI-based so it works without any prefabs/scenes
/// - Filters (Debug/Warning/Errors), Pause, Auto-scroll, Stack Traces
/// - Copy Visible respects current filters
/// </summary>
public class DevLogOverlay : MonoBehaviour
{
    [Serializable]
    public struct LogEntry
    {
        public DateTime time;
        public int frame;
        public int threadId;
        public LogType type;
        public string message;
        public string stackTrace; // may be empty based on policy
        public string contextName; // optional
        public int count; // number of additional consecutive occurrences collapsed into this entry
    }

    private struct QueuedLog
    {
        public DateTime time;
        public int threadId;
        public LogType type;
        public string message;
        public string stackTrace;
        public string contextName;
    }

    private const int DefaultCapacity = 5000;
    private const int WindowPadding = 8;

    private static DevLogOverlay _instance;
    private static readonly object InstanceLock = new object();

    // Threaded producer -> main thread consumer
    private readonly ConcurrentQueue<QueuedLog> _incoming = new ConcurrentQueue<QueuedLog>();
    private readonly List<LogEntry> _entries = new List<LogEntry>(DefaultCapacity + 64);

    [Header("Buffer")]
    [Tooltip("Maximum number of entries kept in memory (oldest are dropped).")]
    public int capacity = DefaultCapacity;

    [Header("Filters")]
    public bool showDebug = true;
    public bool showWarnings = true;
    public bool showErrors = true; // includes Error, Assert, Exception

    [Header("Behavior")]
    public bool paused = false;
    public bool autoScroll = true;
    public bool showStackTraces = false; // toggles display; capture is controlled by Unity's stack trace policy
    public bool collapseRepeats = true; // when true, consecutive identical messages are collapsed

    private Vector2 _scroll;
    private bool _visible = true;
    private Rect _windowRect;
    private int _dockedCorner = 2; // 0=TL,1=TR,2=BL,3=BR

    private GUIStyle _rowStyle;
    private GUIStyle _rowStyleWarn;
    private GUIStyle _rowStyleError;
    private GUIStyle _tinyLabel;

    private string _search = "";
    private bool _focusSearch;

    // ---------- Public API ----------
    public static void Show()
    {
        EnsureInstance();
        _instance._visible = true;
    }

    public static void Hide()
    {
        if (_instance != null) _instance._visible = false;
    }

    public static void ToggleVisible()
    {
        EnsureInstance();
        _instance._visible = !_instance._visible;
    }

    public static bool IsVisible => _instance != null && _instance._visible;

    public static void Clear()
    {
        if (_instance == null) return;
        _instance._entries.Clear();
    }

    public static IReadOnlyList<LogEntry> Snapshot()
    {
        EnsureInstance();
        return _instance._entries;
    }

    private static void EnsureInstance()
    {
        if (_instance != null) return;
        lock (InstanceLock)
        {
            if (_instance != null) return;
            var go = new GameObject("__DevLogOverlay");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<DevLogOverlay>();
        }
    }

    // ---------- Unity lifecycle ----------
    private void Awake()
    {
        // Reasonable defaults: errors/exceptions include stacks; others off for perf until toggled
        try
        {
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.ScriptOnly);
            Application.SetStackTraceLogType(LogType.Assert, StackTraceLogType.ScriptOnly);
            Application.SetStackTraceLogType(LogType.Exception, StackTraceLogType.ScriptOnly);
        }
        catch { /* Some platforms may not support changing this at runtime */ }

        // Sensible default window size
        var w = Mathf.RoundToInt(Screen.width * 0.6f);
        var h = Mathf.RoundToInt(Screen.height * 0.5f);
        _windowRect = new Rect(WindowPadding, Screen.height - h - WindowPadding, w, h);
    }

    private void OnEnable()
    {
        Application.logMessageReceivedThreaded += OnLogMessageReceivedThreaded;
    }

    private void OnDisable()
    {
        Application.logMessageReceivedThreaded -= OnLogMessageReceivedThreaded;
    }

    private void Update()
    {
        // Drain queue to main-thread list
        while (_incoming.TryDequeue(out var q))
        {
            var e = new LogEntry
            {
                time = q.time,
                frame = Time.frameCount,
                threadId = q.threadId,
                type = q.type,
                message = q.message ?? string.Empty,
                stackTrace = q.stackTrace ?? string.Empty,
                contextName = q.contextName ?? string.Empty
            };

            if (!paused)
            {
                if (collapseRepeats && _entries.Count > 0)
                {
                    var lastIndex = _entries.Count - 1;
                    var lastEntry = _entries[lastIndex];
                    if (AreSameForCollapse(lastEntry, e))
                    {
                        lastEntry.count = lastEntry.count + 1;
                        // keep the most recent time/frame/thread for the collapsed line
                        lastEntry.time = e.time;
                        lastEntry.frame = e.frame;
                        lastEntry.threadId = e.threadId;
                        _entries[lastIndex] = lastEntry;
                    }
                    else
                    {
                        _entries.Add(e);
                    }
                }
                else
                {
                    _entries.Add(e);
                }

                if (_entries.Count > Mathf.Max(512, capacity))
                {
                    var over = _entries.Count - capacity;
                    _entries.RemoveRange(0, over);
                }
            }
        }

        // Handle hotkeys without throwing when only the new Input System is active
        HandleHotkeys();
    }

    private void OnGUI()
    {
        if (!_visible) return;

        EnsureStyles();

        // Simple draggable/dockable window
        _windowRect = GUI.Window(GetInstanceID(), _windowRect, DrawWindow, "Developer Log");

        // Keep inside screen
        _windowRect.x = Mathf.Clamp(_windowRect.x, WindowPadding, Screen.width - _windowRect.width - WindowPadding);
        _windowRect.y = Mathf.Clamp(_windowRect.y, WindowPadding, Screen.height - _windowRect.height - WindowPadding);
    }

    private void DrawWindow(int id)
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(paused ? "Resume" : "Pause", GUILayout.Height(24))) paused = !paused;
        if (GUILayout.Button("Clear", GUILayout.Height(24))) _entries.Clear();
        if (GUILayout.Button("Copy Visible", GUILayout.Height(24))) CopyVisibleToClipboard();
        if (GUILayout.Button("Close", GUILayout.Height(24))) _visible = false;
        GUILayout.FlexibleSpace();
        autoScroll = GUILayout.Toggle(autoScroll, "Auto-Scroll", GUILayout.Height(24));
        showStackTraces = GUILayout.Toggle(showStackTraces, "Stack Traces", GUILayout.Height(24));
        collapseRepeats = GUILayout.Toggle(collapseRepeats, "Collapse Repeats", GUILayout.Height(24));
        GUILayout.EndHorizontal();

        GUILayout.Space(4);

        GUILayout.BeginHorizontal();
        showDebug = GUILayout.Toggle(showDebug, "Debug", GUILayout.Width(80));
        showWarnings = GUILayout.Toggle(showWarnings, "Warning", GUILayout.Width(90));
        showErrors = GUILayout.Toggle(showErrors, "Errors", GUILayout.Width(80));

        GUILayout.Space(10);
        GUILayout.Label("Search:", GUILayout.Width(50));
        GUI.SetNextControlName("DevLogSearch");
        _search = GUILayout.TextField(_search ?? string.Empty, GUILayout.MinWidth(120));
        if (_focusSearch)
        {
            _focusSearch = false;
            GUI.FocusControl("DevLogSearch");
        }
        if (GUILayout.Button("×", GUILayout.Width(26))) { _search = string.Empty; GUI.FocusControl(null); }

        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Dock", GUILayout.Width(60))) CycleDock();
        GUILayout.EndHorizontal();

        GUILayout.Space(4);

        // Header line
        GUILayout.BeginHorizontal();
        GUILayout.Label("Time", _tinyLabel, GUILayout.Width(90));
        GUILayout.Label("F", _tinyLabel, GUILayout.Width(36));
        GUILayout.Label("T", _tinyLabel, GUILayout.Width(28));
        GUILayout.Label("Level", _tinyLabel, GUILayout.Width(60));
        GUILayout.Label("Message", _tinyLabel);
        GUILayout.EndHorizontal();

        // List
        _scroll = GUILayout.BeginScrollView(_scroll, GUI.skin.box);

        var countBefore = _entries.Count;
        for (int i = 0; i < _entries.Count; i++)
        {
            var e = _entries[i];
            if (!PassesFilter(e)) continue;
            if (!PassesSearch(e)) continue;

            var style = StyleFor(e.type);
            GUILayout.BeginHorizontal();
            GUILayout.Label(e.time.ToString("HH:mm:ss.fff"), _tinyLabel, GUILayout.Width(90));
            GUILayout.Label(e.frame.ToString(), _tinyLabel, GUILayout.Width(36));
            GUILayout.Label(e.threadId.ToString(), _tinyLabel, GUILayout.Width(28));
            GUILayout.Label(LevelLabel(e.type), _tinyLabel, GUILayout.Width(60));
            if (e.count > 0)
            {
                // Show message with (xN) suffix for collapsed duplicates
                GUILayout.Label(string.Concat(e.message, " (x", (e.count + 1).ToString(), ")"), style);
            }
            else
            {
                GUILayout.Label(e.message, style);
            }
            GUILayout.EndHorizontal();

            if (showStackTraces && HasStack(e))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(90 + 36 + 28 + 60 + 8);
                GUILayout.Label(e.stackTrace, GUI.skin.label);
                GUILayout.EndHorizontal();
            }
        }

        if (autoScroll && Event.current.type == EventType.Repaint)
        {
            _scroll.y = float.MaxValue;
        }

        GUILayout.EndScrollView();

        GUI.DragWindow(new Rect(0, 0, 10000, 20));
    }

    private void EnsureStyles()
    {
        if (_rowStyle == null)
        {
            _rowStyle = new GUIStyle(GUI.skin.label) { wordWrap = true };
            _rowStyleWarn = new GUIStyle(_rowStyle);
            _rowStyleError = new GUIStyle(_rowStyle);

            // Colorize severities for quick scanning
            _rowStyleWarn.normal.textColor = Color.yellow; // Warnings
            _rowStyleError.normal.textColor = Color.red;   // Errors/Exceptions/Asserts

            _tinyLabel = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                alignment = TextAnchor.UpperLeft
            };
        }
    }

    private void HandleHotkeys()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.f9Key.wasPressedThisFrame) ToggleVisible();
            if (kb.f10Key.wasPressedThisFrame) paused = !paused;
        }
#else
        if (Input.GetKeyDown(KeyCode.F9)) ToggleVisible();
        if (Input.GetKeyDown(KeyCode.F10)) paused = !paused;
#endif
    }

    private void CycleDock()
    {
        _dockedCorner = (_dockedCorner + 1) % 4;

        var w = _windowRect.width;
        var h = _windowRect.height;

        switch (_dockedCorner)
        {
            case 0: _windowRect.position = new Vector2(WindowPadding, WindowPadding); break; // TL
            case 1: _windowRect.position = new Vector2(Screen.width - w - WindowPadding, WindowPadding); break; // TR
            case 2: _windowRect.position = new Vector2(WindowPadding, Screen.height - h - WindowPadding); break; // BL
            case 3: _windowRect.position = new Vector2(Screen.width - w - WindowPadding, Screen.height - h - WindowPadding); break; // BR
        }
    }

    private GUIStyle StyleFor(LogType t)
    {
        switch (t)
        {
            case LogType.Warning: return _rowStyleWarn;
            case LogType.Error:
            case LogType.Assert:
            case LogType.Exception: return _rowStyleError;
            default: return _rowStyle;
        }
    }

    private static string LevelLabel(LogType t)
    {
        switch (t)
        {
            case LogType.Warning: return "WARN";
            case LogType.Error: return "ERROR";
            case LogType.Assert: return "ASSERT";
            case LogType.Exception: return "EXC";
            default: return "DEBUG";
        }
    }

    private static bool HasStack(in LogEntry e)
    {
        if (string.IsNullOrEmpty(e.stackTrace)) return false;
        // Many non-error logs have empty stack due to policy; only render if provided
        return true;
    }

    private bool PassesFilter(in LogEntry e)
    {
        switch (e.type)
        {
            case LogType.Warning: return showWarnings;
            case LogType.Error:
            case LogType.Assert:
            case LogType.Exception: return showErrors;
            default: return showDebug;
        }
    }

    private bool PassesSearch(in LogEntry e)
    {
        if (string.IsNullOrEmpty(_search)) return true;
        var s = _search.Trim();
        if (s.Length == 0) return true;
        return (e.message?.IndexOf(s, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0
            || (e.stackTrace?.IndexOf(s, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0
            || (e.contextName?.IndexOf(s, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0;
    }

    private void CopyVisibleToClipboard()
    {
        var sb = new StringBuilder(8192);
        foreach (var e in _entries)
        {
            if (!PassesFilter(e)) continue;
            if (!PassesSearch(e)) continue;

            sb.Append('[').Append(e.time.ToString("HH:mm:ss.fff")).Append("] ");
            sb.Append("F=").Append(e.frame).Append(' ');
            sb.Append("T=").Append(e.threadId).Append(' ');
            sb.Append('[').Append(LevelLabel(e.type)).Append("] ");
            if (!string.IsNullOrEmpty(e.contextName))
            {
                sb.Append('{').Append(e.contextName).Append("} ");
            }
            if (e.count > 0)
            {
                sb.Append(e.message ?? string.Empty).Append(" (x").Append((e.count + 1).ToString()).AppendLine(")");
            }
            else
            {
                sb.AppendLine(e.message ?? string.Empty);
            }

            if (showStackTraces && HasStack(e))
            {
                sb.AppendLine(e.stackTrace);
            }
        }

        GUIUtility.systemCopyBuffer = sb.ToString();
    }

    private static bool AreSameForCollapse(in LogEntry a, in LogEntry b)
    {
        if (a.type != b.type) return false;
        if (!string.Equals(a.message, b.message, StringComparison.Ordinal)) return false;
        if (!string.Equals(a.stackTrace, b.stackTrace, StringComparison.Ordinal)) return false;
        if (!string.Equals(a.contextName, b.contextName, StringComparison.Ordinal)) return false;
        return true;
    }

    private void OnLogMessageReceivedThreaded(string condition, string stacktrace, LogType type)
    {
        var q = new QueuedLog
        {
            time = DateTime.Now,
            threadId = Thread.CurrentThread.ManagedThreadId,
            type = type,
            message = condition,
            stackTrace = stacktrace,
            contextName = string.Empty
        };
        _incoming.Enqueue(q);
    }
}

// Optional convenience for code callers (e.g., MainMenuScreen) without adding compile-time dependency on UnityEngine.UI
public static class DevLog
{
    public static void Show() => DevLogOverlay.Show();
    public static void Hide() => DevLogOverlay.Hide();
    public static void Toggle() => DevLogOverlay.ToggleVisible();
    public static void Clear() => DevLogOverlay.Clear();
}



