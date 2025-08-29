# Schemas Addendum — Progression: Classes, Evolutions & XP (v0.2)
*Additive to the canvas **Modding Data Schemas v0.1 (Draft)**.*

## ClassDef@1
```yaml
id: core.Class.Guardian
schema: ClassDef@1
name_key: loc.class.guardian.name
family: "martial"
starting_abilities_any: ["core.Ability.Taunt","core.Ability.ShieldBash"]
starting_passives_any: ["core.Passive.HoldTheLine"]
weapon_tags_any: ["weapon.1h","shield"]
armor_tags_any: ["armor.medium|armor.heavy"]
stat_bias:
  GuardPct: +0.05
  BlockChance: +0.03
evolve_any:
  - core.Evolve.Guardian.Paladin
  - core.Evolve.Guardian.Blackguard
```

## ClassLevelDef@1
```yaml
id: core.ClassLevels.Guardian
schema: ClassLevelDef@1
class: core.Class.Guardian
xp_curve: core.LevelCurveXP.Class.Default
rewards:
  - { level: 2, passives_any: ["core.Passive.FirmFooting"] }
  - { level: 3, abilities_any: ["core.Ability.Intercept"] }
  - { level: 4, passives_any: ["core.Passive.ShieldWall"] }
  - { level: 5, abilities_any: ["core.Ability.Bulwark"] }
```

## ClassEvolveRuleDef@1
```yaml
id: core.Evolve.Guardian.Paladin
schema: ClassEvolveRuleDef@1
from: core.Class.Guardian
to:   core.Class.Paladin
requires_all:
  - { type: "PawnLevelGte", value: 5 }
  - { type: "Affinity", align: "Good", value: 60 }
  - { type: "SkillGte", skill: core.Skill.School.Holy, value: 15 }
  - { type: "ReputationGte", faction: core.Faction.Church, value: "Friendly" }
  - { type: "QuestFlag", id: core.QuestFlag.HoldTheGate, value: true }
on_evolve:
  - { action: "ReplaceAbility", from: core.Ability.Taunt, to: core.Ability.SanctifiedTaunt }
  - { action: "GrantPassive", id: core.Passive.AuraOfCourage }
```

## LevelCurveXPDef@1 (extensions)
```yaml
id: core.LevelCurveXP.Class.Default
schema: LevelCurveXPDef@1
points:
  - { x: 1, y: 0 }
  - { x: 2, y: 120 }
  - { x: 3, y: 300 }
  - { x: 4, y: 560 }
  - { x: 5, y: 900 }
mode: "ease"
```

## XPEventDef@1 (updated to include ClassXP)
```yaml
id: core.XP.CombatHit
schema: XPEventDef@1
when: "combat_hit"
splits:
  class: 0.60
  skill: 0.30
  pawn:  0.10
mods_any:
  - { cond: "target_boss", mult: 1.5 }
  - { cond: "target_trivial", mult: 0.25 }
```

## CombatSkillDef@1 (example)
```yaml
id: core.Skill.Shield
schema: CombatSkillDef@1
family: "defense"
affects:
  BlockChance: +0.0015/pt
  BlockPowerPct: +0.0010/pt
dr_policy:
  per_encounter_repeat_mult: 0.75
```

## JobXPProfileDef@1 (clarified)
```yaml
id: core.JobXP.Cook
schema: JobXPProfileDef@1
on_work_complete: 1.0
on_training: 0.6
mentor_bonus: 0.2
grants_combat_abilities: false   # moved to classes
```

## ClassChoicePolicyDef@1
```yaml
id: core.ClassChoice.Default
schema: ClassChoicePolicyDef@1
at_pawn_creation: false
at_first_major_combat: true
respec_allowed: false
```

## RestedXPDef@1 (unchanged, referenced)
```yaml
id: core.RestedXP.Default
schema: RestedXPDef@1
thresholds:
  sleep_hours: 6
  recreation_score: 0.6
bonus_pct: 0.25
pool_max: 200
```

## Validation
- A pawn may hold **0 or 1 Class**. Attempting to assign a second is **Error**.
- `ClassEvolveRule` must not create cycles; `from != to`.
- Jobs must set `grants_combat_abilities: false` unless flagged for legacy; linter suggests migration.
- `ClassLevelDef` rewards must reference valid **Ability/Passive** ids.

