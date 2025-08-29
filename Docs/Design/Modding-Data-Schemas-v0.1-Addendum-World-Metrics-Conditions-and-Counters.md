# Schemas Addendum — World Metrics, Conditions & Counters (v0.1)
*Additive to canvas **Modding Data Schemas v0.1 (Draft)**.*

## WorldMetricDef@1
```yaml
id: core.Metric.Region.Danger
schema: WorldMetricDef@1
scope: REGION
# DSL supports: + - * /, min/max, clamp, avg/sum over sources, ema_days, compare_to_avg_days
formula: "clamp( avg(encounter_tier * encounter_rate), 0, 1 )"
smoothing: { ema_days: 3 }
ui:
  show_chip: true
  name_key: loc.metric.danger
```

```yaml
id: core.Metric.Settlement.Sanitation
schema: WorldMetricDef@1
scope: SETTLEMENT
formula: "clamp( 1 - filth_index, 0, 1 )"
smoothing: { ema_days: 2 }
ui: { show_chip: true, name_key: loc.metric.sanitation }
```

```yaml
id: core.Metric.Route.Traffic
schema: WorldMetricDef@1
scope: ROUTE
formula: "normalize(caravan_arrivals_per_day, 0, 10)"
compare_to_avg_days: 7
ui: { show_chip: false }
```

## WorldConditionDef@1
```yaml
id: core.Cond.Region.HighDanger
schema: WorldConditionDef@1
metric: core.Metric.Region.Danger
escalate_at: 0.7
deescalate_below: 0.5
window: { ema_days: 3 }
cooldown_days: 2
ttl_days: 7
on_start:
  - add_tag: { tag: core.Tag.Wildlands, scope: REGION, ttl_days: 7 }
  - spawn_quest: core.Quest.ScoutTheWilds
on_end:
  - remove_tag: core.Tag.Wildlands
ui: { chip_key: loc.cond.high_danger }
```

```yaml
id: core.Cond.Settlement.OutbreakRisk
schema: WorldConditionDef@1
metric: core.Metric.Settlement.Sanitation
escalate_when_below: 0.4
deescalate_when_above: 0.6
window: { days_below: 2 }
cooldown_days: 3
on_start:
  - apply_mutator: core.Mutator.Plagued
  - set_law: { curfew: true, gate_screenings_pct: 0.4 }
on_end:
  - clear_mutator: core.Mutator.Plagued
  - set_law: { curfew: false }
ui: { chip_key: loc.cond.outbreak }
```

## EventCounterDef@1
```yaml
id: core.Counter.Route.Ambushes3d
schema: EventCounterDef@1
scope: ROUTE
window_days: 3
events_any: ["encounter.ambush"]
```

## Additional Examples (starter library)
```yaml
id: core.Metric.Region.CaravanTraffic
schema: WorldMetricDef@1
scope: REGION
formula: "normalize(caravan_arrivals_per_day, 0, 12)"
compare_to_avg_days: 7
```
```yaml
id: core.Cond.Route.HighwaymenSurge
schema: WorldConditionDef@1
requires_all:
  - metric_lte: { metric: core.Metric.Route.Traffic, value: 0.2 }
  - counter_gte: { counter: core.Counter.Route.Ambushes3d, value: 3 }
on_start:
  - add_tag: { tag: core.Tag.HighwaymenSurge, scope: ROUTE, ttl_days: 5 }
  - set_patrol_density: high
  - spawn_quest: core.Quest.EscortConvoy
on_end:
  - remove_tag: core.Tag.HighwaymenSurge
```

## Validation
- `deescalate_below < escalate_at` (or inverse if using above/below fields).
- `ema_days ≤ 14`, `window_days ≤ 14`, `cooldown_days ≤ 30`.
- No direct recursion: a condition cannot start/end itself or require itself.
- UI chips optional; metrics should clamp/normalize to avoid >1 or <0 unless explicitly raw.
