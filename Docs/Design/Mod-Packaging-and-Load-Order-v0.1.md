# Mod Packaging & Load Order (v0.1)
> **Update:** Localization & Assets layout.

## Layout
- **Defs:** `mods/<modid>/defs/*.yaml`
- **Localization:** `mods/<modid>/localization/<locale>.json` (ICU strings)
- **Icons:** `mods/<modid>/icons/*`
- **Sprites:** `mods/<modid>/sprites/*` (raw sheets)
- **Atlas:** `mods/<modid>/atlas/*` (packed PNGs + AtlasDef)
- **Fonts:** `mods/<modid>/fonts/*`
- **About:** `mods/<modid>/about.yaml`

## Import/Export
- Localization: CSV/XLIFF round-trip; keep key stability; renames update refs.
- Assets: drag-drop import; auto-thumbs to `.thumbs/`.

## Load Order
- Same as before; packs can override keys/assets by path, but prefer **PatchDef** for Def changes. Editor warns on path overrides.
