using UnityEngine;
using FantasyColony.UI.Router;
using FantasyColony.UI.Screens;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace FantasyColony.Core {
    /// <summary>
    /// Lightweight global hotkeys available in all builds.
    /// F9: Boot Report  |  F10: UI Creator (stub)
    /// </summary>
    public sealed class GlobalShortcuts : MonoBehaviour {
        private void Awake() {
            // Attach to AppHost GameObject; keep across restarts
            DontDestroyOnLoad(gameObject);
        }

        private void Update() {
            // Open Boot Report (F9)
            bool pressed = false;
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null && kb.f9Key.wasPressedThisFrame) pressed = true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            if (!pressed && UnityEngine.Input.GetKeyDown(KeyCode.F9)) pressed = true;
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

            // Toggle UI Creator (F10)
            bool toggleCreator = false;
#if ENABLE_INPUT_SYSTEM
            var kb2 = Keyboard.current;
            if (kb2 != null && kb2.f10Key.wasPressedThisFrame) toggleCreator = true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            if (!toggleCreator && UnityEngine.Input.GetKeyDown(KeyCode.F10)) toggleCreator = true;
#endif
            if (toggleCreator) {
                var router = UIRouter.Current;
                if (router == null) {
                    var host = AppHost.Instance;
                    router = host != null ? host.Router : null;
                }
                if (router != null) {
                    if (FantasyColony.UI.Screens.UICreatorScreen.IsOpen) {
                        Debug.Log("[UICreator] F10 pressed -> close Creator");
                        router.Pop();
                    } else {
                        Debug.Log("[UICreator] F10 pressed -> open Creator");
                        router.Push(new FantasyColony.UI.Screens.UICreatorScreen());
                    }
                }
                else Debug.LogWarning("[GlobalShortcuts] No UIRouter available to open UI Creator.");
            }
        }
    }
}
