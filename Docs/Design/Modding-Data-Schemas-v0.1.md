# Modding Data Schemas v0.1 (Draft)
*Design-first, data-only. No engine code implied. Defines the shapes modders will author.*

## Conventions
- **IDs:** `modid.DefType.Name` (e.g., `core.Ability.CinderSlash`).
- **Tags:** namespaces like `elem.Ember`, `role.Healer`, `biome.Forest`.
- **Localization Keys:** Every display string uses a key (e.g., `loc.ability.cinder_slash.name`).
- **Versioning:** `schema_version` per file; defs include `version` and optional `migrations`.
- **Dependencies:** `requires: ["modA>=1.2", "modB"]`.
- **Weights:** Use `weight` (0–100) normalized in tables.

---

## Core Gameplay Schemas

### AbilityDef
```yaml
id: core.Ability.CinderSlash
schema: AbilityDef@1
name_key: loc.ability.cinder_slash.name
icon: icons/abilities/cinder_slash.png
school: elem.Ember
costs: { stamina: 12, mana: 0 }
cast: { gcd_ms: 600, cast_ms: 250, channel_ms: 0, off_gcd: false }
range: { tiles: 1, shape: "arc", angle_deg: 90 }
damage: { type: dmg.Slash, amount: 28, variance: 0.15, element: elem.Ember }
status_apply: [{ id: core.Status.Burn, chance: 0.3, stacks: 1, duration_s: 6 }]
flags: ["melee","interruptible","front_arc"]
requirements: { weapon_tags_any: ["sword"], stance_any: ["Offense","Balanced"] }
ui_tags: ["GCD","Melee","Fire"]
```

### StatusDef
- `tick_ms`, `stacks_max`, `on_apply`, `on_tick`, `on_expire` (effect refs), `dispellable`.

### StanceDef
- `id`, `name_key`, `multipliers` (e.g., `{ damage:+0.15, guard:-0.10, block_window:-0.05 }`), `ai_bias`.

### FormationDef
- `shape: Line|Pike|Wedge`, `spacing`, `bonuses` (e.g., `front_guard:+0.1`, `rear_crit:+0.1`).

### GizmoGroupDef & GizmoBarLayoutDef
- Base groups (`Move`,`Attack`,`Ability`,`Magic`,`Item`,`AI`) & sub-bar population rules (tag queries, sort keys).

### GambitDef / ConditionDef / ActionDef
```yaml
id: core.Gambit.Priest.Default
rules:
  - if: { ally_hp_lt: 0.35, has_ability: core.Ability.Heal }
    then: { use_ability: core.Ability.Heal, target: "ally_lowest_hp" }
    prio: 100
  - if: { enemy_casting: "heavy", in_range: core.Ability.Silence }
    then: { use_ability: core.Ability.Silence, target: "enemy_caster" }
    prio: 80
  - if: { focus_fire: true, in_range: "weapon" }
    then: { attack: true, target: "focus" }
    prio: 50
  - if: { always: true }
    then: { attack: true, target: "nearest" }
    prio: 10
```

### EncounterDef / WaveDef / PropDef
- Arena template & spawn tables by tags (e.g., `biome.Forest`, `road.Curve`); props (cover/hazard) with telegraph sprites.

### ItemDef / RecipeDef / BuildingDef / RoomDef
- Items with `tags`, `stats`, `equip_slots`.
- Recipes: `inputs`, `outputs`, `work_ms`, `station_tags`.
- Buildings: `size`, `costs`, `power`, `room_roles`, `beauty`, `effects`.

---

## Overworld & World Systems

### WorldTagDef
```yaml
id: core.Tag.SpiderDens
scope: REGION
intensity: { min: 0, max: 3 }
decay: { per_day: 0.1 }   # default regional decay; override per tag
badges: ["threat","beast"]
```

### RuleDef (Director)
```yaml
id: core.Rule.SpiderBloom
when:
  all:
    - tag_at_least: { tag: core.Tag.SpiderDens, intensity: 2 }
    - season_any: ["Spring","Summer"]
then:
  - spawn_quest: { id: core.Quest.CullBloom, scope: REGION, weight: 60 }
  - add_tag: { tag: core.Tag.WebbedRoads, scope: ROUTE, ttl_days: 5 }
  - mod_encounter_weight: { table: core.Enc.Spiders, delta: "+25%" }
limits:
  cooldown_days: 6
  max_simultaneous: 1
```

### QuestTemplateDef
- `scope`, `objectives` (kill/escort/deliver/defend/explore),  
  `countdown_days_if_visited` (range or table by difficulty),  
  `paused_until_visit: true|false`,  
  `on_success`, `on_fail` (effects/Tag mutations),  
  optional `ui_priority` & `news_template`.

### ActorDef (World Actors)
- Example: **Unknown Hero** with `notoriety`, `behavior_rules`, `recruitable_when: { notoriety_gte: 3, witnessed_deeds_gte: 1 }`.

### ExpeditionRoleDef / TravelActionDef / CampActionDef
- Roles grant travel perks; actions (Cook/Mend/Scout/Storytime) adjust risk/resources at camp.

### NEW: CountdownRuleDef
```yaml
id: core.Countdown.Defaults
visited_region_days:
  Story: [6,7]
  Classic: [4,6]
  Tempest: [3,5]
start_on_first_visit: true   # unvisited regions keep quests Paused
urgency_threshold_hours: 48  # UI highlight when under this
```

### NEW: RegionCapRuleDef
```yaml
id: core.RegionCap.Defaults
cap_active: 3
cap_paused: 1
replacement: "oldest_paused"  # when a new quest would exceed caps
```

### NEW: NewsDef
```yaml
id: core.News.Defaults
per_region_per_day_limit: 1
styles:
  normal: loc.news.normal
  urgent: loc.news.urgent   # used when countdown < 48h
```

### Lifecycle: Visited vs Unvisited (Quest Activation)
- **Visited** regions: new quests **start countdowns** immediately.
- **Unvisited** regions: new quests remain **Paused** until the first **entry**, then start countdowns.
- Caps are enforced by `RegionCapRuleDef` (3 Active / 1 Paused by default).

---

## UI & Input
### InputMapDef
- Binds: `Move/Attack/Ability/Magic/Item/AI`, sub-bar open/close, `AssumeControl`, camera, pause/time scales.

### BehaviorPresetDef + BehaviorParamDef
- Presets (Aggressive/Defensive/Default) + sliders (Engage Range, Retreat HP %, Interrupt Priority, etc.).

---

## Validation Rules
- **References** must resolve; circular quest chains forbidden unless `loop:true`.
- **Weights** normalize to 100; caps enforced (≤ 3 Active quests / region by default).
- **Schema migration** guidelines for renaming IDs/tags.

---

## Example: Ability + Gambit + Rule (End-to-End)
- Adding **Cinder Slash** (AbilityDef) auto-populates **Ability** sub-bar via tags.
- **Priest Default Gambit**: heal > interrupt heavy cast > focus fire > nearest attack.
- **Spider Bloom Rule**: reacts to tags to seed a countdown quest and increase spider encounters.

---

## Open Questions for v0.1
1) Finalize **Stance** multipliers.
2) Lock **Formation** spacing/bonuses.
3) Confirm **Gambit** Conditions/Actions shipping set (≥12 each?).
4) Decide **QuestTemplate** minimums for launch.
5) Approve **validation** severity per rule.

---

## Colony Gameplay Loop & Pacing (excerpt)
(For full loop, see `Docs/Design/Colony-Loop-and-Pacing.md`.)

