# Schemas Addendum — Equipment, Loadouts, Storage & Upkeep (v0.1)
*Additive to canvas **Modding Data Schemas v0.1 (Draft)**. Do not overwrite existing shapes.*

## EquipSlotDef@1
```yaml
id: core.Slot.Mainhand
schema: EquipSlotDef@1
tag: "slot.mainhand"
conflicts_any: ["slot.twohand"]
ui: { name_key: loc.slot.mainhand, icon: icons/slots/mainhand.png }
```

```yaml
id: core.Slot.Offhand
schema: EquipSlotDef@1
tag: "slot.offhand"
conflicts_any: ["slot.twohand"]
ui: { name_key: loc.slot.offhand, icon: icons/slots/offhand.png }
```

```yaml
id: core.Slot.Twohand
schema: EquipSlotDef@1
tag: "slot.twohand"
occupies_any: ["slot.mainhand","slot.offhand"]
ui: { name_key: loc.slot.twohand }
```

## ItemCategoryDef@1
```yaml
id: core.Cat.Weapon.Sword
schema: ItemCategoryDef@1
tags_any: ["weapon","weapon.sword","melee"]
slots_any: ["slot.mainhand"]
```

```yaml
id: core.Cat.Armor.Heavy
schema: ItemCategoryDef@1
tags_any: ["armor","armor.heavy"]
slots_any: ["slot.armor"]
```

## MaterialDef@1
```yaml
id: core.Material.Iron
schema: MaterialDef@1
weight_mult: 1.0
quality_bias: 0
resist_bias: { slash: 0.05, pierce: -0.05, blunt: 0.00 }
```

## QualityTierDef@1
```yaml
id: core.Quality.Common
schema: QualityTierDef@1
scalar: 1.00
affix_budget: 0
ribbon: "common"
```

```yaml
id: core.Quality.Masterwork
schema: QualityTierDef@1
scalar: 1.15
affix_budget: 3
ribbon: "masterwork"
```

## ConditionRuleDef@1
```yaml
id: core.Condition.Default
schema: ConditionRuleDef@1
decay:
  on_hit: 0.3
  on_block: 0.6
  per_minute_equipped: 0.02
  per_craft_use: 0.5   # tools at stations
penalties:
  soft_start: 40
  heavy_start: 15
  soft_mult: 0.9
  heavy_mult: 0.7
repair_floor_field: 60
```

## EncumbranceCurveDef@1
```yaml
id: core.Encumbrance.Default
schema: EncumbranceCurveDef@1
bands:
  - { up_to_capacity_pct: 0.60, move_mult: 1.00, evasion_mult: 1.00, stamina_regen_mult: 1.00 }
  - { up_to_capacity_pct: 1.00, move_mult: 0.95, evasion_mult: 0.95, stamina_regen_mult: 0.90 }
  - { up_to_capacity_pct: 1.30, move_mult: 0.85, evasion_mult: 0.90, stamina_regen_mult: 0.75 }
  - { up_to_capacity_pct: 9.99, move_mult: 0.70, evasion_mult: 0.80, stamina_regen_mult: 0.50, restrict_actions: true }
```

## AffixDef@1
```yaml
id: core.Affix.GuardPlus2
schema: AffixDef@1
applies_to_tags_any: ["armor","shield"]
stats: { GuardPct: +0.02 }
rarity_weight: 40
conflicts_any: []
```

## SocketDef@1
```yaml
id: core.Socket.Generic
schema: SocketDef@1
color: "neutral"
max_gems: 1
```

## GemDef@1
```yaml
id: core.Gem.EmberChip
schema: GemDef@1
socket: core.Socket.Generic
affixes_any: ["core.Affix.EmberResist5","core.Affix.CritPower2"]
```

## RepairRecipeDef@1
```yaml
id: core.Repair.Bench.IronWeapon
schema: RepairRecipeDef@1
applies_to_tags_any: ["weapon","material.iron"]
station_tags_any: ["station.anvil"]
inputs:
  - { item: core.Item.Ingot.Iron, qty: 1 }
  - { item: core.Item.RepairKit.Basic, qty: 1 }
restores: 60
```

## SalvageTableDef@1
```yaml
id: core.Salvage.IronWeapon
schema: SalvageTableDef@1
yields:
  - { item: core.Item.Scrap.Metal, qty_range: [1,3], weight: 70 }
  - { item: core.Item.Ingot.Iron, qty_range: [0,1], weight: 30 }
quality_bonus: 0.2
```

## ArmoryModuleDef@1
```yaml
id: core.Armory.Tier1
schema: ArmoryModuleDef@1
room_tag: "Armory"
storage_filters_any: ["weapon","armor","shield","quiver","focus"]
theft_risk: "medium"
interacts_with_alarm: true
```

## LockerDef@1
```yaml
id: core.Locker.Basic
schema: LockerDef@1
capacity_slots: 8
ownership: "personal"
```

## LockerAssignmentDef@1
```yaml
id: core.LockerAssign.Default
schema: LockerAssignmentDef@1
rules:
  - { pawn_tag: "class.guardian", locker: core.Locker.Basic }
```

## LoadoutTemplateDef@1 (finalized)
```yaml
id: core.Loadout.Guardian.Defense
schema: LoadoutTemplateDef@1
must_any: ["tag:weapon.1h","tag:shield"]
prefer_any: ["tag:armor.heavy","tag:trinket.block"]
forbid_any: ["tag:weapon.2h"]
slots:
  mainhand: "tag:weapon.1h"
  offhand:  "tag:shield"
  armor:    "tag:armor.heavy|tag:armor.medium"
  trinket:  "tag:trinket.block|tag:trinket.guard"
```

## EquipScoreDef@1 (finalized)
```yaml
id: core.EquipScore.Default
schema: EquipScoreDef@1
weights:
  tag_match: 1.0
  quality_scalar: 0.2
  material_scalar: 0.15
  condition_scalar: 0.10
  class_synergy: 0.25
  trait_synergy: 0.10
  encumbrance_penalty: -0.20
```

## EquipReservePolicyDef@1 (finalized)
```yaml
id: core.EquipReserve.ArmoryFirst
schema: EquipReservePolicyDef@1
sources_in_order: ["locker.personal","stockpile.armory","world_loose"]
```

## ItemDef — Equip extension (fields to add)
```yaml
# Extension fields (do not overwrite ItemDef@1; editor adds these when present)
equip:
  slots_any: ["slot.mainhand"]
  twohand: false
  quality: core.Quality.Common
  condition: 100
  material: core.Material.Iron
  weight: 3.5
  sockets_any: []
  affixes_any: []
```
