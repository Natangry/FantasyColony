# Mod Packaging & Load Order (v0.1)
> **Update:** Localization & Assets layout. (XML-first authoring)

## Layout
- **Defs:** `mods/<modid>/defs/**/*.xml`
- **Localization:** `mods/<modid>/localization/<locale>.json` (ICU strings)
- **Icons:** `mods/<modid>/icons/*`
- **Sprites:** `mods/<modid>/sprites/*` (raw sheets)
- **Atlas:** `mods/<modid>/atlas/*` (packed PNGs + AtlasDef)
- **Fonts:** `mods/<modid>/fonts/*`
- **About:** `mods/<modid>/about.xml`

## Import/Export
- **Localization:** CSV/XLIFF round‑trip; keep key stability; renames update refs.
- **Assets:** Drag‑drop import; auto‑thumbs to `.thumbs/`.
- **Defs:** Editor and CLI tools write XML with normalized attribute ordering to minimize diff churn.

## Load Order
- Same as before; packs can override localization keys/assets by path, but prefer **PatchDef** for Def changes.
- The editor warns on path overrides and visualizes effective load order.
- Mods declare dependencies (`requires`), versions (`version`), and optional patch operations in their `about.xml`.

## Notes
- Def filenames mirror their canonical IDs (`<id>.xml`), organized by schema‑specific folders.
- Auto‑generated XML artifacts live in the repo (`XML_INDEX.md`, `XML_SNAPSHOT.txt`, `Docs/Templates/Defs/*.xml`).

