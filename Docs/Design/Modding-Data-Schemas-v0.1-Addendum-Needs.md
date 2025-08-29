# Schemas Addendum — Needs & Fallbacks (v0.1)

This addendum extends the main **Modding Data Schemas v0.1**. Do **not** overwrite the main file.

## NeedDef
```yaml
id: core.Need.Food
schema: NeedDef@1
thresholds:
  satisfied: 0.7
  hungry: 0.4
  starving: 0.2
decay_per_hour: 0.08
sources: [ "Meal", "Buff", "Fallback" ]
```

## NeedSatisfactionDef
```yaml
id: core.NeedSat.Gruel
schema: NeedSatisfactionDef@1
need_id: core.Need.Food
method_id: core.Recipe.Gruel
tier: 0
fallback: true
effects:
  debuffs: [ core.Debuff.HeavyBelly ]
  clears_states: [ "Hungry", "Starving" ]
  duration_hours: 4
requirements:
  station_tags_any: []        # fallback requires none
  ingredients_any: [ "starch.any", "water" ]
  job_level_min: null
```

## MealTierDef
```yaml
id: core.Meal.Hearty
schema: MealTierDef@1
tier: 3
inputs:
  - tag: "veg.any"
  - tag: "meat.or.dairy"
  - tag: "grain.flour"
cook_skill_min: "Journeyman"
effects:
  buffs: [ core.Buff.HeartyMeal ]
  variety_tag: "meal.hearty"
```

## BuffDef / DebuffDef (generic)
```yaml
id: core.Debuff.HeavyBelly
schema: DebuffDef@1
stats:
  might_pct: -0.10
  work_speed_pct: -0.05
  resolve_regen_pct: -0.05
duration_hours: 4
stackable: false
```

```yaml
id: core.Buff.HeartyMeal
schema: BuffDef@1
stats:
  work_speed_pct: 0.05
  grace_flat: 1
duration_hours: 8
```

## ComfortSourceDef
```yaml
id: core.Comfort.Bed
schema: ComfortSourceDef@1
room_tag_required: "bedroom"
comfort_bonus: 1
```

## HygieneSourceDef
```yaml
id: core.Hygiene.ColdWash
schema: HygieneSourceDef@1
warmth: "cold"
debuffs: [ core.Debuff.Chill ]
```

## AutoActionDef (for fallbacks)
```yaml
id: core.AutoAction.CookGruel
schema: AutoActionDef@1
when:
  meals_buffer_lt_days: 0.5
then:
  enable_bill: core.Recipe.Gruel
until:
  meals_buffer_gte_days: 2
```

## PolicyDef (Rationing excerpt)
```yaml
id: core.Policy.Rationing
schema: PolicyDef@1
toggles:
  prefer_fallbacks: true
notes: "Prefer fallback recipes/items while critical buffers are low."
```

## Validation
- `NeedSatisfactionDef.fallback: true` requires at least one **Debuff**.
- Fallbacks must **not** require station hard-gates.
- `MealTierDef` must specify a `variety_tag` to interact with **Meal Boredom**.
- **AutoAction** must have paired `when` and `until` clauses.
