# Schemas Addendum — Settlements & Services (v0.1)
*Additive to canvas **Modding Data Schemas v0.1 (Draft)**.*

## SettlementSceneTemplateDef@1
```yaml
id: core.SettScene.ForestTown
schema: SettlementSceneTemplateDef@1
biome_style: "Forest"
lotset: core.LotSet.SmallTown
entrances_any: ["gate_north","gate_south"]
service_modules_any: ["core.Service.Inn","core.Service.General","core.Service.BountyBoard","core.Service.Stockade"]
mutator_hooks_any: ["core.SettMutHook.Corrupt", "core.SettMutHook.Plagued", "core.SettMutHook.SoulLooms"]
condition_hooks_any: ["core.SettCond.SeasonalWinter","core.SettCond.ProsperityHigh","core.SettCond.RouteClosed"]
```

## LotSetDef@1
```yaml
id: core.LotSet.SmallTown
schema: LotSetDef@1
lots:
  - { type: "civic", max: 3 }
  - { type: "commercial", max: 6 }
  - { type: "market", max: 4 }
  - { type: "housing", max: 10 }
```

## BuildingSpawnRuleDef@1
```yaml
id: core.BuildSpawn.CorruptBias
schema: BuildingSpawnRuleDef@1
when:
  any:
    - has_settlement_mutator: core.Mutator.CorruptRegime
effects:
  add_services_any: ["core.Service.BlackMarket"]
  weight_deltas:
    building.Blacksmith: -10
    building.Stockade: +10
  npc_deltas:
    "guard.patrol": -2
    "clerk.bribe": +1
```

## ServiceModuleDef@1 (examples)
```yaml
id: core.Service.BlackMarket
schema: ServiceModuleDef@1
vendor_profile: core.Vendor.BlackMarket
open_hours: { start: "21:00", end: "03:00" }
law_overrides: { illegal_goods_allowed: true, suspicion_gain: 0.15 }
ui_badges_any: ["illegal","night_only"]
```
```yaml
id: core.Service.Clinic
schema: ServiceModuleDef@1
vendor_profile: core.Vendor.Healer
open_hours: { start: "08:00", end: "20:00" }
screenings_pct: 0.4
```

## SettlementMutatorHookDef@1
```yaml
id: core.SettMutHook.Plagued
schema: SettlementMutatorHookDef@1
applies_if: { has_settlement_mutator: core.Mutator.Plagued }
effects:
  add_services_any: ["core.Service.Clinic"]
  add_lots_any: [{ type: "special", id: "QuarantineTents", count: 1 }]
  law_toggles: { curfew: true, gate_screenings_pct: 0.4 }
  price_bias_any: [{ tag: "herb", delta: -0.1 }, { tag: "luxury", delta: +0.1 }]
```

## SettlementConditionHookDef@1
```yaml
id: core.SettCond.RouteClosed
schema: SettlementConditionHookDef@1
applies_if:
  all:
    - route_closed_any: ["core.Route.TownToPort"]
effects:
  caravan_spawn_mult: 0.5
  add_services_any: ["core.Service.FerryNotice"]
  rumor_table_add: "core.Rumor.BridgeDown"
```

## PopulationProfileDef@1
```yaml
id: core.Pop.SmallTown.Default
schema: PopulationProfileDef@1
archetypes:
  - { id: "guard.patrol", count: [2,3] }
  - { id: "clerk.general", count: [1,2] }
  - { id: "priest", count: 1 }
open_hours:
  market: { start: "09:00", end: "18:00" }
mutator_deltas_any:
  - id: core.PopDelta.Corrupt
  - id: core.PopDelta.Plagued
```

## PopulationDeltaDef@1
```yaml
id: core.PopDelta.Corrupt
schema: PopulationDeltaDef@1
when: { has_settlement_mutator: core.Mutator.CorruptRegime }
adjust:
  "guard.patrol": { add: -1 }
  "clerk.bribe": { add: +1 }
```

## LawEnforcementProfileDef@1, CrimeRuleDef@1, AlertLevelDef@1
```yaml
id: core.LawEnf.Township
schema: LawEnforcementProfileDef@1
response_time_s: [10,16]
jail_capacity: 6
```
```yaml
id: core.Crime.Pickpocket
schema: CrimeRuleDef@1
suspicion_gain: 0.2
fine_coin: [5,15]
```
```yaml
id: core.AlertLevel.Default
schema: AlertLevelDef@1
thresholds: [0.3, 0.6, 0.8]   # levels 1,2,3
curfew_at_or_above: 2
```

## SettlementEventDef@1 & FestivalActionDef@1
```yaml
id: core.Event.MarketDay
schema: SettlementEventDef@1
cadence_days: 3
effects:
  vendor_restock_mult: 0.75
  blueprint_weight_delta: +0.2
```
```yaml
id: core.Festival.Harvest
schema: FestivalActionDef@1
effects:
  morale_buff_mult: 1.02
  duration_h: 12
```

## RumorTableDef@1 & QuestBoardDef@1
```yaml
id: core.Rumor.TownDefault
schema: RumorTableDef@1
entries:
  - { weight: 20, rumor: loc.rumor.night_webs }
  - { weight: 15, rumor: loc.rumor.bridgedown }
```
```yaml
id: core.QuestBoard.Town
schema: QuestBoardDef@1
region_quest_cap: 3
paused_until_visit: true
```

## Validation
- At least one **service** providing rest (Inn) or care (Clinic) must exist unless explicitly flagged `outpost: true`.
- Mutator/Condition hooks must not both remove all guards and set curfew.
- Black Market is **night-only** by default unless overridden.

