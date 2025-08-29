# Localization Key Editor (v0.1, Docs Only)

**Intent:** Create, edit, and validate localization keys/strings at runtime, tightly integrated with the Definition Editor. If a Def references a `name_key`/`desc_key` that doesn’t exist, the editor prompts you to create the key (and translations) on the spot.

---
## Core Capabilities
- **Key lifecycle:** new key, edit text, rename (safe ref updates), deprecate, delete (warn on usage).
- **Namespacing:** enforce `loc.<domain>.<group>.<name>.(name|desc|… )` (see canvas “Modding Data Schemas v0.1 (Draft)”).
- **Parameterized strings:** ICU MessageFormat–style params (e.g., `{n, plural, one{# day} other{# days}}`, `{faction}`); per-param type hints.
- **Plural/Gender/Ordinal:** built-in forms per locale; preview toggles.
- **RTL & Script support:** bidi-aware preview; font fallback hints.
- **Search & fuzz:** full-text search, “unused keys”, “missing translations”, “near-duplicates”.
- **Import/Export:** CSV and XLIFF 1.2 for translators; round-trip diffs.
- **Hot-reload:** write to your mod’s `/localization/<locale>.json` and refresh UI.
- **Safe renames:** updates all Def references (`name_key`, `desc_key`, tooltip keys) in one operation.

---
## UX Flows
- **From a Def field:** typing a non-existent key ➜ *Prompt*: “Create localization key?” ➜ open mini-form (default text, params) ➜ save ➜ field resolves.
- **Bulk fixes:** “Generate missing keys from selected defs” (scans for empty `name_key/desc_key`).
- **Preview:** show rendered string for multiple locales with param playground.

---
## Validation
- **Error:** missing `name_key` referenced by any Def; placeholder mismatch between locales; duplicate key; invalid namespace.
- **Warning:** very long strings; untranslated locales; deprecated key in use.
- **Info:** inconsistent casing or punctuation; identical strings across locales (possible copy).

---
## Packaging
- Files per locale: `mods/<modid>/localization/<locale>.json`
- Optional translator notes block per key: `{ "_comment": "..." }`
