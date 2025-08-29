# Colony Gameplay Loop & Pacing (Phase 1 Foundation)

## Player Verbs
Build · Zone · Assign · Bills · Trade · Research · Ritual/Festival · Policy · Assume Control · Overlays

## Minute→Daily→Weekly→Seasonal
- Minute: place/haul/build; bills fire; needs tick; micro tweaks.
- Daily: schedule blocks; maintenance; logistics; mood economy.
- Weekly: production milestones; visitors/caravans; threat probes.
- Seasonal: climate shifts; story beats; medium goals.

## Construction, Bills, Stockpiles, Upkeep
- **Blueprint Flow:** Ghost → reserve materials → haul → build → quality roll.
- **Bills:** Per-station queues with **conditions** (until X, maintain Y in stock, while tag active). Bills can be **paused** by policy or shortage.
- **Stockpiles:** Filters by tags/material; **pull radius**; spoilage & temperature rules.
- **Upkeep:** Power/fuel/wards consume per-tick; shortages raise alerts and auto-pause dependent bills.

## Policies & Alerts
- **Normal / Night Watch / Lockdown / Festival**.
- **Rationing:** Prefer **fallbacks** (e.g., **Gruel**) when buffers low; disable automatically when buffers recover.
- Alerts: Soft (suggestions) vs Hard (blockers). Examples: Food < 1 day, Beds < pawns, Wards low, Unrepaired breach.
- **Auto-Actions:** Optional toggles: auto-cook **Gruel** at Food < 0.5 day (until 2 days), auto-burn refuse when Infestation risk > threshold, auto-prompt **Hygiene** upgrades when infection risk high.

## Risk & Recovery
- **Soft Failures:** Low food → hunger moodlets; low comfort → slower work; injuries → reduced move/work; filth → disease risk.
- **Recovery Tools:** Emergency recipes (**Gruel**), triage beds, morale events (storytime), **Festival** (clears **Overworked**), rationing, rebuild queue.
- **Fail-Forward:** Temporary debuffs and event downgrades instead of hard fail wherever possible.

## KPIs (Design Targets)
- **First bed** < 4 min; **campfire** < 3 min; **first meal** by Day 1 dusk.
- **Granary + Workshop** by Day 6–7 with average play.
- Average **haul path** < 25 tiles for key chains.
- ≤ **3 simultaneous red alerts** in early game on default storyteller.

> See also: `Docs/Design/Needs-and-Fallbacks-v0.1.md` and `Docs/Design/Numbers-Needs-v0.1.md`.
