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
        public static BootReport Last { get; internal set; }
        public int ModsFound, ModsLoaded;
        public int DefsFound;
        public int DefsLoaded;
        public int Errors, Warnings;
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
            // Read config file and set up crash logging etc
            yield return JsonConfigService.Instance.RefreshAsync();
#if UNITY_EDITOR
            // reduce spam in console during editor play-mode starts
            Debug.unityLogger.logEnabled = JsonConfigService.Instance.GetBool("debug.unityLogs", false);
#endif
        }
    }

    sealed class DiscoverModsTask : IBootTask {
        public string Title => "Discovering mods...";
        public IEnumerator Execute(BootContext ctx) {
            ctx.Mods.Clear();
            foreach (var mod in ModManager.Instance.Discover()) {
                ctx.Mods.Add(mod);
                yield return null; // allow UI to tick while enumerating
            }
            ctx.Report.ModsFound = ctx.Mods.Count;
        }
    }

    sealed class LoadDefsTask : IBootTask {
        public string Title => "Loading defs...";
        public IEnumerator Execute(BootContext ctx) {
            foreach (var mod in ctx.Mods) {
                foreach (var doc in DefRegistry.Instance.LoadFromMod(mod)) {
                    yield return null; // allow UI to tick while loading
                }
            }
            ctx.Report.DefsFound = ctx.Defs.Count;
        }
    }

    sealed class ValidateAndMigrateDefsTask : IBootTask {
        public string Title => "Validating & migrating defs...";
        public IEnumerator Execute(BootContext ctx) {
            var results = DefValidator.RunAll(ctx.Defs);
            ctx.Report.Errors = results.Errors; ctx.Report.Warnings = results.Warnings;
            yield return null;
        }
    }

    sealed class InitServicesTask : IBootTask {
        public string Title => "Initializing services...";
        public IEnumerator Execute(BootContext ctx) {
            // Wire up core services
            var services = new ServiceRegistry();
            services.Register<ILogger>(new FileLogger());
            services.Register<IEventBus>(new SimpleEventBus());
            services.Register<IAssetProvider>(new ResourcesAssetProvider());
            services.Register<IConfigService>(JsonConfigService.Instance);
            // Save service remains optional at this stage
            services.Register<JsonSaveService>(JsonSaveService.Instance);
            AppHost.Instance.AttachServices(services);
            yield return null;
        }
    }

    sealed class WarmAssetsTask : IBootTask {
        public string Title => "Warming assets...";
        public IEnumerator Execute(BootContext ctx) {
            // Load commonly used fonts/icons/prefabs so first menus feel snappy
            yield return ResourcesAssetProvider.WarmupAsync();
        }
    }
}
