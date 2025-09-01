using UnityEngine;
using FantasyColony.UI.Router;
using FantasyColony.UI.Screens;
using UInput = UnityEngine.Input;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

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
            bool pressed = UInput.GetKeyDown(KeyCode.F9);
#if ENABLE_INPUT_SYSTEM
            if (!pressed) {
                var kb = Keyboard.current;
                if (kb != null && kb.f9Key.wasPressedThisFrame) pressed = true;
            }
#endif
            if (pressed) {
                var router = UIRouter.Current;
                if (router == null) {
                    // Fallback if static Current isn't set yet
                    var host = AppHost.Instance;
                    router = host != null ? host.Router : null;
                }
                if (router != null) router.Push<BootReportScreen>();
                else Debug.LogWarning("[GlobalShortcuts] No UIRouter available to open Boot Report.");
            }
        }
    }
}
