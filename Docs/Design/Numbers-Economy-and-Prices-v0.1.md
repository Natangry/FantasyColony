# Numbers — Economy & Prices (v0.1 Defaults)

## Price Scalars
- **Rarity:** Common ×1.00, Uncommon ×1.25, Rare ×1.60, Epic ×2.20, Relic ×3.00
- **Quality:** Shoddy ×0.80, Standard ×1.00, Fine ×1.10, Masterwork ×1.25
- **Material (suggested) adders:** Iron ×1.05, Steel ×1.15, Silk ×1.20, Silver ×1.30, Gold ×1.50

## Vendor Cadence & Margins
- **Restock**: Town **3d**, Port **2d**, Monastery **4d**
- **Buy/Sell**: Vendor buys at **60%** of computed value; sells at **100–120%** (profile-based)
- **Haggling**: success ±7% (cap ±12%); cooldown **1d**; failure applies −3% temp penalty

## Tool & Food Effects
- **Tools**: +2% / +4% / +6% throughput or harvest yield (Basic/Fine/Superior)
- **Meals**:  
  - Gruel: −5% physical stats (8h)  
  - Proper: +2% morale (8h)  
  - Feast: +4% morale & +2% work speed (12h)

## Trade Routes & Tariffs
- **Tariffs**: 0–15% typical (cap 30%); quarantine or toll Aftershocks add +5–10%
- **Supply/Demand**: ±10–25% price skew regionally; decay 10%/day toward baseline after event ends

## Caravans
- **Capacity**: small 40 slots; large 120 slots
- **Guard**: low/med/high → ambush chance 20/10/5% per leg (before modifiers)
- **Ambush Loot**: 30–60% stock transferred to encounter loot table on fail

## Validation Hints
- Restock ≤ 14d; tariffs ≤ 30%; affix scalar total ≤ +25%; rarity×quality×material sanity cap ≤ ×5 overall (lint warns if exceeded)

