# Status & Dispels (v0.1, Docs Only)

**Goal:** Unify how statuses stack, tick, scale, and get removed, so abilities and items remain predictable.

## 1) StatusDef fields (extensions)
- `stacks_max`, `stack_behavior: additive|refresh|independent`
- `tick_ms`, `duration_s`, `snapshot_on_apply: true|false`, `scaling_mode: snapshot|dynamic`
- `schools_any: ["curse","poison","hex","wound"]`
- `dispel_power` (per school), `resist_school` (on target via RES)
- `on_apply`, `on_tick`, `on_expire` (effect refs)

## 2) Stacking Rules
- **additive**: stack value adds; duration refresh optional via `refresh_on_add: true|false`.
- **refresh**: replaces value, refreshes duration.
- **independent**: parallel stacks with independent timers (cap by `stacks_max`).

## 3) Snapshot vs Dynamic
- **snapshot**: capture attacker stats on apply; consistent ticks.
- **dynamic**: re-evaluate each tick (heavier; use sparingly).

## 4) Dispels
- Attempt if actor has a matching **school**; success if `dispel_power ≥ target.resist_school ± RNG`.  
- Bosses/Elites can flag `immune: ["wound"]` etc.

## 5) UI
- Buff/debuff icons with stack counts; tooltip shows **source**, **scaling mode**, **time left**, and **schools**.  
- Dispel abilities display **chance vs current target** in tooltip.

## 6) Acceptance
- Statuses behave consistently across snapshot/dynamic; stacking caps honored.  
- Dispels use schools and show odds; elites/bosses can be school-immune by data.

