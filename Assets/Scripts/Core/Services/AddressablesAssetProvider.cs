#if ADDRESSABLES
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace FantasyColony.Core.Services {
    /// <summary>
    /// Optional Addressables-backed asset provider.
    /// Compiles only when ADDRESSABLES define is present.
    /// </summary>
    public sealed class AddressablesAssetProvider : IAssetProvider {
        private static bool _initialized;

        public static void EnsureInitialized() {
            if (_initialized) return;
            try {
                var h = Addressables.InitializeAsync();
                h.Completed += _ => { _initialized = true; };
            } catch { /* swallow; fail-soft */ }
        }

        public Sprite LoadSprite(string virtualPath) {
            EnsureInitialized();
            return LoadSync<Sprite>(virtualPath);
        }

        public AudioClip LoadAudio(string virtualPath) {
            EnsureInitialized();
            return LoadSync<AudioClip>(virtualPath);
        }

        private static T LoadSync<T>(string key) where T : UnityEngine.Object {
            try {
                AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
                return handle.WaitForCompletion();
            } catch { return null; }
        }
    }
}
#endif
