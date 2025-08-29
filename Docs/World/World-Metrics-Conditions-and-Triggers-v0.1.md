# World Metrics, Conditions & Triggers (v0.1, Docs Only)

**Goal:** A mod-first **signals layer** the Director reads to fire **WorldTags, Quests, Mutators, Route effects, Events**, etc. Authors define **Metrics** (normalized 0–1 where possible), **Conditions** (thresholds with hysteresis & cooling), and **Counters** (windowed event tallies).

> Shapes live in `Docs/Design/Modding-Data-Schemas-v0.1-Addendum-World-Metrics-Conditions-and-Counters.md`. Numbers in `Docs/Design/Numbers-World-Conditions-and-Thresholds-v0.1.md`. Additive to the canvas **Modding Data Schemas v0.1 (Draft)**.

---

## Concepts

- **Metric** — computed value at a scope (GLOBAL / REGION / SETTLEMENT / ROUTE / FACTION / PARTY). Supports formulas, windowed averages (EMA), and clamping.
- **Condition** — watches one metric with `escalate_at`, `deescalate_below`, optional `window`, `cooldown_days`, and `on_start/on_end` actions.
- **Counter** — counts events within a sliding window (e.g., ambushes in 3 days).

### Why normalized?
- Keeps authoring simple and makes thresholds portable. Where raw units matter (coin, bodies, °C), authors can normalize (`/max`, `minmax`, or `zscore`).

---

## Stocked Metrics (starter set)

**Global:** `Season`, `MoonPhase`, `WeatherRegime`, `ArcaneSaturation`, `ApexSagaPressure`.

**Region:** `Danger`, `Prosperity`, `Unrest`, `PlaguePressure`, `MonsterPressure`, `CaravanTraffic`, `FaithFervor`, `Blight`.

**Settlement:** `Sanitation`, `FoodSecurity`, `Morale`, `LawAlert`, `Homelessness`, `JailLoad`, `BlackMarketVisibility`.

**Route:** `PatrolDensity`, `TrafficIndex`, `HazardLevel`, `PropagationDensity`.

**Faction:** `Corruption`, `WarPressure`, `TreasuryHealth`, `StanceTowardPlayer` (normalized favor).

**Party/Player Signals:** `EthosBalance`, `CaptureKillRatio`, `AftershockDensityNearby`, `ScoutingCoverage`, `FailureStreak`.

---

## Example Conditions (with actions)

- **Region High Danger**
  - *Escalate @* 0.70 (*de-escalate @* 0.50, EMA 3d) → add `Tag.Wildlands` (7d), spawn **Scout the Wilds**. On end: remove tag.

- **Settlement Outbreak Risk**
  - *Sanitation <* 0.40 for 2d → apply **Mutator.Plagued**, enable Clinic screenings, price deltas; clear after metric > 0.60 for 3d.

- **Route Highwaymen Surge**
  - *Traffic <* 0.20 & *Ambush Counter ≥* 3 (3d) → add `Tag.HighwaymenSurge` (routes), raise patrols & escort quests.

- **Arcane Leak**
  - *ArcaneSaturation >* 0.80 → add `Tag.RiftLeak` in high-fervor region; spawn anomaly events; end when < 0.60.

- **Faction Corruption**
  - *Corruption >* 0.60 (EMA 5d) → enable Black Market service, bribe dialogs; queue **Expose Corruption** chain.

- **Caravan Slump**
  - *CaravanTraffic* −30% vs 7d avg → **Supply Relief** quests, vendor stock −; cool when rebound ≥ −10%.

- **Aftershock Overload**
  - *AftershockDensityNearby ≥* 2 → stall new Apex saga seeds, bias counter-quests.

---

## Director integration

- **Predicates:** `cond_active: Id`, `metric_gte/lte`, `counter_gte/lte`, `trend_down/up`, `cooldown_ready: Id`.
- **Actions (examples):** `add_tag`, `remove_tag`, `spawn_quest`, `apply_mutator`, `set_patrol_density`, `set_route_open/toll/quarantine`, `add_map_note`, `schedule_event`.

**Rule snippet**
```yaml
id: core.Rule.RegionHighDanger
when:
  all:
    - cond_active: core.Cond.Region.HighDanger
then:
  - spawn_quest: core.Quest.ScoutTheWilds in: REGION weight: 60
  - add_map_note: core.Note.DangerHotspot
limits: { cooldown_days: 3, max_simultaneous: 1 }
```

---

## UI & UX

- **Map Chips:** each active Condition can expose a chip with name/ttl; tooltip shows driver metric (e.g., “Danger 0.74 (↑)”).
- **Inspector:** hover a region/settlement/route to see top 3 metrics + active conditions with explanations.
- **Journal:** when a Condition starts/ends, journal logs cause/effect and linked quests/tags.

---

## Validation & Safety

- All thresholds must respect `deescalate_below < escalate_at`.
- EMA/window periods capped (1–14d) and cooldowns (0–30d).
- No circular `on_start/on_end` actions that immediately re-trigger the same condition.
- Caps: per-scope max active conditions (default 4); overflow queues.

---

## Acceptance (v0.1)
- Metrics & Conditions defined across scopes with examples and defaults.
- Director can query predicates and fire actions from condition start/end.
- UI exposes chips/inspector; Numbers doc provides default thresholds/hysteresis.
