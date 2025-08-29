# Numbers — Items, Inventory, Storage & Spoilage (v0.1 Defaults)

## Stack Sizes (max per stack)
- Logs **20** · Planks **50** · Stone **40** · Ore **30** · Ingots **20**
- Herbs **40** · Cloth **40** · Meals **10** · Raw Meat **10** · Produce **30**
- Medicine (Poultice) **10** · Tincture **6**

## Weight / Volume (typical per unit)
- Log **1.0 / 1.0** · Plank **0.5 / 0.5** · Stone **1.2 / 1.0** · Ore **1.0 / 0.8**
- Meal **0.3 / 0.2** · Produce **0.2 / 0.2** · Herb **0.05 / 0.05**

## Carry Cap (pawn baseline)
- Base **10.0** weight units + **Might × 0.6**
- Encumbrance: ≤50% none · 50–100% Move **−10%**, Stamina **+10%** · >100% slow-walk only

## Containers (capacity / modifiers)
- **Crate:** 16 slots or 16 vol; no temp mod
- **Barrel:** 12 slots; **freshness ×0.9** for liquids/ferments
- **Sack:** 10 slots; only `grain/flour/seed`
- **Rack:** 8 slots; adds **+5% quality** roll on crafted goods stored (decor perk)
- **Tool Rack:** 8 tools; pickup radius **12 tiles**
- **Medicine Cabinet:** 12 slots; **sterile ×0.9** disease chance

## Room Tags (multipliers to decay)
- **Pantry (room.pantry):** decay **×0.9**
- **Sealed (room.sealed):** decay **×0.85**
- **Insulated (room.insulated):** decay **×0.75**
- **Cold Cellar (room.cold_cellar):** decay **×0.5**, below 5°C: **×0.25**; freezing (≤0°C): decay halted; thaw quality −1 tier for some foods

## Spoilage Half-Lives (at 20°C, uncovered)
- **Raw Meat:** **0.8 day**
- **Produce:** **1.2 days**
- **Cooked Meals Tier I:** **1.0 day**
- **Cooked Meals Tier II+:** **1.5 days**
- **Herbs:** **4.0 days**
- **Grain/Flour:** **30+ days**
- **Potions/Tinctures:** non-perishable by default

## Freshness Stages (effects)
- **Fresh (100–60%)**: full buffs
- **Stale (60–30%)**: meal buffs **−50%**
- **Spoiled (30–0%)**: no buffs; **Queasy** chance (10–25%); morale −1
- **Rotted (0%)**: inedible; can compost

> All numbers are first pass and intended for balance iteration.

