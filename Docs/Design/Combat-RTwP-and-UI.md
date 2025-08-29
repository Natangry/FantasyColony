# Combat — RTwP + Assume Control (v0.1)
Real-Time with Pause; fixed-timestep; time controls (0×/0.25×/1×/2×/3×); autopause on key triggers. One **Assume Control** pawn (WASD + mouse aim/lock, dash). Others run **Gambits**.

## Two-tier Gizmos
Base: Move · Attack · Ability · Magic · Item · AI · Assume Control.
Sub-bars: open above base; one open at a time. **Guard**→Ability, **Focus Fire**→Attack, **Retreat/Stance/Formation**→Move.

## Abilities & Timing
Stamina/Mana/Resolve; short GCD; cast/channel; interrupts; readable telegraphs.

## Hit Resolution (summary)
Targeting → Hit/Graze/Miss (Accuracy vs Evasion) → Guard/Block → Damage mods → Armor/Resist → Crit → Status → Procs/Aggro.  
Full math: `Docs/Design/Core-Stats-and-Combat-Math-v0.1.md` and numbers in `Docs/Design/Numbers-Stats-and-Damage-v0.1.md`.

## Stances & Formations
- **Stances:** Offense (+15% dmg, −10% guard, −5% block window), Balanced, Defense (−10% dmg, +20% guard, +10% block window).
- **Formations:** Line (rear Focus +5%), Pike (front Guard +10% vs charges), Wedge (lead +5% dmg on engage).

## Auto-Combat Profiles & Defaults
See `Docs/Design/Gambit-Library-v0.1.md` and `Docs/Design/Numbers-Combat-AI-v0.1.md`.
- Profiles: **Normal** (default), **Conservative**, **Burst**
- Global defaults: Heal ≤35%, Retreat 20%, Kite 5 tiles, AoE caution, Potion reserve ≥1.

## Acceptance
Encounter → scene-swap arena → win/lose → back to sim; abilities & gambits functional; injuries/loot persist.
