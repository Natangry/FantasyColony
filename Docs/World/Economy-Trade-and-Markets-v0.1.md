# Economy, Trade & Markets (v0.1, Docs Only)

**Goal:** Mod-first trade with price models responsive to **tags, sagas, mutators**, and **route safety**.

## 1) Commodities
Tag families: `mat.*`, `food.*`, `tool.*`, `medicine.*`, `luxury.*`, `artifact.*`.

## 2) Markets & Price Models
- **MarketDef** per settlement/faction shop: base prices, stock tables, refresh cadence.
- **PriceModel:** elasticity (supply/demand), scarcity shocks, **mutator modifiers** (e.g., *Plagued* → medicine +25%).
- **Black-market** flag: appears under **Corrupt/Pirate** mutators; trades contraband at markup.

## 3) Currency & Credit
- Soft currency + **CreditLedger** per faction; limits scale with standing.
- Barter fallback uses weighted value by price model.

## 4) Routes & Caravans
- **TradeRouteDef:** frequency & risk; modified by tags (`WebbedRoads`) and treaties.
- Caravans spawn with guards; cargo influenced by **saga** and **mutators** (e.g., famine → grain heavy).

## 5) Player Shop
- **Shop designation** pulls from stockpiles; sets markup; buyers visit on timers; faction standing impacts footfall.

## 6) UI
- **Trade screen:** cart vs pocket, credit limits, scarcity badges, mutator chips.
- **Route overlay:** safety heatmap, tolls, closures.

> Schemas: `Modding-Data-Schemas-v0.1-Addendum-Factions-and-Economy.md`.
