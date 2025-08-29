# Core Stats & Combat Math (v0.1, Docs Only)

**Goal:** Establish a clear, moddable combat/stat model used by abilities, items, meals, tools, jobs, and AI. This closes stance/formation defaults and defines the hit-resolution pipeline and status matrix.

---

## 1) Primary & Secondary Stats

**Primary (allocatable / growth):**  
**Might, Grace, Insight, Resolve**

**Secondaries (derived):**  
- **Health (HP)**, **Stamina**, **Mana**  
- **Work Speed**, **Move Speed**  
- **Accuracy/Focus** (ranged & finesse), **Evasion**  
- **Crit Chance**, **Crit Power**  
- **Guard** (flat incoming damage soak), **Block Window** (timing leniency)  
- **Poise** (stagger threshold), **Interrupt Resist**  
- **Status Resist** (generic gate for debuffs)  
- **Carry Cap**, **Resolve Regen**

> Formulas and first-pass numbers live in `Docs/Design/Numbers-Stats-and-Damage-v0.1.md`.

---

## 2) Damage & Mitigation

**Types**
- **Physical:** `Slash`, `Pierce`, `Blunt`
- **Elemental/Arcane:** `Fire`, `Frost`, `Shock`, `Poison`, `Holy`, `Shadow`

**Mitigation layers**
1) **Guard** (flat soak per hit; mostly from gear/stances)
2) **Armor Soak (Physical only)** (flat soak based on armor class)
3) **Resist %** per damage type (capped at 75%)

**Crits**
- **Crit Chance** roll → **Crit Power** multiplier on post-mitigation damage.

---

## 3) Hit-Resolution Pipeline (readable & moddable)

1) **Targeting**: LoS, range, shapes (cone/arc/line), cover/height modifiers collected.  
2) **Hit Roll**: `Accuracy vs Evasion` → hit / graze / miss (tunable thresholds).  
3) **Guard/Block**: If guarding or within **Block Window**, apply stance/shield rules.  
4) **Raw Damage**: `Base * (1 + %mods) + flat` (from ability/tool/meal/buffs).  
5) **Mitigation**: subtract **Guard**, **Armor Soak** (physical), then apply `(1 - Resist)` per type.  
6) **Crit**: apply Crit Power if roll succeeded.  
7) **Status Application**: per-effect chance vs **Status Resist** and special primers.  
8) **On-Hit Procs / Aggro**: emit procs; update threat.

> Exact math knobs are listed in Numbers. Designers can choose “simple” (hit/miss only) or “advanced” (graze) via `HitFormulaDef`.

---

## 4) Status Matrix & Primers (v0.1)

| Status | Type | Effect (default) | Duration | Notes / Primers |
|---|---|---|---|---|
| **Burn** | Fire DoT | tick dmg; stacks low | 6–10s | **Oily** ↑; **Wet** halves |
| **Chill** | Frost slow | −Move/Attack | 6–8s | 3 stacks → **Frozen** |
| **Frozen** | Frost hard CC | Root + Crit vs Blunt bonus | 2–3s | **Blunt** shatter bonus |
| **Shock** | Electric stagger | brief stagger; ↑ interrupt chance | 0.5–1.5s | **Wet** ↑ |
| **Poison** | Bio DoT | DoT; −healing received | 8–12s | stacks moderate |
| **Bleed** | Physical DoT | DoT; scales with movement | 6–10s | countered by Bandage |
| **Curse** | Arcane | −Resolve regen; −Status Resist small | 10–15s | dispellable |
| **Silence** | Arcane | disables Magic tag abilities | 3–5s | resist via Resolve/insight |
| **Stun** | Control | no actions | 1–2s | heavy telegraphs |
| **Root** | Control | no movement | 2–4s | jump/dash may break |
| **Slow** | Debuff | −Move speed | 4–8s | common light control |
| **Wet** | Primer | enables Shock bonus; halves Burn | 6–10s | placed by water/weather |
| **Oily** | Primer | enables Burn bonus | 6–10s | flammable surfaces |

Resistances reduce apply chance and/or shorten duration per `StatusDef` policy.

---

## 5) Stances (finalized)

- **Offense:** `damage +15%`, `guard −10%`, `block_window −5%`
- **Balanced:** neutral
- **Defense:** `damage −10%`, `guard +20%`, `block_window +10%`

> These appear in gizmos and in `StanceDef` (see schema addendum). AI biases live in presets.

---

## 6) Formations (v0.1 defaults)

- **Line:** spacing 1 tile; **rear Focus +5%** (clear lanes); mild flank risk.  
- **Pike:** spacing 1; **front Guard +10% vs charging beasts**.  
- **Wedge:** spacing 1–2–1; **lead Damage +5%** on engage (short window).

---

## 7) Environment: Cover & Advantage

- **Cover:** light/heavy reduces incoming **Accuracy** and can add **Guard** vs projectiles.  
- **Height:** grants small **Accuracy/Focus** and **Range** bump; down-slope penalties.  
- **Flanking:** attacking a target not facing you grants small **Accuracy** bonus and **Block Window** penalty.

---

## 8) Modding Hooks

- `StatDef`, `DerivedStatFormulaDef`, `DamageTypeDef`, `ResistTypeDef`, `ArmorDef`  
- `HitFormulaDef`, `CoverDef`, `AdvantageDef` (flank/height), `CritRuleDef`  
- `StatusDef` (extended), `StanceDef`, `FormationDef`

See schema shapes in `Modding-Data-Schemas-v0.1-Addendum-Stats-and-Damage.md`.

---

## 9) Worked Example

**Cinder Slash** (Slash + Fire) vs wolf in light cover:
1) Hit roll succeeds (cover −10 Accuracy applied).  
2) Guard 2 → subtract; Light Armor Soak 2 (physical) → subtract on Slash portion.  
3) Apply **Fire Resist** (10%) to Fire portion, **Physical Resist** (0%) to Slash.  
4) Crit? If yes, apply Crit Power 1.5×.  
5) Roll **Burn** at 30% vs Status Resist 10% → success → 6s DoT.
