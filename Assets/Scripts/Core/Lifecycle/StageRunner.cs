using System;
using System.Diagnostics;
using UnityEngine;
namespace FantasyColony.Core.Lifecycle
{
    /// <summary>
    /// Drives stage transitions and invokes ordered handlers. Scene-free.
    /// </summary>
    public sealed class StageRunner
    {
        private static StageRunner _instance;
        public static StageRunner Instance => _instance ??= new StageRunner();
        public LifecycleStage Current { get; private set; } = LifecycleStage.None;
        public LifecycleStage Previous { get; private set; } = LifecycleStage.None;
        private bool _advancing;
        private StageRunner() { }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Bootstrap()
        {
            try { StageRegistry.Build(); Instance.AdvanceTo(LifecycleStage.AppBoot); }
            catch (Exception ex) { Debug.LogException(ex); }
        }
        /// <summary>
        /// Transition to the next stage. Safe to call from UI (e.g., Intro Start → WorldSetup).
        /// Ensures Exit(prev) handlers run before Enter(next) handlers.
        /// </summary>
        public void AdvanceTo(LifecycleStage next)
        {
            if (_advancing) { Debug.LogWarning($"[Lifecycle] Re-entrant AdvanceTo({next}) ignored while transitioning."); return; }
            if (next == Current) { Debug.LogWarning($"[Lifecycle] AdvanceTo({next}) ignored; already in this stage."); return; }
            _advancing = true;
            try
            {
                var prev = Current; InvokeHandlers(prev, StageMoment.Exit);
                Previous = prev; Current = next; InvokeHandlers(Current, StageMoment.Enter);
                Debug.Log($"[Lifecycle] Transition {prev} → {Current}");
            }
            catch (Exception ex) { Debug.LogException(ex); }
            finally { _advancing = false; }
        }
        private void InvokeHandlers(LifecycleStage stage, StageMoment moment)
        {
            if (stage == LifecycleStage.None) return;
            var handlers = StageRegistry.GetHandlers(stage, moment); if (handlers.Count == 0) return;
            var ctx = new StageContext { Previous = Previous, Current = stage, Moment = moment, UtcNow = DateTime.UtcNow, Runner = this };
            foreach (var meta in handlers)
            {
                if (meta.RunOnce && meta.HasRun) continue;
                var sw = Stopwatch.StartNew();
                try { var ps = meta.Method.GetParameters(); if (ps.Length == 0) meta.Method.Invoke(null, null); else meta.Method.Invoke(null, new object[] { ctx }); }
                catch (Exception ex) { Debug.LogError($"[Lifecycle] Exception in {meta.DeclaringType.FullName}.{meta.Method.Name}: {ex.GetBaseException().Message}"); Debug.LogException(ex); }
                finally { sw.Stop(); StageRegistry.MarkRun(meta); Debug.Log($"[Lifecycle] {stage}/{moment} → {meta.DeclaringType.Name}.{meta.Method.Name} ({sw.ElapsedMilliseconds} ms)"); }
            }
        }
    }
}
