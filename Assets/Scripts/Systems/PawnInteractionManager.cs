using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Detects pawn/pawn overlaps and starts short "chat" interactions:
/// they walk side-by-side for a few seconds, then the follower returns to
/// its collision point along the shortest route.
/// If one pawn is controlled, the non-controlled pawn is always the follower.
/// </summary>
[AddComponentMenu("Systems/Pawn Interaction Manager")]
public class PawnInteractionManager : MonoBehaviour
{
    [SerializeField] private float minChatSeconds = 2.5f;
    [SerializeField] private float maxChatSeconds = 4.0f;
    [SerializeField] private float extraRadiusPadding = 0.05f; // small fudge in world units
    [SerializeField] private float pairRetestCooldown = 2.0f;  // seconds after an interaction ends before same pair can retrigger

    // Remember last time two specific pawns interacted to avoid immediate retriggers.
    private readonly Dictionary<(int,int), float> pairCooldownUntil = new Dictionary<(int,int), float>();

    private void Update()
    {
        // If any pawn is being controlled, still allow interactions with others (but controlled pawn will always be leader).
        if (SpritePawn.Instances.Count < 2) return;

        var now = Time.unscaledTime;
        // Copy to list to avoid potential enumeration issues if Instances changes mid-frame.
        var list = ListCache;
        list.Clear();
        foreach (var p in SpritePawn.Instances) if (p != null) list.Add(p);

        int n = list.Count;
        for (int i = 0; i < n; i++)
        {
            var a = list[i];
            if (a == null || !a.IsInteractable) continue;

            for (int j = i + 1; j < n; j++)
            {
                var b = list[j];
                if (b == null || !b.IsInteractable) continue;

                // Pair cooldown gate
                var key = GetKey(a, b);
                if (pairCooldownUntil.TryGetValue(key, out float until) && now < until) continue;

                // Distance check (XZ)
                Vector3 pa = a.transform.position;
                Vector3 pb = b.transform.position;
                float dx = pa.x - pb.x;
                float dz = pa.z - pb.z;
                float dist2 = dx * dx + dz * dz;
                float rad = a.CollisionRadius + b.CollisionRadius + extraRadiusPadding;
                if (dist2 > rad * rad) continue;

                // Decide leader/follower
                SpritePawn leader, follower;
                if (a.IsControlled && !b.IsControlled) { leader = a; follower = b; }
                else if (b.IsControlled && !a.IsControlled) { leader = b; follower = a; }
                else
                {
                    // 50/50 random when both are AI
                    if (Random.value < 0.5f) { leader = a; follower = b; } else { leader = b; follower = a; }
                }

                // Start chat interaction
                float seconds = Random.Range(minChatSeconds, maxChatSeconds);
                var returnMarker = follower.CaptureCurrentMarker();
                leader.BeginChatLeader(follower, seconds, returnMarker);
                follower.BeginChatFollower(leader, seconds, returnMarker);

                // Per-pair cooldown
                pairCooldownUntil[key] = now + pairRetestCooldown;
            }
        }
    }

    private static (int,int) GetKey(SpritePawn a, SpritePawn b)
    {
        int ia = a.GetInstanceID();
        int ib = b.GetInstanceID();
        return ia < ib ? (ia, ib) : (ib, ia);
    }

    // Simple reusable list to avoid allocs
    private static readonly List<SpritePawn> ListCache = new List<SpritePawn>(16);
}
