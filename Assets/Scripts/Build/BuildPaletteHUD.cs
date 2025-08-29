using UnityEngine;

/// <summary>
/// Simple build palette shown only while Build Mode is active.
/// </summary>
public class BuildPaletteHUD : MonoBehaviour
{
    [SerializeField] private Vector2 offset = new Vector2(12f, 120f);
    [SerializeField] private float widthPct = 0.22f;

    private GUIStyle _box;
    private GUIStyle _button;
    private GUIStyle _label;

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
        if (bm == null || !bm.IsActive) return;

        Ensure();

        float w = Mathf.Max(220f, Screen.width * widthPct);
        Rect r = new Rect(offset.x, offset.y, w, Screen.height * 0.5f);
        GUILayout.BeginArea(r, "Build Palette", _box);

        GUILayout.Label("Stations", _label);
        GUILayout.Space(6f);

        // Construction Board entry
        bool exists = BuildModeController.UniqueBuildingExists<ConstructionBoard>();
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Construction Board (Free)", GUILayout.ExpandWidth(true));
            GUI.enabled = !exists;
            if (GUILayout.Button(exists ? "Placed" : "Place", _button, GUILayout.Width(90f)))
            {
                bm.SetTool(BuildTool.PlaceConstructionBoard);
            }
            GUI.enabled = true;
        }

        GUILayout.FlexibleSpace();
        GUILayout.Space(8f);
        if (GUILayout.Button("Exit Build Mode (B)", _button, GUILayout.Height(32f)))
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
