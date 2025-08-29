# Numbers — Director, Tags & Quests (v0.1 Defaults)

## Cadence & Caps
- **Director tick:** every **6h** and **on region entry**.
- **Per-region quest caps:** **3 Active**, **1 Paused** (Visited starts timers; Unvisited stays Paused).
- **Quest countdowns:** Story/Easy **6–7d** · Classic **4–6d** · Tempest **3–5d**.

## Proximity Distances (graph hops)
- **Same region:** 0 hops
- **Adjacent region/route:** 1 hop
- **Within 2 regions:** 2 hops

## Mutation Thresholds (first pass)
- `SpiderDen → BroodNest`: intensity **≥2**, season **Spring/Summer**.
- `BroodNest + UndeadPresence → CursedWebs`: intensity **≥1** each, proximity ≤2.
- `Any Beast + EvilHunter → TrainedBeasts (overlay)`: proximity ≤1.
- `Any Beast + WitchRite → Giant variant`: if Rite active within ≤1.

## Decay & TTL
- Default regional threat decay: **0.1/day**; TTL optional for temporary tags (e.g., `WebbedRoads` **5d**).
- After quest **success**, remove −1 to −2 intensity; **fail** adds +1 intensity or propagates to neighbor.

## Safety Valves
- If colony **alert count ≥ 3 red** or **food < 1 day**, bias **recovery/resource** quests 60/40 for 2 days.
- Limit **simultaneous escalations** from same family to **1 per region** per 5 days.

