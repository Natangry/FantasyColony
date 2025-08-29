# Progression v0.2 — Pawn Level, **Classes** (non-switch), Job XP & Combat Skills (Docs Only)

**What changed in v0.2:**  
- Added **Class Tracks** (can’t be switched).  
- Classes **evolve** when thresholds are met (branching by affinity/world state).  
- **Combat unlocks** moved from day-Jobs to **Classes**. Jobs now grant colony/off-combat benefits.  
- XP flows updated to include **ClassXP**.

---

## A) Buckets & Sources (now 4)
- **PawnXP** — global level from quests, milestones, a bit from everything.
- **ClassXP** — from **combat** (primary), quest climaxes, and small from class drills.
- **JobXP[track]** — from **work orders** (primary), mentorship, reduced training.
- **SkillXP[combat_skill]** — use-to-improve for weapon styles, schools, shield/tactics.

Anti-cheese: trivial-target DR, per-day training caps, repeat-action DR.

---

## B) Classes (non-switchable)
- **Choice Timing:** at pawn creation *or* first major combat (policy-driven). Until chosen, pawn is **Unclassed** with limited combat bar.
- **No Respec:** once chosen, class is permanent; only **evolution** changes it.
- **Base Class List (v0.2)**  
  *Martial:* **Squire → Guardian → Defender/Paladin/Blackguard***  
  *Ranged:* **Scout → Ranger → Sharpshooter/Beastmaster/Assassin**  
  *Arcane:* **Adept → Elementalist/Battlemage/Warlock**  
  *Divine:* **Acolyte → Cleric → Hierophant/Paladin***  
  *Warding:* **Warder** (battle wards/lines); (separate from day-job **Wardkeeper**)  
  \*Paladin/Blackguard branch requires **Good/Evil** affinity thresholds (see Step 20).

- **Class Rewards:** per level, unlock **combat abilities**, **class passives**, and small stat curves (e.g., Guardian → Guard/Block; Elementalist → Haste/INT).

---

## C) Evolutions (threshold-gated)
- **Evolve Rules** (examples):
  - **Squire → Guardian**: PawnLevel ≥ 5, Shield skill ≥ 20, completed “Hold the Gate” quest step.
  - **Guardian → Paladin**: Affinity **Good ≥ 60**, Holy school ≥ 15, Faction reputation (Church) ≥ Friendly.
  - **Guardian → Blackguard**: Affinity **Evil ≥ 60**, Void school ≥ 15, completed “Oath Broken” branch.
  - **Adept → Elementalist**: any two schools (Ember/Frost/Venom) ≥ 18; “Elemental Trial” completed.
  - **Scout → Ranger**: Bow ≥ 18 or Beast Bond ≥ 12; “Huntmaster’s Rite”.
- **World-aware:** evolutions can be locked/unlocked by **World Tags** (e.g., “Holy Schism”, “Void Storm”).
- **One-way:** evolving replaces the base class; you keep prior abilities unless flagged `replaced_by`.

---

## D) Day-Jobs vs Classes (clean split)
- **Jobs (daily assignment)** are **civilian/economic**: **Harvester, Artisan, Cook, Scholar, Wardkeeper (renamed), Infirmarian, Hunter (field)** etc.  
  Jobs grant **throughput/quality**, colony **policies**, **field actions** (non-combat), and **overworld buffs**.
- **Classes** grant **combat abilities & passives** that populate the **Ability/Magic** bars in RTwP.
- Former job-based combat unlocks moved to classes (see Migration).

---

## E) XP Distribution (updated defaults)
- **Combat action**: 60% → **ClassXP**, 30% → used **CombatSkill**, 10% → **PawnXP**.  
- **Work order complete**: 70% → **JobXP**, 20% → **PawnXP**, 10% → related **SkillXP** (if applicable).  
- **Training (class/job)**: 50% to the trained track, 20% → PawnXP; reduced daily caps.  
- **Quest turn-in / boss**: 60% → PawnXP, 30% → active ClassXP, 10% spread to top CombatSkills used.

**Catch-up:** mentorship, under-leveled multipliers, **Rested Inspiration** (+25% pool next day if needs met).

---

## F) UI/UX
- **Class Panel** on Character Sheet: class portrait, level, next rewards, evolution **requirements checklist** (live checkmarks).
- **Class Choice** popup when eligible; shows 2–3 candidates with previews and future evolutions.
- **Dusk Recap** adds **ClassXP gained** and “evolution progress” pips.

---

## G) Acceptance
- Pawns choose and **keep** a class; they can **evolve** by meeting thresholds.  
- Combat ability unlocks come from **Class Level** (not jobs).  
- Jobs still matter for the colony and provide non-combat perks.  
- XP flows, UI, and editor shapes support authoring & balancing.

