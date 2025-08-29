# Schemas Addendum — Buildings & Designations (v0.1)

Extends the main **Modding Data Schemas v0.1**. Do **not** overwrite the main file.

## BlueprintDef
```yaml
id: core.Blueprint.Wall_Wood
schema: BlueprintDef@1
layer: "Structure"
brush: "line|box|poly"
materials: [{ tag: "wood.plank", count_per_tile: 6 }]
build_ms_per_tile: 6000
```
## RoomRoleDef
```yaml
id: core.RoomRole.Tavern
schema: RoomRoleDef@1
tags: ["social","service"]
score_weights: { size: 0.3, decor: 0.4, lighting: 0.3 }
```

## BuildingDesignationDef
```yaml
id: core.Building.Bar
schema: BuildingDesignationDef@1
display_key: loc.designation.bar
allowed_room_roles: [ core.RoomRole.Tavern ]
requirements:
  mandatory:
    - { type: "service_point", id: core.ServicePoint.BarCounter, count: 1 }
    - { type: "furniture", tag: "table", count: 2 }
    - { type: "seat_per", of_tag: "table", ratio: 1.0 }
    - { type: "open_hours", block: "PM" }
    - { type: "staff", job: core.Job.Bartender, count: 1, shift: "PM" }
    - { type: "adjacency", needs: core.Building.Kitchen, within_tiles: 16, network_ok: true }
  optional:
    - { type: "furniture", id: core.Furniture.Stage, count: 1, perk: "music_aura" }
    - { type: "stock", tag: "drink.any", min: 10 }
operational:
  closed_if_missing_mandatory: true
  excellent_if_all_optional: true
```

## RequirementDef (shapes)
```yaml
# furniture/station
{ type: "furniture", id?: FurnitureId, tag?: "table|bed|rack", count: 2 }
# service point
{ type: "service_point", id: ServicePointId, count: 1 }
# seat-per-x
{ type: "seat_per", of_tag: "table", ratio: 1.0 }
# open hours
{ type: "open_hours", block: "AM|PM|Night" }
# staff
{ type: "staff", job: JobId, count: 1, shift: "AM|PM|Night" }
# stock link
{ type: "stock", tag: "meal.tier1|drink.any", min: 10 }
# adjacency to other building
{ type: "adjacency", needs: BuildingDesignationId, within_tiles: 16, network_ok: true }
# storage radius
{ type: "storage_within", tiles: 12 }
```

## ServicePointDef
```yaml
id: core.ServicePoint.BarCounter
schema: ServicePointDef@1
interactions: ["order","serve"]
queue_slots: 3
staff_jobs_any: [ core.Job.Bartender, core.Job.Cook ]
```

## OperationalRuleDef
```yaml
id: core.Operational.Rule.Default
schema: OperationalRuleDef@1
states:
  closed_if_missing_mandatory: true
  limited_if_missing_any_optional: true
  excellent_if_all_optional: true
```

## BoMRuleDef
```yaml
id: core.BoM.Defaults
schema: BoMRuleDef@1
derive:
  wall_wood: { from: core.Blueprint.Wall_Wood, per_tile: [{ tag:"wood.plank", count:6 }] }
  door_wood: { from: core.Blueprint.Door_Wood, per: [{ tag:"wood.plank", count:6 }, { tag:"iron.nail", count:2 }] }
```

## StaffRequirementDef
```yaml
id: core.Staff.BartenderPM
schema: StaffRequirementDef@1
job: core.Job.Bartender
shift: "PM"
count: 1
```

## StockRequirementDef
```yaml
id: core.Stock.BarDrinks
schema: StockRequirementDef@1
tag: "drink.any"
min: 10
```

## AdjacencyRequirementDef
```yaml
id: core.Adjacency.BarKitchen
schema: AdjacencyRequirementDef@1
needs: core.Building.Kitchen
within_tiles: 16
network_ok: true
```

## Validation
- Every `BuildingDesignationDef` must have at least one `mandatory` requirement.
- Staff requirements must point to valid **workforce** jobs.
- `seat_per` requires a matching `of_tag` present.
- BoM derivations must resolve to valid blueprints.
