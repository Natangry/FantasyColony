using UnityEngine;
using FantasyColony.UI.Root;
using FantasyColony.UI.Router;
using FantasyColony.Core.Services;

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
    /// Lifetime owner. Builds services, creates UIRoot and pushes Main Menu screen.
    /// </summary>
    public class AppHost : MonoBehaviour
    {
        private ServiceRegistry _services;
        private UIRoot _uiRoot;
        private UIRouter _router;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;

            _services = new ServiceRegistry();
            _services.Register<FCLogger>(new FCFileLogger());
            _services.Register<FCConfigService>(new FCDummyConfig());
            _services.Register<FCEventBus>(new FCSimpleEventBus());
            _services.Register<FCAssetProvider>(new FCResourcesProvider());

            // Create UI root (Canvas + EventSystem)
            _uiRoot = UIRoot.Create(transform);

            // Router mounts screens under UIRoot
            _router = new UIRouter(_uiRoot.ScreenParent, _services);

            // Push Main Menu
            _router.Push<FantasyColony.UI.Screens.MainMenuScreen>();
        }
    }
}
