# Settlement Visit Scenes & Services (v0.1, Docs Only)
**Goal:** Screen-swap settlements that are procedurally composed and **react to Mutators & World Conditions** (tags, aftershocks, season, prosperity, faction law, route closures). 100% data-driven.

> Shapes live in `Docs/Design/Modding-Data-Schemas-v0.1-Addendum-Settlements-and-Services.md`. Numbers in `Docs/Design/Numbers-Settlements-Law-and-Festivals-v0.1.md`. This extends the canvas **Modding Data Schemas v0.1 (Draft)** you last edited—no overwrites.

---

## 1) Scene Generation

**SettlementSceneTemplateDef** → picks a **LotSet** and spawns buildings per **BuildingSpawnRule**. The template receives **Mutator & WorldCondition deltas** that bias lot counts, substitute buildings, and add service modules.

- **Lot types:** civic (chapel, stockade, guildhall), commercial (inn, tavern, general, blacksmith), markets (stalls), housing, special (festival grounds, soul looms, plague tents).
- **Mutator & Condition inputs:** SettlementMutators, Region/Route Tags (Aftershocks), season, prosperity, dominant faction, law profile, active events.

### Examples
- **Corrupt Regime:** +Black Market stall; −Town Watch guard count; +Bribe desk in Stockade; vendor price +5%; contraband allowed.
- **Plagued:** +Clinic & Quarantine tents; gate screenings; curfew after dusk; price −10% for herbs, +10% for luxuries; chapel sermons more frequent.
- **Soul Looms (Aftershock):** add **Soul Loomer** vendor, unique blueprint stock; rumor lines reference recent “looming”.
- **Bridge Down (route tag):** town map flips to a ferry lot variant; caravan arrival rate −; trade board posts “bridge repair” quests.

---

## 2) Services (Modules)

Each **ServiceModule** (Inn, Bar, General Store, Blacksmith, Chapel, Clinic, Workshop/Guildhall, Bounty Board, Quest Board, Stockade/Jail, Bank, Festival Grounds) exposes:
- **Integrations:** VendorProfile, BountyContract, CustodyFacility, Blueprint unlocks, LawProfile hooks, Caravan schedules.
- **Mutator/Condition Modifiers:** e.g., Plagued Clinic opens “screenings” interaction; Corrupt Chapel sells indulgence slips; Festival Grounds add mini-games & buffs.

---

## 3) Population & Schedules

**PopulationProfile** spawns archetypes (guard, clerk, priest, smith, reveler) with **open hours**. Mutators modify counts and behavior:
- Corrupt: guards patrol lazily, bribe-friendly; Black Market NPCs appear at night.
- Plagued: more medics/wardens, masked townsfolk; curfew toggles after dusk.
- Prosperity high: more stalls, finer interiors; low: boarded lots & fewer services.

---

## 4) Law, Crime & Suspicion

**LawEnforcementProfile** + **CrimeRules** + **AlertLevels** govern response times, contraband checks, and curfews. **Disguise items** and **SuspicionModel** allow stealth shopping or illicit trades.

Mutator examples:
- Corrupt → tolls at gates, bribe dialog on arrest.
- Plagued → quarantine radius; jail capacity repurposed as sick bay; jailbreaks trigger plague spread risk.

---

## 5) Events & Festivals

**SettlementEvents** (market day, harvest fest, sermons, plague screenings):
- Affect prices/stock/rep and spawn micro-quests.
- Conditioned by season, prosperity, and tags (e.g., Night Webs → spider warding rite).

---

## 6) Rumors & Boards

**RumorTable** pulls from world tags/aftershocks to seed leads (“Night Webs on East Road”).  
**QuestBoard** respects per-region caps and **paused-until-visit**.

---

## 7) UI/UX

- **Visit HUD:** minimap with service icons + open/closed, law banner (tariffs/curfew), **Aftershock chips**.
- **Service panels:** Buy/Sell/Blueprints/Bounty/Parole/Recruit; haggling; law notices; mutator tooltips.
- **Wanted banner** if your crime heat is high.

---

## 8) Acceptance
- Settlement scene composition clearly changes under mutators/conditions (lots, interiors, services, NPC counts, law).
- Vendor stock/prices, quest boards, and law enforcement reflect world state.
- UI shows which mutators/conditions are active and why the town looks this way.

