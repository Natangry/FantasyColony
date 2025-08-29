# Logistics, Stockpiles, Hauling & Spoilage (v0.1, Docs Only)

**Intent:** Make material flow predictable and moddable from source → storage → stations → outputs, with **filters, links, reservations, batching, carts/pack animals, and cold chain**. Additive to the canvas **Modding Data Schemas v0.1 (Draft)** you last edited.

---

## A) Stockpiles, Containers & Filters

- **Stockpile Zones**: areas with **Filter Rules** (tags/material/quality/freshness/owner), **Priority (0–5)**, and **Pull Radius** (tiles).
- **Storage Containers**: shelves (stack size ↑), barrels/bins (category boosts), cold chests (freshness multiplier), display cases (beauty).
- **Input/Output Links**: a **Station** can *link* to one or more stockpiles/containers to fetch inputs and push outputs deterministically, overriding general priorities.

**UX**: Filter chips, priority badge, link lines on overlay. Container cards show capacity, stack rules, freshness multiplier.

---

## B) Reservations & Counters

- **Material Reservations**: when a bill/work order starts, inputs are reserved at the source to prevent double-fetch and tug-of-war.
- **Batch Reservations**: multiple orders can reserve partial stacks; haulers consolidate en route.
- **Virtual Counters**: colony-level counts by tag/material/quality/freshness feed “Maintain Y in stock” bills and Alerts.

---

## C) Hauling & Routes

- **Heuristics**: nearest viable **+ batch-pickup** (collect multiple parcels if along route) **+ smart drop** (closest consumer/linked output).
- **Floor Speed**: roads boost speed; mud/rough slow; doors/stairs add **Path Penalties**.
- **Carts & Pack Animals**: carts (road bonus, high carry), pack animals (mid-range routes). Routes are cached and invalidated on map changes.

---

## D) Spoilage & Temperature

- **SpoilageDef** drives phase changes: *Fresh → Stale → Spoiled → Rot*; a **temp curve** scales timers (+humidity optional).
- **Room Temperature**: base climate ± buildings (heaters/coolers), insulation, door leak; **Cold Rooms** apply freshness multipliers.
- **Cold Chain Rules**: thawing has a grace window; re-freeze penalties to prevent exploits.

---

## E) Station Pull/Push Policies

- **Pull Policy**: radius, preferred sources, min-stack, allow-floor? fetch-in-advance?.
- **Push Policy**: preferred outputs (by filter/priority), overflow fallback.
- **Maintain Y**: auto-haul/craft until a stock threshold is met (reads Virtual Counters).

---

## F) UI/Overlays & Alerts

- **Logistics Overlay**: route heat (recent traffic), blocked tiles, floor speed; **Stockpile Overlay**: filter chips & priority digits.
- **Spoilage Overlay**: freshness color + ETA labels; cold chain badges on containers.
- **Reservations View**: ghost labels on items (“Reserved by: Cook@Kitchen”).
- **Alerts**: “No cold storage”, “Haul backlog”, “Input link broken”, “Food < 1 day”, with one-click fix (e.g., create cold chest bill).

---

## G) Numbers (defaults)
- Stack sizes, carry weights, cart multipliers, floor speeds, spoilage half-lives by temperature, cooler/heater power draw, pack animal base speeds/capacity. See `Numbers-Logistics-Stockpiles-and-Spoilage-v0.1.md`.

---

## H) Integrations

- **Jobs**: Logistics as a Secondary (and/or dedicated Hauler); Maintenance resets links and repairs carts; Warden keeps cold rooms powered.
- **Alarm**: Pause low-priority hauling; carts auto-park at secure bays.
- **Director/Storyteller**: filth/spoilage can raise **Disease Risk**; raids may target high-value stockpiles or carts on roads.
- **Editor**: all filters/links/spoilage curves are editable; missing refs prompt creation (Dependency Wizard).
- **Validation**: broken links, unreachable sources, missing cold chain for perishables elevate warnings/errors (see Addendum).

---

## Acceptance (v0.1)

- Stockpiles/containers obey filters and priorities; stations resolve via links.
- Reservations prevent double-fetch; haulers batch smartly; carts/pack animals reduce churn.
- Perishables visibly change with temperature; overlays explain bottlenecks; alerts propose fixes.
- All shapes defined in schema addendum; numbers tunable; validations guard obvious pitfalls.
