using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FantasyColony.Core.Services;
using FantasyColony.Core.Mods;

/// <summary>
/// Real (yet lenient) boot pipeline that prepares core services without blocking Main Menu.
/// Phases always succeed; errors are logged to help future hardening.
/// Signature kept the same (Run(Action<string>)) to avoid UI changes in BootScreen.
/// </summary>
namespace FantasyColony.Boot
{
    public static class AppBootstrap
    {
        public static IEnumerator Run(Action<string> setPhase)
        {
            // Phase 0: Build info for diagnostics
            setPhase?.Invoke("Reading build info...");
            yield return null;

            // Phase 1: Config
            setPhase?.Invoke("Loading configuration...");
            try
            {
                JsonConfigService.Instance.Load();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Config load failed, using defaults. {e.Message}");
            }
            yield return null;

            // Phase 2: Discover Mods
            setPhase?.Invoke("Discovering mods...");
            List<ModInfo> mods = null;
            try
            {
                mods = ModDiscovery.Discover();
                Debug.Log($"Mods discovered: {mods.Count}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Mod discovery failed: {e.Message}");
                mods = new List<ModInfo>();
            }
            yield return null;

            // Phase 3: Load Defs (lenient)
            setPhase?.Invoke("Loading defs...");
            try
            {
                var reg = DefRegistry.Instance;
                var errors = new List<DefError>();
                XmlDefLoader.Load(mods, reg, errors);
                if (errors.Count > 0)
                {
                    Debug.LogWarning($"Defs loaded with {errors.Count} issues. See log for details.");
                }
                else
                {
                    Debug.Log($"Defs loaded. Count={reg.Count}");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Def loading failed (lenient): {e.Message}");
            }
            yield return null;

            // Phase 4: Core Services (Localization, Audio, Save)
            setPhase?.Invoke("Initializing services...");
            try
            {
                var cfg = JsonConfigService.Instance;

                // Localization
                var lang = cfg.Get("language", "en");
                LocService.Instance.SetLanguage(lang);

                // Audio (volumes default 1.0)
                float vMaster = Parse01(cfg.Get("vol_master", "1"));
                float vMusic = Parse01(cfg.Get("vol_music", "1"));
                float vSfx = Parse01(cfg.Get("vol_sfx", "1"));
                AudioService.Instance.SetVolume("master", vMaster);
                AudioService.Instance.SetVolume("music", vMusic);
                AudioService.Instance.SetVolume("sfx", vSfx);

                // Save service touch (build slot cache)
                JsonSaveService.Instance.RefreshCache();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Service init had issues: {e.Message}");
            }
            yield return null;

            // Phase 5: Warm minimal assets (non-blocking)
            setPhase?.Invoke("Warming assets...");
            yield return null;

            // Phase 6: Finalize
            setPhase?.Invoke("Ready");
        }

        private static float Parse01(string s)
        {
            if (float.TryParse(s, out var f)) return Mathf.Clamp01(f);
            return 1f;
        }
    }
}
