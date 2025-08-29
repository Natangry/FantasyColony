# New Game Flow, Starting Scenarios, and Oath/Burden/Keepsake (v0.1, Docs Only)

**Goal:** Lock a mod-first, data-driven **New Game → Worldgen → Start Site → Scenario → Party/Loadout** pipeline. Scenarios seed initial world tags, auto-blueprints, and tutorial hooks. Nothing here assumes engine code.

---

## UX Flow (screens & actions)

1) **Worldgen**
   - **Seed** (string or random), **Size** (S/M/L), **Biome Weights** (slider set), **Start World Tags** (optional presets).
   - **Advanced:** road density, river frequency, starting factions, encounter density.
   - **Preview**: small map with candidate start sites highlighted.

2) **Start Site**
   - Pick a tile: shows **Biome**, **Nearby POIs/Routes**, **Baseline Threat/Economy**, **Season**.
   - Warnings: “Short growing season”, “Bandit road nearby”, “Poor stone.”

3) **Starting Scenario**
   - Choose a **Scenario** (design-time bundles below). Shows **auto-blueprints**, **starting resources**, **party size**, **recommended storytellers**.

4) **Party & Loadout**
   - **Pawn Templates** (scenario-provided); roll/lock within **trait bands**; edit names/looks.
   - Assign **Oath** (colony ethos) & **Burden** (challenge). Each pawn selects **one Keepsake**.
   - Tools: baseline **Common** or **Loaners** per scenario; optional swap if inventory allows.

5) **Storyteller & Difficulty**
   - Story/Easy · Classic · Tempest (ties into ADR-0002 countdowns and caps).
   - Optional custom: death rules, injury severity, economy multipliers.

6) **Confirm & Start**
   - Summary: world seed, site, scenario, party, Oath/Burden, keepsakes, storyteller/difficulty.
   - Start drops you into **Day 0 – First Fires** (Phase-1 onboarding beats).

---

## Starting Scenarios (v0.1 set)

### Frontier Refugees (Default)
- **Party:** 2 colonists (Novice→Apprentice), identities skewed Cook/Harvester/Artisan.
- **Gear/Tools:** Common basics; 1–2 **Loaner** allowed.
- **Resources:** modest food, wood, cloth; 2 Repair Kits.
- **Auto-Blueprints:** Campfire, 2× Bedroll, 1× Stockpile zone.
- **World Tags:** +1 *Road Bandits* (regional), +1 *Forage Richness* (local).
- **Notes:** Aligns with Week-1 objectives.

### Vanguard Band
- **Party:** 4 colonists (Apprentice→Journeyman), mix Guardian/Ranger/Artisan/Cook.
- **Gear/Tools:** Some **Fine** tools; starter weapons.
- **Resources:** lean food, more metal/wood.
- **Auto-Blueprints:** Campfire, Tool Rack, small Workshop ghost.
- **World Tags:** +1 *Warden Crystal Vein*, +1 *Road Threat*.
- **Notes:** Higher pressure, earlier combat.

### Pilgrim Monastery
- **Party:** 3 colonists (Scholar/Healer/Cook focus).
- **Gear/Tools:** Chapel furnishings kit; limited weapons.
- **Resources:** herbs, cloth, light grain.
- **Auto-Blueprints:** Shrine, Wash Barrel, Dorm ghost.
- **World Tags:** +1 *Pilgrim Traffic*, −1 *Banditry* locally.
- **Notes:** Favors Hygiene/Spiritual tech path.

---

## Oath · Burden · Keepsake

**Oath (colony ethos, one choice)**
- **Hearthkeepers:** +1 Comfort cap in bedrooms; Festivals clear +1 Overworked.
- **Tinkerers:** Crafting work speed +5% in Workshops; +1 Tool Rack capacity auras.
- **Wardens of the Lattice:** Ward charge/regen +5%; patrol morale +1 at night.

**Burden (challenge, one choice)**
- **Lean Years:** Farm yields −20%; Forage richness +1 in Spring.
- **Roving Bandits:** Road encounter threat +1 tier; caravan frequency +10%.
- **Fragile Health:** Injury severity +10%; Healer potency +5%.
- **Harsh Winters:** Cold penalties start earlier; food spoilage slower in Winter.

**Keepsakes (each pawn picks one)**
- **Heirloom Tool:** Upgrades one starting tool to **Fine**; +3% work speed on that class.
- **Lucky Charm:** Once/day avoid a negative moodlet; +1 Inspiration on trigger.
- **Scout’s Compass:** Overworld move speed +5% for the party if bearer is present.
- **Old Recipe:** Unlocks **Simple Stew** and improves its morale by +1.

---

## Tutorial Hooks (Day 0–1)
- Place **Campfire** and **Bedrolls** → cook **6 meals** (Gruel fallback if buffer low).
- Build **Stockpile** and **Workbench** → craft **Repair Kits**, **Hatchet/Pick**.
- First **Station Shift** lesson: assign **Cook/Harvester**; **Free Time** notice.

---

## Modding Notes
- Scenarios, Oaths, Burdens, Keepsakes, and Storytellers are all **data** (`*Def`) and can be added or replaced by mods.
- Auto-Blueprints are pure data; no engine assumptions.

See: `Docs/Design/Modding-Data-Schemas-v0.1-Addendum-StartScenario.md`, `Docs/Design/Numbers-Start-v0.1.md`.
