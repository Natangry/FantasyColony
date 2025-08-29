# Schemas Addendum — Ethos, Class Aspects & Quest Paths (v0.1)
*Extends the canvas **Modding Data Schemas v0.1 (Draft)** (do not overwrite).* 

## EthosAxisDef@1
```yaml
id: core.Ethos.Main
schema: EthosAxisDef@1
bands:
  wicked_max: -60
  dark_max: -20
  neutral_max: 19
  good_min: 20
  virtuous_min: 60
decay:
  conscience_drift_per_day: 0   # storyteller can override
```
## EthosDeltaRuleDef@1
```yaml
id: core.EthosDelta.CaptureVsExecute
schema: EthosDeltaRuleDef@1
triggers:
  - quest_path_chosen: { quest: core.Quest.TrackHunter, path: "good" }
  - quest_path_chosen: { quest: core.Quest.TrackHunter, path: "evil" }
effects:
  - { on: "good", delta: +8 }
  - { on: "evil", delta: -8 }
```

## ClassAspectDef@1
```yaml
id: core.Aspect.Guardian.WardKnight
schema: ClassAspectDef@1
job: "Guardian"
requires: { ethos_band_min: "good_min" }  # ≥ +60
passives_any: [ core.Passive.WardKnight.Aura ]
morphs_any:   [ core.Morph.Ability.Guardian.AegisSmite ]
vfx_swap: "vfx/guardian_ward_knight"
```
```yaml
id: core.Aspect.Guardian.DreadWarden
schema: ClassAspectDef@1
job: "Guardian"
requires: { ethos_band_max: "wicked_max" } # ≤ −60
passives_any: [ core.Passive.DreadWarden.FearPulse ]
morphs_any:   [ core.Morph.Ability.Guardian.SoulShackles ]
vfx_swap: "vfx/guardian_dread_warden"
```

## AbilityMorphDef@1
```yaml
id: core.Morph.Ability.Guardian.AegisSmite
schema: AbilityMorphDef@1
base: core.Ability.ShieldBash
changes:
  damage: { element: elem.Sanctity }
  on_hit_any: [{ add_status: core.Status.Sanctified, chance: 0.3, duration_s: 4 }]
  vs_tags: [{ tag: "undead|fiend", damage_mult: 1.15 }]
```

## PawnMutatorDef@1  # (for non-class tweaks tied to ethos, optional)
```yaml
id: core.PawnMutator.Ethos.WickedAura
schema: PawnMutatorDef@1
requires: { ethos_band_max: "wicked_max" }
effects:
  social: { intimidate_weight: +0.1 }
  ai: { aggression_bias: +0.05 }
ttl_days: null
```

## QuestPathDef@1
```yaml
id: core.QPath.TrackHunter.Good
schema: QuestPathDef@1
quest: core.Quest.TrackHunter
ethos: "good"
objectives:
  - { subdue: { actor: core.Actor.EvilHunter, non_lethal: true } }
  - { deliver: { to_faction: core.Faction.Town } }
rewards:
  bundle: core.Reward.TrackHunter.Good
aftermath:
  - { apply_world_effect: core.WorldEffect.Roads.SaferEast, ttl_days: 3 }
```
```yaml
id: core.QPath.TrackHunter.Evil
schema: QuestPathDef@1
quest: core.Quest.TrackHunter
ethos: "evil"
objectives:
  - { kill: { actor: core.Actor.EvilHunter } }
  - { learn: { overlay: core.Overlay.TrainedSpiders } }
rewards:
  bundle: core.Reward.TrackHunter.Evil
aftermath:
  - { add_tag: { tag: core.Tag.FearTax, scope: REGION, ttl_days: 5 } }
```
```yaml
id: core.QPath.TrackHunter.Neutral
schema: QuestPathDef@1
quest: core.Quest.TrackHunter
ethos: "neutral"
objectives:
  - { expose: { actor: core.Actor.EvilHunter, at_poi: core.POI.TownSquare } }
rewards:
  bundle: core.Reward.TrackHunter.Neutral
aftermath:
  - { link_quest: core.Quest.HunterChase }
```

## RewardBundleDef@2  # extension with ethos gates & rep multipliers
```yaml
id: core.Reward.TrackHunter.Good
schema: RewardBundleDef@2
items_any: [{ tag: "mat.silk", count: 8 }]
standing_deltas_any:
  - { faction: core.Faction.Town, delta: +8, multiplier_tag_any: ["mutator.BenevolentLeader:+1.25"] }
ethos_delta: +8
```

## QuestTemplateDef@2 (extension fields)
```yaml
id: core.Quest.TrackHunter
schema: QuestTemplateDef@2
paths_any:
  - core.QPath.TrackHunter.Good
  - core.QPath.TrackHunter.Evil
  - core.QPath.TrackHunter.Neutral
ui:
  show_ethos_tabs: true
```

## Director Rule Extensions
- **Predicates:** `party_ethos_avg_gte`, `party_ethos_avg_lte`
- **Actions:** `apply_ethos_delta`, `bias_quest_paths` (weights Good/Evil/Neutral), `grant_class_aspect`, `revoke_class_aspect`

## Validation
- ClassAspect must reference an existing job; ethos gates must be consistent (min ≤ max).
- Quest must list ≥1 `QuestPathDef`; each path must define objectives and a reward bundle.
- RewardBundle@2: ethos deltas in [−50, +50]; standing deltas respect faction id.
