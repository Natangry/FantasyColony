# Schemas Addendum — Localization & Assets (v0.1)
*Additive to canvas **Modding Data Schemas v0.1 (Draft)**.*

## Localization

### LocalizationKeyDef@1 (metadata for editor; strings live in per-locale JSON)
```yaml
id: core.Loc.Ability.CinderSlash.Name
schema: LocalizationKeyDef@1
key: "loc.ability.cinder_slash.name"
params_any:
  - { name: "n", type: "number", style: "plural" }
  - { name: "faction", type: "string" }
notes: "Displayed on the ability button & tooltip title."
```

### LocalePackDef@1 (packaging reference)
```yaml
id: mymod.Locale.en-US
schema: LocalePackDef@1
locale: "en-US"
file: "localization/en-US.json"
```

### FieldWidgetDef — locKey
```yaml
id: core.Widget.LocKey
schema: FieldWidgetDef@1
type: "locKey"
behavior:
  on_unresolved:
    prompt_create: true
    default_domain: "ability"
```

## Assets

### AtlasDef@1
```yaml
id: mymod.Atlas.UI
schema: AtlasDef@1
image: "atlas/ui_atlas.png"
entries:
  - { id: "icons/abilities/cinder_slash", x: 0, y: 0, w: 64, h: 64 }
  - { id: "frames/panel_9slice", x: 64, y: 0, w: 96, h: 96, nine_slice: { l: 24, r: 24, t: 24, b: 24 } }
```

### SpriteSheetDef@1
```yaml
id: mymod.Sheet.Spider
schema: SpriteSheetDef@1
source: "sprites/spider.png"
grid: { w: 48, h: 48, cols: 6, rows: 4 }
animations:
  - { id: "walk", frames: [0,1,2,3,4,5], fps: 12, loop: true }
  - { id: "attack", frames: [6,7,8,9], fps: 10, loop: false }
```

### AnimationClipDef@1
```yaml
id: mymod.Anim.Spider.Attack
schema: AnimationClipDef@1
sheet: mymod.Sheet.Spider
frames: [6,7,8,9]
fps: 10
loop: false
```

### NineSliceDef@1
```yaml
id: mymod.UI.PanelNineSlice
schema: NineSliceDef@1
atlas: mymod.Atlas.UI
entry: "frames/panel_9slice"
borders: { l: 24, r: 24, t: 24, b: 24 }
```

### PaletteDef@1 (optional)
```yaml
id: mymod.Palette.Spider.Venom
schema: PaletteDef@1
remap:
  "#4B3A2E": "#2E4B3A"
  "#D6C3A5": "#A5C3D6"
```

### FontDef@1
```yaml
id: mymod.Font.UISans
schema: FontDef@1
file: "fonts/ui_sans_sdf.asset"
fallback_any: ["core.Font.FallbackCJK"]
size_px: 32
```

### FieldWidgetDef — asset pickers
```yaml
id: core.Widget.Asset.Icon
schema: FieldWidgetDef@1
type: "asset"
accepts_any: ["png","svg"]
behavior:
  on_unresolved:
    import_prompt: true
    place_into: "icons/"
```

```yaml
id: core.Widget.Asset.AtlasEntry
schema: FieldWidgetDef@1
type: "atlasEntry"
expects: AtlasDef@1
behavior:
  on_unresolved:
    open_atlas_builder: true
```

## Validation
- **Localization:** keys must match namespace regex; params consistent across locales; ICU syntax parseable.
- **Assets:** referenced path or atlas entry must exist; nine-slice borders within bounds; animation frames valid.
