using System;
using UnityEngine;

namespace Core.Services
{
    /// <summary>
    /// Lightweight audio bootstrap: one BGM source and one-shot SFX source.
    /// No Addressables; uses Resources for now. Safe no-ops if assets are missing.
    /// </summary>
    public class AudioService : MonoBehaviour
    {
        private static AudioService _instance;
        public static AudioService Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("__AudioService");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<AudioService>();
                }
                return _instance;
            }
        }

        private AudioSource _bgm;
        private AudioSource _sfx;

        private float _volMaster = 1f, _volMusic = 1f, _volSfx = 1f;

        private void Awake()
        {
            _bgm = gameObject.AddComponent<AudioSource>();
            _bgm.loop = true;
            _bgm.playOnAwake = false;

            _sfx = gameObject.AddComponent<AudioSource>();
            _sfx.loop = false;
            _sfx.playOnAwake = false;
        }

        public void SetVolume(string channel, float linear01)
        {
            linear01 = Mathf.Clamp01(linear01);
            switch (channel)
            {
                case "master": _volMaster = linear01; break;
                case "music": _volMusic = linear01; break;
                case "sfx": _volSfx = linear01; break;
            }
            ApplyVolumes();
        }

        private void ApplyVolumes()
        {
            if (_bgm != null) _bgm.volume = _volMaster * _volMusic;
            if (_sfx != null) _sfx.volume = _volMaster * _volSfx;
        }

        public void PlayBgm(string resourceKey)
        {
            try
            {
                var clip = Resources.Load<AudioClip>(resourceKey);
                if (clip == null)
                {
                    Debug.LogWarning($"AudioService: BGM not found at Resources/{resourceKey}");
                    return;
                }
                _bgm.clip = clip;
                ApplyVolumes();
                _bgm.Play();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"AudioService PlayBgm error: {e.Message}");
            }
        }

        public void StopBgm()
        {
            if (_bgm != null) _bgm.Stop();
        }

        public void PlaySfx(string resourceKey)
        {
            try
            {
                var clip = Resources.Load<AudioClip>(resourceKey);
                if (clip == null)
                {
                    Debug.LogWarning($"AudioService: SFX not found at Resources/{resourceKey}");
                    return;
                }
                ApplyVolumes();
                _sfx.PlayOneShot(clip);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"AudioService PlaySfx error: {e.Message}");
            }
        }
    }
}
