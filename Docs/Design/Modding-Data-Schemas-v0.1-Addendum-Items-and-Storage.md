# Schemas Addendum — Items, Inventory, Storage & Spoilage (v0.1)
*Extends the current **Modding Data Schemas v0.1 (Draft)**. Do **not** overwrite the canvas file—this is an addendum.*

## ItemDef+ (extensions)
```yaml
id: core.Item.Meal.SimpleStew
schema: ItemDef@2   # extends base
tags: ["food.meal", "meal.tier1"]
stack_size: 10
weight: 0.3
volume: 0.2
perishable:
  model: core.Spoilage.MealTier1
  on_stage_effects:
    Stale: { buff_multiplier_pct: -0.50 }
    Spoiled: { remove_buffs: true, add_status: core.Status.Queasy }
    Rotted: { convert_to: core.Item.Waste.Compstable }
```

## SpoilageModelDef
```yaml
id: core.Spoilage.MealTier1
schema: SpoilageModelDef@1
base_half_life_days: 1.0      # at 20°C, uncovered
temp_profile: core.TempProfile.FoodDefault
stages:
  - { name: "Fresh", min_freshness: 0.60 }
  - { name: "Stale", min_freshness: 0.30 }
  - { name: "Spoiled", min_freshness: 0.00 }
  - { name: "Rotted", min_freshness: -1.0 }
```

## TemperatureProfileDef
```yaml
id: core.TempProfile.FoodDefault
schema: TemperatureProfileDef@1
factors:
  - { when: "temp_c <= 0", decay_mult: 0.0, on_enter: { add_tag: "frozen" } }
  - { when: "0 < temp_c && temp_c < 5", decay_mult: 0.25 }
  - { when: "5 <= temp_c && temp_c < 12", decay_mult: 0.5 }
  - { when: "12 <= temp_c && temp_c < 25", decay_mult: 1.0 }
  - { when: "temp_c >= 35", decay_mult: 1.5 }
```

## ContainerDef
```yaml
id: core.Container.Barrel
schema: ContainerDef@1
capacity:
  slots: 12
filters:
  tags_any: ["liquid.*","ferment.*","meal.tier1"]
modifiers:
  freshness_mult: 0.9
access_points: [{ dir: "N" }, { dir: "S" }]
```

## StorageZoneDef
```yaml
id: core.StorageZone.Pantry
schema: StorageZoneDef@1
filters:
  tags_any: ["food.*"]
  perishable_only: true
priority: "High"
pull_radius_tiles: 24
accept_rotten: false
room_tags_any: ["room.pantry"]
```

## StockpileFilterDef
```yaml
id: core.StockpileFilter.Tools
schema: StockpileFilterDef@1
tags_any: ["tool.*"]
quality_min: "Common"
```

## ReservationTokenDef
```yaml
id: core.Reservation.ItemStack
schema: ReservationTokenDef@1
scope: "item_stack"
ttl_ms: 20000
```

## HaulJobDef
```yaml
id: core.Job.Haul.Basic
schema: HaulJobDef@1
bundling:
  prefer_same_destination: true
  max_waypoints: 3
reservations_required: ["core.Reservation.ItemStack", "core.Reservation.ContainerSlot"]
```

## CarryModelDef
```yaml
id: core.Carry.Default
schema: CarryModelDef@1
carry_cap_base: 10.0
carry_cap_per_might: 0.6
encumbrance_bands:
  - { pct_max: 0.5, move_mult: 1.0, stamina_mult: 1.0 }
  - { pct_max: 1.0, move_mult: 0.9, stamina_mult: 1.1 }
  - { pct_max: 9.9, move_mult: 0.6, stamina_mult: 1.4 }  # >100%
```

## Validation
- `ItemDef@2` requires `stack_size`, `weight`, and `volume` if `perishable` is set.
- `ContainerDef.capacity` must define at least `slots` or `volume`.
- `StorageZoneDef.pull_radius_tiles` must be positive; `priority` ∈ {Low,Normal,High}.
- `SpoilageModelDef.stages` must be ordered by `min_freshness` descending.
- `HaulJobDef` must reference valid `ReservationTokenDef`s.

