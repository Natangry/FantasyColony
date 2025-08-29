# Schemas Addendum — Capture, Custody & Bounties (v0.1)
*Extends the canvas **Modding Data Schemas v0.1 (Draft)**. Additive only; do not overwrite.*

## CaptureRuleDef@1
```yaml
id: core.Capture.Default
schema: CaptureRuleDef@1
ko_threshold_hp_pct: 0.25
bosses_require_flag: true
nonlethal_overrides_any: ["core.Ability.NonLethalShot","core.Ability.ShieldBash"]
bleed_accelerates_ko: true
```
## SurrenderThresholdDef@1
```yaml
id: core.Surrender.Default
schema: SurrenderThresholdDef@1
weights:
  hp_low: +0.25
  outnumbered_2to1: +0.20
  outnumbered_3to1: +0.35
  fear_aura: +0.10
  leader_present: +0.10
exceptions:
  undead: { immune: true }
  zealot: { mult: 0.5 }
```
## NonLethalToggleDef@1
```yaml
id: core.NonLethal.RangerShot
schema: NonLethalToggleDef@1
ability: core.Ability.Ranger.Shot
dps_mult: 0.8
damage_type: dmg.NonLethal
status_swap_any: [{ from: core.Status.Bleed, to: core.Status.Daze }]
```
## KOStatusDef@1
```yaml
id: core.Status.KO
schema: KOStatusDef@1
timer_s: [45,90]
blocks_actions: true
interacts_with:
  bleed_accelerates: true
  triage_resets_to_s: 60
```
## RestraintItemDef@1
```yaml
id: core.Restraint.Rope
schema: RestraintItemDef@1
quality: "basic"
escape_dc: 10
encumbrance: 1
```
```yaml
id: core.Restraint.Shackles
schema: RestraintItemDef@1
quality: "superior"
escape_dc: 18
encumbrance: 2
```
## CustodyFacilityDef@1
```yaml
id: core.Custody.Stockade
schema: CustodyFacilityDef@1
capacity: 6
security: 0.6
comfort: 0.3
requires_role_any: ["Warden"]
policy_default: core.CustodyPolicy.Stern
event_tables_any: ["core.PrisonEvents.Default"]
room_tag: "Stockade"
```
## CustodyPolicyDef@1
```yaml
id: core.CustodyPolicy.Stern
schema: CustodyPolicyDef@1
multipliers: { escape_chance: 0.9, reform_chance: 0.95, ethos_impact: -0.02 }
```
## TransferActionDef@1
```yaml
id: core.Transfer.ToFacility
schema: TransferActionDef@1
from_any: ["party","cage"]
to: "facility"
requires_keys_any: []
```
## BountyContractDef@1
```yaml
id: core.Bounty.Hunter.Alive
schema: BountyContractDef@1
issuer_faction: core.Faction.Town
target_filter: { actor: core.Actor.EvilHunter }
payout_coin: 180
alive_only: true
ttl_days: 7
law_profile: core.Law.Township
standing_deltas_any: [{ faction: core.Faction.Town, delta: +8 }]
```
## LawProfileDef@1
```yaml
id: core.Law.Township
schema: LawProfileDef@1
accepts_alive_only: true
bribeable: false
execution_allowed: false
paperwork_required: true
rep_multipliers: { bounty_turnin_alive: 1.0, execute_public: -1.0 }
```
## PrisonEventDef@1
```yaml
id: core.PrisonEvents.Default
schema: PrisonEventDef@1
entries:
  - { weight: 25, event: core.PrisonEvent.EscapeAttempt }
  - { weight: 15, event: core.PrisonEvent.BribeOffer }
  - { weight: 10, event: core.PrisonEvent.ConversionMoment }
```
## ParoleRuleDef@1
```yaml
id: core.Parole.Default
schema: ParoleRuleDef@1
success_base: 0.6
recidivism_base: 0.25
modifiers:
  humane: +0.1
  chaplain_present: +0.1
  harsh: -0.15
tag_on_release: core.Tag.Paroled
```
## RecruitmentOfferDef@1
```yaml
id: core.Recruit.Hunter
schema: RecruitmentOfferDef@1
requires_any:
  - { relationship_gte: 20 }
  - { saga_shared: true }
costs: { oath: true, stipend: 20 }
ethos_clash_penalty: 0.2
```
## Gambit Extensions
- **Conditions:** `enemy_surrenders`, `enemy_ko`, `capture_slot_free`, `ally_carrying_captive`, `nonlethal_ready`
- **Actions:** `toggle_nonlethal`, `apply_restraints`, `carry_to_rally`, `threaten_surrender`, `triage_bleed`

## Validation
- Alive-only quests must reference ≥1 tool source or vendor hint.  
- Boss capture requires `can_be_captured: true`.  
- Facility `capacity >= 1`; policy id must resolve.
