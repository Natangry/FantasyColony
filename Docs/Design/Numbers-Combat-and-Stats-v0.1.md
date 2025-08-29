# Numbers — Combat & Stats (v0.1 Defaults)

> All numbers are defaults and intended to be tuned by mods/storytellers. Use `TTKTargetDef` to sanity-check build targets.

## 1) Primary → Derived (examples)
- **MaxHP** = `Base(80) + 12*VIT + LevelCurveHP(level)`  
- **Stamina** = `Base(50) + 5*VIT + 2*STR`
- **Mana** = `Base(50) + 8*INT + 2*RES`
- **Accuracy** = `Base(0.75) + 0.0025*DEX` (to 0–1)
- **Evasion** = `Base(0.05) + 0.0020*DEX + 0.0010*LUCK` (to 0–0.4 soft cap)
- **CritChance** = `Base(0.05) + 0.0015*LUCK + 0.0008*DEX` (soft cap 0.35)
- **CritPower** = `1.50 + 0.002*LUCK` (× damage; soft cap 2.0)
- **Haste** = `0.0 + 0.002*INT + 0.001*DEX` (soft cap 0.30)
- **InterruptPower** = `Base(5) + 0.4*STR + 0.3*INT`
- **Poise** = `Base(10) + 0.6*RES + ArmorTierBonus`

## 2) GCD/Cast/Recovery
- **Base GCD** = 1.0 s (melee) / 1.2 s (ranged) / 1.4 s (magic).  
- **Effective GCD** = `BaseGCD * (1 - Haste)` with **floor 0.30 s** (lint warning < 0.30).  
- **Cast** and **Channel** scale similarly with Haste; **Recovery** = `0.6 * Effective GCD`.

## 3) Hit Table
- **Miss/Dodge** chance = `max(0, target.Evasion - attacker.Accuracy)` (clamped 0–0.30).  
- **Glance** chance = `base_glance (0.10) + angle/range mismatch bonus (0–0.15)`; **GlancePower** = −30% damage.  
- **Crit** rolls after hit and glance; **CritChance** soft-capped at 35% (DR beyond).

## 4) Mitigation
- **Armor curve**: `DR_phys = Armor / (Armor + K)` with **K = 80 + 8*AttackerLevel** (cap 0.70).  
- **Resist curve**: `DR_elem = Resist / (Resist + 100)` (cap 0.75).  
- **Guard%** cap **0.50**; **Block%** cap **0.50**; **BlockPower** (percent) cap **0.50**; flat Block applies before percent BlockPower.

## 5) Status
- **Tick cadences**: 1.0 s default DoT/HoT; 0.5 s for rapid burns/bleeds; 2.0 s for heavy toxins.  
- **Scaling**: defaults to **snapshot_on_apply: true** for base power; per-tick **dynamic** multipliers allowed for a subset (flag on `StatusDef`).  
- **Durations**: baseline 6 s minor, 10 s moderate, 15 s major; extended by `INT` (up to +20%) or resisted by `RES` (up to −20%).  
- **Dispels**: base success 100% vs non-elite with matching school; elites check `dispel_power vs target.resist_school`.

## 6) Threat
- **Threat** = `post_mit_damage * (1 + ability.threat_mult + stance.threat_mult)`; **taunt** sets temporary hard focus (3–6 s).

## 7) AoE & Falloff
- **Splash**: −20% per ring beyond primary unless flagged `no_falloff`.  
- **Cone/Arc**: inner 50% of arc/angle gets +10% damage if flagged `focus_inner`.

## 8) TTK Targets (for balancing)
- **Trash**: 2–4 GCDs at tier parity.  
- **Elites**: 12–18 GCDs with intermittent healing.  
- **Boss (early)**: 45–60 GCDs, with 2–3 mechanics (stagger/interrupt windows).

## 9) Caps & Floors
- **CritChance ≤ 0.50** (hard), DR kicks in at 0.35.
- **Guard%, Block%, BlockPower% ≤ 0.50**; **Armor DR ≤ 0.70**, **Element DR ≤ 0.75**.
- **GCD floor 0.30 s**; **Accuracy floor 0.60** vs even-tier targets.

## 10) Validation Hints
- Warn if any build simultaneously hits **ArmorDR ≥ 0.70** and **Guard ≥ 0.40** and **BlockPower% ≥ 0.40**.  
- Warn if `EffectiveGCD < 0.30 s`.  
- Error on circular formulas or missing referenced stats.

