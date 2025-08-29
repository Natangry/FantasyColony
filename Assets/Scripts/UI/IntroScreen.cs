using UnityEngine;

public class IntroScreen : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] private float titlePct = 0.18f;            // % of screen height for the title font size
    [SerializeField] private float buttonPct = 0.06f;           // % of screen height for button height
    [SerializeField] private float minButtonHeight = 64f;       // hard floor so buttons are never tiny
    [SerializeField] private Color backgroundColor = new Color(0.08f, 0.09f, 0.11f, 1f); // opaque

    [Header("Content")]
    [SerializeField] private string gameTitle = "Fantasy Colony";

    [Header("Map Settings")]
    [Tooltip("Select the starting map size.")]
    [SerializeField] private string[] mapSizeLabels = { "32×32", "64×64", "128×128", "256×256" };
    private static readonly int[] mapSizes = { 32, 64, 128, 256 };
    [SerializeField] private int selectedMapIndex = 2; // Default to 128×128

    private bool showMenu = true;
    private GUIStyle titleStyle;
    private GUIStyle buttonStyle;
    private GUIStyle sizeStyle;
    private GUIStyle sizeSelectedStyle;
    private GUIStyle confirmStyle;
    private GUIStyle bgStyle;
    private Texture2D bgTex;
    private Texture2D sizeTex;
    private Texture2D sizeSelTex;

    private void EnsureStyles()
    {
        if (bgTex == null)
        {
            bgTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            bgTex.SetPixel(0, 0, backgroundColor);
            bgTex.Apply();
        }
        if (sizeTex == null)
        {
            sizeTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            sizeTex.SetPixel(0, 0, new Color(0.22f, 0.24f, 0.28f, 1f));
            sizeTex.Apply();
        }
        if (sizeSelTex == null)
        {
            sizeSelTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            sizeSelTex.SetPixel(0, 0, new Color(0.32f, 0.52f, 0.92f, 1f));
            sizeSelTex.Apply();
        }
        if (bgStyle == null)
        {
            bgStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = bgTex },
                border = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(0, 0, 0, 0)
            };
        }
        if (titleStyle == null)
        {
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                wordWrap = true
            };
            titleStyle.normal.textColor = Color.white;
        }
        if (buttonStyle == null)
        {
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter
            };
        }
        if (sizeStyle == null)
        {
            sizeStyle = new GUIStyle(buttonStyle);
            sizeStyle.normal.background = sizeTex;
            sizeStyle.hover.background = sizeTex;
            sizeStyle.active.background = sizeTex;
            sizeStyle.fontStyle = FontStyle.Normal;
            sizeStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        }
        if (sizeSelectedStyle == null)
        {
            sizeSelectedStyle = new GUIStyle(buttonStyle);
            sizeSelectedStyle.normal.background = sizeSelTex;
            sizeSelectedStyle.hover.background = sizeSelTex;
            sizeSelectedStyle.active.background = sizeSelTex;
            sizeSelectedStyle.fontStyle = FontStyle.Bold;
            sizeSelectedStyle.normal.textColor = Color.white;
        }
        if (confirmStyle == null)
        {
            confirmStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            confirmStyle.normal.textColor = Color.white;
        }
    }

    private void OnGUI()
    {
        if (!showMenu) return;

        EnsureStyles();

        // Full-screen opaque background
        Rect full = new Rect(0, 0, Screen.width, Screen.height);
        GUI.Box(full, GUIContent.none, bgStyle);

        // Dynamic sizes based on screen height
        float titleSize = Mathf.Max(32f, Screen.height * titlePct);
        float btnH = Mathf.Max(minButtonHeight, Screen.height * buttonPct);

        titleStyle.fontSize = Mathf.RoundToInt(titleSize);
        buttonStyle.fontSize = Mathf.RoundToInt(btnH * 0.38f);
        sizeStyle.fontSize = buttonStyle.fontSize;
        sizeSelectedStyle.fontSize = buttonStyle.fontSize;
        confirmStyle.fontSize = Mathf.RoundToInt(btnH * 0.45f);

        GUILayout.BeginArea(full);
        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();

        // Title
        GUILayout.Label(gameTitle, titleStyle);
        GUILayout.Space(btnH * 0.6f);

        // Map size grid (2×2) with large, tappable buttons
        float gridPadding = Mathf.Max(8f, btnH * 0.25f);
        for (int row = 0; row < 2; row++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(gridPadding);
            for (int col = 0; col < 2; col++)
            {
                int i = row * 2 + col;
                if (i >= mapSizeLabels.Length) break;

                bool isActive = selectedMapIndex == i;
                string label = isActive ? mapSizeLabels[i] + "   \u2713" : mapSizeLabels[i];
                GUIStyle st = isActive ? sizeSelectedStyle : sizeStyle;
                if (GUILayout.Button(label, st, GUILayout.Height(btnH), GUILayout.ExpandWidth(true)))
                {
                    selectedMapIndex = i;
                }

                GUILayout.Space(gridPadding);
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(gridPadding * 0.6f);
        }

        GUILayout.Space(btnH * 0.2f);
        GUILayout.Label($"Map Size: {mapSizeLabels[selectedMapIndex]}", confirmStyle);

        GUILayout.Space(btnH * 0.4f);

        // Start button (extra tall)
        if (GUILayout.Button("Start", buttonStyle, GUILayout.Height(btnH * 1.2f)))
        {
            OnStartGame();
        }
        GUILayout.Space(gridPadding * 0.5f);

        // Quit button
        if (GUILayout.Button("Quit", buttonStyle, GUILayout.Height(btnH)))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    // Called when Start is pressed: clear/hide the intro overlay.
    private void OnStartGame()
    {
        // Generate the selected grid map and frame the camera before hiding the menu.
        int idx = Mathf.Clamp(selectedMapIndex, 0, mapSizes.Length - 1);
        int size = mapSizes[idx];
        WorldBootstrap.GenerateDefaultGrid(size, size, 1f);

        // Spawn test pawns
        PawnBootstrap.SpawnSpritePawn();
        PawnBootstrap.SpawnSecondPawn();

        // Reset the game clock to Day 1 at the configured start time.
        var clock = GameClockAPI.Find();
        if (clock != null) clock.ResetClock(1, 8, 0);

        showMenu = false;
    }
}

