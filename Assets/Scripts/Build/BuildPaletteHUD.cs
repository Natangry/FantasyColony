using UnityEngine;

/// <summary>
/// Simple build palette shown only while Build Mode is active.
/// </summary>
public class BuildPaletteHUD : MonoBehaviour
{
    [SerializeField] private Vector2 offset = new Vector2(12f, 120f);
    [SerializeField] private float widthPct = 0.28f;
    [SerializeField] private float heightPct = 0.65f;
    [SerializeField] private float fontPct = 0.03f; // scale with resolution
    [SerializeField] private float buttonHPct = 0.05f;

    private GUIStyle _box;
    private GUIStyle _button;
    private GUIStyle _label;
    private Vector2 _scroll;

    private void Ensure()
    {
        if (_box == null)
        {
            _box = new GUIStyle(GUI.skin.box);
        }
        if (_button == null)
        {
            _button = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
        }
        if (_label == null)
        {
            _label = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold };
            _label.normal.textColor = Color.white;
        }
    }

    private void OnGUI()
    {
        var bm = BuildModeController.Instance;
        if (IntroScreen.IsVisible) return; // never show on intro
        if (bm == null || !bm.IsActive) return;

        Ensure();

        int fontSize = Mathf.RoundToInt(Mathf.Max(14f, Screen.height * fontPct));
        _button.fontSize = fontSize;
        _label.fontSize = fontSize;

        float w = Mathf.Max(260f, Screen.width * widthPct);
        float h = Mathf.Max(260f, Screen.height * heightPct);
        Rect r = new Rect(offset.x, offset.y, w, h);
        GUILayout.BeginArea(r, "Build Palette", _box);

        GUILayout.Label("Stations", _label);
        GUILayout.Space(6f);
        _scroll = GUILayout.BeginScrollView(_scroll);

        // Construction Board entry
        bool exists = BuildModeController.UniqueBuildingExists<ConstructionBoard>();
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Construction Board (Free)", GUILayout.ExpandWidth(true));
            GUI.enabled = !exists;
            float btnH = Mathf.Max(32f, Screen.height * buttonHPct);
            if (GUILayout.Button(exists ? "Placed" : "Place", _button, GUILayout.Width(120f), GUILayout.Height(btnH)))
            {
                bm.SetTool(BuildTool.PlaceConstructionBoard);
            }
            GUI.enabled = true;
        }
        GUILayout.EndScrollView();

        GUILayout.FlexibleSpace();
        GUILayout.Space(8f);
        if (GUILayout.Button("Exit Build Mode (B)", _button, GUILayout.Height(Mathf.Max(36f, Screen.height * (buttonHPct * 0.9f)))))
        {
            bm.SetActive(false);
        }

        GUILayout.EndArea();
    }
}

internal struct GUIStateScope : System.IDisposable
{
    private readonly bool prev;
    public GUIStateScope(bool enabled) { prev = GUI.enabled; GUI.enabled = enabled; }
    public void Dispose() { GUI.enabled = prev; }
}
