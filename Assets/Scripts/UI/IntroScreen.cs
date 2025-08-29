using UnityEngine;

/// <summary>
/// Temporary IMGUI-based startup overlay so we can verify Codex end-to-end
/// without creating prefabs. Will be replaced by a uGUI Canvas later.
/// </summary>
public class IntroScreen : MonoBehaviour
{
    [SerializeField] private bool showMenu = true;
    [SerializeField] private string title = "Fantasy Colony";
    [SerializeField] private float panelWidth = 360f;
    [SerializeField] private float panelHeight = 220f;

    void Start()
    {
        Debug.Log("Welcome to Fantasy Colony!");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            showMenu = true;
        }
    }

    /// <summary>
    /// Bound to the Start button; for now just hides the menu and logs.
    /// Later this will call Game.Startup().
    /// </summary>
    public void OnStartGame()
    {
        Debug.Log("Start Game pressed.");
        showMenu = false;
    }

    void OnGUI()
    {
        if (!showMenu) return;

        // Centered panel rect
        float w = panelWidth;
        float h = panelHeight;
        float x = (Screen.width - w) * 0.5f;
        float y = (Screen.height - h) * 0.5f;
        Rect rect = new Rect(x, y, w, h);

        // Panel background
        GUI.Box(rect, GUIContent.none);

        // Content area
        GUILayout.BeginArea(rect);
        GUILayout.FlexibleSpace();

        // Title
        var titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 24,
            fontStyle = FontStyle.Bold,
            wordWrap = true
        };
        GUILayout.Label(title, titleStyle);
        GUILayout.Space(16);

        // Start button
        if (GUILayout.Button("Start Game", GUILayout.Height(36)))
        {
            OnStartGame();
        }

        // Quit button
        if (GUILayout.Button("Quit", GUILayout.Height(32)))
        {
            QuitGame();
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndArea();
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

