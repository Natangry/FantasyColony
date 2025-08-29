# Schemas Addendum — Fail-Forward & Rebuild (v0.1)
*Additive to canvas **Modding Data Schemas v0.1 (Draft)**. Do not overwrite.*

## CatastropheProfileDef@1
```yaml
id: core.Catastrophe.MajorI
schema: CatastropheProfileDef@1
death_pct_range: [0.0, 0.10]
captives_range: [1,2]
gear_loss_pct_range: [0.25,0.40]
scatter_table: core.Scatter.Default
world_consequence_deltas_any:
  - { mutator: core.Mutator.EvilLeaderAdvance, step: +1 }
  - { route_tag: core.Tag.TollRoads, ttl_days: 7 }
rebuild_packages_any:
  - core.Rebuild.BuilderKit
  - core.Rebuild.DefenderKit
  - core.Rebuild.HealerKit
reputation_credit: core.RepCredit.Default
```
## ScatterTableDef@1
```yaml
id: core.Scatter.Default
schema: ScatterTableDef@1
placements_any:
  - { weight: 30, where: "SETTLEMENT_NEARBY" }
  - { weight: 25, where: "ROUTE_NEARBY" }
  - { weight: 20, where: "POI_FACTION_CAMP" }
  - { weight: 15, where: "WILDS_HIDDEN" }
biases_any:
  - { last_friendly_region: +15 }
  - { faction_ties: +10 }
  - { active_quest_links: +8 }
survivor_states_any:
  - { state: core.Survivor.Alive, weight: 40 }
  - { state: core.Survivor.Injured, weight: 20 }
  - { state: core.Survivor.Captured, weight: 25 }
  - { state: core.Survivor.Missing, weight: 10 }
  - { state: core.Survivor.Dead, weight: 5 }
```
## SurvivorStateDef@1
```yaml
id: core.Survivor.Captured
schema: SurvivorStateDef@1
description_key: loc.survivor.captured
hooks:
  - spawn_rescue_chain: core.Rescue.Captive
  - add_map_note: core.Note.CaptiveLocation
```
## RuinSiteDef@1
```yaml
id: core.Ruin.Settlement
schema: RuinSiteDef@1
scene_template: core.Scene.Ruins
debris_tables_any: ["core.Loot.RuinDebris"]
ambush_table: core.Enc.RuinAmbush
legacy_rewards_any: ["core.Item.MemorialPlaque"]
```
## RebuildPackageDef@1 & ReputationCreditDef@1
```yaml
id: core.Rebuild.BuilderKit
schema: RebuildPackageDef@1
start_gear_any:
  - { item: core.Item.BasicTools, qty: 1 }
  - { item: core.Item.LumberStack, qty: 6 }
  - { item: core.Item.Bedroll, qty: 3 }
  - { item: core.Item.MealBundle, qty: 9 }
temp_buffs_any: ["core.Buff.ResilienceMode"]
```
```yaml
id: core.RepCredit.Default
schema: ReputationCreditDef@1
discounts:
  build_mult: 0.85
  tariff_mult: 0.85
vendor_bias:
  stock_bonus_rolls: 1
  haggle_floor_bonus: 0.05
duration_days: 7
```
## RescueChainDef@1
```yaml
id: core.Rescue.Captive
schema: RescueChainDef@1
steps:
  - { type: "lead", source: "RumorTable", ttl_days: [3,5] }
  - { type: "encounter", table: core.Enc.Kidnappers }
  - { type: "return", target: "COLONY_OR_OUTPOST" }
```
## MemorialDef@1
```yaml
id: core.Memorial.Stone
schema: MemorialDef@1
moodlet_key: loc.memorial.aura
aura_radius_tiles: 8
inscription: true
```
## QuestTemplateDef@4 (extension)
```yaml
# Adds an optional catastrophe link for major quests:
catastrophe_on_fail:
  profile: core.Catastrophe.MajorI
  severity_bias: 0   # −1 smaller, +1 harsher
```

## Director Hooks (predicates/actions)
- **Predicates:** `just_failed_major_quest`, `catastrophe_active`, `rebuild_days_remaining`, `survivors_scattered`.
- **Actions:** `spawn_ruin_site`, `grant_rebuild_package`, `apply_reputation_credit`, `seed_rescue_chain`, `advance_saga_consequence`.

## Validation
- At least one **RebuildPackage** present in a CatastropheProfile.
- ScatterTable must include ≥1 reachable placement.
- Quest using `catastrophe_on_fail` must be **major** tier (lint otherwise).
