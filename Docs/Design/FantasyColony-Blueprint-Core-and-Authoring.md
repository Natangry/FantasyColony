# FantasyColony — Core & Authoring Blueprint

**Status:** Adopted
**Scope:** Engine architecture, XML data model, in-game Creator UX, Mods/Asset Packs, Conflict & Snapshot handling, One-Def Wizard, folder discipline, guardrails.

---

## 1) Vision

- **Mod-first, XML-first.** Stable string IDs, load order + patching, validation, and migrations.
- **Deterministic sim.** One fixed-step clock; event-driven systems; stable saves.
- **Creator-friendly.** An in-game editor that looks like XML, validates as you go, and can package changes into local mods.

---

## 2) Engine Shape (Spine + Rings)

- **Lifecycle stages:** **Boot → Mods → Defs → World → UI**.
- **Core services (C#):** Clock (fixed-step), EventBus, DefRegistry, SaveService.
- **Domain systems (C#):** Build, AI, Needs, Economy, etc. Each owns its sim state and communicates via events.
- **Adapters (C#):** UI/Input/VFX/SFX — no game rules here.

**Rules:** one clock; one scheduler; events in/out; exclusive state ownership; schema-driven data; versioned saves.

---

## 3) Data Model: XML vs C#

- **C# = how** (algorithms/behaviors): AI actions, pathfinding, damage, cooldowns.
- **XML = what** (content/config): Monsters, Brains, Abilities, Items/Buildings, LootTables, Factions, Spawns, Visuals, SoundSets.
- **Patching:** `<Add>/<Set>/<Remove>/<Merge>`; schema + validation to prevent drift.

**IDs:** `prefix.family.name` (e.g., `core.brain.melee_aggressive`).

---

## 4) AI Model (Brains, Actions, Abilities)

- **Actions (C#):** reusable behavior patterns (e.g., `Chase`, `Flee`, `Kite`, `Flank`, `UseAbility`, `GuardAlly`, `MoveToPoint`, `Idle`).

* * One class per **action type**; referenced by **Action Registry** (string ID → class).
    +- **Abilities (XML):** effects/moves (range, cooldown, damage, tags, VFX/SFX). Executed by generic **UseAbility** action.
    +- **BrainDefs (XML):** generic archetypes (e.g., `melee_aggressive`, `ranged_aggressive`) listing candidate actions, base weights, parameters, and **stat-gated rules**.
*

**Baseline rule:**
- A **BrainDef must ensure at least one ability path**.
- A **Monster’s Abilities list is optional** and treated as **extras**.
- Validation enforces: Monster must have BrainRef; Brain must have a usable baseline.

**Per-monster variety:** Prefer keeping the same Brain and adding abilities as monster-specific extras (no new BrainDef required).

---

## 5) In-Game Creator (Authoring Tool)

**XML-ish token UI:** fields render like `<Field>`; `<`/`>` cycle values; click edits; lists pick. Unknown references trigger **“Create that Def”** mini-wizards.

**Validation (live):** types, required fields, ID rules, ranges, reference resolution, load-order conflicts.

**Changes Panel:** shows Created/Modified/Patch/Delete with **Open / Diff / Revert / Remove from Pack**.

**Conflict Resolver:** when two packs touch the same Def ID, pick winners by **load order** (reorder or prefer). Preview the **effective Def**.

**Create Asset Pack (mod):** bundles selected changes into a local pack:
- Choose **Patch Pack** (recommended) or **Fork**.
- Writes new XML and patch XML + `mod.xml` (id/version/deps) and copies assets.

**Update Local Defs / Create Local Copy:**
- Local packs edit in place.
- Read-only packs prompt **Create Local Copy** → default to **Patch Pack** (tiny diffs), or **Fork** (full copies).

---

## 6) Visuals & Assets (also XML)

- **VisualDef** stays **separate** from gameplay defs for clean reuse/skin mods.
- Creator **feels inline** on Item/Building screens, but **Save** writes **two files**: gameplay Def + linked **VisualDef** (`<VisualRef id="...">`).
- Assets copied under the mod: `/Art/{Sprites,VFX,Audio}`.

**Sprite UX:** minimal 3-view (front/side/back), **Stretch to Footprint** default, pivot/PPU, preview.
**Ability FX tab:** per-ability VFX/SFX for Cast/Travel/Impact.
**Monster SFX tab:** roar/hurt/death/footsteps.

---

## 7) Startup Flow & Stability

**Intro screen:** enable/disable asset packs (mods), set load order, open Creator.

**Auto Reload Content:** whenever returning from Creator or changing mods:
- Rescan enabled packs, apply order, apply patches.
- Rebuild DefRegistry + Asset Index, show change summary.

**World Setup:**
1) **Asset Pack Selection** (covers all XML):

* * Pack-level toggles.
* * Family toggles (gameplay vs visuals).
* * Optional per-Def winner/skin picks with dependency-aware checks (no broken refs).
    +2) **Content Snapshot**:
* * Save **enabled packs + order**, **per-pack/per-def selections**, and a **canonical resolved XML** set with per-family + global checksums, plus versions.
* * Worlds **load with their snapshot** by default. If current content differs: **Add / Ignore / Review / Add Only Safe** (visual/text only).
*

---

## 8) Removing, Isolating, and Repairing Content

**Mask, don’t nuke.** Per-Def toggles mask content instead of deleting files.

**One-Def Wizard (general-purpose):** launch from Selection/Changes/Conflicts/Creator to **Extract / Repair / Isolate / Transplant**:
- **Scope:** Single / Minimal Closure / Extended Closure / Batch.
- **Closure rules & invariants:**

* * Monster: must have BrainRef; Brain baseline ensures ability path; Visual with footprint; Faction; Loot optional.
* * Brain: must have at least one ability path.
* * Ability: valid action & numbers.
* * Visual: sprite or placeholder + footprint.
* * Loot/Spawn: lists not empty.
    +- **Per-dep policy:** **Carry** (keep referencing source), **Copy** (fork into local pack), **Synthesize** (placeholder).
    +- **Auto-repairs:** default melee, placeholder visuals (stretch to footprint), neutral faction, test spawn.
    +- **Output:** Patch Pack (recommended) or Fork; snapshot updates accordingly.
*

**Removal flows** always show impacted refs and provide one-click fixes.

---

## 9) Folder Discipline

**Engine (C#):**
`
Assets/Scripts/Core/                # Clock/EventBus/Save/Config
Assets/Scripts/Sim/AI/Actions/      # behavior bricks
Assets/Scripts/Sim/AI/Brain/        # brain loop + config
Assets/Scripts/Sim/AI/Registry/     # action registry
... keep Defs/, Rendering/, Build/, Editor/
`

**Content (per mod):**
```
Assets/StreamingAssets/Mods/<pack>/

* Defs/
* Monsters/ Brains/ Abilities/ Items/ Buildings/
* LootTables/ Factions/ Spawns/ Visuals/
* Art/
* Sprites/ VFX/ Audio/
* mod.xml
  ```
*

---

## 10) Validation Rules (high-level)

- **MonsterDef:** BrainRef **required**; Abilities list **optional** (extras).
- **BrainDef:** must guarantee a **baseline ability path**.
- **AbilityDef:** action type + numbers valid; optional FX/SFX.
- **VisualDef:** at least one sprite (or placeholder) and a valid footprint.
- **Loot/Spawn:** lists not empty; references must exist.
- **IDs:** `prefix.family.name` unique within load order.
- **Patching:** all refs resolved post-patch.

---

## 11) Safe vs Risky Updates (Snapshot Guidance)

- **Safe-ish (often OK mid-campaign):** purely visual/audio/icon/text changes.
- **Risky:** stat changes (HP/damage), Brain baseline, Ability numbers, LootTables, Spawns, **Visual footprint**.

**Add Only Safe** in Snapshot applies visuals/audio/text; skips gameplay changes unless explicitly accepted.

---

## 12) Defaults & Guardrails

- Small, reusable **action** set; most variety via XML params/stats/tags.
- Do **not** invent new tags in content; extend schema deliberately.
- **Patch-first** when modifying external/core content.
- Snapshot + checksums prevent silent drift.

---

## 13) Glossary

- **Asset Pack (Mod):** a foldered set of XML defs + referenced art/audio with a `mod.xml` manifest.
- **Patch Pack:** a mod that changes other mods via patch XML instead of full copies.
- **Snapshot:** frozen, canonical XML + metadata stored per world to ensure stability.
- **One-Def Wizard:** extractor/repair tool that isolates or fixes content with safe defaults.

---

## 14) Change Log

- v1 — Initial blueprint (architecture, authoring, snapshot, wizard, folder discipline).
