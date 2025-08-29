using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central registry for job providers (buildings) and pawn job assignments.
/// </summary>
public class JobService : MonoBehaviour
{
    private class JobEntry
    {
        public int slots;
        public readonly List<PawnJob> assigned = new List<PawnJob>();
    }

    // Building -> JobType -> JobEntry
    private readonly Dictionary<Building, Dictionary<JobType, JobEntry>> _providers = new();

    // All known pawns
    private readonly List<PawnJob> _pawns = new();

    public void RegisterPawn(PawnJob pj)
    {
        if (!_pawns.Contains(pj)) _pawns.Add(pj);
    }

    public void UnregisterPawn(PawnJob pj)
    {
        _pawns.Remove(pj);
        foreach (var map in _providers.Values)
        {
            foreach (var e in map.Values)
            {
                e.assigned.Remove(pj);
            }
        }
    }

    public void SetSlots(Building b, JobType type, int slots)
    {
        if (b == null) return;
        if (!_providers.TryGetValue(b, out var map))
        {
            map = new Dictionary<JobType, JobEntry>();
            _providers[b] = map;
        }
        if (!map.TryGetValue(type, out var entry))
        {
            entry = new JobEntry();
            map[type] = entry;
        }
        entry.slots = Mathf.Max(0, slots);

        Rebalance(b, type, entry);
    }

    public List<PawnJob> AssignedFor(Building b, JobType type)
    {
        if (b == null) return new List<PawnJob>();
        if (_providers.TryGetValue(b, out var map) && map.TryGetValue(type, out var e))
        {
            return e.assigned;
        }
        return new List<PawnJob>();
    }

    private void Rebalance(Building b, JobType type, JobEntry e)
    {
        // Remove overfill
        while (e.assigned.Count > e.slots)
        {
            var pj = e.assigned[e.assigned.Count - 1];
            e.assigned.RemoveAt(e.assigned.Count - 1);
            if (pj != null && pj.AssignedBy == b) pj.SetJob(JobType.None, null);
        }

        // Fill underfill
        if (e.assigned.Count < e.slots)
        {
            foreach (var pj in _pawns)
            {
                if (pj == null) continue;
                if (!pj.IsIdle) continue;
                e.assigned.Add(pj);
                pj.SetJob(type, b);
                if (e.assigned.Count >= e.slots) break;
            }
        }
    }
}
