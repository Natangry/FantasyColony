using System;
namespace FantasyColony.Core.Lifecycle
{
    /// <summary>
    /// Attach to a <c>static</c> method to run it at a specific lifecycle stage/moment.
    /// Method signature may be <c>void M()</c> or <c>void M(StageContext ctx)</c>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public sealed class OnStageAttribute : Attribute
    {
        public LifecycleStage Stage { get; }
        public StageMoment Moment { get; }
        public int Order { get; }
        public bool RunOnce { get; }
        public OnStageAttribute(LifecycleStage stage, StageMoment moment = StageMoment.Enter, int order = 0, bool runOnce = false)
        { Stage = stage; Moment = moment; Order = order; RunOnce = runOnce; }
    }
    /// <summary>
    /// Declare that this handler should run before handlers declared on the given type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public sealed class OrderBeforeAttribute : Attribute
    { public Type Type { get; } public OrderBeforeAttribute(Type type) => Type = type; }
    /// <summary>
    /// Declare that this handler should run after handlers declared on the given type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public sealed class OrderAfterAttribute : Attribute
    { public Type Type { get; } public OrderAfterAttribute(Type type) => Type = type; }
}
