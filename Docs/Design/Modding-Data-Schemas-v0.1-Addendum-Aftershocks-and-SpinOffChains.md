# Schemas Addendum — Aftershocks: Spin-Off Chains & Mutator Propagation (v0.1)
*Extends the canvas **Modding Data Schemas v0.1 (Draft)**. Additive only.*

## SpinOffChainDef@1
```yaml
id: core.Chain.HuntersCovenant
schema: SpinOffChainDef@1
seed_any:
  - betrayal_trigger: core.Betrayal.GoodVsEvil
  - quest_path: { quest: core.Quest.TrackHunter, path: "evil" }
gates_any:
  - party_has_trait: "Greedy|HotHeaded"
  - region_has_tag: core.Tag.Banditry
steps_any:
  - core.ChainStep.HC.Signal
  - core.ChainStep.HC.Cell
  - core.ChainStep.HC.Trial
max_concurrent: 1
ttl_days: null
```

## ChainStepDef@1
```yaml
id: core.ChainStep.HC.Signal
schema: ChainStepDef@1
quest: core.Quest.HC.InterceptSignal
edges:
  on_success: core.ChainStep.HC.Cell
  on_fail: core.ChainStep.HC.CellAlt
  on_timeout: core.ChainStep.HC.CellAlt
```

## MutationPropagationDef@1
```yaml
id: core.Propagate.SoulSilk
schema: MutationPropagationDef@1
source_any:
  - quest_path: { quest: core.Quest.SanctifyWebs, path: "evil" }
applies:
  - { scope: SETTLEMENT, mutator: core.SettMut.SoulLooms, hops: 0, count_max: 1, ttl_days: 7 }
  - { scope: ROUTE, tag: core.Tag.NightWebs, hops: 2, density: 0.5, ttl_days: 5 }
caps:
  per_scope_max: { SETTLEMENT: 2, ROUTE: 3 }
resist_any:
  - { scope: REGION, tag: core.Tag.ChapelWard, effect: "halve_density" }
```

## QuestTemplateDef@4 (extensions)
```yaml
id: core.Quest.TrackHunter
schema: QuestTemplateDef@4
paths_any:
  - core.QPath.TrackHunter.Good
  - core.QPath.TrackHunter.Evil
  - core.QPath.TrackHunter.Neutral
betrayal_hooks_any:
  - core.Betrayal.GoodVsEvil
on_betrayal_any:
  - spawn_chain: core.Chain.HuntersCovenant
on_major_mutator_any:
  - propagate: core.Propagate.SoulSilk
```

## BetrayalTriggerDef (extension fields)
```yaml
id: core.Betrayal.GoodVsEvil
schema: BetrayalTriggerDef@1
fires_at: ["choice_gate","boss_phase","loot_objective","hostage_moment"]
spin_off_chain_on_trigger: core.Chain.HuntersCovenant   # optional
```

## WorldTagDef (extension)
```yaml
id: core.Tag.NightWebs
schema: WorldTagDef@1
scope: ROUTE
intensity: { min: 0, max: 1 }
catalyst: true
badges: ["threat","aftershock"]
decay: { per_day: 0.2 }
```

## SettlementMutatorDef (example)
```yaml
id: core.SettMut.SoulLooms
schema: SettlementMutatorDef@1
effects:
  black_market: true
  vendor_add_any: ["Soul Loomer"]
  law_policy: core.Law.CurfewLight
ui_chip_key: loc.mutator.settlement.soullooms
```

## Director Rule Additions
- **Predicates:** `chain_active`, `propagation_density_lte`, `adjacent_scope_has_mutator`, `was_seeded_by`
- **Actions:** `spawn_chain`, `propagate_mutator`, `propagate_tag`, `halt_propagation`, `cap_propagation_scope`

## Validation
- `SpinOffChainDef.steps_any` must be acyclic unless `loop: true`.
- `MutationPropagationDef.caps.per_scope_max` ≥ applied counts; TTLs ≥ 1 day if set.
- QuestTemplate@2/@3 remain valid; @4 fields are optional.
