using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FantasyColony.Core.Services;
using FantasyColony.Core.Mods;

namespace FantasyColony.Boot {
    /// <summary>
    /// Modular boot pipeline. Each step is an IBootTask for extensibility and robust error handling.
    /// </summary>
    public static class BootPipeline {
        public static IEnumerator Run(Action<string> setPhase) {
            var report = new Boot.BootReport();
            var ctx = new Boot.BootContext { Report = report };

            foreach (var task in Boot.BootTaskRegistry.DefaultTasks()) {
                setPhase?.Invoke(task.Title);
                yield return task.Execute(ctx);
            }

            // Ready
            setPhase?.Invoke("Ready");
            Boot.BootReport.Last = report;
        }
    }

    // --- Boot framework ---------------------------------------------------
    public interface IBootTask {
        string Title { get; }
        IEnumerator Execute(BootContext ctx);
    }

    public sealed class BootContext {
        public List<ModInfo> Mods = new List<ModInfo>();
        public DefRegistry Defs => DefRegistry.Instance;
        public JsonConfigService Config => JsonConfigService.Instance;
        public BootReport Report;
    }

    public sealed class BootReport {
        public struct Step { public string title; public float seconds; public string warn; public string error; }
        public readonly List<Step> steps = new();
        public static BootReport Last { get; internal set; }
        internal void Add(string title, float dt, string warn = null, string error = null) {
            steps.Add(new Step { title = title, seconds = dt, warn = warn, error = error });
        }
    }

    public static class BootTaskRegistry {
        public static IEnumerable<IBootTask> DefaultTasks() {
            yield return new ConfigTask();
            yield return new DiscoverModsTask();
            yield return new LoadDefsTask();
            yield return new ValidateAndMigrateDefsTask();
            yield return new InitServicesTask();
            yield return new WarmAssetsTask();
        }
    }

    // --- Concrete tasks ---------------------------------------------------
    sealed class ConfigTask : IBootTask {
        public string Title => "Loading configuration...";
        public IEnumerator Execute(BootContext ctx) {
            var t0 = Time.realtimeSinceStartup;
            string warn = null, err = null;
            try { ctx.Config.Load(); }
            catch (Exception e) { warn = $"Config load failed, defaults used: {e.Message}"; Debug.LogWarning(warn); }
            ctx.Report?.Add(Title, Time.realtimeSinceStartup - t0, warn, err);
            yield return null;
        }
    }

    sealed class DiscoverModsTask : IBootTask {
        public string Title => "Discovering mods...";
        public IEnumerator Execute(BootContext ctx) {
            var t0 = Time.realtimeSinceStartup;
            string warn = null;
            try { ctx.Mods = ModDiscovery.Discover(); Debug.Log($"Mods discovered: {ctx.Mods.Count}"); }
            catch (Exception e) { warn = $"Mod discovery failed: {e.Message}"; Debug.LogWarning(warn); ctx.Mods = new List<ModInfo>(); }
            ctx.Report?.Add(Title, Time.realtimeSinceStartup - t0, warn, null);
            yield return null;
        }
    }

    sealed class LoadDefsTask : IBootTask {
        public string Title => "Loading defs...";
        public IEnumerator Execute(BootContext ctx) {
            var t0 = Time.realtimeSinceStartup;
            string warn = null;
            try {
                var errors = new List<DefError>();
                XmlDefLoader.Load(ctx.Mods, ctx.Defs, errors);
                if (errors.Count > 0) warn = $"Defs loaded with {errors.Count} issues. See log.";
                if (ctx.Defs.ConflictCount > 0) {
                    var note = $"Conflicts={ctx.Defs.ConflictCount}";
                    warn = string.IsNullOrEmpty(warn) ? note : ($"{warn} | {note}");
                }
                Debug.Log($"Defs loaded. Count={ctx.Defs.Count}");
            } catch (Exception e) { warn = $"Def loading failed (lenient): {e.Message}"; Debug.LogWarning(warn); }
            ctx.Report?.Add(Title, Time.realtimeSinceStartup - t0, warn, null);
            yield return null;
        }
    }

    sealed class ValidateAndMigrateDefsTask : IBootTask {
        public string Title => "Validating & migrating defs...";
        public IEnumerator Execute(BootContext ctx) {
            var t0 = Time.realtimeSinceStartup;
            string warn = null;
            int warnCount = 0, migCount = 0;
            try {
                // Build index first, then load schemas from StreamingAssets and from each mod root inferred via index
                var index = FantasyColony.Core.Defs.DefIndex.Build(ctx.Mods, ctx.Defs);
                FantasyColony.Core.Defs.Validation.SchemaCatalog.EnsureLoadedFromIndex(index);
                var results = FantasyColony.Core.Defs.Validation.DefValidator.Run(index);
                foreach (var r in results) { Debug.LogWarning($"[Defs] {r}"); }
                warnCount = results.Count;
                migCount = FantasyColony.Core.Defs.Migrations.MigrationEngine.Run(index);
                if (warnCount > 0 || migCount > 0) {
                    warn = $"Validation warnings={warnCount}, migrations={migCount}";
                }
            } catch (Exception e) {
                warn = $"Validation phase had issues: {e.Message}";
                Debug.LogWarning(warn);
            }
            ctx.Report?.Add(Title, Time.realtimeSinceStartup - t0, warn, null);
            yield return null;
        }
    }

    sealed class InitServicesTask : IBootTask {
        public string Title => "Initializing services...";
        public IEnumerator Execute(BootContext ctx) {
            var t0 = Time.realtimeSinceStartup;
            string warn = null;
            try {
                var cfg = ctx.Config;
                var lang = cfg.Get("language", "en");
                LocService.Instance.SetLanguage(lang);
                float vMaster = Parse01(cfg.Get("vol_master", "1"));
                float vMusic  = Parse01(cfg.Get("vol_music", "1"));
                float vSfx    = Parse01(cfg.Get("vol_sfx", "1"));
                AudioService.Instance.SetVolume("master", vMaster);
                AudioService.Instance.SetVolume("music", vMusic);
                AudioService.Instance.SetVolume("sfx", vSfx);
                JsonSaveService.Instance.RefreshCache();
            } catch (Exception e) { warn = $"Service init had issues: {e.Message}"; Debug.LogWarning(warn); }
            ctx.Report?.Add(Title, Time.realtimeSinceStartup - t0, warn, null);
            yield return null;
        }
        private static float Parse01(string s) {
            if (float.TryParse(s, out var v)) {
                if (float.IsNaN(v) || float.IsInfinity(v)) return 1f;
                return Mathf.Clamp01(v);
            }
            return 1f;
        }
    }

    sealed class WarmAssetsTask : IBootTask {
        public string Title => "Warming assets...";
        public IEnumerator Execute(BootContext ctx) {
            var t0 = Time.realtimeSinceStartup;
#if ADDRESSABLES
            try {
                var handle = UnityEngine.AddressableAssets.Addressables.InitializeAsync();
                yield return handle;
            } catch { /* fail-soft */ }
#else
            yield return null;
#endif
            ctx.Report?.Add(Title, Time.realtimeSinceStartup - t0, null, null);
        }
    }
}

