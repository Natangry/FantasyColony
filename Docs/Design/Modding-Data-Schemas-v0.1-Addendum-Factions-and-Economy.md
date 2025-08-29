# Schemas Addendum — Factions, Diplomacy & Economy (v0.1)
*Extends the canvas **Modding Data Schemas v0.1 (Draft)** (do **not** overwrite).*

## FactionDef@1
```yaml
id: core.Faction.Town
schema: FactionDef@1
kind: "Town"
home_region: "Elderwood"
traits: ["lawful","mercantile"]
likes_any: ["mat.silk","tag.Saga.Spider.Aftermath.Success"]
dislikes_any: ["tag.Banditry","tag.UndeadPresence"]
rivals_any: ["core.Faction.Bandits"]
mutators_any: []   # applied via rules/leader/quests
```
## StandingRuleDef@1
```yaml
id: core.StandingRule.TownDefaults
schema: StandingRuleDef@1
thresholds:
  hostile: -50
  wary_low: -10
  friendly: 30
  allied: 60
decay_per_week: 1
```
## TreatyDef@1
```yaml
id: core.Treaty.TradePact
schema: TreatyDef@1
upkeep_per_day: 5
cooldown_days: 7
effects:
  route_freq_mult: 1.2
  tariff_mult: 0.9
gates:
  min_standing: 20
```
## DiplomacyActionDef@1
```yaml
id: core.Diplomacy.SendEnvoy
schema: DiplomacyActionDef@1
cost: { coin: 25 }
effects: [{ standing_delta: { faction: core.Faction.Town, delta: +5 } }]
gates: { min_standing: -10 }
```
## ContractDef@1
```yaml
id: core.Contract.CullSpiders
schema: ContractDef@1
board: "Bounty"
objectives: [{ kill: { tag: "enc.spider", count: "scale:2+intensity" } }]
rewards: { coin: "scale:15+intensity*5", rep: { faction: core.Faction.Town, delta: 4 } }
expires_days: [3,5]
```
## MarketDef@1 & PriceModelDef@1
```yaml
id: core.Market.TownGeneral
schema: MarketDef@1
price_model: core.PriceModel.Default
stock_tables: [ core.Stock.BasicGoods ]
refresh_days: [2,4]
black_market: false
```
```yaml
id: core.PriceModel.Default
schema: PriceModelDef@1
base_prices: { "mat.silk": 10, "food.meal": 3 }
elasticity: { scarcity_mult: 0.2 }
tag_mods:
  - { tag: core.WorldEffect.Spider.SilkBoom, price_mult: { "mat.silk": 0.8 } }
```
## CommodityTagDef@1
```yaml
id: core.CommodityTag.Luxury
schema: CommodityTagDef@1
tags_any: ["luxury.*"]
```
## TradeRouteDef@1 / CaravanDef@1 / CreditLedgerDef@1 / ShopDef@1
```yaml
id: core.TradeRoute.TownToPort
schema: TradeRouteDef@1
frequency_days: [3,5]
risk_base: 0.15
toll_base: 0.0
modifiers: [{ tag: core.Tag.WebbedRoads, risk_add: 0.2 }]
```
```yaml
id: core.Caravan.Basic
schema: CaravanDef@1
guards: 3
cargo_table: core.Stock.CaravanGeneral
```
```yaml
id: core.CreditLedger.Default
schema: CreditLedgerDef@1
credit_limit_by_standing: { wary: 25, friendly: 75, allied: 150 }
```
```yaml
id: core.Shop.Player
schema: ShopDef@1
pulls_from_stockpiles: true
markup_pct: 0.15
```
## Validation
- `FactionDef` may list mutators but must not *define* them (see separate addendum).
- `PriceModelDef` token refs must exist; no negative prices; refresh ranges sane.
- `TradeRouteDef.frequency_days` must be positive; risk in [0,1].
