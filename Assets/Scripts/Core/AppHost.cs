using UnityEngine;
using FantasyColony.UI.Root;
using FantasyColony.UI.Router;
using FantasyColony.Core.Services;
using FantasyColony.UI.Screens;

// Disambiguate our service interfaces from Unity's similarly named types
using FCLogger = FantasyColony.Core.Services.ILogger;
using FCFileLogger = FantasyColony.Core.Services.FileLogger;
using FCConfigService = FantasyColony.Core.Services.IConfigService;
using FCDummyConfig = FantasyColony.Core.Services.DummyConfigService;
using FCEventBus = FantasyColony.Core.Services.IEventBus;
using FCSimpleEventBus = FantasyColony.Core.Services.SimpleEventBus;
using FCAssetProvider = FantasyColony.Core.Services.IAssetProvider;
using FCResourcesProvider = FantasyColony.Core.Services.ResourcesAssetProvider;

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
            Instance = this;
            QualitySettings.vSyncCount = 1;
            // On desktop, let vSync drive frame pacing. Leave targetFrameRate unset (-1) unless vSync is disabled.
            Application.targetFrameRate = -1;

            _services = new ServiceRegistry();
            _services.Register<FCLogger>(new FCFileLogger());
            _services.Register<FCConfigService>(new FCDummyConfig());
            _services.Register<FCEventBus>(new FCSimpleEventBus());
            _services.Register<FCAssetProvider>(new FCResourcesProvider());

            // Create UI root (Canvas + EventSystem)
            _uiRoot = UIRoot.Create(transform);

            // Router mounts screens under UIRoot
            _router = new UIRouter(_uiRoot.ScreenParent, _services);

            // Attach AppFlow helper to manage restart/quit across the app
            var flow = gameObject.GetComponent<AppFlow>();
            if (flow == null) flow = gameObject.AddComponent<AppFlow>();
            flow.Initialize(_router, _services);

            // Global shortcuts (e.g., F9 to open Boot Report) available in all builds
            var shortcuts = gameObject.GetComponent<GlobalShortcuts>();
            if (shortcuts == null) shortcuts = gameObject.AddComponent<GlobalShortcuts>();

            // Show Boot screen first so the UI covers while startup work initializes
            _router.Push<BootScreen>();
        }
    }
}
