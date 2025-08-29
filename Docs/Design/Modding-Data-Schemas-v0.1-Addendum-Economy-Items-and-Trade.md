# Schemas Addendum — Economy, Itemization & Trade (v0.1)
*Additive to canvas **Modding Data Schemas v0.1 (Draft)**. Do not overwrite.*

## RarityDef@1
```yaml
id: core.Rarity.Rare
schema: RarityDef@1
color: "#4AA3FF"
price_scalar: 1.6
```

## QualityDef@1
```yaml
id: core.Quality.Masterwork
schema: QualityDef@1
ui_chip_key: loc.quality.masterwork
stat_scalar: 1.05
price_scalar: 1.25
```

## MaterialDef@1 & MaterialTierDef@1
```yaml
id: core.Mat.Silk
schema: MaterialDef@1
tier: core.MatTier.T2
tags_any: ["textile"]
price_scalar: 1.2
```
```yaml
id: core.MatTier.T2
schema: MaterialTierDef@1
req_level: 10
```

## AffixDef@1 & AffixPoolDef@1
```yaml
id: core.Affix.Swiftness
schema: AffixDef@1
tags_any: ["gear.light","school.Swift"]
effects_any: [{ stat: "move_speed", add: 0.03 }]
weight: 15
```
```yaml
id: core.AffixPool.LightGear
schema: AffixPoolDef@1
rolls: 1
affixes_any: ["core.Affix.Swiftness","core.Affix.CarefulHands"]
```

## ItemDef (extensions)
```yaml
id: core.Item.WoodenHoe
schema: ItemDef@1
base_value: 18
rarity: core.Rarity.Common
quality_rolls_any: ["core.Quality.Standard","core.Quality.Fine"]
material_any: ["core.Mat.Wood"]
affix_pools_any: []
tags_any: ["tool.harvest"]
tool_effects: { harvest_yield_mult: 1.02 }
```

## BlueprintDef@1
```yaml
id: core.Blueprint.Station.ForgeT2
schema: BlueprintDef@1
unlocks_any: ["core.Building.ForgeT2","core.Upgrade.Forge.AdditionalSlot"]
drops_any: ["core.POI.AncientForge","core.Questline.SmithGuild.Step2"]
vendor_profiles_any: ["core.Vendor.Blacksmith"]
```

## CraftingTierDef@1 & UpgradeRecipeDef@1
```yaml
id: core.CraftingTier.T2
schema: CraftingTierDef@1
requires_any: ["core.MatTier.T2","core.Building.ForgeT2"]
```
```yaml
id: core.Upgrade.Forge.AdditionalSlot
schema: UpgradeRecipeDef@1
station: core.Building.Forge
inputs: [{ item: core.Item.IronIngot, count: 6 }, { item: core.Item.Gear, count: 2 }]
work_ms: 180000
result: { station_mod: { slot_add: 1 } }
```

## SalvageDef@1
```yaml
id: core.Salvage.LeatherBoots
schema: SalvageDef@1
yields_any:
  - { item: core.Item.LeatherScrap, count: [1,3], weight: 70 }
  - { item: core.Item.Thread, count: [0,2], weight: 30 }
```

## LootTableDef@1
```yaml
id: core.Loot.SpiderNest
schema: LootTableDef@1
entries:
  - { weight: 40, item: core.Item.SilkBundle, qty: [1,2] }
  - { weight: 10, item: core.Item.SpiderGland, qty: [0,1] }
rarity_bias: { base: "Uncommon" }
quality_bias: { base: "Standard" }
affix_pool_any: []
```

## VendorProfileDef@1
```yaml
id: core.Vendor.TownGeneral
schema: VendorProfileDef@1
restock_days: 3
buy_mult: 0.6
sell_mult: 1.1
stock_any:
  - { weight: 30, item: core.Item.Rope, qty: [2,6] }
  - { weight: 20, item: core.Item.BasicTools, qty: [1,2] }
  - { weight: 15, item: core.Item.MealBundle, qty: [1,3] }
rare_gates_any:
  - { rep_gte: { faction: core.Faction.Town, value: 10 }, add_stock: { item: core.Blueprint.Station.ForgeT2, qty: 1 } }
```

## PriceRuleDef@1
```yaml
id: core.Price.Default
schema: PriceRuleDef@1
formula:
  base: "@item.base_value"
  mults:
    - "@rarity.price_scalar"
    - "@quality.price_scalar"
    - "@material.price_scalar"
    - "@market.prosperity_scalar"
    - "@market.aftershock_scalar"
    - "@law.tariff_scalar"
    - "@rep.vendor_discount_scalar"
```

## CurrencyDef@1
```yaml
id: core.Coin
schema: CurrencyDef@1
symbol_key: loc.currency.coin
minor_per_major: 100
```

## TradeRouteDef@2 (extends @1)
```yaml
id: core.Route.TownToPort
schema: TradeRouteDef@2
connects_any: ["core.Region.Elderwood","core.Region.PortAlba"]
tariffs_pct: 0.08
supply_tags_any: ["fish","salt"]
demand_tags_any: ["wood","silk"]
event_flags_any: []
closed_when_any: ["core.Tag.Quarantine","core.Tag.BridgeDown"]
```

## CaravanDef@1
```yaml
id: core.Caravan.TownMerchant
schema: CaravanDef@1
route: core.Route.TownToPort
capacity_slots: 60
guard_level: "medium"
vendor_profile: core.Vendor.TownGeneral
```

## Validation
- Items must have `base_value` or inherit; `rarity/quality/material` IDs must resolve.
- Vendor `restock_days ≤ 14`; `sell_mult ≤ 1.25`; `buy_mult ≥ 0.4`.
- TradeRoute tariffs 0–0.30; affix scalar sums ≤ +0.25 suggested (lint above).
- Loot tables weight > 0 and entries valid.

