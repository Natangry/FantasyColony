# Schemas Addendum — Stats, Damage & Status (v0.1)
*Extends the current **Modding Data Schemas v0.1** (do not overwrite the main file).*

## StatDef
```yaml
id: core.Stat.Might
schema: StatDef@1
group: "primary"
min: 0
max: 99
```

## DerivedStatFormulaDef
```yaml
id: core.Derived.HP
schema: DerivedStatFormulaDef@1
formula: "100 + Might*6"
caps: { min: 1, max: 999 }
```

## DamageTypeDef
```yaml
id: core.Damage.Fire
schema: DamageTypeDef@1
family: "elemental"
resist: core.Resist.Fire
```

## ResistTypeDef
```yaml
id: core.Resist.Fire
schema: ResistTypeDef@1
cap_pct: 0.75
```

## ArmorDef (physical soak)
```yaml
id: core.Armor.Light
schema: ArmorDef@1
soak_flat: 2
tags: ["light"]
```

## HitFormulaDef
```yaml
id: core.HitFormula.Default
schema: HitFormulaDef@1
modes: ["hit","miss","graze"]
exprs:
  accuracy_score: "Focus + tags.accuracy_bonus - cover.penalty + height.bonus + flank.bonus"
  evade_score: "Evasion"
  graze_window: 0.1
```

## CritRuleDef
```yaml
id: core.CritRule.Default
schema: CritRuleDef@1
chance_base: 0.05
power_base: 1.5
```

## CoverDef / AdvantageDef
```yaml
id: core.Cover.Light
schema: CoverDef@1
accuracy_penalty: 10
guard_bonus_vs_projectiles: 1
```

```yaml
id: core.Advantage.Flank
schema: AdvantageDef@1
accuracy_bonus: 5
block_window_penalty: 10
```

## StatusDef (extended)
```yaml
id: core.Status.Burn
schema: StatusDef@2
family: "fire_dot"
duration_s: [6,10]
tick_ms: 1000
stacks_max: 3
on_tick: [{ effect: "damage", amount: 3, type: core.Damage.Fire }]
primers: [{ requires: core.Status.Oily, bonus: { damage_pct: +0.25 } }]
resist_interaction: { type: core.Resist.Fire }
dispellable: true
```

## StanceDef (finalized)
```yaml
id: core.Stance.Offense
schema: StanceDef@1
multipliers: { damage_pct: +0.15, guard_pct: -0.10, block_window_pct: -0.05 }
ai_bias: { aggressiveness: +2 }
```

## FormationDef (defaults)
```yaml
id: core.Formation.Line
schema: FormationDef@1
shape: "Line"
spacing_tiles: [1,1,1,1]
bonuses: { rear_focus_pct: +0.05 }
```

## Validation
- All formula references must resolve (`StatDef`, `ResistTypeDef`, etc.).
- Resist caps must be `0–1`.
- Status with `primers` must reference valid statuses.
- Stance and Formation bonuses must be bounded (|bonus| ≤ 0.5) unless explicitly whitelisted.
