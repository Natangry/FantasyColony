using UnityEngine;

/// <summary>
/// Small top-right button to toggle Build Mode. Works alongside the B hotkey.
/// </summary>
public class BuildToggleHUD : MonoBehaviour
{
    [SerializeField] private Vector2 offset = new Vector2(12f, 12f);
    [SerializeField] private float fontPct = 0.028f;     // slightly smaller than speed label
    [SerializeField] private float topExtraPct = 0.12f;  // below speed/clock text by a proportion of screen height
    [SerializeField] private float minTopExtra = 64f;

    private GUIStyle _btn;
    private GUIStyle _btnActive;

    private void Ensure()
    {
        if (_btn == null)
        {
            _btn = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
        }
        if (_btnActive == null)
        {
            _btnActive = new GUIStyle(_btn);
            _btnActive.normal.textColor = Color.white;
            _btnActive.normal.background = MakeTex(new Color(0.32f, 0.52f, 0.92f, 1f));
            _btnActive.hover.background = _btnActive.normal.background;
            _btnActive.active.background = _btnActive.normal.background;
        }
    }

    private void OnGUI()
    {
        if (IntroScreen.IsVisible) return; // hide on intro screen
        Ensure();

        var bm = BuildModeController.Instance;
        bool active = bm != null && bm.IsActive;

        int fontSize = Mathf.RoundToInt(Mathf.Max(12f, Screen.height * fontPct));
        _btn.fontSize = fontSize;
        _btnActive.fontSize = fontSize;

        string label = active ? "🔨 Exit Build (B)" : "🔨 Build (B)";

        float w = Mathf.Max(160f, fontSize * 10f);
        float topExtra = Mathf.Max(minTopExtra, Screen.height * topExtraPct);
        Rect r = new Rect(Screen.width - w - offset.x, offset.y + topExtra, w, fontSize * 1.8f);

        if (GUI.Button(r, label, active ? _btnActive : _btn))
        {
            // Ensure systems exist
            BuildBootstrap.Ensure();
            bm = BuildModeController.Instance;
            if (bm == null)
            {
                var go = GameObject.Find("BuildSystems (Auto)");
                if (go == null) go = new GameObject("BuildSystems (Auto)");
                bm = go.GetComponent<BuildModeController>();
                if (bm == null) bm = go.AddComponent<BuildModeController>();
            }
            bm.ToggleBuildMode();
        }
    }

    private static Texture2D MakeTex(Color c)
    {
        var t = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        t.SetPixel(0, 0, c);
        t.Apply();
        return t;
    }
}

