# Core Stats, Damage & Combat Math (v0.1, Docs Only)

**Intent:** A transparent, moddable combat spine that plugs into **AbilityDef**, **StatusDef**, **StanceDef**, **GambitDef**, **ItemDef**, and the colony jobs that unlock combat abilities. Authors can tune formulas/data without code.

*Additive to canvas **Modding Data Schemas v0.1 (Draft)**.*

---

## 1) Stat Model

### Primary (per pawn)
- **STR** (strength) — melee power, carry.
- **DEX** (dexterity) — ranged power, accuracy/evasion.
- **INT** (intellect) — magic power, status potency/duration.
- **VIT** (vitality) — max HP & regen.
- **RES** (resolve) — resistances, poise, status resist.
- **LUCK** — crit chance/power bias, rare-proc bias.

### Derived (computed via formulas)
- **MaxHP, Stamina, Mana**
- **Accuracy, Evasion**
- **CritChance, CritPower**
- **Guard% (pre-mitigation), Block%/BlockPower (flat/%)**
- **Armor (phys), Resist[Element] (ember/frost/venom/void/holy)**
- **Haste** (scales GCD/cast/channel/recovery)
- **MoveSpeed, InterruptPower, Poise**
- **ThreatMult** (for AI aggro heuristics)

> All derived values come from `DerivedStatFormulaDef` and `StatCurveDef` (see schemas addendum).

---

## 2) Damage Pipeline (authorable order of operations)

1. **Base**: `Ability or Weapon` → `amount ± variance` → attacker multipliers (stance, job passives, item affixes).
2. **Hit Table**: resolve **miss/dodge → glance → normal → crit** using **Accuracy vs Evasion** and **CritChance/CritPower**.
3. **Mitigation**:
   - **Armor** curve vs **physical** types (Slash/Pierce/Blunt).
   - **Resist[Element]** vs elements (Ember/Frost/Venom/Void/Holy).
   - **Guard%** (stance/wards) then **Block** (chance → reduce by flat/percent).
   - Caps and floors applied (Numbers doc).
4. **On-Hit**:
   - **StatusApply** chance rolls (per `StatusDef`), snapshot rules (see §5).
   - **Interrupt** if `attacker.InterruptPower ≥ target.Poise`.
5. **Overflow/AoE**: splash and falloff per `AbilityDef.range.shape` and Numbers.
6. **Threat**: generate aggro from post-mitigation damage × `ThreatMult` (+taunt flags).

*Authorable precedence via `DamagePipelineDef` if future variants are desired.*

---

## 3) Time System (RTwP hooks)

- **GCD** (global cooldown), **Cast**, **Channel**, **Recovery**.  
- **Haste** multiplicatively reduces GCD/Cast/Channel/Recovery with soft caps to avoid <300ms GCDs (linted).  
- **Animation locks** optional; **Pause** never alters outcomes.

---

## 4) Guard / Block / Poise

- **Guard%**: multiplicative reduction applied **before** Block. Comes from stances, wards, formations.  
- **Block**: chance to reduce damage by **BlockPower** (flat or percent) after Guard.  
- **Poise**: threshold against **InterruptPower**; if exceeded, **Stagger** applies (short GCD lock or cast cancel).

---

## 5) Status, Stacking & Dispels (summary)

- **Stacking**: per-status `stacks_max`, `stack_behavior: additive|refresh|independent`.  
- **Tick cadence**: `tick_ms` per status; damage/heal scales with snapshot (on apply) or dynamic (every tick) per `scaling_mode`.  
- **Dispels**: schools (curse, poison, hex, wound); `dispel_power` vs `resist`.  
- **On-reapply**: configurable (refresh duration, add stack, convert to empowered variant).

(Full details in `Docs/Combat/Status-and-Dispels-v0.1.md`.)

---

## 6) UI / Transparency

- **Character Sheet** shows base + gear + stance + buffs breakdown for each derived stat.  
- **Ability Tooltips** preview damage after attacker multipliers and **vs selected target’s** current Armor/Resists; show status odds and durations.  
- **Compare Gear** surfaces deltas (+HP, −Evasion, +Crit) and any cap warnings.

---

## 7) Integrations

- **AbilityDef** may reference formulas (e.g., `coeff: 0.8*STR + 0.6*WeaponDamage`).  
- **ItemDef** applies stat mods and sockets; **StanceDef** multiplies derived outputs.  
- **GambitDef** conditions read derived stats (`ally_hp_lt`, `enemy_casting`, `has_block_window`).  
- **Jobs** unlock battle abilities that ride this math (Guardian/Warden/Healer/Ranger).

---

## 8) Acceptance (v0.1)

- Primary + derived stat model documented with formulas.  
- Damage pipeline order of ops clear with caps/floors referenced in Numbers doc.  
- Status stacking/dispels rules standardized.  
- Tooltips & sheets show transparent breakdowns.  
- All shapes defined in the **schemas addendum**; values in **Numbers** are tunable; validations guard extremes.

