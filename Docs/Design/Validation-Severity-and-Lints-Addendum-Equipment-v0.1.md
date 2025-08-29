# Validation — Addendum: Equipment & Loadouts (v0.1)

## Errors (block save)
- 2H weapon equipped with shield (conflict not resolved).
- Missing required slots in a `LoadoutTemplateDef`.
- Negative durability/condition; unknown `QualityTierDef/MaterialDef`.
- Item with `equip.slots_any` empty but tagged as equippable.

## Warnings
- Over-encumbrance (>130%) at rest; suggest lighter set or pack.
- Condition < 15 and `allow_damaged` false in Alarm config.
- Affix incompatibility detected; socket/gem mismatch.
- Armory with theft risk **high** but no Warden/Guardian assigned.

## Infos
- Items in lockers unused for > 10 days (stale gear).
- Loadout template leaves critical slot empty (e.g., no armor for Guardian).
