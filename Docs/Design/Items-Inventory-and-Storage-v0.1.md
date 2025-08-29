# Items, Inventory, Storage & Spoilage (v0.1, Docs Only)

**Goal:** Lock a mod-first backbone for how **things** work: item shapes, stacks, weight/volume, carry & reservations, stockpiles & containers, and how perishables decay with temperature and rooms. This underpins food, tools, crafting, buildings, and hauling AI.

> Schema shapes live in `Docs/Design/Modding-Data-Schemas-v0.1-Addendum-Items-and-Storage.md`. This addendum **extends** the current “Modding Data Schemas v0.1 (Draft)” document you edited in canvas; it does not overwrite it.

---

## 1) Item Model

**Categories & tags**
- `food.*`, `material.*`, `tool.*`, `medicine.*`, `component.*`, `furniture_kit.*`, `quest.*`
- Ingredient matching is **tag-based** (e.g., `starch.any`, `veg.any`, `meat.any`) and reuses `RecipeDef` style.

**Core fields (data)**
- `stack_size` (max in one stack), `weight` (per unit), `volume` (per unit)
- Optional: `quality`, `durability`, `perishable` (with `spoilage_model`), `affixes`

**Examples**
- `material.wood.log` (stack 20, weight 1.0, volume 1.0)
- `food.meal.simple_stew` (stack 10, weight 0.3, perishable: yes)
- `tool.pick.iron.common` (quality: Common, durability: 120)

---

## 2) Inventory & Carry

- **Carry Cap** derives from Might and gear; **encumbrance bands** impact Move and Stamina drain.
- **Smart Bundling**: Pawns plan pickup routes to fill remaining carry weight/volume for nearby deliveries.
- **Reservation Tokens**: Stacks and container **slots** can be reserved to prevent haul fights (per-claim TTL).

**Encumbrance bands (defaults)**
- ≤ **50%**: no penalty
- **50–100%**: Move −10%, Stamina drain +10%
- **>100%**: slow-walk; hauling-only actions until relieved

---

## 3) Storage: Stockpiles & Containers

**Stockpile Zones**
- Filters by `tag/id/quality/perishable`, priority (Low/Normal/High), **pull radius**, and “Accept Rotten”.
- Presets: Pantry, Medicine, Tools, Raw Materials, Finished Goods.

**Containers**
- **Crate/Barrel/Sack/Rack** define capacity (by slots and/or volume), access points, and optional **insulation/freshness** modifiers.
- Special containers: **Tool Rack** (ties to Step 6), **Medicine Cabinet**, **Display Rack** (Shop), **Cold Cellar** (room-bound container network).

**Room tags that modify storage**
- `room.sealed`, `room.insulated`, `room.cold_cellar`, `room.pantry` apply multipliers to decay or capacity auras.

---

## 4) Spoilage & Freshness

**Stages**
- **Fresh → Stale → Spoiled → Rotted**
- Stage effects are **item-defined** (e.g., meals lose buffs at Stale; eating Spoiled causes **“Queasy”**; Rotted becomes waste/fertilizer).

**Decay drivers**
- **Ambient Temperature**, **Container modifiers**, **Room tags**, and **Base half-life** on the item.
- **Freezing** halts decay but may apply a **quality penalty on thaw** for some foods.

**Colony policies**
- **Rationing** can temporarily allow **Stale** meals for non-guard roles; **Never Spoiled** is enforced unless forced by survival events.

---

## 5) Hauling Flow (pickup → reserve → haul → store)

1) **Order** emits a target stack or container slot.
2) **Reserve** source stack & destination slot (tokens).
3) **Pickup** bundles until carry cap or route end.
4) **Deliver** to container/stockpile matching filters & priority.
5) **Overflow**: If destination full, fallback to next allowed container/zone by priority.

**Debug UI**
- Per job: show reservations, sources/destinations, and why a move failed (filter mismatch, full, pathing).

---

## 6) UI Surfaces

- **Item Tooltip**: freshness bar, stack/weight, tags, storage advice (best container/room).
- **Container Panel**: capacity, contents list, temperature and freshness modifier, filter preview.
- **Stockpile Panel**: filter chips, pull radius, priority, “Accept Rotten”.
- **Overlays**: Storage heatmap (fill %), Spoilage risk (hot/cold), Haul routes.

---

## 7) Interactions with Other Systems

- **Needs & Meals**: Meal **freshness** scales buffs/debuffs (see Numbers & Needs doc).
- **Tools**: **Tool Rack** is a `ContainerDef` with pickup radius; auto-assign chooses the **best quality** within range.
- **Buildings**: **Pantry/Granary/Cold Cellar** are designations that require certain containers/room tags.

---

## 8) Acceptance (v0.1 docs)

- Clear item shape (stack/weight/volume/perishable) and storage model.
- Encumbrance math & bundling documented with first-pass numbers.
- Spoilage rules with temperature/container/room multipliers.
- Stockpile filters and container capacities defined.
- Cross-links added to Needs, Tools, and Buildings docs.

