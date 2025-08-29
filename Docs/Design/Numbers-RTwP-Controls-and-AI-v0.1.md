# Numbers — RTwP Controls & AI (v0.1 Defaults)

## Stances
- Offense: **damage +0.15**, **guard −0.10**, **block window −0.05s**.
- Balanced: **0** changes.
- Defense: **damage −0.10**, **guard +0.15**, **block window +0.05s**.
- **Swap GCD:** 300 ms.

## Formations
- Line: spacing **1.5** tiles; front guard **+0.05**.
- Pike: spacing **1.0**; charge stagger resist **+0.10**.
- Wedge: spacing **1.25**; penetration **+0.05** on first engage.
- Loose: spacing **2.25**; AoE damage taken **−0.10**.
- **radius_stay:** 6 tiles; **radius_break:** 8 tiles; **swap warm-up:** 400 ms.

## Behavior Params (Default)
- Engage Range: **medium (8 tiles)**  
- Interrupt Priority: **baseline (0)**  
- Retreat HP%: **25%** (facet Boldness can shift −5..+5 pp)  
- Focus-Fire Compliance: **60%**  
- Formation Strictness: **50%**  
- Chase Distance: **4 tiles**  
- Cover Preference: **medium**

**Aggressive delta:** Engage +10, Retreat −5 pp, FF +15, Chase +6.  
**Defensive delta:** Engage −10, Interrupt +10, Retreat +5 pp, Strictness +15.

## Gambit / AI Guards
- Retarget cooldown min **900 ms**; target swap cost increases by **+200 ms** if done twice within 3 s.  
- Global GCD lower bound **300 ms** (lint warns below).  
- Hold-position leash **2 tiles**.

## Input / Time
- Pauses: **Full Pause** and **Slow Time (0.5×)**.  
- Time scale steps: **0.75× / 1.00× / 1.25×**.

## QuestTemplate@4
- At least **1** of `paths_any` or `director_rule_seeds` required; countdown or paused-until-visit must be present.
