# Schemas Addendum — New Game & Starting Scenarios (v0.1)
*Extends the current **Modding Data Schemas v0.1** (do not overwrite the main file in canvas).*

## StartScenarioDef
```yaml
id: core.Start.FrontierRefugees
schema: StartScenarioDef@1
display_key: loc.start.frontier_refugees
description_key: loc.start.frontier_refugees.desc
recommended_storytellers: [ core.Storyteller.Story, core.Storyteller.Classic ]
difficulty_default: core.Difficulty.Classic
worldgen_overrides:
  biome_weights: { forest: 1.0, plains: 0.8, swamp: 0.2 }
  route_density: "medium"
start_site_params:
  prefer_biomes_any: [ forest, plains ]
  avoid_traits_any: [ "short_growing_season" ]
initial_world_tags:
  - { id: core.Tag.RoadBandits, scope: REGION, intensity: 1 }
  - { id: core.Tag.ForageRich, scope: REGION, intensity: 1 }
starting_pawns:
  - template: core.PawnTemplate.Colonist.CookApprentice
  - template: core.PawnTemplate.Colonist.HarvesterNovice
resource_packs: [ core.StartPack.Refugee.Basic ]
auto_blueprints:
  - core.AutoBlueprint.Campfire
  - core.AutoBlueprint.BedrollPair
  - core.AutoBlueprint.StockpileBasic
oath_pool: [ core.Oath.Hearthkeepers, core.Oath.Tinkerers, core.Oath.Wardens ]
burden_pool: [ core.Burden.LeanYears, core.Burden.RovingBandits, core.Burden.FragileHealth, core.Burden.HarshWinters ]
keepsake_pool: [ core.Keepsake.HeirloomTool, core.Keepsake.LuckyCharm, core.Keepsake.ScoutCompass, core.Keepsake.OldRecipe ]
tutorial_hooks:
  - { id: core.Tutorial.FirstFires, trigger: "game_start" }
  - { id: core.Tutorial.StationShiftIntro, trigger: "day1_morning" }
```

## PawnTemplateDef
```yaml
id: core.PawnTemplate.Colonist.CookApprentice
schema: PawnTemplateDef@1
name_pool: "human.common"
job_bias_any: [ core.Job.Cook, core.Job.Artisan ]
trait_bands:
  might: [2, 4]
  grace: [2, 4]
  insight: [3, 5]
starting_abilities_any: [ core.Ability.FieldRations ]
gear_prefs_any: [ "knife", "apron" ]
```

## StartResourcePackDef
```yaml
id: core.StartPack.Refugee.Basic
schema: StartResourcePackDef@1
items:
  - { tag: "meal.tier1", count: 12 }
  - { tag: "wood.log", count: 50 }
  - { tag: "cloth", count: 20 }
  - { id: core.Item.RepairKit, count: 2 }
tools:
  - { id: core.Tool.Hatchet.Wood.Common, count: 1 }
  - { id: core.Tool.Pick.Iron.Common, count: 1 }
```

## AutoBlueprintDef
```yaml
id: core.AutoBlueprint.Campfire
schema: AutoBlueprintDef@1
objects:
  - { id: core.Building.Campfire, pos: [0,0] }
```

## OathDef / BurdenDef / KeepsakeDef
```yaml
id: core.Oath.Hearthkeepers
schema: OathDef@1
effects: { bedroom_comfort_cap: +1, festival_clear_overworked: +1 }
```

```yaml
id: core.Burden.LeanYears
schema: BurdenDef@1
effects: { farm_yield_pct: -0.20, forage_richness_spring: +1 }
```

```yaml
id: core.Keepsake.HeirloomTool
schema: KeepsakeDef@1
grant_tool_upgrade:
  quality: core.ToolQuality.Fine
  class_any: [ core.ToolClass.Pick, core.ToolClass.Hatchet, core.ToolClass.Hammer, core.ToolClass.ChefsKit ]
  work_speed_pct: 0.03
```

## StorytellerDef & DifficultyDef (links to ADR-0002)
```yaml
id: core.Storyteller.Classic
schema: StorytellerDef@1
dials:
  director_tick_hours: 6
  news_per_region_per_day: 1
```

```yaml
id: core.Difficulty.Tempest
schema: DifficultyDef@1
quest_countdown_days_if_visited: [3,5]
pressure_pct: +0.15
economy_pct: -0.10
```

## Validation
- `StartScenarioDef` must include at least one **PawnTemplate**, **StartResourcePack**, and **AutoBlueprint**.
- `keepsake_pool` items must be unique per pawn at start.
- Initial world tags must resolve to valid `WorldTagDef`.
- Difficulty/storyteller references must resolve.
- Auto-blueprint objects must be placeable.
