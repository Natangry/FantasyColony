# Naming & Folders (Authoring & Mods)

This document defines the canonical folder structure and naming rules for **systems-first** content authoring. It keeps gameplay data cleanly separated from visuals while staying friendly for modders and skin packs.

---

## 1) Per-mod layout (deep, type-first)

```
Mods/<pack>/
  defs/
    object/
      pawn/
        monster/                # MonsterDef XML (e.g., goblin)
      item/                     # ItemDef XML
      building/                 # BuildingDef XML
    brain/                      # BrainDef XML
    ability/                    # AbilityDef XML
    loot/
      tables/                   # LootTableDef XML
    faction/                    # FactionDef XML
    spawn/                      # Spawn lists/rules XML
  visuals/                      # VisualDef XML (separate from gameplay defs)
    object/
      pawn/
        monster/
    item/
    building/
  art/                          # Pack-local assets (sprites/vfx/audio)
    sprites/
      object/pawn/monster/<monster_name>/{front.png,side.png,back.png}
      item/<item_name>/{front.png,side.png,back.png}
      building/<building_name>/{front.png,side.png,back.png}
    vfx/
      ability/<ability_name>/{cast.prefab,impact.prefab}
    audio/
      monster/<monster_name>/{roar.ogg,hurt.ogg,death.ogg}
      ability/<ability_name>/{cast.ogg,impact.ogg}
  mod.xml
```

**Why:** Mirrors your taxonomy (`object/pawn/monster`) and keeps **VisualDef** separate for clean skinning and fewer patch conflicts. Art paths match visuals for zero guesswork.

---

## 2) IDs & filenames (stable & searchable)

- **ID format:** `<pack>.<family>[.<type>].<name>[.@variant]`
  * Monster: `core.monster.goblin`
  * Visual (monster): `core.visual.monster.goblin`
  * Building: `core.building.construction_board`
  * Visual variant: `core.visual.monster.goblin@bronze`

- **Filenames mirror IDs** (plus `.xml`) and live in the matching folder:
  * `Mods/Core/defs/object/pawn/monster/core.monster.goblin.xml`
  * `Mods/Core/defs/visuals/object/pawn/monster/core.visual.monster.goblin.xml`
  * `Mods/Core/defs/visuals/object/pawn/monster/core.visual.monster.goblin@bronze.xml`

- **Patch files** use the same base with `.patch.xml`:
  * `core.monster.goblin.patch.xml`
  * `core.visual.monster.goblin@bronze.patch.xml`

**Benefits:** filename == ID makes search, diffs, and patches trivial.

---

## 3) Visuals are separate, but equally nested

Visuals live under `defs/visuals/...` in the same deep nesting as gameplay defs. Example:

```
Mods/Core/defs/object/pawn/monster/core.monster.goblin.xml
Mods/Core/defs/visuals/object/pawn/monster/core.visual.monster.goblin.xml
```

This keeps gameplay vs visuals decoupled (skin packs don’t collide with balance mods) while staying easy to find.

---

## 4) Pixel art & load paths

**Recommended (future-ready):** ship sprites/audio inside the mod under `art/` and load via Addressables or bundles using the same path shape shown above.

**MVP (if using `Resources.Load` today):** mirror pack art under:
```
Assets/Resources/Mods/<pack>/sprites/object/pawn/monster/<monster_name>/front
```
and put the same relative path into the `VisualDef`. When you switch to Addressables, only the loader changes—paths stay the same.

---

## 5) Variants & skins

- Add `@variant` to the **VisualDef** ID and filename.
- Art goes in a variant subfolder:
```
Mods/Core/art/sprites/object/pawn/monster/goblin/bronze/front.png
```
- The editor should provide a **“Make Variant”** button that clones the base visual and rewires selected defs.

---

## 6) Validation guardrails

- **ID rules:** lowercase, `_` or `-` allowed, must be unique within load order.
- **VisualDef:** must reference at least one sprite (or placeholder) and a valid footprint.
- **Spawn/Loot:** lists not empty; refs exist.
- **Patch files:** post-patch references must resolve.

The in-game Creator runs these checks and shows friendly fixes.

---

## 7) Quick examples

**Goblin (Monster & Visual)**
```
Mods/Core/defs/object/pawn/monster/core.monster.goblin.xml
Mods/Core/defs/visuals/object/pawn/monster/core.visual.monster.goblin.xml
Mods/Core/art/sprites/object/pawn/monster/goblin/{front.png,side.png,back.png}
```

**Construction Board (Building & Visual)**
```
Mods/Core/defs/object/building/core.building.construction_board.xml
Mods/Core/defs/visuals/building/core.visual.building.construction_board.xml
Mods/Core/art/sprites/building/construction_board/{front.png,side.png,back.png}
```

---

## 8) Why this works long-term

- **Clarity:** Deep, human-friendly folders; ID = filename for instant search.
- **Separation:** Gameplay and visuals are decoupled to avoid mod conflicts.
- **Scalability:** Variants and new families slot in without changing rules.
- **Future-proof:** Same paths work whether you load via Resources now or Addressables later.
