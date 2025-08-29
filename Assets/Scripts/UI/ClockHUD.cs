using UnityEngine;

/// <summary>
/// Minimal IMGUI overlay showing Year/Season/Day and 24h time (HH:MM).
/// Rendered under the speed indicator at the top-right, slightly smaller.
/// </summary>
public class ClockHUD : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] private Vector2 topRightOffset = new Vector2(12f, 12f);
    [SerializeField] private float fontPct = 0.032f; // slightly smaller than speed text
    [SerializeField] private float extraTopOffsetPct = 0.040f; // approximate height of speed label + padding

    private GUIStyle _label;
    private GUIStyle _shadow;
    private void Ensure()
    {
        if (_label == null)
        {
            _label = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperRight,
                fontStyle = FontStyle.Bold
            };
            _label.normal.textColor = Color.white;
        }
        if (_shadow == null)
        {
            _shadow = new GUIStyle(_label);
            _shadow.normal.textColor = new Color(0f, 0f, 0f, 0.6f);
        }
    }

    private void OnGUI()
    {
        var clock = GameClockAPI.Find();
        if (clock == null) return; // nothing to show

        var cal = GameCalendarAPI.Find();

        Ensure();

        int fontSize = Mathf.RoundToInt(Mathf.Max(14f, Screen.height * fontPct));
        _label.fontSize = fontSize;
        _shadow.fontSize = fontSize;

        // Compose text: prefer calendar if available
        string prefix;
        if (cal != null)
        {
            prefix = $"Y{cal.Year} · {cal.CurrentSeasonName} {cal.DayOfSeason:00}";
        }
        else
        {
            prefix = $"Day {clock.Day:0}";
        }
        string text = $"{prefix} — {clock.TimeHHMM}";

        // Layout: top-right, below speed label. We approximate the speed label height via extraTopOffsetPct.
        float y = topRightOffset.y + Mathf.Max(24f, Screen.height * extraTopOffsetPct);
        Rect r = new Rect(0f + topRightOffset.x, y, Screen.width - (topRightOffset.x * 2f), Screen.height);

        // Shadow + main text
        Rect rShadow = new Rect(r.x + 1, r.y + 1, r.width, r.height);
        GUI.Label(rShadow, text, _shadow);
        GUI.Label(r, text, _label);
    }
}
