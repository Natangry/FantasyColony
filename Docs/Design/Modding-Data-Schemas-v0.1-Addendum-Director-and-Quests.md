# Schemas Addendum — Director, World Tags & Quests (v0.1)
*Extends the canvas document **Modding Data Schemas v0.1 (Draft)**. Do **not** overwrite that file.*

## WorldTagDef@2 (extended)
```yaml
id: core.Tag.SpiderDens
schema: WorldTagDef@2
scope: REGION
intensity: { min: 0, max: 3 }
decay: { per_day: 0.1 }
ttl_days: null           # optional; if set, auto-expires
badges: ["threat","beast"]
origin: { source: "rule", id: core.Rule.SpiderBloom }
overlays_any: []         # secondary tag ids that modify encounters
mutations_any: []        # references to MutationDef
created_utc: null
```
> WorldTagDef@1 (from canvas) remains valid. @2 only **adds** optional fields.

## MutationDef
```yaml
id: core.Mutation.Spider.CursedWebs
schema: MutationDef@1
from: core.Tag.BroodNest
to: core.Tag.CursedWebs
requires:
  all:
    - near_tag: { id: core.Tag.UndeadPresence, hops_lte: 2 }
    - intensity_gte: 1
effects:
  - post_rumor: loc.rumor.cursed_webs
  - mod_encounter_weight: { table: core.Enc.UndeadSpiders, delta_pct: +50 }
cooldown_days: 6
```
## OverlayDef
```yaml
id: core.Overlay.TrainedSpiders
schema: OverlayDef@1
applies_to_any: [ core.Tag.SpiderDens, core.Tag.BroodNest ]
requires:
  any:
    - near_actor: { id: core.Actor.EvilHunter, hops_lte: 1 }
effects:
  - combat_mod: { accuracy_pct: +0.05, ambush_weight_pct: +25 }
ttl_days: 7
```

## RuleDef@2 (Director rules)
```yaml
id: core.Rule.SpiderBloom
schema: RuleDef@2
when:
  all:
    - tag_at_least: { tag: core.Tag.SpiderDens, intensity: 2 }
    - season_any: ["Spring","Summer"]
    - quest_cap_free: true
then:
  - spawn_quest: { id: core.Quest.CullBloom, scope: REGION, weight: 60 }
  - mutate_tag: { via: core.Mutation.Spider.BroodNest }            # or inline { from: to: }
  - add_tag: { tag: core.Tag.WebbedRoads, scope: ROUTE, ttl_days: 5 }
  - post_rumor: { key: loc.rumor.webbed_roads }
limits:
  cooldown_days: 6
  max_simultaneous: 1
```
**New predicates:** `near_tag`, `near_actor`, `time_since`, `intensity_trend`, `visited_state`, `quest_cap_free`, `random_chance`.  
**New actions:** `mutate_tag`, `add_overlay`, `remove_tag`, `move_actor`, `post_dialogue`, `attach_conversation_to_poi`, `start_timer`, `link_quest`.

## QuestTemplateDef@2
```yaml
id: core.Quest.CullBloom
schema: QuestTemplateDef@2
scope: REGION
categories: ["threat","beast"]
eligibility:
  requires_any: [ core.Tag.BroodNest ]
site_selector:
  prefer_near_tag: { id: core.Tag.BroodNest, hops_lte: 0 }
objectives:
  - { kill: { table: core.Enc.Spiderlings, count: "scale:intensity*X" } }
  - { optional_harvest: { tag: "mat.silk", count: "scale:1+intensity" } }
scaling:
  power: { by_tag_intensity: core.Tag.BroodNest, curve: "linear:1.0..1.6" }
countdown_days_if_visited: [4,6]
paused_until_visit: true
rewards:
  items: [{ tag: "mat.silk", count: "scale:3+intensity*2" }]
  rep: { faction: core.Faction.Town, delta: 5 }
aftermath:
  - remove_tag: { tag: core.Tag.BroodNest, delta_intensity: 1 }
  - add_tag: { tag: core.Tag.ForagerBoom, scope: REGION, ttl_days: 3 }
dialogue_sets:
  hook: loc.q.cull_bloom.hook
  reminder: loc.q.common.reminder_48h
  success: loc.q.cull_bloom.success
  fail: loc.q.common.fail
cast:
  quest_giver: { archetype: core.NPCArchetype.Innkeep, persona: core.Persona.Cheerful }
```

## ActorDef@2 (influence)
```yaml
id: core.Actor.EvilHunter
schema: ActorDef@2
behaviors:
  - patrol: { regions_any: ["forest"], hops: 1..2 }
  - set_overlay_nearby: { overlay: core.Overlay.TrainedSpiders, hops_lte: 1 }
  - ambushes: { table: core.Enc.HunterAmbush, weight_pct: +30 }
influence_aura:
  tags_any: ["beast"]
  effects: [{ accuracy_pct: +0.05 }]
conversation_entrypoints:
  - attach_conversation_to_poi: core.Conv.Hunter.Parley
```

## Validation
- `WorldTagDef@1` remains valid; `@2` fields are optional.
- `MutationDef.from` must match an existing tag; `to` must resolve.
- `RuleDef@2.when` must have at least one predicate; limits are optional but recommended.
- `QuestTemplateDef@2` must define at least one **objective** and **reward/aftermath**.
- Cyclic quest chains require `loop: true`.

