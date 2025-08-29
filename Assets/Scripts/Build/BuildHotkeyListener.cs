using UnityEngine;

/// <summary>
/// Global listener that guarantees the B hotkey toggles Build Mode even if
/// the BuildModeController hasn't been instantiated yet.
/// </summary>
public class BuildHotkeyListener : MonoBehaviour
{
    private void Update()
    {
        if (IntroScreen.IsVisible) return; // ignore while at the intro menu

        if (Input.GetKeyDown(KeyCode.B))
        {
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
    }
}

