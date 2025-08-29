# RTwP Controls — Stances, Formations & Gizmo Bars (v0.1, Docs Only)

**Goal:** Finalize the player control layer for Real-Time-with-Pause: Stances, Formations, one-pawn control with **sub-bars**, and the mapping for Guard / Focus Fire / Retreat / Stance / Formation you requested.

> Schema shapes live in `Docs/Design/Modding-Data-Schemas-v0.1-Addendum-RTwP-Controls-and-AI.md`. This doc references the latest **Modding Data Schemas v0.1 (Draft)** in canvas and resolves its open questions for Stances, Formations, Gambits, and QuestTemplate requireds.

---

## 1) Stances (final)

**Stances:** `Offense`, `Balanced`, `Defense`. Swapping applies a tiny **GCD** and AI bias changes.

| Stance   | Damage | Guard | Block Window | AI Bias                  |
|----------|--------|-------|--------------|--------------------------|
| Offense  | +15%   | −10%  | −0.05s       | Engage sooner, chase more|
| Balanced | 0%     | 0%    | 0s           | Default                  |
| Defense  | −10%   | +15%  | +0.05s       | Hold ground, cover allies|

- **Swap GCD:** 300 ms (cannot chain stance swaps).
- **Retreat Curve Hook:** Retreat threshold reads **Behavior Params** and **Facets** (e.g., Boldness lowers retreat %).

---

## 2) Formations (final)

**Shapes:** `Line`, `Pike`, `Wedge`, `Loose`.  
Common fields: `anchor` (controlled pawn or rally flag), `radius_stay` (try to hold), `radius_break` (AI allowed to chase/evade).

- **Line** — spacing 1.5 tiles; **front guard +5%**; good lanes; weak to AoE.  
- **Pike** — spacing 1 tile; **braced vs charge** (stagger resist +10%); best with reach.  
- **Wedge** — spacing 1.25 tile; **on engage: penetration +5%**; flanking bonus behind tip.  
- **Loose** — spacing 2.25 tiles; **AoE damage −10%**; ranged prefers this.

Defaults: `radius_stay = 6 tiles`, `radius_break = 8 tiles`. Formation swap warm-up 400 ms.

---

## 3) One Controlled Pawn + Gizmo Bar with Single Sub-Bar

- Only **one pawn** is player-controlled at a time (**Assume Control**).
- **Base bar:** `Move`, `Attack`, `Ability`, `Magic`, `Item`, `AI`.
- Clicking a base gizmo opens **a single sub-bar above it** (icons from tag queries); ESC or re-click closes. Only **one sub-bar** can be open at a time.

**Integrations you asked for**

- **Guard → Ability** sub-bar shows context actions (Guard ally, Guard area).  
- **Focus Fire → Attack** sub-bar adds **Focus Target** toggle (party AI obeys target).  
- **Retreat → Move** sub-bar exposes **Retreat To Rally**, **Fall Back Path**.  
- **Stance & Formation → Move** sub-bar includes micro switchers (three stance buttons + four formation buttons).  
- **AI gizmo:** Aggressive / Defensive / Default / Custom (opens Behavior Params panel).

---

## 4) Input Map (final)

- **WASD**: move when controlling; mouse click sets point when not.  
- **Space**: pause / unpause (tiered pauses available).  
- **Tab / Shift+Tab**: cycle controlled pawn.  
- **Q/E**: quick cycle **Stance**.  
- **Z/X/C/V/B/N**: open **Move/Attack/Ability/Magic/Item/AI** sub-bars.  
- **F**: Focus Fire toggle (shares target).  
- **R**: Retreat to Rally.  
- **1–0**: sub-bar hotkeys.  
- **, . /**: time scale down / up / reset.

---

## 5) Behavior Presets & Params

**Presets:** `Aggressive`, `Defensive`, `Default`, `Custom`.  
They write to **Behavior Params** (sliders):

- **Engage Range**, **Interrupt Priority**, **Retreat HP %**, **Focus-Fire Compliance**, **Formation Strictness**, **Chase Distance**, **Cover Preference**.

Example deltas (relative to Default):

- *Aggressive:* Engage +10, Retreat −5%, FF Compliance +15, Chase +6 tiles.  
- *Defensive:* Engage −10, Interrupt +10, Retreat +5%, Formation +15.

---

## 6) Gambit Library v0 (final)

**Conditions (≥14):**  
hp_lt / hp_gt, ally_hp_lt, status_present/absent, enemy_casting_heavy, enemy_tagged (by leader), distance_band, in_cover / cover_available, formation_breached, duotech_ready, retreat_state, target_marked, item_available, path_clear, timer_elapsed.

**Actions (≥14):**  
use_ability(X), interrupt, reposition_to (cover/anchor), guard(ally/area), swap_stance, set_formation, collapse_to_rally, focus_fire (on target/leader), hold_position, basic_attack, nonlethal_shot, use_item(X), take_cover, kite_step.

**Thrash guard:** retarget cooldown 900 ms minimum; priority ranges 10–100; later rules only fire if no higher rule is runnable.

---

## 7) QuestTemplate Baseline & Requireds

`QuestTemplateDef@4` is the baseline:

**Required:** `scope`, `objectives`, `countdown_days_if_visited` *or* `paused_until_visit`, `rewards.on_success`, `rewards.on_fail`, and **at least one** of `paths_any` **or** `director_rule_seeds`.  
**Optional:** `betrayal_hooks_any`, `interjections_any`, `variant_mutators_any`, `on_betrayal_any`, `on_major_mutator_any`.

---

## 8) Validation (summary)

- Missing requireds → **Error** (block).  
- Stance multipliers outside ±25% → **Warning**.  
- Formation spacing < 0.75 tiles → **Error**.  
- GCD < 300 ms or retarget cd < 600 ms → **Warning**.  
- Gambit list with 0 runnable rules → **Error** (must have a safe fallback).

---

## 9) UI Flows (text sketches)

- **Sub-bar:** click `Ability` → 10-slot strip opens above base bar; hit `2` to fire slot 2; ESC closes.  
- **Move sub-bar:** left: `Rally` + `Retreat`; right: Stance toggle (3) + Formation toggle (4).  
- **Risk chips:** quest path tabs show betrayal risk (Low/Med/High) from Step 15 math.

