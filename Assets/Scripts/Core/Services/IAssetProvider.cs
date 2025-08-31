using UnityEngine;

namespace FantasyColony.Core.Services
{
    public interface IAssetProvider
    {
        Sprite LoadSprite(string virtualPath);
        AudioClip LoadAudio(string virtualPath);
    }

    public sealed class ResourcesAssetProvider : IAssetProvider
    {
        public Sprite LoadSprite(string virtualPath)
        {
            return Resources.Load<Sprite>(virtualPath);
        }

        public AudioClip LoadAudio(string virtualPath)
        {
            return Resources.Load<AudioClip>(virtualPath);
        }
    }
}
