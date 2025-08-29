using UnityEngine;

/// <summary>
/// Minimal IMGUI overlay showing Day and 24h time (HH:MM). Placed at top-left.
/// </summary>
public class ClockHUD : MonoBehaviour
{
    [SerializeField] private Vector2 offset = new Vector2(12f, 12f);
    [SerializeField] private float fontPct = 0.035f; // % of screen height

    private GUIStyle _label;
    private void Ensure()
    {
        if (_label == null)
        {
            _label = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontStyle = FontStyle.Bold
            };
            _label.normal.textColor = Color.white;
        }
    }

    private void OnGUI()
    {
        var clock = GameClockAPI.Find();
        if (clock == null) return;

        Ensure();

        _label.fontSize = Mathf.RoundToInt(Mathf.Max(14f, Screen.height * fontPct));
        string text = $"Day {clock.Day} — {clock.TimeHHMM}";

        // Shadow for legibility
        Rect r = new Rect(offset.x, offset.y, Screen.width, Screen.height);
        var shadow = new GUIStyle(_label);
        shadow.normal.textColor = new Color(0f, 0f, 0f, 0.6f);
        Rect rShadow = new Rect(r.x + 1, r.y + 1, r.width, r.height);
        GUI.Label(rShadow, text, shadow);
        GUI.Label(r, text, _label);
    }
}
