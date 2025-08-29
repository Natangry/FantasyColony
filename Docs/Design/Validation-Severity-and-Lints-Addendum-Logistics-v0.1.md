# Validation — Addendum: Logistics, Stockpiles & Spoilage (v0.1)

## Errors (block save)
- **LinkDef** references non-existent stockpile/container/station.
- **Cold container** with `power_draw_w > 0` placed in a room without a **ColdRoomDef** or available power/fuel source.
- **SpoilageDef** phases do not cover 0–1 or temperature multipliers not monotonic.

## Warnings
- **Haul backlog** sustained > 8 jobs for 1 day (consider carts/pack animals or more Logistics labor).
- **Food < 1 day** with no **Gruel** emergency recipe enabled.
- **No cold storage** while perishables present.
- **Broken route**: station link path blocked (door locked, wall placed).
- **Unreachable stockpile** (filter radius too small or obstacles).

## Infos
- Over-provisioned stockpiles (filters overlap causing churn).
- Counters drifting due to recipe byproducts (suggest counter inclusion tweaks).
