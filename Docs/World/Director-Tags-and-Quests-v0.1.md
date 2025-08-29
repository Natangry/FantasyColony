# Director, World Tags & Dynamic Quests (v0.1, Docs Only)
> **Update:** Logistics hooks & disease risk signals.

## New Predicates
- `counter_lt: { counter: VirtualCounterId, value: number }`
- `spoilage_rate_gt: { scope: COLONY|ROOM, value: 0.0-? }`
- `haul_backlog_gt: { value: number }`

## New Actions
- `spawn_event: "MerchantCaravanFood"` (relief)
- `apply_mutator: core.Mutator.Rats` (if filth/spoilage rises)
- `schedule_beat: core.Incident.Festival` (morale after shortages)

## Example — Food Shortage Relief
```yaml
id: core.Rule.FoodRelief
when:
  all:
    - counter_lt: { counter: core.Counter.Meals, value: 12 }
    - beat_recent: { type: core.Incident.Raid, within_days: 2 }
then:
  - spawn_event: "MerchantCaravanFood"
limits: { cooldown_days: 3 }
```
