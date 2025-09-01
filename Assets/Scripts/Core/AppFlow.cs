using System.Collections;
using UnityEngine;
using FantasyColony.UI.Router;
using FantasyColony.UI.Screens;

namespace FantasyColony.Core {
    /// <summary>
    /// Centralized app-level flows (Restart, Quit) so buttons/screens don't need to know wiring.
    /// </summary>
    public class AppFlow : MonoBehaviour {
        private UIRouter _router;
        private ServiceRegistry _services;

        public static AppFlow Instance { get; private set; }

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        internal void Initialize(UIRouter router, ServiceRegistry services) {
            _router = router;
            _services = services;
        }

        /// <summary>
        /// Cleanly restarts to the Boot screen, re-running the boot pipeline.
        /// </summary>
        public void Restart() {
            if (_router == null) return;
            StartCoroutine(RestartCo());
        }

        private IEnumerator RestartCo() {
            // Cover with Boot screen first
            _router.PopAll();
            _router.Push<BootScreen>();

            // Give UI one frame to present cover
            yield return null;

            // Free anything not referenced anymore
            yield return Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }

        public void Quit() {
            Application.Quit();
        }
    }
}

namespace FantasyColony.UI.Screens {
    // Convenience shim to keep existing UI button bindings simple
    public static class AppFlowCommands {
        public static void Restart() => FantasyColony.Core.AppFlow.Instance?.Restart();
        public static void Quit() => FantasyColony.Core.AppFlow.Instance?.Quit();
    }
}
