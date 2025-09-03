using UnityEngine;
using FantasyColony.UI.Root;
using FantasyColony.UI.Router;
using FantasyColony.Core.Services;
using FantasyColony.UI.Screens;
using FantasyColony.UI.Util;

// Disambiguate our service interfaces from Unity's similarly named types
using FCLogger = FantasyColony.Core.Services.ILogger;
using FCFileLogger = FantasyColony.Core.Services.FileLogger;
using FCConfigService = FantasyColony.Core.Services.IConfigService;
using FCEventBus = FantasyColony.Core.Services.IEventBus;
using FCSimpleEventBus = FantasyColony.Core.Services.SimpleEventBus;
using FCAssetProvider = FantasyColony.Core.Services.IAssetProvider;
using FCResourcesProvider = FantasyColony.Core.Services.ResourcesAssetProvider;
using FCJsonConfig = FantasyColony.Core.Services.JsonConfigService;

namespace FantasyColony.Core
{
    /// <summary>
    /// Lifetime owner.
    /// Builds services, creates UIRoot and pushes Main Menu screen.
    /// </summary>
    public class AppHost : MonoBehaviour
    {
        public static AppHost Instance { get; private set; }
        public ServiceRegistry Services => _services;
        public UIRouter Router => _router;

        private ServiceRegistry _services;
        private UIRoot _uiRoot;
        private UIRouter _router;

        private void Awake()
        {
            // Duplicate guard: prefer the first instance
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            _services = new ServiceRegistry();
            _services.Register<FCLogger>(new FCFileLogger());
            // Use JSON-backed config as the single source of truth
            var cfg = FCJsonConfig.Instance;
            cfg.Load();
            _services.Register<FCConfigService>(cfg);
            _services.Register<FCEventBus>(new FCSimpleEventBus());
            _services.Register<FCAssetProvider>(new FCResourcesProvider());
            // Make AudioService discoverable via the registry
            _services.Register<AudioService>(AudioService.Instance);

            // Apply desktop frame pacing from config (defaults: vsync=1, target_fps=-1)
            int vsync = 1; int targetFps = -1;
            System.Int32.TryParse(cfg.Get("video.vsync", "1"), out vsync);
            System.Int32.TryParse(cfg.Get("video.target_fps", "-1"), out targetFps);
            QualitySettings.vSyncCount = Mathf.Max(0, vsync);
            Application.targetFrameRate = targetFps;

            // Create UI root (Canvas + EventSystem)
            _uiRoot = UIRoot.Create(transform);

            // Router mounts screens under UIRoot
            _router = new UIRouter(_uiRoot.ScreenParent, _services);

            // Attach AppFlow helper to manage restart/quit across the app
            var flow = gameObject.GetComponent<AppFlow>();
            if (flow == null) flow = gameObject.AddComponent<AppFlow>();
            flow.Initialize(_router);

            // Global shortcuts (e.g., F9 to open Boot Report) available in all builds
            var shortcuts = gameObject.GetComponent<GlobalShortcuts>();
            if (shortcuts == null) shortcuts = gameObject.AddComponent<GlobalShortcuts>();

            // Show Boot screen first so the UI covers while startup work initializes
            _router.Push<BootScreen>();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                GrayscaleSpriteCache.Clear();
            }
        }
    }
}
