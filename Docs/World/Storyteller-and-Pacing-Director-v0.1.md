# Storyteller & Pacing Director (v0.1, Docs Only)

**Goal:** A moddable **Storyteller** that turns World Metrics/Conditions, Director Rules, raids, quests, festivals, caravans, and fail-forward into a **coherent rhythm**: tension rises → incident hits → respite → next beat. 100% data-driven and additive to the canvas **Modding Data Schemas v0.1 (Draft)**.

> Shapes live in `Docs/Design/Modding-Data-Schemas-v0.1-Addendum-Storyteller-and-Pacing.md`. Numbers in `Docs/Design/Numbers-Storyteller-and-Pacing-v0.1.md`.

---

## 1) Core Model

- **Tension (0–1)** per scope *(Global / Region / Colony / Party)* driven by metrics (Danger, Unrest, Prosperity inverse, Aftershock density, etc.) with EMA smoothing and hysteresis (Step 22).
- **Cadence Curve** per Storyteller controls gaps between incidents and required **Respite** after hard beats.
- **Incident Budget**: daily/weekly points spent on **beats** (quests, raids, festivals, caravans, rumors, aftershocks, social events).

### Beat Types
**Hook**, **Probe**, **QuestBeat**, **Festival/Social**, **Raid/Siege**, **Aftershock/Twist**, **Climax**, **Respite**.

---

## 2) Storyteller Profiles (examples)

- **Wayfarer** — exploration focus; favors Hook/Quest/Festival; soft raids.  
- **Warden** — defense focus; steadier probes, periodic sieges; generous respites.  
- **Oracle** — saga-forward; spiky curve; reserves budget for arc steps; allows dramatic swings.

Each profile sets: `CadenceCurve`, `TensionModel`, budget rates, bias weights, and safety rails (no double-catastrophes; minimum respite windows).

---

## 3) Scheduling Flow

1. **Update Tension** using metric inputs and recent outcomes.  
2. **Accrue Budget** (base + tension bonus − catastrophe tax + difficulty).  
3. **BeatSelector** proposes incidents ranked by fit: hit the curve target, diversify recent types, respect cooldowns/caps, focus current Region/Colony.  
4. **Spend & Schedule** winning beat via Director (`schedule_beat`), setting cooldowns and **Respite** if needed.  
5. **Transparency:** Story Panel shows **why now** (top drivers) and ETA band.

---

## 4) Respite & Safety Rails

- **RespitePolicy** guarantees soft beats (market day, visitors, festivals, social) after major hits for N days.  
- **Safety Rails:** cap simultaneous hard incidents, enforce raid cooldowns (Step 24), clamp max tension after fail-forward (Step 23).

---

## 5) Player Options

- **Difficulty** modifies budget rates, enemy scaling, cooldowns, and fail-forward severity bias.  
- **Storyteller Switcher** (mid-run allowed with a cooldown) re-seeds curves; shows clear effects.  
- **Respite Toggle** (on by default) ensures gentler windows.

---

## 6) UI/UX — Story Panel

- Storyteller portrait, **Tension bar** with trend arrow, **Next Beat ETA** band, **Last 3 Beats** history.  
- Tooltip: “Top Drivers” (e.g., Danger 0.74 ↑, Prosperity 0.32 ↓, Aftershock Density 2).  
- Difficulty/Profile selector with descriptive deltas.

---

## 7) Integrations

- **Director** gains predicates: `tension_gte/lte`, `budget_gte`, `beat_recent: type` and actions: `spend_budget`, `schedule_beat`.  
- **Raids & Alarm** (Step 24): raid scheduling passes through budget & cadence; `Imminent` beats may pre-trigger **Sound the Alarm**.  
- **Settlements** (Step 21): festivals preferred during Respite; law toggles during high tension.  
- **Fail-Forward** (Step 23): catastrophe clamps tension and grants respite credits.  
- **World Metrics** (Step 22): storyteller reads normalized metrics; configurable weights per profile.

---

## 8) Acceptance (v0.1)
- Tension/cadence model defined; budgets and beat costs documented.  
- 3 Storyteller profiles ship with distinct rhythms.  
- BeatSelector ranks incidents using diversity/cooldowns and scope focus.  
- Respite & safety rails prevent dogpiles; UI explains “why now.”  
- Schema addendum + numbers shipped, fully editable via the Definition Editor (Steps 25–26).

