# Numbers — Equipment, Encumbrance & Repair (v0.1 Defaults)

## Quality Scalars
- Poor **0.90**, Common **1.00**, Fine **1.05**, Exceptional **1.10**, Masterwork **1.15**.

## Condition Penalties
- Condition ≥ 40: **1.00×**; 15–39: **0.90×**; < 15: **0.70×** to item stat contributions.

## Encumbrance (capacity from STR/VIT)
- Capacity = `Base(20) + 1.5*STR + 0.5*VIT` (weight units).
- Bands/penalties per `EncumbranceCurveDef@1` (Default).

## Repair
- Field Repair: restores **+20** (cap = 60), **time 8s**, consumes **RepairKit.Basic**.  
- Bench Repair: restores up to **100**, time scales with missing condition (`0.15s/pt`) + material factor.
- Repair cost modifier by quality: Poor **−20%**, Masterwork **+40%**.

## Salvage
- Returns scale with material & quality; condition < 15 reduces yields by **−25%**.

## EquipScore Weights (baseline)
- As in `EquipScoreDef@1`. Suggested trait synergies: *Sure Hands* +0.05 for shields, *Sharpshooter* +0.05 for bows.

## Armory/Locker
- Armory theft risk during raids: *Low 5%*, *Medium 12%*, *High 22%* chance an item gets stolen per breach event (guarded by Warden/Guardian presence).
