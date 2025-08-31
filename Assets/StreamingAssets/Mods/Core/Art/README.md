# Art (per-mod assets)

Store mod-local assets alongside Defs:
- **Sprites/**  — PNGs, sprite sheets
- **VFX/**      — particle prefabs, flipbooks, decals
- **Audio/**    — OGG/WAV (converted at import)

VisualDefs in `/Defs/Visuals/` should reference files here by relative paths.

Recommended:
- predictable names derived from the Def ID
- keep source images reasonable in size to aid packing
