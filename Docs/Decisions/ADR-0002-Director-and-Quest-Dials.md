# ADR-0002: World Director & Quest Dials (v0.1)
**Decision date:** 2025-08-27  
**Status:** Accepted  
**Owner:** Design

## Context
The dynamic world is driven by **Tags** + a **Director** (rule engine) that spawns quests, moves borders, and alters encounters. We need clear defaults (“dials”) so v0.1 behaves predictably and modders have a stable baseline.

## Decision (Defaults)
- **Per-region quest caps:** **3 Active**, **1 Paused**. New seeds queue; if at cap, replace the **oldest Paused** (never force-add).
- **Countdowns (visited regions):** **3–7 in-game days**. Story/Easy: **6–7**; Classic: **4–6**; Tempest: **3–5**.
- **Director tick cadence:** Every **6 in-game hours**, plus an **immediate eval** on **region entry**.
- **FoW reseal:** Hostile regions **slowly re-fog** unless an **Outpost** is present (reseal off) or a **recent Patrol** occurred (reseal slowed).
- **Paused→Active rule:** Quests in **unvisited** regions remain **Paused** (no timer); **first entry** to that region starts their countdown.
- **Tag TTL/decay defaults:** Regional threats decay at **0.1/day** unless reinforced by rules; **Global** tags require explicit removal rules.
- **Rule cooldown defaults:** `cooldown_days: 6`, `max_simultaneous: 1` unless overridden.
- **Unknown Hero actor:** May clear minor dens off-screen; becomes **recruitable** when **notoriety ≥ 3** and the player has witnessed ≥ **1 deed**.
- **News/Rumors cadence:** At most **1 news item per day per region**; urgency styling when a countdown is **< 48h**.

## Alternatives considered
- Unlimited regional quests (overwhelming UI and pacing).
- Shorter tick (hourly) (noisy churn, perf overhead).
- No reseal (exploration felt “done” too quickly).

## Consequences
- Predictable pacing and readable UI load.
- Clear player agency: travel to start timers; establish outposts to hold visibility.
- Mod packs can safely deviate via Rule/Tag overrides without breaking base expectations.

## Follow-ups
- Surface these dials in a Numbers sheet for tuning (done in `Docs/Design/Numbers-World-Director-v0.1.md`).
- Update Overworld and Schemas docs with lifecycle & new defs (included in this change).

