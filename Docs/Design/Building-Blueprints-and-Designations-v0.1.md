# Building Blueprints, Designations & Functional Requirements (v0.1, Docs Only)

## Goal
Plan → Build → Operate:
- Player **blueprints** walls, doors, floors, rooms, and places furniture/stations.
- Selecting a **building** lets you set **designations** (tags like **Bar**, **Workshop**, **Inn**, **Shop**, **Chapel**).
- Each designation shows a live **requirements checklist** and **operational state**: Closed · Limited · Open · Excellent.
- Designations emit **workforce shifts** (Bartender, Artisan, Innkeep, etc). All data is moddable.

---

## Player Flow

1) **Blueprint**
   Choose layer (Structure / Surface / Room / Furniture & Stations / Service Points / Utilities / Decor). Paint ghosts, preview cost.

2) **Build**
   **BoM** (bill of materials) generated → reserve → haul → construct → quality roll. Undo/clone tools supported.

3) **Designate**
   Select any enclosed room or multi-room complex → assign **designation tags** (Bar, Workshop, Inn, Shop, Chapel…). Multiple tags allowed if compatible (e.g., Inn + Tavern).

4) **Operate**
   The **Building Inspector** shows **requirements**, **throughput**, **open hours**, **staffing**, **stock links**, and an **Auto-place suggestions** button.

---

## Building Inspector (UI)
- **Header:** Name (auto or custom), tag chips, Operational badge (Closed/Limited/Open/Excellent).
- **Requirements Panel:** Mandatory vs Optional. Live checkmarks & counts (e.g., “Tables: 2/2”, “Bar Counter: missing 1”).
- **Staffing:** Emits shift requests (e.g., **PM Bartender**). Shows assigned pawns.
- **Stock Links:** Source/consumer hints (e.g., “Kitchen within 16 tiles or connected room network”).
- **Perks:** Optional items (stage, reliquary, display cases) push **Open → Excellent**.
- **Buttons:** Auto-place suggestions · Toggle Open Hours · Rename · Copy layout blueprint.

---

## Operational States
- **Closed:** Missing any **mandatory** requirement.
- **Limited:** Mandatory met, but a key optional is missing (reduced service or menu).
- **Open:** All mandatory met; normal service.
- **Excellent:** Open + all **perk** requirements met (small auras/bonuses).

---

## Designation Examples (first pass)

### BAR (Tavern)
- **Allowed room roles:** `Tavern`
- **Mandatory:** `1× BarCounter(ServicePoint)`, `≥2 Tables`, `≥1 Seat per Table`, `Open Hours: PM`.
- **Stock/Adjacency:** Access to **Kitchen** (same room network OR within **16 tiles** via door path).
- **Staff:** **1× Bartender** (PM); optional **1× Cook** (shared with Kitchen).
- **Perks (Excellent):** Stage (music), wall racks (drinks).

### WORKSHOP
- **Allowed room roles:** `Workshop`
- **Mandatory:** `≥1 Workbench/Lathe/Anvil`, storage within **12 tiles**.
- **Staff:** **1× Artisan** (AM/PM shifts).
- **Perks:** Tool rack, lamp, tidy floor → small work speed bonus.

### INN
- **Allowed roles:** `Inn`
- **Mandatory:** `1× Reception(ServicePoint)`, `≥2 Guest Beds`, **Tavern OR Kitchen** access.
- **Staff:** **1× Innkeep** (PM).
- **Perks:** Washstand per 2 rooms; décor → guest morale.

### SHOP
- **Allowed roles:** `Market`
- **Mandatory:** `1× ShopTill(ServicePoint)`, `≥2 Display Racks`.
- **Staff:** **1× Clerk (Scribe/Artisan)** any shift.
- **Perks:** Signboard, measuring stand → small barter bonus.

### CHAPEL
- **Allowed roles:** `Chapel`
- **Mandatory:** `1× Altar(ServicePoint)`, `≥4 Seats`.
- **Staff:** **1× Acolyte/Healer** during ritual.
- **Perks:** Reliquary, choir dais → stronger Resolve buff.

---

## Storage Designations (NEW)

### PANTRY
- **Allowed roles:** `Pantry`
- **Mandatory:** `≥2 Containers (Crate/Barrel/Sack)` with **filters food.\***; **Room tag:** `room.pantry`.
- **Perks:** Shelving + lighting grants small **freshness ×0.95** aura.

### GRANARY
- **Allowed roles:** `Granary`
- **Mandatory:** `≥2 Sacks (grain/flour/seed)`, **Sealed** room tag.
- **Perks:** Raised floor reduces **rodent spoilage events**.

### COLD CELLAR
- **Allowed roles:** `ColdCellar`
- **Mandatory:** `room.cold_cellar` tag; `≥2 Barrels` or **Cold Racks**.
- **Effect:** Decay multipliers per Numbers; freezing halts decay.

---

## Staffing Integration (Workforce)
- Each **BuildingDesignation** may define **ShiftNeeds** (Bar → Bartender PM; Shop → Clerk).
- Storage designations **do not** require staff, but can emit **haul requests** when buffer policies fail.

---

## Auto-Place Suggestions
- If requirements unmet, the inspector can **ghost** missing items in valid spots (player approves to add to BoM).

---

## Modding Surfaces
- `BuildingDesignationDef`, `RequirementDef`, `ServicePointDef`, `OperationalRuleDef`, `BoMRuleDef`, `StockRequirementDef`, `AdjacencyRequirementDef`.

## Validation Rules
- Every designation must have ≥1 **mandatory** requirement.
- Radii/adjacency positive; service points inside designated rooms.
- Staff requirements must map to a valid **workforce** job track.

---

## Acceptance (v0.1)
- Player can designate Bar/Workshop/Inn/Shop/Chapel and Pantry/Granary/Cold Cellar.
- Inspector shows requirements and status; **Auto-place suggestions** ghosts missing items.
- Designations emit **shift needs** as applicable.

