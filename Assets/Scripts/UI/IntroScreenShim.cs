using UnityEngine;

/// <summary>
/// Compatibility shim for legacy code that references `IntroScreen`.
/// Delegates behavior to AppBootstrap's intro overlay discovery/show/hide.
/// This allows existing calls like `IntroScreen.ShowIntro()` and checks of
/// `IntroScreen.IsVisible` to keep working without depending on a specific
/// intro implementation or scene.
/// </summary>
public class IntroScreen : MonoBehaviour
{
    /// <summary>
    /// Returns whether any intro/title/menu overlay is currently visible.
    /// </summary>
    public static bool IsVisible
    {
        get => IntroMenuOverlay.IsOpen || AppBootstrap.IsIntroVisible();
        set
        {
            if (value) IntroMenuOverlay.Show();
            else IntroMenuOverlay.Hide();
        }
    }

    /// <summary>
    /// Legacy entry point to show the intro overlay.
    /// </summary>
    public static void ShowIntro() => IntroMenuOverlay.Show();

    /// <summary>
    /// If some legacy bootstrap does `new GameObject(...).AddComponent<IntroScreen>()`,
    /// keep the object around but delegate visibility to AppBootstrap.
    /// </summary>
    void Awake()
    {
        // Do not create any UI here; AppBootstrap handles discovery/fallback.
        DontDestroyOnLoad(gameObject);
    }

    // Prevent accidental IMGUI drawing from legacy stubs.
    void OnGUI() { /* intentionally empty; real intro overlay is external */ }
}
