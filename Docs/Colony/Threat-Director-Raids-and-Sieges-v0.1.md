# Colony Threat Director, Raids, Sieges & Base Defense (v0.1, Docs Only)

**Goal:** Scalable, moddable colony attacks with pacing, breach rules, defender flow, and aftermath. Integrated with **Sound the Alarm** for instant readiness.

## A) Threat Inputs & Scaling
- **ThreatBudget** from Region Danger, Wealth/Power, recent outcomes, Patrol density, active Conditions (Step 22).
- **Pacing:** cooldown bands, day/night windows, hysteresis to avoid thrash.
- **ThreatMix:** per-faction/mutator templates (Plagued → shamblers; Corrupt → heists; Night Webs → blooms).

## B) Raid/Siege Types
- Skirmish, Heist, Siege, Beast Bloom, Calamity (weather/arcane). Each with **Goals** (kill/kidnap/loot/sabotage/terror) and **Exit Rules** (morale/time/objective met).

## C) Breach & Destruction
- **BreachRule**: walls/doors/wards HP & resists; damage types (pick/acid/spell); ladders/ramps; fire & smoke spread.
- **Pathing & Targeting:** attackers probe weakest approach (cover, traps, ward coverage, path cost).

## D) Defender Flow (RTwP)
- **Alert States:** Normal → Watch → **Lockdown** (see Sound the Alarm).
- **Evac & Rally:** Civilians evacuate to **EvacZones**; defenders rally at **RallyPoints** with formation presets.
- **Night Watch:** auto-convert selected jobs to Guardian/Warden; Maintenance resets traps/repairs.

## E) Traps, Fortifications & Wards
- **TrapDef:** spike/snare/firepot/glyph; friendly-fire rules; reset jobs; charge/overload for wards.
- **FortificationDef:** barricades, crenellations, murder holes; cover/accuracy buffs.
- **WardGrid:** power/charges; Warden job interacts.

## F) AI Behaviors
- **RaidTactics:** flanks/feints/focused breach; **SiegePlan** phases (bombard → breach → push → extract).
- **Morale & Surrender** interop with Step 19; capture both ways feeds custody/bounties.

## G) Sound the Alarm (overview)
- One-click or hotkey action: **equip best combat loadouts**, **toggle wards/traps**, and **rally** defenders to a designated point; civilians **evacuate**. Fully data-driven; see dedicated spec: `Docs/Colony/Sound-the-Alarm-v0.1.md`.

## H) Aftermath & Recovery
- Loot/prisoner resolution; **Repair Orders** auto-queued; trauma moodlets; reprisal windows on the overworld; insurer/sponsor micro-aid.

## I) UI/UX
- **Defense Overlay:** wall HP, trap charge, ward coverage, breach predictions.
- **Raid Card:** type/force/ETA; phase timeline.
- **Alert Banner:** Sound the Alarm, Evacuate, Rally, Lockdown, Toggle Wards/Traps.

## J) Acceptance (v0.1)
- ThreatBudget & pacing rules; ≥4 raid/siege templates; breach/traps/wards specified.
- **Alarm** equips & rallies; evac/rally split works; UI flows defined.
