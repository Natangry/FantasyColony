using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Temporary intro overlay with Start / Quit buttons.
/// This version scales its layout and fonts with the screen resolution so it remains readable on 720p–4K.
/// </summary>
[AddComponentMenu("UI/Intro Screen (Temporary)")]
public class IntroScreen : MonoBehaviour
{
    [Header("Responsive Layout")]
    [Tooltip("Panel size as a percentage of the screen (width, height).")]
    [SerializeField] private Vector2 panelPercent = new Vector2(0.40f, 0.45f);

    [Tooltip("Min/Max width (pixels) clamp for the panel.")]
    [SerializeField] private Vector2 panelMinMaxW = new Vector2(420f, 900f);

    [Tooltip("Min/Max height (pixels) clamp for the panel.")]
    [SerializeField] private Vector2 panelMinMaxH = new Vector2(260f, 700f);

    [Header("Typography (as % of screen height)")]
    [SerializeField] private float titlePct = 0.060f;
    [SerializeField] private float buttonPct = 0.035f;

    [Header("Content")]
    [SerializeField] private string gameTitle = "Fantasy Colony";

    private bool showMenu = true;
    private bool focusedFirstButton;

    private GUIStyle titleStyle;
    private GUIStyle buttonStyle;
    private GUIStyle boxStyle;

    private void Awake()
    {
        // Ensure visible on first frame in case another script toggled this beforehand.
        showMenu = true;
    }

    private void OnGUI()
    {
        if (!showMenu)
            return;

        float sw = Screen.width;
        float sh = Screen.height;

        // Compute responsive panel size with sensible clamps.
        float panelW = Mathf.Clamp(sw * Mathf.Clamp01(panelPercent.x), panelMinMaxW.x, panelMinMaxW.y);
        float panelH = Mathf.Clamp(sh * Mathf.Clamp01(panelPercent.y), panelMinMaxH.x, panelMinMaxH.y);
        Rect panelRect = new Rect((sw - panelW) * 0.5f, (sh - panelH) * 0.5f, panelW, panelH);

        // Styles that scale with resolution.
        int titleFontSize = Mathf.Max(18, Mathf.RoundToInt(sh * titlePct));
        int buttonFontSize = Mathf.Max(12, Mathf.RoundToInt(sh * buttonPct));
        float buttonHeight = Mathf.Max(40f, sh * 0.06f);
        float contentPadding = Mathf.Round(panelH * 0.08f);

        // Title
        titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = titleFontSize,
            wordWrap = true,
            richText = true
        };

        // Buttons
        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = buttonFontSize
        };

        // Panel background
        boxStyle = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(
                Mathf.RoundToInt(contentPadding),
                Mathf.RoundToInt(contentPadding),
                Mathf.RoundToInt(contentPadding),
                Mathf.RoundToInt(contentPadding))
        };

        GUILayout.BeginArea(panelRect, GUIContent.none, boxStyle);
        {
            GUILayout.FlexibleSpace();

            GUILayout.Label($"<b>{gameTitle}</b>", titleStyle);

            GUILayout.Space(panelH * 0.06f);

            // Focus first button once so keyboard/gamepad users can press Enter/Space.
            if (!focusedFirstButton)
            {
                GUI.SetNextControlName("StartButton");
            }
            if (GUILayout.Button("Start", buttonStyle, GUILayout.Height(buttonHeight)))
            {
                OnStartGame();
            }
            if (!focusedFirstButton)
            {
                GUI.FocusControl("StartButton");
                focusedFirstButton = true;
            }

            GUILayout.Space(panelH * 0.02f);

            if (GUILayout.Button("Quit", buttonStyle, GUILayout.Height(buttonHeight)))
            {
                QuitGame();
            }

            GUILayout.FlexibleSpace();
        }
        GUILayout.EndArea();
    }

    // Called when Start is pressed: clear/hide the intro overlay.
    private void OnStartGame()
    {
        // Generate a small default grid map and frame the camera before hiding the menu.
        WorldBootstrap.GenerateDefaultGrid();
        // Spawn a SNES-style sprite pawn that patrols the visible area.
        PawnBootstrap.SpawnSpritePawn();

        showMenu = false;
        // The bootstrap GameObject is marked DontDestroyOnLoad, so we simply hide UI here.
        // Additional game flow can be wired in later when a real title scene exists.
    }

    // Called when Quit is pressed: exit the game (or stop play mode in Editor).
    private void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

