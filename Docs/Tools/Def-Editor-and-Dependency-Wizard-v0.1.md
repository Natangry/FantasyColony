# Definition Editor & Dependency Wizard (v0.1, Docs Only)

**Intent:** A schema-driven, in-game editor for all data *Defs* with inline linting, clone/derive, dependency graph, **and** a wizard that creates missing referenced defs on the spot. Targets your current **Modding Data Schemas v0.1 (Draft)** (canvas) and extends it additively.

---

## 1) Core Concepts

- **Schema-Driven Forms:** The editor reads Def schemas (field types, enums, ranges, `DefRef` annotations) and auto-renders controls. Adding new schemas automatically adds new forms.
- **Dependency Wizard:** When a field expects a Def (e.g., `FactionDefId`) and the id you enter doesn’t exist, the editor prompts to **create it now** using a minimal template, then links it back.
- **Clone / Derive:** Create a new Def from any existing one; choose **Copy** or **Patch** (overlay only differences).
- **Validation & Lints:** Realtime validation with severities (Error/Warning/Info) pulled from `Validation-Severity-and-Lints.md`; one-click “Fix” for common issues (e.g., **Add missing vendor hint** for alive-only capture quests).
- **Preview & Hot-Reload:** Save to a writable Mod and preview behavior: tooltip renders, loot/price rolls, encounter sandbox.
- **Undo/Redo & Snapshots:** Per-Def history with diffs and timestamps.
- **Packaging:** Mods declare dependencies, versions, load order, and patch operations.

---

## 2) UX Flows

### A) Edit Existing Def
- Open Def → schema-rendered form → inline hints & errors → **Save** writes YAML with normalized ordering; hot-reload previews update.

### B) Create From Baseline
- **New from Baseline…** → pick a source Def → choose “Copy” (full) or “Patch” (overlay) → wizard picks a new `id` (modid prefix), rewrites refs, prompts for required fields.

### C) Fill Missing Reference (the big feature)
- In any `DefRef` field (e.g., `issuer_faction`), choose existing **or** type `core.Faction.Goblins`. If not found → **Prompt:** “Create FactionDef `core.Faction.Goblins`?” → opens **Faction** mini-form (name, law profile, colors, mutators). On save, returns to the parent and resolves the ref.

### D) Validation & Fix-ups
- Inline chips: **Error** blocks save; **Warning** suggests patches (e.g., add `can_be_captured: true` to boss if alive-only quest). Click “Fix” to open a guided patch or auto-insert defaults.

### E) Graph & Search
- **References** panel: “Used by…”, “References…”; click to jump. Full-text and tag filter across all defs.

### F) Preview & Playground
- **Ability Preview:** cast time/GCD/tooltip; test in a micro arena.
- **Economy Preview:** vendor price breakdown; haggling odds.
- **Quest Preview:** card + objective chain, capture paths, TTLs.
- **Settlement Preview:** chosen template + applied mutator hooks listing.

---

## 3) Saves, Mods & Load Order

- **Mod Folder Layout:**  
  `mods/<modid>/defs/*.yaml` • `mods/<modid>/localization/*.json` • `mods/<modid>/icons/*` • `mods/<modid>/about.yaml`
- **Load Order:** Core → DLCs → Mods by order; **PatchDef** applies last by priority.
- **Snapshots:** `mods/<modid>/.history/<defid>/<timestamp>.yaml` for undo/redo.

---

## 4) Safety & Permissions

- **Core Lock:** Editing `core.*` requires **Patch** overlay unless `dev_mode` true.
- **Refs Must Resolve:** Save is blocked if any `DefRef` remains unresolved; wizard can stub with `TODO` but lints remain **Error** until resolved.
- **Migrations:** If schema version bumps, show **Migrate** button running `MigrationDef` steps.

---

## 5) Integrations

- **Validation:** Uses `Docs/Design/Validation-Severity-and-Lints.md` severities; editor shows reasons & links to docs.
- **Director & Signals:** Special pickers for **Metric/Condition/Rule** with previews (trend, thresholds).
- **Dialogue:** Localization key picker + phrasebook preview.

---

## 6) Acceptance (v0.1)

- Open/edit/clone any Def with schema-driven forms.
- Enter a non-existent `DefRef` → wizard prompts to create the right Def type, then links it.
- Inline validation prevents broken saves; previews render correctly.
- Saves land in a mod folder; hot-reload reflects changes.

