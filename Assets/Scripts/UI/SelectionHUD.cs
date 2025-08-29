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

    private void OnGUI()
    {
        var selected = SelectionController.Selected;
        if (selected == null) return;

        float sw = Screen.width;
        float sh = Screen.height;

        // Sizing
        float panelW = Mathf.Clamp(sw * panelWidthPct, panelMinMaxW.x, panelMinMaxW.y);
        float panelH = Mathf.Clamp(sh * panelHeightPct, panelMinMaxH.x, panelMinMaxH.y);
        float btnH = Mathf.Max(28f, sh * buttonHeightPct);

        // Panel rect (bottom-left anchor)
        var panelRect = new Rect(margin, sh - panelH - margin, panelW, panelH);

        // Gizmo strip to the right of the panel
        var gizmoRect = new Rect(panelRect.xMax + gizmoSpacing, panelRect.y, 0f, panelRect.height);

        EnsureStyles(sh);

        // Draw panel (blank content for now; just a header for visual structure)
        GUILayout.BeginArea(panelRect, GUIContent.none, _panelStyle);
        {
            GUILayout.Label("Unit Info", _headerStyle);
            GUILayout.Space(btnH * 0.2f);
            // Blank content placeholder
            GUILayout.Label("(Coming soon)", _labelStyle);
            GUILayout.FlexibleSpace();
        }
        GUILayout.EndArea();

        // Gizmos
        GUILayout.BeginArea(new Rect(gizmoRect.x, gizmoRect.y, Mathf.Max(160f, sw * 0.15f), gizmoRect.height));
        {
            // Assume/Release Control
            bool isControlled = (ControlManager.Controlled == selected);
            string btn = isControlled ? "Release Control" : "Assume Control";
            if (GUILayout.Button(btn, _buttonStyle, GUILayout.Height(btnH)))
            {
                if (isControlled) ControlManager.ReleaseControl();
                else ControlManager.AssumeControl(selected);
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

