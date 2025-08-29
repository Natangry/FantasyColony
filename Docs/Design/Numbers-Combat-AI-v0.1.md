# Numbers — Combat AI Defaults (v0.1)

## Thresholds
- Heal ally at **≤ 35%** HP
- Self-heal (potion) at **≤ 25%** HP
- Mana potion at **≤ 15%** mana
- Retreat at **≤ 20%** HP
- Kite distance: **5 tiles**
- Telegraph dodge window: **≤ 600 ms**

## AoE Rules
- Cast if **≥ 3** enemies in **r ≤ 3** and **0 allies** in radius
- Conservative profile raises ally safety to **strict** (no allies allowed)
- Burst profile allows cast with **≥ 2** enemies if lethal

## Profiles
- **Normal** (default), **Conservative**, **Burst** (see Gambit Library)

## Items
- Keep **≥ 1** healing potion in reserve
- Don’t consume last potion if a **Healer** is in party and not suppressed (toggle)

> Tunables intended for balancing passes and community tweaks.

