# Schemas Addendum — RTwP Controls & AI (v0.1)
*Extends the canvas **Modding Data Schemas v0.1 (Draft)**. Additive only — do not overwrite the canvas file.*

## StanceDef@1
```yaml
id: core.Stance.Offense
schema: StanceDef@1
multipliers: { damage: +0.15, guard: -0.10, block_window_s: -0.05 }
ai_bias: { engage_weight: +10, chase_weight: +10 }
swap_gcd_ms: 300
```
```yaml
id: core.Stance.Defense
schema: StanceDef@1
multipliers: { damage: -0.10, guard: +0.15, block_window_s: +0.05 }
ai_bias: { hold_weight: +10, cover_weight: +10 }
swap_gcd_ms: 300
```

## FormationDef@1
```yaml
id: core.Formation.Line
schema: FormationDef@1
shape: "Line"
spacing_tiles: 1.5
anchor: "controlled|rally"
radius_stay: 6
radius_break: 8
bonuses: [{ front_guard: +0.05 }]
ai_bias: { keep_lanes: true }
```
```yaml
id: core.Formation.Loose
schema: FormationDef@1
shape: "Loose"
spacing_tiles: 2.25
bonuses: [{ aoe_damage_taken_mult: 0.9 }]
```

## GizmoGroupDef@1 & GizmoBarLayoutDef@1
```yaml
id: core.GizmoGroup.Base
schema: GizmoGroupDef@1
groups: ["Move","Attack","Ability","Magic","Item","AI"]
subbar_rules:
  Ability: { query: { tag_any: ["ability.*"] }, max_slots: 10 }
  Magic:   { query: { tag_any: ["magic.*"] },   max_slots: 10 }
  Item:    { query: { tag_any: ["item.use"] },  max_slots: 10 }
  Move:    { embeds: ["Rally","Retreat","StanceSwitch","FormationSwitch"] }
```
```yaml
id: core.GizmoLayout.Default
schema: GizmoBarLayoutDef@1
group: core.GizmoGroup.Base
open_one_subbar_only: true
```

## InputMapDef@1
```yaml
id: core.Input.Default
schema: InputMapDef@1
binds:
  wasd_move: "WASD"
  assume_control: "Ctrl"
  pause_toggle: "Space"
  time_down: ","
  time_up: "."
  time_reset: "/"
  next_controlled: "Tab"
  prev_controlled: "Shift+Tab"
  stance_cycle_prev: "Q"
  stance_cycle_next: "E"
  open_move: "Z"
  open_attack: "X"
  open_ability: "C"
  open_magic: "V"
  open_item: "B"
  open_ai: "N"
  focus_fire_toggle: "F"
  retreat_to_rally: "R"
  subbar_1: "1"
  subbar_2: "2"
  subbar_3: "3"
  subbar_4: "4"
  subbar_5: "5"
  subbar_6: "6"
  subbar_7: "7"
  subbar_8: "8"
  subbar_9: "9"
  subbar_10: "0"
```

## BehaviorPresetDef@1 & BehaviorParamDef@1
```yaml
id: core.BehaviorPreset.Default
schema: BehaviorPresetDef@1
params: { engage_range: "medium", interrupt_priority: 0, retreat_hp_pct: 0.25, ff_compliance: 0.6, formation_strictness: 0.5, chase_tiles: 4, cover_pref: "medium" }
```
```yaml
id: core.BehaviorPreset.Aggressive
schema: BehaviorPresetDef@1
inherits: core.BehaviorPreset.Default
params: { engage_range: "medium+", retreat_hp_pct: 0.20, ff_compliance: 0.75, chase_tiles: 10 }
```

## GambitConditionDef@1 (catalog excerpt)
```yaml
id: core.Cond.AllyHpLt
schema: GambitConditionDef@1
args: { pct: float }
```
```yaml
id: core.Cond.EnemyCastingHeavy
schema: GambitConditionDef@1
args: {}
```
```yaml
id: core.Cond.FormationBreached
schema: GambitConditionDef@1
args: {}
```

## GambitActionDef@1 (catalog excerpt)
```yaml
id: core.Act.UseAbility
schema: GambitActionDef@1
args: { ability: AbilityId, target: "self|ally|enemy|focus|nearest" }
```
```yaml
id: core.Act.RepositionTo
schema: GambitActionDef@1
args: { where: "cover|anchor|rally" }
```
```yaml
id: core.Act.SetFormation
schema: GambitActionDef@1
args: { formation: FormationId }
```

## GambitDef@1 (example)
```yaml
id: core.Gambit.General.Default
schema: GambitDef@1
retarget_cooldown_ms: 900
rules:
  - if: { cond: core.Cond.AllyHpLt, pct: 0.35, has_ability: core.Ability.Heal }
    then: { act: core.Act.UseAbility, ability: core.Ability.Heal, target: "ally_lowest_hp" }
    prio: 100
  - if: { cond: core.Cond.EnemyCastingHeavy }
    then: { act: core.Act.UseAbility, ability: core.Ability.Interrupt, target: "enemy_caster" }
    prio: 80
  - if: { focus_fire: true, in_range: "weapon" }
    then: { act: "basic_attack", target: "focus" }
    prio: 50
  - if: { always: true }
    then: { act: "basic_attack", target: "nearest" }
    prio: 10
```

## QuestTemplateDef@4 (requireds & optionals)
```yaml
id: core.Quest.Template.Baseline
schema: QuestTemplateDef@4
scope: "REGION|SETTLEMENT|ROUTE"
objectives: []
countdown_days_if_visited: 3
rewards:
  on_success: { items_any: [], standing_deltas_any: [] }
  on_fail:    { items_any: [], standing_deltas_any: [] }
paths_any: []   # or director_rule_seeds: []
# Optional:
# betrayal_hooks_any, interjections_any, variant_mutators_any, on_betrayal_any, on_major_mutator_any
```

## Validators (severity)
- Missing QuestTemplate requireds → **error**.  
- `FormationDef.spacing_tiles < 0.75` → **error**.  
- `StanceDef.multipliers` any magnitude > 0.25 → **warning**.  
- `GambitDef` with no fallback rule → **error**.  
- `swap_gcd_ms < 300` or `retarget_cooldown_ms < 600` → **warning**.
