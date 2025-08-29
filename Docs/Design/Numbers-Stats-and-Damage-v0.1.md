# Numbers — Stats, Damage & Status (v0.1 Defaults)

## Bases
- **HP:** 100 · **Stamina:** 100 · **Mana:** 50
- **Move Speed:** 4.5 tiles/s
- **Crit Chance:** 5% · **Crit Power:** 1.5×
- **Resist Cap:** 75%

## Per-Point Curves (first pass)
- **Might:** `HP +6`, Melee Dmg `+1%` / pt
- **Grace:** `Evasion +0.4`, **Focus +0.4**, Move `+0.02` / pt
- **Insight:** `Mana +5`, Status Potency `+0.4` / pt
- **Resolve:** Resolve Regen `+0.02`, Status Resist `+0.5` / pt

## Armor Soak (per hit)
- **Light:** 2 · **Medium:** 5 · **Heavy:** 9

## Cover (ranged)
- **Light:** Accuracy −10, adds Guard +1 vs projectiles
- **Heavy:** Accuracy −20, adds Guard +2 vs projectiles

## Formations
- **Line:** rear Focus +5%
- **Pike:** front Guard +10% vs charging beasts
- **Wedge:** lead Damage +5% for 2s on engage

## Stances
- **Offense:** Damage +15%, Guard −10%, Block Window −5%
- **Balanced:** 0
- **Defense:** Damage −10%, Guard +20%, Block Window +10%

## Status Defaults (tuning surfaces)
- **Burn:** DoT ticks every 1s; 6–10s
- **Chill:** −15% Move/Attack; 6–8s; 3 stacks → Frozen (2–3s)
- **Shock:** stagger 0.5–1.5s; Interrupt chance +20%
- **Poison:** DoT 8–12s; −healing 15%
- **Bleed:** DoT 6–10s; +25% when moving
- **Curse:** −Resolve regen 25%; −Status Resist 10%; 10–15s
- **Silence:** 3–5s
- **Stun:** 1–2s
- **Root:** 2–4s
- **Slow:** −20% Move; 4–8s

## Hit/Graze (optional mode)
- **Hit:** 100% damage  
- **Graze:** 60% damage (± tuning)  
- **Miss:** 0%

## Advantage
- **Flank:** Accuracy +5; Block Window −10 for defender
- **Height (1 level):** Accuracy/Focus +3; Range +1 tile (ranged only)

> All numbers are first-pass defaults for balance and iteration.
