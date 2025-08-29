# Schemas Addendum — Social, Personality & Relationships (v0.1)
*Extends the canvas **Modding Data Schemas v0.1 (Draft)** you most recently edited. Additive only; no breaking changes.*

## PawnTraitDef@1
```yaml
id: core.Trait.Brave
schema: PawnTraitDef@1
ui_key: loc.trait.brave
effects:
  rtwp_retreat_threshold_delta: -0.05
  morale_loss_from_fear_mult: 0.9
tags: ["bravery","combat"]
```

## PersonalityFacetDef@1
```yaml
id: core.Facet.Boldness
schema: PersonalityFacetDef@1
range: [0,100]
ai_hooks:
  retreat_threshold_curve: "inverse:0.2..0.6"
  initiative_bias: "linear:0..+0.1"
```

## ValueAxisDef@1 & OpinionTopicDef@1
```yaml
id: core.Value.Tradition
schema: ValueAxisDef@1
default: 50
```
```yaml
id: core.Opinion.BlackMarket
schema: OpinionTopicDef@1
axis: core.Value.Tradition
stance_keys:
  approve: loc.opinion.blackmarket.approve
  disapprove: loc.opinion.blackmarket.disapprove
```

## RelationshipTypeDef@1
```yaml
id: core.RelType.Friendship
schema: RelationshipTypeDef@1
thresholds: { acquaintance: 10, close: 40, bonded: 70 }
perks:
  - { at_affinity_gte: 40, perk: core.Perks.CoverMe }
```

## RelationshipEventDef@1
```yaml
id: core.RelEvent.DiceNight
schema: RelationshipEventDef@1
venue_tags_any: ["venue.tavern"]
cost_pips: 1
effects:
  affinity_delta: { each_pair: "roll:2..4" }
  moodlets_any: ["loc.mood.music_night"]
```

## GrievanceDef@1 & MediationActionDef@1
```yaml
id: core.Grievance.BrokenPromise
schema: GrievanceDef@1
weight_init: 6
ttl_days: 10
triggers_any: ["quest_failed_with_pledge"]
```
```yaml
id: core.Mediation.ApologyRestitution
schema: MediationActionDef@1
costs: { time_min: 30 }
gates: { mediator_job_any: ["Steward","Acolyte"] }
effects:
  clear_if_weight_lte: 3
  weight_mult: 0.5
```

## SocialActivityDef@1 & VenueDef@1
```yaml
id: core.Social.Storytime
schema: SocialActivityDef@1
pips: 1
venue_tags_any: ["venue.tavern","venue.plaza","venue.camp"]
effects:
  morale_delta: +1
  affinity_delta: { each_pair: 2 }
```
```yaml
id: core.Venue.Tavern
schema: VenueDef@1
room_tag: "Tavern"
auras: [{ morale: +1 }]
```

## DuoTechDef@1
```yaml
id: core.DuoTech.BraceAndBind
schema: DuoTechDef@1
requires:
  all:
    - relationship: { type: core.RelType.Friendship, affinity_gte: 40 }
    - jobs_any: ["Guardian","Healer"]
ability: core.Ability.Duo.BraceAndBind
cooldown_s: 90
```

## SocialPolicyDef@1
```yaml
id: core.Policy.FestivalCadence
schema: SocialPolicyDef@1
cadence_days: [7,10]
effects:
  colony_morale_burst: +2
```

## Dialogue tokens (extension)
- New safe tokens: `{pawn.trait.Brave}`, `{pawn.facet.Boldness}`, `{rel.affinity}`, `{rel.type}`, `{value.Tradition}`, `{duotech.name}`.

## Validation
- Multiple perks from relationships hard-cap to +3% aggregate.
- DuoTech must reference an existing AbilityDef; cooldown ≥ 30s.
- SocialActivity venues must resolve to a valid `VenueDef` or room tag.
