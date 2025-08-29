# Major Sagas — World-Scale Tags, Branching Questlines & Boss Arcs (v0.1, Docs Only)

**Goal:** Make “big arcs” first-class and fully data-driven. A **Major Saga** is a world-scale tag with states, a progressing **Saga Clock**, branching questlines, and a **multi-phase boss** whose resolution applies **world-altering effects**.

> Schemas live in `Docs/Design/Modding-Data-Schemas-v0.1-Addendum-Sagas.md`. This addendum **extends** the canvas “Modding Data Schemas v0.1 (Draft)” and does not overwrite it.

---

## 1) Major Saga Model

**State machine:** `Dormant → Stirring → Ascendant → Apex → Aftermath`

- **Dormant:** Seeds minor related tags/events (low pressure).
- **Stirring:** New rumors; minor quests; clock begins visibly.
- **Ascendant:** Regional systems change (e.g., roads threatened, weather shifts); mid-tier quests unlock.
- **Apex:** Boss lair revealed; boss quests unlock; strong world pressure.
- **Aftermath:** Resolution effects apply; decay/cleanup quests; optional epilogues.

**Saga Clock**
- Progress sources: time passage, Director rules, tag intensity trends, quest outcomes, actor proximity.
- Regress sources: counter-quests, rituals, allied tags, world effects.
- Multiple **branches** can share the same clock but alter **what** advances it.

**Synergy Mutations**
- Major Sagas can **absorb/branch** when near other tags (e.g., SpiderSaga + UndeadPresence → *Webs of Bone*).

---

## 2) Saga Arcs & Branching

**SagaArc** = milestones + branch gates:
- **Milestone**: quest hubs or POIs; unlocks when conditions met; can spawn multiple templates.
- **Branch**: conditionally choose path; **fail-forward** allowed (world changes but arc continues).

**Branch Drivers**
- Nearby tags (threat families), actors (Evil Hunter/Witch), season/biome, player’s previous outcomes (DataJournal facts), storyteller difficulty.

---

## 3) Boss Encounters (Data-Driven)

**BossEncounter**
- **Phases**: thresholds (HP %, timer, tag presence) change moves/arena.
- **Arena Modifiers**: cover/hazards, ritual pylons, weather locks.
- **Prep Breakers**: optional pre-quests that disable/soften phases.

---

## 4) World-Altering Rewards & Consequences

**WorldEffect**
- Regional: route safety, spawn tables, resource nodes, faction rep/ownership.
- Global: climate dial, encounter families, trade prices, storyteller bias.
- Effects can be **timed (TTL)** or **permanent** (until countered).

---

## 5) Worked Mini-Saga — *Queen of Webs*

**MajorWorldTag:** `SpiderSaga`
- **Dormant:** spider dens seed at low intensity; occasional silk bounties.
- **Stirring:** `BroodNest` mutations; **WebbedRoads** overlays on routes.
- **Ascendant (branches):**
  - *Webs of Bone* (Undead within ≤2 hops) → undead spiders; chapel rituals become key.
  - *Trained Packs* (Evil Hunter nearby) → ambush escorts; kill trainer to reduce boss phase adds.
  - *Giant Brood* (WitchRite active) → giant variants; break pylons to reduce boss HP threshold.
- **Apex:** **BossEncounter: Silk Queen** (egg-shield → tethered pull → enraged lunge).
- **Aftermath (success):** `SilkBoom` world effect (+silk nodes 7d, safer roads, prices ↓).  
  **Fail:** `Web Dominion` (roads taxed by webs, caravans ↓, prices ↑) until counter-saga triggered.

---

## 6) Ultimate Arcs (Elder Sagas)

Ship with 2–3 **Elder Sagas**:
- **Lichwinter:** creeping cold + undead empire; Apex boss **The Pale Regent**. *Success:* winters ease 2 seasons (global temp bias, disease ↓). *Fail:* perpetual chill, harvest windows −1 day.
- **Conqueror Crowned:** evil leader consolidates; borders move; taxes/tolls. *Success:* new allied patrols; trade routes flourish. *Fail:* tyrant regime (global law/order shifts).
- **Witchstorm** *(optional third)*: chaotic weather & hexed wildlife; boss **The Storm Matron**. *Success:* rare weather boons. *Fail:* lightning seasons and travel risk spikes.

---

## 7) UI/UX

- **Saga Journal:** shows current **state**, **clock**, visible **branches**, known **milestones**, and **consequences** preview.
- **Region Panel:** major tag badge; synergy chips; provenance of changes.
- **Dialogue/Rumor:** saga-aware phrasebooks (branch hints, apex warnings, epilogues).

---

## 8) Director Integration

- Rules can: advance/regress saga clocks, open/close branches, spawn boss lairs, apply world effects.
- Quest outcomes emit **clock deltas** and unlock **followup** rules.
- Safety valves: cap concurrent **Apex** states; bias resource/recovery quests if colony is weak.

---

## Acceptance (v0.1)

- Major saga states/clock/branches documented with examples.
- Boss encounters defined as multi-phase with arena hooks & prep breakers.
- World effects enumerated (regional/global) with TTL/permanent options.
- Director hooks for advancing/regressing sagas and branching outcomes.
