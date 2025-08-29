using System;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Central selection controller.
/// - Single click selects a pawn (click empty clears).
/// - Drag-select (marquee) to select one or many pawns.
/// </summary>
[AddComponentMenu("Systems/Selection Controller")]
public class SelectionController : MonoBehaviour
{
    public static SpritePawn Selected { get; private set; }
    public static event Action<SpritePawn> OnSelectionChanged;

    private static readonly List<SpritePawn> _selectedGroup = new List<SpritePawn>();
    public static IReadOnlyList<SpritePawn> SelectedGroup => _selectedGroup;

    private Camera _cam;

    // Drag/marquee state (screen-space, origin bottom-left)
    private bool _dragging;
    private Vector2 _dragStart;
    private Vector2 _dragNow;
    private const float _dragThreshold = 6f; // pixels

    // If a press began over HUD, ignore the whole press/drag/release sequence.
    private bool _pressOverHUD;

    // GUI helpers
    private static Texture2D _texWhite;

    private void Awake()
    {
        _cam = Camera.main;
        if (_cam == null)
        {
            // Fallback to any camera in scene
#if UNITY_2022_2_OR_NEWER
            var any = UnityEngine.Object.FindAnyObjectByType<Camera>();
#else
            var any = UnityEngine.Object.FindObjectOfType<Camera>();
#endif
            _cam = any;
        }
    }

    private void Update()
    {
        // Handle mouse input for both legacy and new input systems.
#if ENABLE_INPUT_SYSTEM
        var mouse = Mouse.current;
        if (mouse != null)
        {
            Vector2 pos = mouse.position.ReadValue();
            if (IsOverHUD(pos)) { if (mouse.leftButton.wasPressedThisFrame) _pressOverHUD = true; return; }
            if (mouse.leftButton.wasPressedThisFrame) OnMouseDown(pos);
            if (mouse.leftButton.isPressed) OnMouseDrag(pos);
            if (mouse.leftButton.wasReleasedThisFrame) OnMouseUp(pos);
            return;
        }
#endif

        // Legacy Input fallback
        Vector2 mpos = Input.mousePosition;
        if (IsOverHUD(mpos))
        {
            if (Input.GetMouseButtonDown(0)) _pressOverHUD = true;
            return;
        }
        if (Input.GetMouseButtonDown(0)) OnMouseDown(mpos);
        if (Input.GetMouseButton(0)) OnMouseDrag(mpos);
        if (Input.GetMouseButtonUp(0)) OnMouseUp(mpos);
    }

    public static void SetSelected(SpritePawn pawn)
    {
        if (Selected == pawn)
        {
            // Keep group as-is; still raise event for listeners.
            try { OnSelectionChanged?.Invoke(Selected); } catch { }
            return;
        }
        Selected = pawn; // primary selection
        try { OnSelectionChanged?.Invoke(Selected); } catch { /* no-op */ }
    }

    private void ApplyGroupSelection(List<SpritePawn> newGroup)
    {
        // Turn off previous rings
        for (int i = 0; i < _selectedGroup.Count; i++)
        {
            var p = _selectedGroup[i];
            if (p != null) p.SetSelected(false);
        }

        _selectedGroup.Clear();
        if (newGroup != null && newGroup.Count > 0)
        {
            _selectedGroup.AddRange(newGroup);
            // Turn on rings for new group
            for (int i = 0; i < _selectedGroup.Count; i++)
            {
                var p = _selectedGroup[i];
                if (p != null) p.SetSelected(true);
            }
            // Primary = first
            SetSelected(_selectedGroup[0]);
        }
        else
        {
            SetSelected(null);
        }
    }

    private void SingleClickSelect(Vector2 screenPosBL)
    {
        if (_cam == null) return;
        var ray = _cam.ScreenPointToRay(screenPosBL);
        if (Physics.Raycast(ray, out var hit, 1000f, ~0, QueryTriggerInteraction.Ignore))
        {
            var pawn = hit.collider != null ? hit.collider.GetComponentInParent<SpritePawn>() : null;
            var list = new List<SpritePawn>();
            if (pawn != null) list.Add(pawn);
            ApplyGroupSelection(list);
        }
        else
        {
            ApplyGroupSelection(null);
        }
    }

