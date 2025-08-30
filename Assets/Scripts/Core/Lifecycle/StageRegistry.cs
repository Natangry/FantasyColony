using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
namespace FantasyColony.Core.Lifecycle
{
    internal readonly struct StageKey : IEquatable<StageKey>
    {
        public readonly LifecycleStage Stage;
        public readonly StageMoment Moment;
        public StageKey(LifecycleStage stage, StageMoment moment) { Stage = stage; Moment = moment; }
        public bool Equals(StageKey other) => Stage == other.Stage && Moment == other.Moment;
        public override bool Equals(object obj) => obj is StageKey k && Equals(k);
        public override int GetHashCode() => ((int)Stage * 397) ^ (int)Moment;
        public override string ToString() => $"{Stage}/{Moment}";
    }
    internal sealed class HandlerMeta
    {
        public MethodInfo Method;
        public Type DeclaringType;
        public OnStageAttribute Attr;
        public int Order;
        public bool RunOnce;
        public bool HasRun;
        public List<Type> BeforeTypes = new();
        public List<Type> AfterTypes = new();
        public override string ToString() => $"{DeclaringType.FullName}.{Method.Name} [Order={Order}{(RunOnce?", RunOnce":"")}]";
    }
    /// <summary>
    /// Discovers and orders lifecycle handlers. Scene-free and domain-reload safe.
    /// </summary>
    internal static class StageRegistry
    {
        private static bool _built;
        private static readonly Dictionary<StageKey, List<HandlerMeta>> _handlers = new();
        public static void Build()
        {
            if (_built) return;
            var sw = Stopwatch.StartNew();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var metas = new List<(StageKey key, HandlerMeta meta)>();
            foreach (var asm in assemblies)
            {
                Type[] types;
                try { types = asm.GetTypes(); }
                catch (ReflectionTypeLoadException e) { types = e.Types.Where(t => t != null).ToArray(); }
                foreach (var type in types)
                {
                    var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    foreach (var m in methods)
                    {
                        var onStages = (OnStageAttribute[])m.GetCustomAttributes(typeof(OnStageAttribute), false);
                        if (onStages == null || onStages.Length == 0) continue;
                        if (m.ReturnType != typeof(void)) { Debug.LogError($"[Lifecycle] {type.FullName}.{m.Name} must return void."); continue; }
                        var ps = m.GetParameters();
                        if (ps.Length > 1 || (ps.Length == 1 && ps[0].ParameterType != typeof(StageContext)))
                        { Debug.LogError($"[Lifecycle] {type.FullName}.{m.Name} has invalid parameters. Use () or (StageContext)."); continue; }
                        var before = ((OrderBeforeAttribute[])m.GetCustomAttributes(typeof(OrderBeforeAttribute), false)).Select(a => a.Type).Where(t => t != null).Distinct().ToList();
                        var after  = ((OrderAfterAttribute[]) m.GetCustomAttributes(typeof(OrderAfterAttribute),  false)).Select(a => a.Type).Where(t => t != null).Distinct().ToList();
                        foreach (var attr in onStages)
                        {
                            var meta = new HandlerMeta
                            { Method = m, DeclaringType = type, Attr = attr, Order = attr.Order, RunOnce = attr.RunOnce, BeforeTypes = new List<Type>(before), AfterTypes = new List<Type>(after) };
                            metas.Add((new StageKey(attr.Stage, attr.Moment), meta));
                        }
                    }
                }
            }
            foreach (var group in metas.GroupBy(x => x.key))
            {
                var ordered = OrderHandlers(group.Select(x => x.meta).ToList());
                _handlers[group.Key] = ordered;
            }
            _built = true;
            sw.Stop();
            Debug.Log($"[Lifecycle] Registry built in {sw.ElapsedMilliseconds} ms");
            PrintReport();
        }
        private static List<HandlerMeta> OrderHandlers(List<HandlerMeta> list)
        {
            var nodes = new HashSet<Type>(list.Select(m => m.DeclaringType));
            foreach (var m in list) { foreach (var t in m.BeforeTypes) nodes.Add(t); foreach (var t in m.AfterTypes) nodes.Add(t); }
            var edges = new Dictionary<Type, HashSet<Type>>();
            foreach (var n in nodes) edges[n] = new HashSet<Type>();
            foreach (var m in list)
            { foreach (var b in m.BeforeTypes) edges[m.DeclaringType].Add(b); foreach (var a in m.AfterTypes) edges[a].Add(m.DeclaringType); }
            var indeg = nodes.ToDictionary(n => n, n => 0);
            foreach (var kv in edges) foreach (var v in kv.Value) indeg[v] = indeg.TryGetValue(v, out var d) ? d + 1 : 1;
            var q = new Queue<Type>(indeg.Where(kv => kv.Value == 0).Select(kv => kv.Key));
            var topo = new List<Type>();
            while (q.Count > 0)
            {
                var u = q.Dequeue(); topo.Add(u);
                foreach (var v in edges[u]) { indeg[v]--; if (indeg[v] == 0) q.Enqueue(v); }
            }
            if (topo.Count != nodes.Count)
            {
                Debug.LogError("[Lifecycle] Ordering cycle detected among lifecycle handlers. Check OrderBefore/OrderAfter usage.");
                return list.OrderBy(m => m.Order).ThenBy(m => m.DeclaringType.FullName).ThenBy(m => m.Method.Name).ToList();
            }
            var rank = topo.Select((t, i) => (t, i)).ToDictionary(x => x.t, x => x.i);
            return list
                .OrderBy(m => rank.TryGetValue(m.DeclaringType, out var r) ? r : int.MaxValue)
                .ThenBy(m => m.Order)
                .ThenBy(m => m.DeclaringType.FullName)
                .ThenBy(m => m.Method.Name)
                .ToList();
        }
        public static IReadOnlyList<HandlerMeta> GetHandlers(LifecycleStage stage, StageMoment moment)
        { var key = new StageKey(stage, moment); return _handlers.TryGetValue(key, out var list) ? list : Array.Empty<HandlerMeta>(); }
        public static void PrintReport()
        {
            var sb = new StringBuilder(); sb.AppendLine("[Lifecycle] Registry Report →");
            foreach (var kv in _handlers.OrderBy(k => (int)k.Key.Stage).ThenBy(k => (int)k.Key.Moment))
            { sb.AppendLine($"  {kv.Key}:"); var i = 0; foreach (var m in kv.Value) sb.AppendLine($"    {++i,2}. {m}"); }
            Debug.Log(sb.ToString());
        }
        public static void MarkRun(HandlerMeta meta) => meta.HasRun = true;
    }
}
