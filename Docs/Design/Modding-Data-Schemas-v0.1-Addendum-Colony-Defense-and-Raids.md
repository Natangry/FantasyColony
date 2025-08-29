# Schemas Addendum — Colony Defense, Raids & Alarm (v0.1)
*Additive to canvas **Modding Data Schemas v0.1 (Draft)**.*

## AlertPolicyDef@1
```yaml
id: core.Alert.Lockdown
schema: AlertPolicyDef@1
door_rules: { nonessential: "lock", vital: "open" }
lighting: { exterior: "dim", interior: "bright" }
ward_state: "on"
trap_state: "armed"
civilian_behavior: "evacuate"
```
## AlarmActionDef@1
```yaml
id: core.Alarm.Default
schema: AlarmActionDef@1
alert_state: core.Alert.Lockdown
equip_profile: core.EquipProfile.Defense
rally_point: core.Rally.MainGate
rally_fallback: core.Rally.Courtyard
evac_zone: core.Evac.Civilians
stance_overrides:
  Guardian: Defense
  Ranger: Balanced
  Healer: Balanced
gambit_override: core.Gambit.AlarmDefensive
grace_ms: 2000
max_prep_time_ms: 20000
stand_down_policy: core.StandDown.Revert
```
## EquipProfileDef@1
```yaml
id: core.EquipProfile.Defense
schema: EquipProfileDef@1
templates_any: ["core.Loadout.Guardian.Defense","core.Loadout.Ranger.Defense","core.Loadout.Healer.Defense"]
reservation_policy: core.EquipReserve.ArmoryFirst
```
## LoadoutTemplateDef@1 (examples)
```yaml
id: core.Loadout.Guardian.Defense
schema: LoadoutTemplateDef@1
must_any: ["tag:weapon.1h","tag:shield"]
prefer_any: ["tag:armor.heavy","tag:trinket.block"]
forbid_any: ["tag:weapon.2h"]
slots:
  mainhand: "tag:weapon.1h"
  offhand: "tag:shield"
  armor: "tag:armor.heavy|tag:armor.medium"
  trinket: "tag:trinket.block|tag:trinket.guard"
```
```yaml
id: core.Loadout.Ranger.Defense
schema: LoadoutTemplateDef@1
must_any: ["tag:weapon.bow"]
prefer_any: ["tag:quiver","tag:armor.light"]
slots:
  mainhand: "tag:weapon.bow"
  offhand: null
  armor: "tag:armor.light|tag:armor.medium"
  accessory: "tag:quiver"
```
## EquipScoreDef@1
```yaml
id: core.EquipScore.Default
schema: EquipScoreDef@1
weights:
  tag_match: 1.0
  quality_scalar: 0.2
  material_scalar: 0.15
  condition_scalar: 0.1
  class_synergy: 0.25
  trait_synergy: 0.1
```
## EquipReservePolicyDef@1
```yaml
id: core.EquipReserve.ArmoryFirst
schema: EquipReservePolicyDef@1
sources_in_order: ["locker.personal","stockpile.armory","world_loose"]
```
## RallyPointDef@1
```yaml
id: core.Rally.MainGate
schema: RallyPointDef@1
marker: "markers/rally_main_gate.png"
formation: "Line"
facing_deg: 180
```
## EvacZoneDef@1
```yaml
id: core.Evac.Civilians
schema: EvacZoneDef@1
room_tag: "SafeRoom"
capacity: 20
escorts_any: ["Guardian","Warden"]
```
## StandDownPolicyDef@1
```yaml
id: core.StandDown.Revert
schema: StandDownPolicyDef@1
revert_equipment: true
reopen_doors: true
ward_state: "auto"
trap_state: "safe"
```
## GambitDef (override example)
```yaml
id: core.Gambit.AlarmDefensive
schema: GambitDef@1
rules:
  - if: { ally_hp_lt: 0.5, has_ability: core.Ability.Heal } then: { use_ability: core.Ability.Heal, target: "ally_lowest_hp" } prio: 100
  - if: { enemy_casting: "heavy" } then: { use_ability: core.Ability.Interrupt, target: "enemy_caster" } prio: 80
  - if: { focus_fire: true } then: { attack: true, target: "focus" } prio: 60
  - if: { always: true } then: { attack: true, target: "nearest" } prio: 10
```

## Validation
- Alarm must reference valid `AlertPolicy`, `EquipProfile`, and at least one `RallyPoint` OR `EvacZone`.
- Loadout templates must resolve required slots or fallback provided.
- StandDown policy must specify equipment/door/ward resets.