    private void OnMouseDown(Vector2 screenPosBL)
    {
        if (_pressOverHUD) return;
        _dragStart = screenPosBL;
        _dragNow = screenPosBL;
        _dragging = false;
    }

    private void OnMouseDrag(Vector2 screenPosBL)
    {
        if (_pressOverHUD) return;
        _dragNow = screenPosBL;
        if (!_dragging && Vector2.Distance(_dragStart, _dragNow) > _dragThreshold)
        {
            _dragging = true;
        }
    }

    private void OnMouseUp(Vector2 screenPosBL)
    {
        if (_pressOverHUD) { _pressOverHUD = false; return; }
        _dragNow = screenPosBL;
        if (!_dragging)
        {
            // Treat as a click
            SingleClickSelect(screenPosBL);
        }
        else
        {
            // Marquee select
            var rect = GetScreenRectBL(_dragStart, _dragNow);
            var candidates = new List<SpritePawn>();
            if (_cam != null)
            {
                foreach (var pawn in SpritePawn.Instances)
                {
                    if (pawn == null) continue;
                    var wp = pawn.transform.position;
                    var sp = _cam.WorldToScreenPoint(wp);
                    if (sp.z < 0f) continue; // behind camera
                    var p = new Vector2(sp.x, sp.y); // bottom-left origin
                    if (rect.Contains(p))
                    {
                        candidates.Add(pawn);
                    }
                }
            }
            ApplyGroupSelection(candidates);
        }
        _dragging = false;
    }

    private static Rect GetScreenRectBL(Vector2 aBL, Vector2 bBL)
    {
        float xMin = Mathf.Min(aBL.x, bBL.x);
        float xMax = Mathf.Max(aBL.x, bBL.x);
        float yMin = Mathf.Min(aBL.y, bBL.y);
        float yMax = Mathf.Max(aBL.y, bBL.y);
        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }

    private bool IsOverHUD(Vector2 screenPosBL)
    {
        var p = SelectionHUD.LastPanelRectBL;
        var g = SelectionHUD.LastGizmoRectBL;
        bool overPanel = p.width > 0f && p.height > 0f && p.Contains(screenPosBL);
        bool overGizmo = g.width > 0f && g.height > 0f && g.Contains(screenPosBL);
        return overPanel || overGizmo;
    }

    private void OnGUI()
    {
        if (!_dragging) return;

        var sw = Screen.width;
        var sh = Screen.height;

        // Convert bottom-left rect to GUI-space (top-left origin)
        var rBL = GetScreenRectBL(_dragStart, _dragNow);
        var rGUI = new Rect(rBL.xMin, sh - rBL.yMax, rBL.width, rBL.height);

        if (_texWhite == null)
        {
            _texWhite = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            _texWhite.SetPixel(0, 0, Color.white);
            _texWhite.Apply(false, false);
        }

        // Fill
        var fillCol = new Color(0.2f, 0.6f, 1f, 0.15f);
        var borderCol = new Color(0.2f, 0.6f, 1f, 0.9f);
        GUI.color = fillCol;
        GUI.DrawTexture(rGUI, _texWhite);
        // Border
        GUI.color = borderCol;
        GUI.DrawTexture(new Rect(rGUI.xMin, rGUI.yMin, rGUI.width, 2f), _texWhite);
        GUI.DrawTexture(new Rect(rGUI.xMin, rGUI.yMax - 2f, rGUI.width, 2f), _texWhite);
        GUI.DrawTexture(new Rect(rGUI.xMin, rGUI.yMin, 2f, rGUI.height), _texWhite);
        GUI.DrawTexture(new Rect(rGUI.xMax - 2f, rGUI.yMin, 2f, rGUI.height), _texWhite);
        GUI.color = Color.white;
    }
}
