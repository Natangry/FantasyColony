# Gambit Library v0.1 — Conditions, Actions, Presets (Docs Only)

Baseline combat AI that “just works,” is readable, and is friendly to modding. All examples are **data-shape** only (no engine code implied).

## 1) Condition Catalog (v0.1)

| id | params | notes |
|---|---|---|
| `ally_hp_lt` | `{ pct: 0.35 }` | Any ally below % HP exists |
| `self_hp_lt` | `{ pct: 0.25 }` | Pawn’s own HP below % |
| `enemy_casting` | `{ weight: "heavy" \| "light" \| "any", tag?: "stun" }` | Reads telegraph weight or tags |
| `status_present` | `{ target: "self" \| "ally" \| "enemy", id: StatusId }` | Check for a status |
| `cooldown_ready` | `{ ability: AbilityId }` | Ability not on cooldown & resources available |
| `resource_pct` | `{ stat: "stamina" \| "mana" \| "resolve", op: "<" \| ">", value: 0.2 }` | Resource threshold |
| `in_range` | `{ of: "weapon" \| AbilityId }` | Within weapon or ability range |
| `enemies_in_radius_gte` | `{ r: 3, count: 3 }` | Enemy cluster check |
| `allies_in_radius_gte` | `{ r: 3, count: 2 }` | Ally cluster check |
| `allies_in_radius_lte` | `{ r: 3, count: 0 }` | Friendly-fire guard for AoE |
| `threat_on_self_gt` | `{ value: 2 }` | Targeting/threat weight on self |
| `has_focus_target` | `{ }` | Team focus target exists |
| `telegraph_on_me` | `{ time_ms_lte?: 600 }` | Standing in incoming danger |
| `line_of_fire_clear` | `{ }` | No ally blocking shot |
| `target_tag` | `{ tag: "beast" \| "undead" \| "humanoid" \| string }` | Targets with tag |
| **`poise_lt`** | `{ value: 0.2 }` | New: use when enemy is near stagger |
| **`status_resist_lt`** | `{ value: 0.2 }` | New: prefer debuffs on low-resist targets |

> Validation: Gambits referencing new stat readers rely on defs in `Schemas Addendum — Stats & Damage`.

## 2) Action Catalog (v0.1)
(unchanged; see earlier sections)

## 3) Target Selectors
(unchanged)

## 4) Preset Stacks (ready-to-play)
(unchanged examples; presets implicitly benefit from new stat readers where present)

## 5) Auto-Combat Profiles
(unchanged)

## 6) Modding Notes
- Each Condition/Action is a `*Def` with schema and validation (see **Schemas Addendum**).
- Presets are `GambitDef`s; mods can add/edit/replace without touching code.
- Always include localization keys for preset names and tooltips.
