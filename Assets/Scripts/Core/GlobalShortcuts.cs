using UnityEngine;
using FantasyColony.UI.Router;
using FantasyColony.UI.Screens;

namespace FantasyColony.Core {
    /// <summary>
    /// Lightweight global hotkeys available in all builds.
    /// Currently: F9 opens the Boot Report screen.
    /// </summary>
    public sealed class GlobalShortcuts : MonoBehaviour {
        private void Awake() {
            // Attach to AppHost GameObject; keep across restarts
            DontDestroyOnLoad(gameObject);
        }

        private void Update() {
            // Open Boot Report
            if (Input.GetKeyDown(KeyCode.F9)) {
                var router = UIRouter.Current;
                if (router == null) {
                    // Fallback if static Current isn't set yet
                    var host = AppHost.Instance;
                    router = host != null ? host.Router : null;
                }
                router?.Push<BootReportScreen>();
            }
        }
    }
}
