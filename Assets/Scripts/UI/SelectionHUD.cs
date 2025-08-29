using UnityEngine;

/// <summary>
/// Bottom-left info panel (blank for now) and a right-side gizmo strip.
/// Appears only when a pawn is selected. First gizmo: Assume Control / Release.
/// </summary>
[AddComponentMenu("UI/Selection HUD")]
public class SelectionHUD : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] private Vector2 panelMinMaxW = new Vector2(260f, 420f);
    [SerializeField] private Vector2 panelMinMaxH = new Vector2(130f, 220f);
    [SerializeField] private float panelWidthPct = 0.28f; // of screen width
    [SerializeField] private float panelHeightPct = 0.22f; // of screen height
    [SerializeField] private float margin = 12f;
    [SerializeField] private float gizmoSpacing = 8f;
    [SerializeField] private float buttonHeightPct = 0.055f; // of screen height

    private GUIStyle _panelStyle;
    private GUIStyle _headerStyle;
    private GUIStyle _buttonStyle;
    private GUIStyle _labelStyle;

    // Expose last-drawn HUD rects in SCREEN (bottom-left origin) space so SelectionController can ignore clicks over HUD.
    public static Rect LastPanelRectBL { get; private set; }
    public static Rect LastGizmoRectBL { get; private set; }

    private void OnGUI()
    {
        var selected = SelectionController.Selected;
        if (selected == null)
        {
            LastPanelRectBL = Rect.zero;
            LastGizmoRectBL = Rect.zero;
            return;
        }

        float sw = Screen.width;
        float sh = Screen.height;

        // Sizing
        float panelW = Mathf.Clamp(sw * panelWidthPct, panelMinMaxW.x, panelMinMaxW.y);
        float panelH = Mathf.Clamp(sh * panelHeightPct, panelMinMaxH.x, panelMinMaxH.y);
        float btnH = Mathf.Max(28f, sh * buttonHeightPct);

        // Panel rect (bottom-left anchor)
        var panelRectGUI = new Rect(margin, sh - panelH - margin, panelW, panelH);

        // Gizmo strip to the right of the panel
        var gizmoRectGUI = new Rect(panelRectGUI.xMax + gizmoSpacing, panelRectGUI.y, Mathf.Max(160f, sw * 0.15f), panelRectGUI.height);

        EnsureStyles(sh);

        // Update BL-space rects for input guarding
        LastPanelRectBL = new Rect(panelRectGUI.xMin, sh - (panelRectGUI.yMin + panelRectGUI.height), panelRectGUI.width, panelRectGUI.height);
        LastGizmoRectBL = new Rect(gizmoRectGUI.xMin, sh - (gizmoRectGUI.yMin + gizmoRectGUI.height), gizmoRectGUI.width, gizmoRectGUI.height);

        // Draw panel (blank content for now; just a header for visual structure)
        GUILayout.BeginArea(panelRectGUI, GUIContent.none, _panelStyle);
        {
            GUILayout.Label("Unit Info", _headerStyle);
            GUILayout.Space(btnH * 0.2f);
            // Blank content placeholder
            GUILayout.Label("(Coming soon)", _labelStyle);
            GUILayout.FlexibleSpace();
        }
        GUILayout.EndArea();

        // Gizmos
        GUILayout.BeginArea(gizmoRectGUI);
        {
            // Assume/Release Control
            bool isControlled = (ControlManager.Controlled == selected);
            string btn = isControlled ? "Release Control" : "Assume Control";
            if (GUILayout.Button(btn, _buttonStyle, GUILayout.Height(btnH)))
            {
                if (isControlled) ControlManager.ReleaseControl();
                else ControlManager.AssumeControl(selected);
            }
            // Keep selection pinned to controlled pawn even if HUD was clicked first
            if (ControlManager.Controlled != null && SelectionController.Selected != ControlManager.Controlled)
            {
                SelectionController.SelectOnly(ControlManager.Controlled);
            }

            GUILayout.Space(btnH * 0.25f);
            GUILayout.Label("Tip: WASD/Arrows to move when controlled.\nSpace = Pause. 1/2/3 = Speed.", _labelStyle);
        }
        GUILayout.EndArea();
    }

    void EnsureStyles(float sh)
    {
        if (_panelStyle == null)
        {
            _panelStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(12, 12, 10, 10)
            };
        }
        if (_headerStyle == null)
        {
            _headerStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = Mathf.Max(14, Mathf.RoundToInt(sh * 0.028f)),
                fontStyle = FontStyle.Bold
            };
        }
        if (_buttonStyle == null)
        {
            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.Max(12, Mathf.RoundToInt(sh * 0.024f))
            };
        }
        if (_labelStyle == null)
        {
            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Max(11, Mathf.RoundToInt(sh * 0.02f)),
                wordWrap = true
            };
        }
    }
}

