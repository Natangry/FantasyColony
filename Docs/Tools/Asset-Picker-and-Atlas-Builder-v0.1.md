# Asset Picker & Atlas/Sprite Tools (v0.1, Docs Only)

**Intent:** Pick or import icons, spritesheets, animations, and nine-slice frames directly from Def fields. If an asset path doesn’t exist, the editor offers to import or create the atlas/spritesheet entry, then links it back.

---
## Core Capabilities
- **Asset Picker widgets:** file chooser filtered by type (icon, sprite, atlas entry, audio, font).
- **Import Wizard:** drop PNG/SVG ➜ choose usage (icon/sprite/nine-slice/atlas) ➜ auto place in `mods/<modid>/icons|sprites|atlas`.
- **Atlas Builder:** define `AtlasDef` and `SpriteSheetDef` (grid or manual slices), pack, and preview.
- **Animation Clips:** author `AnimationClipDef` (fps, frames list, loops); live preview.
- **Nine-slice:** define borders, preview stretch; save `NineSliceDef`.
- **Palette swaps (optional):** `PaletteDef` with recolor tables; preview variants.
- **Hot-reload:** re-pack atlases as needed, update references.

---
## Validation
- **Error:** missing asset path referenced by a Def; atlas frame id not found; invalid slice bounds.
- **Warning:** very large textures; mismatched DPI; non-power-of-two atlas (if policy requires).
- **Info:** unused assets in mod; duplicate frames.

---
## Packaging & Conventions
- `mods/<modid>/icons/*.png|svg`
- `mods/<modid>/sprites/*.png` (raw sheets)
- `mods/<modid>/atlas/*.png` + `defs/AtlasDef.yaml`
- `mods/<modid>/fonts/*` (bitmap/SDF) and `FontDef` for mapping
- Thumbnails generated to `mods/<modid>/.thumbs/`
