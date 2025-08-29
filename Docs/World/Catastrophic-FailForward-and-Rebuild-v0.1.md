# Catastrophic Fail-Forward & Rebuild Onramp (v0.1, Docs Only)

**Goal:** When a **major quest** fails, we **fail forward**: the old settlement becomes a **Ruin Site**, the party is **scattered** into survivor states, the world **advances** (mutators/tags/aftermath escalate), and the player gets a **Rebuild Onramp** that avoids slog via rep-based sponsorships and starter kits.

> Shapes live in `Docs/Design/Modding-Data-Schemas-v0.1-Addendum-FailForward-and-Rebuild.md`. Numbers live in `Docs/Design/Numbers-FailForward-and-Rebuild-v0.1.md`. This extends the canvas **Modding Data Schemas v0.1 (Draft)** without overwriting it.

---

## A) Core Flow

1) **Major quest fails** → invoke a `CatastropheProfile` keyed to quest tier/path.
2) **Shatter & Scatter**:
   - Old settlement → **Ruin Site POI** (lootable, dangerous).
   - Each pawn rolls a **Survivor State**: *Alive*, *Injured*, *Captured*, *Missing*, or *Dead* (tier- & trait-weighted).
   - Scattered placements (region/POI/settlement) create **Rescue Hooks**.
3) **World Advances**:
   - Saga/quest **Consequence Meter** jumps.
   - Related mutators escalate; routes/law/encounter weights adjust.
4) **Rebuild Onramp**:
   - Player awakens as one pawn at a safe **Rebuild Start**.
   - Choose an **OutpostStartProfile** (Builder/Defender/Healer kits).
   - Spend **Reputation Credits** for discounts, stock boosts, and a **sponsored caravan**.
5) **Reunion Arc**:
   - Director seeds **RescueChain** quests to recover pawns/gear.
   - **Ruin Site** can be cleared for legacy materials & memorial items.
6) **Stabilize**:
   - Temporary **Resilience Mode** buffs (build/clean/repair) last a few in-game days, then taper off.

---

## B) Survivor States

- **Alive (Free):** can path to rendezvous; low risk.
- **Injured:** requires healing/escort; time-boxed camp checks.
- **Captured:** creates a **Bounty/Prison** objective at a nearby faction site.
- **Missing:** spawns **Rumor/Track** notes; leads expire and refresh.
- **Dead:** places a **Grave Marker**; unlocks memorial options and grief lines.

Scatter is biased to last-visited regions, faction ties, active quests, and personal backstory entries.

---

## C) World Consequences

- **Mutator escalation:** e.g., *Evil Leader* consolidates control; *Night Webs* spreads to adjacent routes.
- **Tags & Law:** curfews, tolls, patrol density changes; faction celebrations, crackdowns, or mourning events.
- **Aftershocks:** “Hunt the Survivors”, “Loot the Ruins”, “Refugee Flow” chains.

---

## D) Rebuild Onramp

**OutpostStartProfiles** (choose one):
- **Builder’s Kit:** bench, lumber, basic tools, 3 beds, 3d food.
- **Defender’s Kit:** guard shack, ward totem, basic arms/armor.
- **Healer’s Kit:** clinic cot, herbs/bandages; injury recovery boost.

**Reputation Credits** convert existing faction rep to:
- **Build/tax/tariff discounts** for 7d.
- **Vendor stock boosts** & price nudges.
- **Sponsored caravan** on Day 1 with essentials.

**Legacy Carryover:** keep **blueprints**, partial **map reveal**, **camp tech**, and **quest knowledge**; retain a portion of coin.

---

## E) Reunion & Ruin

- **RescueChain** per Captured/Missing pawn: lead → encounter → safe return (sane timers).
- **Ruin Site**: debris caches, ambush chance, legacy keepsakes; clearing yields **Legacy Tokens** that unlock a **Memorial** building.

---

## F) UI/UX

- **Shattered Colony** splash: casualty summary, survivor states, world chips with TTLs.
- **Rebuild Picker**: choose start kit & start region; shows sponsor effects.
- **Reunion Ledger**: survivors list with pins, timers, and approach options.
- **Memorial UI**: inscription, aura radius, moodlets.

---

## G) Acceptance (v0.1)

- Major-quest fail creates Ruin Site, sets survivor states & scatter placements.
- Consequence meter advances; related mutators/tags/law adjust.
- Player receives rebuild package & rep credits; sponsor caravan arrives.
- Rescue chains and ruin clearing enable reunion & recovery.
- All structures authored via data; no engine code implied.
