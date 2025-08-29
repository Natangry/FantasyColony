# Schemas Addendum — Combat Stats & Formulas (v0.1)
*Additive to canvas **Modding Data Schemas v0.1 (Draft)**. No overwrites.*

## StatDef@1
```yaml
id: core.Stat.STR
schema: StatDef@1
kind: PRIMARY
clamp: [0, 999]
ui: { name_key: loc.stat.str.name, icon: icons/stats/str.png }
```

## DerivedStatFormulaDef@1
```yaml
id: core.Formula.MaxHP
schema: DerivedStatFormulaDef@1
formula: "80 + 12*VIT + curve(LevelCurveHP, level)"
clamp: [1, 9999]
ui: { name_key: loc.stat.hp.name }
```

## StatCurveDef@1
```yaml
id: core.LevelCurveHP
schema: StatCurveDef@1
points:
  - { x: 1, y: 0 }
  - { x: 10, y: 60 }
  - { x: 20, y: 140 }
  - { x: 30, y: 240 }
mode: "linear"
```

## DamageTypeDef@1
```yaml
id: dmg.Slash
schema: DamageTypeDef@1
class: "physical"
armor_curve: core.Mitigation.Physical
```

## ElementDef@1
```yaml
id: elem.Ember
schema: ElementDef@1
resist_curve: core.Mitigation.Elemental
status_bias: { burn: +0.1 }
```

## MitigationCurveDef@1
```yaml
id: core.Mitigation.Physical
schema: MitigationCurveDef@1
formula: "x / (x + (80 + 8*attacker_level))"
cap: 0.70
```

```yaml
id: core.Mitigation.Elemental
schema: MitigationCurveDef@1
formula: "x / (x + 100)"
cap: 0.75
```

## HitTableDef@1
```yaml
id: core.HitTable.Default
schema: HitTableDef@1
miss_dodge: "clamp(Evasion - Accuracy, 0, 0.30)"
glance: "0.10 + clamp(angle_mismatch + range_mismatch, 0, 0.15)"
crit: "clamp(CritChance, 0, 0.50)"
order: ["miss_dodge","glance","crit","normal"]
```

## CritRuleDef@1
```yaml
id: core.Crit.Default
schema: CritRuleDef@1
power: "1.50 + 0.002*LUCK"
soft_cap: 2.0
dr_after: 0.35
```

## GlanceRuleDef@1
```yaml
id: core.Glance.Default
schema: GlanceRuleDef@1
power: "-0.30"
```

## GuardRuleDef@1
```yaml
id: core.Guard.Default
schema: GuardRuleDef@1
cap: 0.50
```

## BlockRuleDef@1
```yaml
id: core.Block.Default
schema: BlockRuleDef@1
chance_cap: 0.50
power_cap_pct: 0.50
flat_first: true
```

## InterruptRuleDef@1
```yaml
id: core.Interrupt.Default
schema: InterruptRuleDef@1
rule: "interrupt_if attacker.InterruptPower >= target.Poise"
stagger_ms: 800
```

## RegenDef@1
```yaml
id: core.Regen.Default
schema: RegenDef@1
hp_per_5s: 2
stamina_per_5s: 5
mana_per_5s: 3
```

## TTKTargetDef@1
```yaml
id: core.TTK.EarlyBoss
schema: TTKTargetDef@1
gcds: [45, 60]
comment: "Used by validation sims to keep encounters in band."
```

## Formula DSL (summary)
- Operators: `+ - * /`, `min/max`, `clamp(a,b,c)`, `curve(CurveId, x)`, `if(cond, a, b)`.
- Inputs: any primary/derived stat, `level`, `attacker_*`, `target_*`, positional hints like `angle_mismatch`.
- All formulas must clamp or be clamped by the consumer; circular references are **Error**.

