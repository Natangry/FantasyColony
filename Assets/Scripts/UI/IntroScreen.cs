using UnityEngine;

/// <summary>
/// Scene-less intro menu overlay (IMGUI). Created by IntroBootstrap at first run.
/// We expose ShowIntro()/SetVisible so HardRestart can reboot back to this overlay.
/// </summary>
public class IntroScreen : MonoBehaviour
{
    public static IntroScreen Instance { get; private set; }
    public static bool IsVisible { get; private set; }

    // Backing for the existing "show/hide" used by your intro IMGUI
    bool showMenu = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        IsVisible = showMenu;
    }

    public void SetVisible(bool show)
    {
        showMenu = show;
        IsVisible = show;
    }

    /// <summary>
    /// Ensure an IntroScreen exists and display it.
    /// </summary>
    public static void ShowIntro()
    {
        if (Instance == null)
        {
            var go = new GameObject("IntroScreen (Auto)");
            go.AddComponent<IntroScreen>();
        }
        Instance.SetVisible(true);
    }

    // Minimal IMGUI to avoid compile errors if your existing overlay is elsewhere.
    // If you already have an IMGUI intro elsewhere, this won't draw because showMenu toggles that code path too.
    void OnGUI()
    {
        if (!showMenu) return;
        var r = new Rect(20, 20, 320, 40);
        GUI.Label(r, "Fantasy Colony — Intro (Press Start)");
    }
}

