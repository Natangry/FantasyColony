# Schemas Addendum — Combat AI (Conditions/Actions) v0.1

This addendum extends the current **Modding Data Schemas v0.1** (do not overwrite the main file).

## ConditionDef (shape)
```yaml
id: mod.Condition.SomeCheck
schema: ConditionDef@1
type: ally_hp_lt | self_hp_lt | enemy_casting | status_present | cooldown_ready | resource_pct | in_range | enemies_in_radius_gte | allies_in_radius_gte | allies_in_radius_lte | threat_on_self_gt | has_focus_target | telegraph_on_me | line_of_fire_clear | target_tag
params: {}   # see Gambit Library table for per-type params
```

## ActionDef (shape)
```yaml
id: mod.Action.SomeAction
schema: ActionDef@1
type: use_ability | attack | guard | interrupt | dodge | kite | use_item | reposition | mark_target | taunt | stance_set | formation_set | focus_fire | revive_ally | dispel | heal | buff | nonlethal_toggle
params: {}
```

## TargetSelector (shape)
```yaml
type: self | focus | enemy_nearest | enemy_caster | enemy_lowest_hp | ally_lowest_hp | ally_downed | cluster_center(enemies|allies)
```

## Validation
- `cooldown_ready.ability`, `use_ability.ability` must reference valid `AbilityId`.
- Percent thresholds are `0.0–1.0`.
- Radius `r > 0`, `count ≥ 0`.
- Avoid friendly fire: if an action has AoE radius, validator warns when no `allies_in_radius_lte` or safety flag present.

## PresetDef (Gambit)
```yaml
id: mod.Gambit.Preset.Name
schema: GambitDef@1
rules: [ { prio:int, if:{...}, then:{...} } ]   # ordered; first match wins
```

> For the full catalog and examples, see `Docs/Design/Gambit-Library-v0.1.md`.

