# Schemas Addendum — Faction Mutators & Settlement Scenes (v0.1)
*Extends canvas **Modding Data Schemas v0.1 (Draft)**. Additive only.*

## FactionMutatorDef@1
```yaml
id: core.Mutator.CorruptRegime
schema: FactionMutatorDef@1
triggers:
  any:
    - leader_trait_any: ["corrupt","avaricious"]
    - near_tag: { id: core.Tag.Banditry, hops_lte: 1 }
effects:
  tariffs_mult: 1.15
  allows_bribes: true
  black_market: true
  law_policy: core.Law.Corrupt
  patrol_density_mult: 0.9
  settlement_gen_knobs:
    crowd_density_mult: 0.9
    add_modules_any: ["BlackMarket","BribeDesk"]
ui_chip_key: loc.mutator.corrupt
```
## FactionLeaderDef@1
```yaml
id: core.Leader.BaronVex
schema: FactionLeaderDef@1
faction: core.Faction.Town
traits: ["corrupt","vain"]
applies_mutators: [ core.Mutator.CorruptRegime ]
```
## LawPolicyDef@1 / GuardPatrolDef@1
```yaml
id: core.Law.Corrupt
schema: LawPolicyDef@1
rules:
  bribe_threshold: 10
  search_chance_pct: 20
  curfew_hours: null
```
```yaml
id: core.Patrol.TownDefault
schema: GuardPatrolDef@1
routes: ["gate-market","market-plaza"]
density: "medium"
```
## SettlementTemplateDef@1
```yaml
id: core.Settlement.Town
schema: SettlementTemplateDef@1
modules_any: ["Gate","Market","Tavern","Plaza","Chapel","Guardhouse"]
style: core.SettlementStyle.Woodland
```
## SettlementModuleDef@1
```yaml
id: core.Module.Quarantine
schema: SettlementModuleDef@1
place_rules: { near: "Gate", tiles: 4..8 }
vendors_any: ["Infirmary"]
hooks:
  on_enter: [{ law_check: { policy: core.Law.Quarantine } }]
```
## VisitSceneDef@1 / SettlementStateDef@1
```yaml
id: core.VisitScene.Town
schema: VisitSceneDef@1
template: core.Settlement.Town
build_inputs:
  faction: "{{faction}}"
  mutators: "{{mutators}}"
  prosperity: "{{prosperity}}"
  quests: "{{active_quests}}"
  tags: "{{regional_tags}}"
```
```yaml
id: core.SettlementState.Default
schema: SettlementStateDef@1
seed: 42069
persisted_modules_any: ["BlackMarket","Quarantine"]
vendor_stocks_any: ["TownGeneral","Apothecary"]
cast_ids_any: []
```
## VendorDef (extension fields)
```yaml
id: core.Vendor.Fence
schema: VendorDef@2     # extends base
black_market: true
stock_tables: [ core.Stock.StolenGoods ]
```
## ContractBoardDef (extension)
```yaml
id: core.Board.Pirate
schema: ContractBoardDef@2
mutators_any: [ core.Mutator.PirateGang ]
```
## PriceModelDef/TradeRouteDef (extensions)
```yaml
id: core.PriceModel.MutatorAware
schema: PriceModelDef@2
mutator_mods:
  - { mutator: core.Mutator.Plagued, price_mult: { "medicine.*": 1.3 } }
```
```yaml
id: core.TradeRoute.WithTolls
schema: TradeRouteDef@2
toll_mods:
  - { mutator: core.Mutator.CorruptRegime, toll_add: 0.1 }
```
## Validation
- Mutator effects must not reduce tariffs below 0 or push price mult < 0.2 or > 3.0.
- VisitScene must resolve a `SettlementTemplateDef`; state persistence optional but recommended.
- Vendor/Board v2 fields remain optional; v1 content remains valid.
