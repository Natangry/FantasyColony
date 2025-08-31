# Naming & Folders (Authoring & Game)

This document defines the canonical folder structure and naming rules for a **game-first** layout. XML lives under `Assets/defs/` (not StreamingAssets; no Mods/ prefix). Visuals are separate but mirrored, and pixel art loads via `Resources` for MVP.

---

## 1) XML roots (deep, type-first)

```
Assets/defs/
  object/
    pawn/
      monster/            # MonsterDef XML
  item/                   # ItemDef XML
  building/               # BuildingDef XML
  brain/                  # BrainDef XML
  ability/                # AbilityDef XML
  loot/
    tables/               # LootTableDef XML
  faction/                # FactionDef XML
  spawn/                  # Spawn lists/rules XML
  visuals/                # VisualDef XML (separate, mirrored nesting)
    object/
      pawn/
        monster/
    item/
    building/
```

---

## 2) Art roots (MVP via Resources)

```
Assets/Resources/
  sprites/
    object/pawn/monster/<name>/{front.png,side.png,back.png}
    item/<name>/{front.png,side.png,back.png}
    building/<name>/{front.png,side.png,back.png}
  vfx/
    ability/<name>/{cast.prefab,impact.prefab}
  audio/
    monster/<name>/{roar.ogg,hurt.ogg,death.ogg}
    ability/<name>/{cast.ogg,impact.ogg}
```

XML stores **virtual paths only** (no `Assets/`), e.g. `spritePath="sprites/object/pawn/monster/goblin/front"`. Works with `Resources.Load` now; later we can swap to Addressables without changing XML.

---

## 3) IDs & filenames (stable)

- **IDs:** `game.<family>[.<type>].<name>`
  * Examples: `game.monster.goblin`, `game.visual.monster.goblin`, `game.building.construction_board`
  * **Filenames = IDs** (`<id>.xml`) placed in the matching folder:
    * `Assets/defs/object/pawn/monster/game.monster.goblin.xml`
    * `Assets/defs/visuals/object/pawn/monster/game.visual.monster.goblin.xml`
  * **Visual variants:** `@variant` suffix + art subfolder:
    * `game.visual.monster.goblin@bronze.xml`
    * `Resources/sprites/object/pawn/monster/goblin/bronze/front.png`

---

## 4) Validation guardrails

- Filename must equal ID (plus `.xml`).
- VisualDef: ≥1 sprite path resolves; footprint present.
- Loot/Spawn: lists not empty; refs exist.
- Post-patch (later): all refs resolve.

---

## 5) Quick examples

**Goblin (Monster & Visual)**
```
Assets/defs/object/pawn/monster/game.monster.goblin.xml
Assets/defs/visuals/object/pawn/monster/game.visual.monster.goblin.xml
Assets/Resources/sprites/object/pawn/monster/goblin/{front.png,side.png,back.png}
```

**Construction Board (Building & Visual)**
```
Assets/defs/object/building/game.building.construction_board.xml
Assets/defs/visuals/building/game.visual.building.construction_board.xml
Assets/Resources/sprites/building/construction_board/{front.png,side.png,back.png}
```

---

## 6) Why this works

- Clear, deep, human-friendly folders.
- Gameplay vs visuals decoupled for fewer conflicts.
- Same virtual paths work now (Resources) and later (Addressables).

