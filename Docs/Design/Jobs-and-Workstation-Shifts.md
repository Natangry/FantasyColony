# Colony Jobs & Workstation Shifts — Design Spec v0.1
Distinct from per-task priorities: pawns take a **daily job** (levels into **abilities**/**passives**). Only **workforce jobs** are scheduled via **station shifts** (AM/PM/Night). Other activities are autonomous: **Training** (guided by Character Info), **Maintenance/Logistics**, **Study**, **Scouting**, **Forage-lite**, **Social**. Free Time is enforced; Overtime is punishing.

## Workforce vs Autonomous
- Workforce (schedulable): Harvester, Artisan, Cook, Miner, Logger, Warden, Healer, Bartender, Scribe. Guardian can be schedulable only under **Night Watch**.
- Autonomous: Training, Maintenance, Logistics, Study, Scouting, Forage-lite, Social; Guardian patrols when not under Night Watch.

## Stations Own Shifts
- Shift blocks (AM 06–14, PM 14–22, Night 22–06).
- Allowed jobs per shift; requested slots; break windows to guarantee Free Time.
- Assign Workers UI: Auto (Plan), Pinned, Reserve, Training; throughput forecast & conflict warnings.

## Training & Secondary (Autonomous)
- **Character Info → Combat Training**: Offense/Defense/Support sliders, weapon focus, drills. When idle, pawns follow these.
- Mentorship pairs add XP/quality.

## Plans & Policies
- Plan presets: Balanced/Growth/Defense/Festival (workforce mixes only).
- Policies: Night Watch, Lockdown, Overtime, Rationing.

## Tool Requirements & Loaners (NEW)
- Many workforce jobs/stations declare **Tool Class** requirements (e.g., Miner → **Pick**, Cook → **Chef’s Kit**).
- If no physical tool meeting **min quality** is within pickup radius, the pawn uses a **Loaner** (always available, penalties applied).
- **Tool Rack** stores tools; auto-assignment chooses **best available** within **12 tiles**; warnings appear on the Assignment Board and Station panels.
- See `Docs/Design/Tools-and-Equipment-v0.1.md` for tiers, durability, and repair details.

## Acceptance
- Farm (AM) + Tend Bar (PM) via station shifts.
- Non-workforce activities cannot be scheduled; pawns train/secondary automatically.
- Free Time violations produce Overworked → Exhausted; festivals clear stacks.

