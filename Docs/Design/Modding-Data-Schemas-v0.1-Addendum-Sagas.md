# Schemas Addendum — Major Sagas, Branching Questlines & Boss Arcs (v0.1)

*Extends canvas **Modding Data Schemas v0.1 (Draft)**. Do **not** overwrite that file.*

## MajorWorldTagDef
```yaml
id: core.Saga.Spider
schema: MajorWorldTagDef@1
states: ["Dormant","Stirring","Ascendant","Apex","Aftermath"]
state: "Dormant"
clock: core.SagaClock.Spider
branches_any:
  - core.SagaArc.Spider.WebsOfBone
  - core.SagaArc.Spider.TrainedPacks
  - core.SagaArc.Spider.GiantBrood
synergy:
  - { requires_tag: core.Tag.UndeadPresence, branch: core.SagaArc.Spider.WebsOfBone, hops_lte: 2 }
  - { requires_actor: core.Actor.EvilHunter, branch: core.SagaArc.Spider.TrainedPacks, hops_lte: 1 }
  - { requires_tag: core.Tag.WitchRite, branch: core.SagaArc.Spider.GiantBrood, hops_lte: 1 }
world_effects:
  on_success: core.WorldEffect.Spider.SilkBoom
  on_fail: core.WorldEffect.Spider.WebDominion
```

## SagaClockDef
```yaml
id: core.SagaClock.Spider
schema: SagaClockDef@1
tick_hours: 6
deltas:
  time_passage: { per_tick: { Dormant: 0, Stirring: +1, Ascendant: +1, Apex: 0 } }
  tag_trend:
    - { tag: core.Tag.SpiderDens, rising: true, delta: +1, states_any: ["Dormant","Stirring"] }
  quest_outcome:
    - { quest_family: "spider", on_success: -2, on_fail: +1 }
  synergy_present:
    - { tag: core.Tag.UndeadPresence, hops_lte: 2, delta: +1, states_any: ["Stirring","Ascendant"] }
gates:
  to_Apex:
    requires:
      all:
        - milestones_cleared_gte: 2
        - days_in_state_gte: 6
```

## SagaArcDef / SagaMilestoneDef
```yaml
id: core.SagaArc.Spider.WebsOfBone
schema: SagaArcDef@1
title_key: loc.saga.spider.webs_of_bone
description_key: loc.saga.spider.webs_of_bone.desc
branch_key: "undead"
milestones:
  - core.SagaMilestone.Spider.RitualSite
  - core.SagaMilestone.Spider.SanctifyWebs
```

```yaml
id: core.SagaMilestone.Spider.RitualSite
schema: SagaMilestoneDef@1
unlocks:
  quests_any: [ core.Quest.SanctifyWebs, core.Quest.ScoutBoneSpinners ]
regress_on_fail: false
```

## BossEncounterDef
```yaml
id: core.Boss.Spider.Queen
schema: BossEncounterDef@1
lair_poi: core.POI.SpiderQueenLair
phases:
  - { id: "eggshell", trigger: { hp_lte_pct: 0.70 }, adds: core.Enc.Spiderlings, shield_tag: "eggs" }
  - { id: "tethers", trigger: { hp_lte_pct: 0.40 }, arena_mods_any: [ core.Prop.WebTethers ] }
  - { id: "enrage", trigger: { timer_s: 120 }, buff: core.Buff.Spider.Enrage }
prep_breakers:
  - { if_quest_success: core.Quest.TrackHunter, remove_phase: "tethers" }
  - { if_quest_success: core.Quest.BreakRite, hp_bonus_pct: -10 }
loot_table: core.Loot.SpiderQueen
```

## WorldEffectDef
```yaml
id: core.WorldEffect.Spider.SilkBoom
schema: WorldEffectDef@1
scope: REGION
ttl_days: 7
effects:
  - { spawn_nodes: { tag: "mat.silk", count: 3 } }
  - { trade_price_mult: { tag: "mat.silk", mult: 0.8 } }
  - { route_safety_delta: +0.25 }
```

```yaml
id: core.WorldEffect.Spider.WebDominion
schema: WorldEffectDef@1
scope: REGION
ttl_days: null
effects:
  - { trade_price_mult: { all_goods: true, mult: 1.1 } }
  - { route_safety_delta: -0.25 }
```

## Validation
- `MajorWorldTagDef.states` must include **Dormant** and **Apex**.
- `SagaClockDef.gates.to_Apex` must reference at least one milestone or time gate.
- `BossEncounterDef.phases` must be ordered and unique ids.
- `WorldEffectDef` must specify `scope` and at least one effect.
- Branch cycles allowed only if flagged `loop: true`.
