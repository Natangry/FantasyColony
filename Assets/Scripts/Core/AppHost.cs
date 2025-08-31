using UnityEngine;
using FantasyColony.UI.Root;
using FantasyColony.UI.Router;
using FantasyColony.Core.Services;

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
            _services.Register<ILogger>(new FileLogger());
            _services.Register<IConfigService>(new DummyConfigService());
            _services.Register<IEventBus>(new SimpleEventBus());
            _services.Register<IAssetProvider>(new ResourcesAssetProvider());

            // Create UI root (Canvas + EventSystem)
            _uiRoot = UIRoot.Create(transform);

            // Router mounts screens under UIRoot
            _router = new UIRouter(_uiRoot.ScreenParent, _services);

            // Push Main Menu
            _router.Push<FantasyColony.UI.Screens.MainMenuScreen>();
        }
    }
}
