# Schemas Addendum — Logistics, Stockpiles, Hauling & Spoilage (v0.1)
*Additive to **Modding Data Schemas v0.1 (Draft)**. Do not overwrite core shapes.*

## StockpileFilterDef@1
```yaml
id: core.StockFilter.FoodCold
schema: StockpileFilterDef@1
include_tags_any: ["food","perishable"]
exclude_tags_any: ["raw.poison"]
materials_any: []
quality_min: null
freshness_any: ["Fresh","Stale"]
owner: null
```
## StorageContainerDef@1
```yaml
id: core.Container.ColdChest
schema: StorageContainerDef@1
capacity_slots: 12
stack_mult: 1.0
freshness_mult: 0.5
filter: core.StockFilter.FoodCold
power_draw_w: 80
effects_any: ["room.cold.aura"]
```
## LinkDef@1
```yaml
id: core.Link.KitchenInputs
schema: LinkDef@1
station: core.Building.Kitchen
pulls_any: ["core.Stockpile.KitchenPantry","core.Container.ColdChest"]
pushes_any: ["core.Stockpile.ReadyMeals"]
priority: 5
```
## ReservationPolicyDef@1
```yaml
id: core.Reserve.Default
schema: ReservationPolicyDef@1
merge_partial_stacks: true
prefetch_if_queue_gt: 1
allow_floor_pickup: false
timeout_s: 60
```
## HaulPolicyDef@1
```yaml
id: core.Haul.Default
schema: HaulPolicyDef@1
batch_pickup: core.BatchPickup.Small
prefer_linked_outputs: true
prefer_road_tiles: true
```
## BatchPickupDef@1
```yaml
id: core.BatchPickup.Small
schema: BatchPickupDef@1
max_items: 3
detour_limit_tiles: 6
```
## CartDef@1
```yaml
id: core.Cart.Wooden
schema: CartDef@1
carry_bonus: 40
move_mult_on_road: 1.25
move_mult_off_road: 0.95
requires_harness: false
durability: 100
```
## PackAnimalDef@1
```yaml
id: core.Pack.Donkey
schema: PackAnimalDef@1
base_speed: 1.05
carry_bonus: 60
upkeep_food_per_day: 2
temperament: "calm"
stable_required: true
```
## HarnessDef@1
```yaml
id: core.Harness.Basic
schema: HarnessDef@1
for_cart: core.Cart.Wooden
comfort_mod: +0.05
```
## StableModuleDef@1
```yaml
id: core.Stable.Tier1
schema: StableModuleDef@1
capacity_animals: 4
upkeep: { straw_per_day: 2, water_per_day: 6 }
```
## SpoilageDef@1
```yaml
id: core.Spoilage.Food
schema: SpoilageDef@1
phases:
  - { name: "Fresh",   min_pct: 0.66 }
  - { name: "Stale",   min_pct: 0.33 }
  - { name: "Spoiled", min_pct: 0.00 }
temp_curve:
  # multiplier to base half-life by ambient °C
  - { celsius_le: 0,  mult: 0.25 }
  - { celsius_le: 4,  mult: 0.40 }
  - { celsius_le: 10, mult: 0.60 }
  - { celsius_le: 20, mult: 1.00 }
  - { celsius_le: 30, mult: 1.50 }
  - { celsius_le: 40, mult: 2.25 }
humidity_mult: 1.00
thaw_penalty_mult: 1.25
```
## TemperatureProfileDef@1
```yaml
id: core.Temp.Room.Cold
schema: TemperatureProfileDef@1
target_celsius: 4
leak_rate: 0.10
power_draw_w: 300
```
## ColdRoomDef@1
```yaml
id: core.ColdRoom.Small
schema: ColdRoomDef@1
min_volume_m3: 12
requires: ["core.Temp.Room.Cold"]
freshness_mult: 0.5
```
## FloorSpeedDef@1
```yaml
id: core.Floor.Road
schema: FloorSpeedDef@1
move_mult: 1.20
```
## PathPenaltyDef@1
```yaml
id: core.Path.Door
schema: PathPenaltyDef@1
time_ms: 300
```
## VirtualCounterDef@1
```yaml
id: core.Counter.Meals
schema: VirtualCounterDef@1
include_filters_any: ["core.StockFilter.FoodCold"]
count_tags_any: ["meal"]
min_quality: null
```

## Validation
- Links must reference existing stockpiles/containers/stations.
- Cold containers require power if `power_draw_w > 0` or the room is flagged as **ColdRoom**.
- Spoilage curves must be monotonic by temperature; phases must cover 0–1.
- Floor/Path modifiers must clamp within [0.5×, 1.5×] unless flagged `extreme_ok`.
