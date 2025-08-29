# Numbers — Logistics, Stockpiles, Hauling & Spoilage (v0.1 Defaults)

## Stock & Stacks
- Default stack size **wood: 50**, **stone: 50**, **ingots: 20**, **meals: 10**, **herbs: 20**.
- Container bonuses: **Shelf +50% stack** (non-perishables), **Barrel +100% liquids**, **Cold Chest freshness ×0.5**.

## Carry & Hauling
- Pawn base carry **= 10** weight units + STR/VIT bonuses (see Equipment numbers).
- Batch pickup (Small): up to **3** items; detour ≤ **6** tiles.
- Cart (Wooden): carry bonus **+40**, road mult **×1.25**; Pack Donkey: **+60** carry, speed **×1.05**.

## Floor & Path
- Road **×1.20**, Stone **×1.10**, Dirt **×1.00**, Mud **×0.85**; Door penalty **+300 ms**, Stairs **+200 ms**.

## Spoilage (base half-life at 20°C)
- Raw meat **12 h**, Cooked meals **36 h**, Vegetables **48 h**, Herbs **72 h**.
- Apply **Temp Mult** from `SpoilageDef.temp_curve` and **Container/Room freshness multipliers**.
- Thaw penalty: if frozen then thawed, subsequent phases tick **×1.25** faster.

## Temperature & Power
- Cold chest draw **80 W**; Small Cold Room **300 W** (maintains ~4°C in temperate).
- Door leak raises effective room temp by **+1–3°C** per open door (size/insulation dependent).

## Alerts (thresholds)
- **Food < 1 day** (based on counters & half-lives), **No Cold Storage** (perishables present, no cold multiplier), **Haul Backlog** (> 8 queued haul jobs), **Link Broken** (input/output link target absent).
