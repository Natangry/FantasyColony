# SOUND THE ALARM — Auto-Equip, Rally & Evacuate (v0.1, Docs Only)
> **Update:** Loadouts, Lockers/Armory integration, quick-swap sets.

**Additions**
- **Quick-swap sets:** choose **Field/Defense** templates; Alarm equips **Defense** by default (config).
- **Storage order:** **locker.personal → stockpile.armory → world** (per `EquipReservePolicyDef`).
- **Repair gate:** items under **repair_floor_field** are skipped unless `allow_damaged: true`.
- **Revert:** `StandDownPolicy` can revert to **Field** set and reopen doors; normalize ward/trap states.
