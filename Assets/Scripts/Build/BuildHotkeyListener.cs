using UnityEngine;

/// <summary>
/// Global listener that guarantees the B hotkey toggles Build Mode even if
/// the BuildModeController hasn't been instantiated yet.
/// </summary>
public class BuildHotkeyListener : MonoBehaviour
{
    private void Toggle()
    {
        if (IntroScreen.IsVisible) return; // ignore while at the intro menu

        BuildBootstrap.Ensure();
        var bm = BuildModeController.Instance;
        if (bm == null)
        {
            var go = GameObject.Find("BuildSystems (Auto)");
            if (go == null) go = new GameObject("BuildSystems (Auto)");
            bm = go.GetComponent<BuildModeController>();
            if (bm == null) bm = go.AddComponent<BuildModeController>();
        }
        bm.ToggleBuildMode();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            Toggle();
        }
    }

    // Backstop for projects configured with the new Input System where GetKeyDown might be ignored
    private void OnGUI()
    {
        var e = Event.current;
        if (e != null && e.type == EventType.KeyDown && e.keyCode == KeyCode.B)
        {
            Toggle();
            e.Use();
        }
    }
}

