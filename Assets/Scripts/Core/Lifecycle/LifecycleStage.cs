using System;
namespace FantasyColony.Core.Lifecycle
{
    /// <summary>
    /// High-level, mutually exclusive app stages. Scene-free.
    /// Keep this list small and stable; prefer domain events for frequent gameplay signals.
    /// </summary>
    public enum LifecycleStage
    {
        None = 0,
        AppBoot,
        Intro,
        WorldSetup,
        WorldSim,
        EmbarkPrep,
        MapGen,
        Gameplay,
        // Terminal control stages
        RestartRequested,
        Shutdown,
    }
    /// <summary>
    /// Moments within a stage transition.
    /// </summary>
    public enum StageMoment
    {
        Enter = 0,
        Exit = 1
    }
    /// <summary>
    /// Context passed to lifecycle handlers (optional parameter).
    /// </summary>
    public sealed class StageContext
    {
        public LifecycleStage Previous { get; internal set; }
        public LifecycleStage Current { get; internal set; }
        public StageMoment Moment { get; internal set; }
        public DateTime UtcNow { get; internal set; }
        public StageRunner Runner { get; internal set; }
    }
}
