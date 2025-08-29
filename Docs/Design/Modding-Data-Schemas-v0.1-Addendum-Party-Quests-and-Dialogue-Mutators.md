# Schemas Addendum — Party Loyalty, Betrayals, Interjections & Quest/Dialog Mutators (v0.1)
*Additive extension to the canvas **Modding Data Schemas v0.1 (Draft)**. Do **not** overwrite the canvas file.*

## LoyaltyStatDef@1
```yaml
id: core.Loyalty.Default
schema: LoyaltyStatDef@1
base: 0.5          # 0..1 (0.5 = neutral)
sources:
  relationship_to_leader: { weight: 0.25 }
  job_satisfaction: { weight: 0.15 }
  debt_favor_score: { weight: 0.15 }
  fear_respect: { weight: 0.10 }
  promises_kept: { weight: 0.10 }
clamps: [0.0, 1.0]
debug_key: "ui.debug.loyalty"
```

## TemptationRuleDef@1
```yaml
id: core.Temp.Default
schema: TemptationRuleDef@1
components:
  ethos_mismatch: { good_path_penalty_if_evil: 0.25, evil_path_penalty_if_good: 0.25 }
  trait_bias:
    greedy: 0.10
    hot_headed: 0.08
    zealous: 0.10
  external_offer: { bribe: 0.10, time_pressure: 0.05, public_setting: 0.05 }
  target_ties: { kin: 0.15, sworn_enemy: -0.10 }
clamps: [0.0, 1.5]
```

## BetrayalTriggerDef@1
```yaml
id: core.Betrayal.GoodVsEvil
schema: BetrayalTriggerDef@1
fires_at: ["choice_gate","boss_phase","loot_objective","hostage_moment"]
filters:
  any:
    - path_ethos_is: "good"
    - party_has_ethos_band: "wicked"
formula:
  use: core.Temp.Default
  versus: core.Loyalty.Default
  difficulty_add: 0.10
outcomes:
  - { type: "betrayal_flip", weight: 40, spawn: core.Enc.HunterAmbush }
  - { type: "refuse_interject", weight: 40, dialogue: core.Dlg.Interject.Mercy }
  - { type: "comply", weight: 20 }
cooldown_days: 3
ui_risk: "Medium"
```

## PartyInterjectionDef@1
```yaml
id: core.Interject.ScholarExpose
schema: PartyInterjectionDef@1
when:
  all:
    - speaker_has_job_at_least: { job: "Scholar", level: "Journeyman" }
    - quest: core.Quest.TrackHunter
    - path_any: ["neutral","good"]
lines_any: [ loc.interject.scholar.expose_a, loc.interject.scholar.expose_b ]
adds_option:
  text_key: loc.opt.expose_the_lie
  changes:
    objectives_add_any: [{ expose: { actor: core.Actor.EvilHunter } }]
    timer_delta_days: -1
```

## QuestVariantMutatorDef@1
```yaml
id: core.QMut.ArtisanCounterseal
schema: QuestVariantMutatorDef@1
when:
  all:
    - quest: core.Quest.TrackHunter
    - party_has_job_at_least: { job: "Artisan", level: "Journeyman" }
effects:
  objectives_add_any:
    - { craft: { recipe: core.Recipe.Counterseal, station_tag: "Forge" } }
  path_bias: { neutral: +20 }
```

## DialogueFlavorMutatorDef@1
```yaml
id: core.Dialog.DreadWardenFlavor
schema: DialogueFlavorMutatorDef@1
applies_if:
  all:
    - pawn_has_aspect: core.Aspect.Guardian.DreadWarden
swap_phrasebooks:
  base: core.Phrasebook.Guardian
  with: core.Phrasebook.DreadWarden
```

## QuestTemplateDef@3 (extensions)
```yaml
id: core.Quest.TrackHunter
schema: QuestTemplateDef@3
paths_any:
  - core.QPath.TrackHunter.Good
  - core.QPath.TrackHunter.Evil
  - core.QPath.TrackHunter.Neutral
betrayal_hooks_any:
  - core.Betrayal.GoodVsEvil
interjections_any:
  - core.Interject.ScholarExpose
variant_mutators_any:
  - core.QMut.ArtisanCounterseal
ui:
  show_ethos_tabs: true
  show_risk_chip: true
```

## Validation
- `BetrayalTriggerDef.fires_at` must include at least one valid timing key.
- Interjections must not create cycles; timer deltas must keep remaining time ≥ 0.
- QuestTemplate@2 remains valid; @3 fields are optional and additive.

