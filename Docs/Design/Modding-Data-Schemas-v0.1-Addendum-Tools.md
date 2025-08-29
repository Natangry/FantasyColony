# Schemas Addendum — Tools & Equipment (v0.1)
*Extends the main Modding Data Schemas v0.1. Do **not** overwrite the main file.*

## ToolClassDef
```yaml
id: core.ToolClass.Pick
schema: ToolClassDef@1
display_key: loc.toolclass.pick
tags: ["mining"]
```

## ToolQualityDef
```yaml
id: core.ToolQuality.Fine
schema: ToolQualityDef@1
order: 2   # L=0, C=1, F=2, S=3, M=4, E=5 (example)
modifiers: { work_speed_pct: 0.05, output_quality_pct: 0.02, error_pct: -0.02 }
```

## ToolDef
```yaml
id: core.Tool.Pick.Iron.Common
schema: ToolDef@1
class: core.ToolClass.Pick
quality: core.ToolQuality.Common
durability_max: 120
base_modifiers: { }
affixes: []   # for Enchanted variants
```

## ToolSlotDef (Pawn)
```yaml
id: core.ToolSlot.Work
schema: ToolSlotDef@1
classes_supported: [ core.ToolClass.Pick, core.ToolClass.Hatchet, core.ToolClass.Hammer, core.ToolClass.ChefsKit, ... ]
loaner_allowed: true
```

## JobToolRequirementDef
```yaml
id: core.JobReq.Miner.Basic
schema: JobToolRequirementDef@1
job: core.Job.Miner
classes: [ core.ToolClass.Pick ]
min_quality: core.ToolQuality.Common   # Loaner insufficient
```

## StationToolRequirementDef
```yaml
id: core.StationReq.Forge.Basic
schema: StationToolRequirementDef@1
station: core.Station.Forge
classes: [ core.ToolClass.SmithHammer ]
min_quality: core.ToolQuality.Common
```

## DurabilityModelDef
```yaml
id: core.Dura.Pick.Default
schema: DurabilityModelDef@1
class: core.ToolClass.Pick
loss_per_action: 1
actions_per_loss: 6
loss_per_second: null
```

## RepairRecipeDef
```yaml
id: core.Repair.GenericKit
schema: RepairRecipeDef@1
inputs: [ { tag: "wood.plank", count: 1 }, { tag: "iron.nail", count: 1 }, { tag: "resin", count: 1 } ]
restore_amount: 40
station_tags_any: ["workbench"]
time_ms: 10000
```

## AutoAssignRuleDef
```yaml
id: core.AutoAssign.Tools.Default
schema: AutoAssignRuleDef@1
pickup_radius_tiles: 12
scoring: [ "quality", "condition", "distance" ]
```

## ToolRackDef
```yaml
id: core.Furniture.ToolRack
schema: ToolRackDef@1
capacity: 8
pickup_radius_tiles: 12
room_role_bonus: { Workshop: { work_speed_pct: 0.02 } }
```

## Validation
- Every job/station requirement must resolve to a valid **Tool Class**.
- `min_quality` must exist on the tier ladder and **≥ Common** if Loaners are disallowed.
- At least one **RepairRecipeDef** should exist for each Tool Class.
- Auto-assign radius must be positive; racks must have capacity ≥ 1.

> See `Docs/Design/Tools-and-Equipment-v0.1.md` and `Docs/Design/Numbers-Tools-v0.1.md` for the gameplay-facing overview.
