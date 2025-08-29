# Ethos (Good ↔ Evil), Class Aspects, and Multi-Path Quests (v0.1, Docs Only)

**Goal:** Add a moddable **Ethos** axis (Good↔Evil) that:
1) shifts per-pawn morality via choices and deeds,
2) unlocks **Class Aspects** (virtue/vice mutators that alter abilities & passives),
3) gives major quests **Good / Evil / Neutral** resolutions with distinct rewards & consequences.

> Schemas live in `Docs/Design/Modding-Data-Schemas-v0.1-Addendum-Ethos-Classes-and-QuestPaths.md`. Extends the canvas file **Modding Data Schemas v0.1 (Draft)** without overwriting it.

---

## 1) Ethos Axis (per pawn)
- **Scale:** `−100 .. +100` with bands:
  - *Wicked ≤ −60*, *Dark-leaning −20..−59*, *Neutral −19..+19*, *Good-leaning +20..+59*, *Virtuous ≥ +60*.
- **Sources:** quest path outcomes, dialogue/settlement deeds (bribe vs donate), combat conduct (spare/execute), faction contracts, saga epilogues.
- **Signals:** moodlets (*Guilt, Pride, Doubt*), **DataJournal** facts (“Spared the Hunter”), relationship nudges based on values, optional **Conscience Drift** storyteller dial (slow move toward Neutral).

## 2) Class Aspects (virtue/vice mutators)
Ethos unlocks **aspects** that morph class kits (abilities & passives) and optional visuals.
- **Guardian**
  - *Virtuous — Ward Knight:* small protective aura; **Aegis Smite** (sanctity burst vs undead/fiends).
  - *Wicked — Dread Warden:* fear pulse on Taunt; **Soul Shackles** (brief root + lifedrain).
- **Healer**
  - *Sanctifier:* cleanses grant short Resolve; **Sanctify** zone.
  - *Leech:* offensive drains heal self; **Hemophage** (convert HoT → self sustain).
- **Ranger**
  - *Beastfriend:* pacify chance ↑; **Mark of Mercy** (non-lethal shot improves capture).
  - *Poacher:* trophy yield ↑; **Hamstring** (bleed + harvest bonus).
*(…similar pairs for Warden, Scholar, Artisan, etc.)*

Small **stat/aura** tweaks (±1–3%) keep identity without raw DPS creep. Some aspects adjust **Gambit biases** (e.g., Virtuous Healer prefers cleanse before leech).

## 3) Multi-Path Quests (Good/Evil/Neutral)
Each major template defines **paths** with different objectives, rules, and aftermaths:
- **Good:** rescue/capture, dispel, donate, reconcile, sanctify.
- **Evil:** execute, extort, corrupt, seize, desecrate.
- **Neutral/Pragmatic:** expose, broker deal, redirect, stand aside.

**Rewards/Consequences:** items/blueprints, **EthosDelta**, faction standing shifts (per-faction multipliers), **WorldEffects**, tag mutations, actor movement. All paths are **fail-forward** compatible.

**UI:** Quest card shows path options with ethos icons, previewed rewards & world changes; tooltips clarify time/cost constraints.

## 4) Director / Sagas / Factions
- **Director** can bias offered paths by region culture, active mutators (e.g., Corrupt), or party average Ethos.
- **Sagas** branch by **dominant path record** over the arc (merciful chain vs domination chain).
- **Factions** apply **rep multipliers** for path types (Order likes Good, Pirates like Evil, Merchants prefer Neutral).

## 5) Dialogue & Social
- Tokens: `{pawn.ethos}`, `{ethos.band}`, `{class_aspect.name}`, `{quest.path}`.
- Phrasebooks for **judgment**, **redemption**, **intimidation**, **pragmatism**; companions react per **Values**.

## 6) Worked Example — “Track the Hunter”
- **Good (Capture):** Subdue, deliver for trial → **Ethos +8**, town rep +, short **Safer Roads** effect.
- **Evil (Execute & Seize Methods):** Kill, learn traps → **Ethos −8**, town rep −, bandit rep +, unlock **Trained Spiders** recipe; Director may seed *Fear Tax* overlay.
- **Neutral (Expose & Leave):** Reveal identity; hunter flees → small Ethos ±0..±2, modest rep shifts, spawns **Chase** follow-up.

## 7) Numbers (first pass)
- **Ethos deltas:** minor ±2..4; quest path ±6..10; saga milestones ±15..25.
- **Aspect thresholds:** unlock at **±60**; previews at **±20**.
- **Balance guardrails:** aspect passives stay within **±3%**; morphs trade utility vs flat DPS.
