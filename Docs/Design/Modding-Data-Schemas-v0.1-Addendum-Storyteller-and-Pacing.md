# Schemas Addendum — Storyteller & Pacing (v0.1)
*Additive to canvas **Modding Data Schemas v0.1 (Draft)**. No overwrites.*

## StorytellerProfileDef@1
```yaml
id: core.Storyteller.Wayfarer
schema: StorytellerProfileDef@1
cadence: core.Cadence.Classic
tension_model: core.Tension.Wayfarer
budget: { daily_base: 10, max_carry: 20 }
bias:
  weights: { explore: 0.45, social: 0.25, combat: 0.20, twist: 0.10 }
safety:
  min_gap_hard_days: 0.75
  respite_policy: core.Respite.Default
  raid_cooldown_days: 3
arc_policy:
  saga_reserve_pct: 0.25
  sandbox_vs_saga: 0.0   # -1 sandbox, +1 saga
```
## CadenceCurveDef@1
```yaml
id: core.Cadence.Classic
schema: CadenceCurveDef@1
gaps_days:
  minor: { min: 0.3, max: 0.8 }
  medium:{ min: 0.8, max: 1.5 }
  major: { min: 1.5, max: 3.0 }
respite_days:
  medium: { min: 0.5, max: 1.0 }
  major:  { min: 1.5, max: 2.5 }
```
## TensionModelDef@1
```yaml
id: core.Tension.Wayfarer
schema: TensionModelDef@1
inputs:
  - { metric: core.Metric.Region.Danger,         weight: 0.45 }
  - { metric: core.Metric.Region.Unrest,         weight: 0.20 }
  - { metric: core.Metric.Region.Prosperity,     weight: -0.15 } # inverse
  - { metric: core.Metric.Region.AftershockDensity, weight: 0.10 }
  - { metric: core.Metric.Region.PatrolDensity,  weight: -0.10 } # inverse
smoothing: { ema_days: 3 }
hysteresis: { escalate_at: 0.70, deescalate_below: 0.50 }
clamp: [0,1]
```
## IncidentTypeDef@1
```yaml
id: core.Incident.Raid
schema: IncidentTypeDef@1
cost_range: [6,9]
cooldown_days: { min: 2, max: 4 }
tags_any: ["combat","hard"]
director_action: { schedule_raid: "AUTO" }
```
```yaml
id: core.Incident.Festival
schema: IncidentTypeDef@1
cost_range: [3,4]
cooldown_days: { min: 1, max: 2 }
tags_any: ["social","respite"]
director_action: { schedule_event: "market_day" }
```
## EventBudgetDef@1
```yaml
id: core.Budget.Default
schema: EventBudgetDef@1
daily_base: 10
carry_cap: 20
tension_bonus: { at: 0.6, add: 2 }
catastrophe_tax: { days: 2, per_day: 2 }
```
## BeatSelectorDef@1
```yaml
id: core.BeatSelector.Default
schema: BeatSelectorDef@1
weights:
  fit_to_curve: 0.40
  diversity:    0.25
  focus_scope:  0.20
  cooldown_fit: 0.15
diversity_rules:
  avoid_repeat_types: 2
scope_focus:
  prefer_current_region_pct: 0.70
```
## DifficultyDef@1
```yaml
id: core.Difficulty.Classic
schema: DifficultyDef@1
budget_mult: 1.0
enemy_scale: 1.0
raid_cooldown_mult: 1.0
fail_forward_bias: 0
```
## RespitePolicyDef@1
```yaml
id: core.Respite.Default
schema: RespitePolicyDef@1
after_medium_days: { min: 0.5, max: 1.0 }
after_major_days:  { min: 1.5, max: 2.5 }
soft_beats_any: ["core.Incident.Festival","core.Incident.Caravan","core.Incident.SocialVisit"]
```

## Validation
- Hysteresis must satisfy `deescalate_below < escalate_at`.  
- Incident costs ≥ 1 and cooldowns declared.  
- Diversity rule `avoid_repeat_types` ≥ 1.  
- Budget cannot go negative; carry ≤ `carry_cap`.

