# Tiered Needs & Fallbacks (v0.1, Docs Only)

**Goal:** Every core need has an **always-available fallback** (works in a pinch but applies a **debuff**) and **progressive upgrades** (require better stations/ingredients/skills/rooms, grant **buffs**). Policies/Auto-Actions can temporarily prefer fallbacks.

## Master Table (Overview)

| Need | Fallback (always available) | Tier I | Tier II | Tier III |
|---|---|---|---|---|
| **Food** | **Gruel** (water + any starch) → **Heavy Belly** (−10% Might, −5% Work Speed, −5% Resolve regen, 4h) | **Simple Stew** (+Small Morale, 4h) | **Bread & Stew** (+Work Speed +3%, 6h) | **Hearty Meal** (+Work Speed +5%, +Grace +1, 8h); **Festive Feast** (team Morale + Resolve burst) |
| **Rest/Comfort** | **Ground Sleep** → **Sore** (−10% Grace, −5% Work Speed, until noon) | **Bedroll** (neutral) | **Bed** (+Comfort +1, +Inspiration tick) | **Comfortable Room** (+Work Speed +3%, +Inspiration +1) |
| **Hygiene** | **Cold Wash Barrel** → **Chill** (−2% Move for 30m) | **Warm Washstand** (infection chance ↓ small) | **Bathhouse** (infection ↓, +Inspiration +1) | **Perfumed Baths** (+Social buff, +Inspiration +2) |
| **Safety** | **Huddle at Warding Totem** (small radius) → **Shelter Anxiety** (−Focus, −Work Radius) | **Palings / Torches** (threat ↓ small) | **Ward Tower** (+Resolve regen in radius) | **Fortified Perimeter** (+Focus +1, +Inspiration near walls) |
| **Medicine** | **Makeshift Bandage** (cloth) → **Scar Risk +** | **Poultice** (heal over time) | **Tincture/Salves** (stronger, infection ↓) | **Physicker Care** (quality beds + Healer) |
| **Recreation/Morale** | **Storytime Solo** (tiny mood bump) | **Group Meal (Tavern)** (+Social) | **Music Night** (+Morale, relationships) | **Festival** (+Morale big, clears Overworked stacks) |
| **Heat/Cold** | **Campfire Warmth** (fuel hungry) | **Hearth** (room aura) | **Stove + Insulation** | **Radiant Lattice** (late) |
| **Spiritual** | **Quiet Focus** (minor Resolve) | **Shrine** | **Chapel** (+Resolve regen) | **Rituals** (strong Resolve/morale) |

> **Design intent:** Fallback debuffs mainly hit **physical output/mobility**, not just mood, so they’re tactically viable but clearly worse than proper solutions.

---

## Food Detail

- **Fallback:** **Gruel** *(recipe flagged `fallback: true`)*  
  **Inputs:** water + any starch (berries/roots/grain). **Station:** none (cookfire optional).  
  **Effects:** **Heavy Belly** (−10% Might, −5% Work Speed, −5% Resolve regen) for **4h**; removes *Starving*.  
  **Auto-Action:** if **Meals Buffer < 0.5 day**, toggle **Cook Gruel** until buffer reaches **2 days** (then disable).

- **Upgrades:**  
  - **Simple Stew:** *(Kitchen)* Small Morale buff (4h).  
  - **Bread & Stew:** *(Kitchen + Mill/Carpentry)* +3% Work Speed (6h).  
  - **Hearty Meal:** *(Cook ≥ Journeyman, quality ingredients)* +5% Work Speed, +1 Grace (8h).  
  - **Festive Feast:** *(Tavern + Festival)* Team Morale surge; clears **Overworked** stacks by 1.

- **Variety:** Repeating the same Tier ≥ I meal > 4 times in 2 days applies **Meal Boredom** (−1 Morale) until variety resets.

### Meal Freshness (NEW)

- Meals track **Freshness**: **Fresh → Stale → Spoiled → Rotted** (see Items & Numbers).
- **Buff scaling:** Fresh = 100%; **Stale = −50%** to meal buffs; Spoiled = no buffs + chance **“Queasy”**; Rotted = inedible.
- **Storage advice:** Pantry and Cold Cellar slow decay; freezers halt but may reduce quality on thaw.

## Rest/Comfort Detail

- **Fallback:** **Ground Sleep** anywhere → **Sore** (−10% Grace, −5% Work Speed) until noon.  
- **Upgrades:** **Bedroll** (neutral) → **Bed** (+Comfort +1) → **Comfortable Room** (+Work Speed +3%, Inspiration +1).

## Hygiene Detail

- **Fallback:** **Cold Wash Barrel** (craftable anywhere) → **Chill** (−2% Move, 30m).  
- **Upgrades:** **Warm Washstand** → **Bathhouse** (infection risk ↓, Inspiration +1) → **Perfumed Baths** (+Social buff).

## Safety Detail

- **Fallback:** **Huddle at Warding Totem** (small radius) reduces fear but lowers **Work Radius** and Focus.  
- **Upgrades:** **Palings/Torches** → **Ward Tower** → **Fortified Perimeter**.

## Medicine Detail

- **Fallback:** **Makeshift Bandage** consumes cloth, stops bleeding, but adds **Scar Risk**.  
- **Upgrades:** **Poultice** → **Tincture/Salves** → **Physicker Care** (quality beds + Healer bonuses).

## Recreation/Morale Detail

- **Fallback:** **Storytime Solo** (tiny Morale).  
- **Upgrades:** **Tavern Meal** (+Social), **Music Night**, **Festival** (big Morale; clears Overworked stacks).

## Heat/Cold Detail

- **Fallback:** **Campfire Warmth** zones (high fuel).  
- **Upgrades:** **Hearth** → **Stove + Insulation** → **Radiant Lattice**.

## Spiritual Detail

- **Fallback:** **Quiet Focus** (minor Resolve).  
- **Upgrades:** **Shrine** → **Chapel** → **Rituals**.

---

## Policies & Auto-Actions

- **Rationing (Policy):** Prefer **fallback** recipes/items when buffers low; lifts when buffers recover.  
- **Festival (Policy/Event):** Counts as Free Time and clears **Overworked** stacks.  
- **Hygiene Priority:** If **Infection Risk > threshold**, auto-suggest **Warm Washstand**.

---

## Modding Surfaces
- `MealTierDef`, `NeedDef`, `NeedSatisfactionDef` (`fallback: true|false`), `BuffDef`, `DebuffDef`, `ComfortSourceDef`, `HygieneSourceDef`, `AutoActionDef`
- **Freshness hooks** via `SpoilageModelDef` (see Items addendum) to scale buffs.

