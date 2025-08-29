# Validation — Severity & Lints (v0.1)
> **Amended (v0.2):** Class ownership & migration checks.

## Errors
- Pawn assigned more than one Class.
- `ClassEvolveRule` with `from == to` or cyclic path.
- Job marked `grants_combat_abilities: true` without legacy flag.

## Warnings
- Pawn eligible for Class choice for > 2 days without selection (policy suggests auto-prompt).
- Evolution nearly met (≥ 80%) but blocked by Affinity misalignment—suggest class-appropriate content.
- Class with no `ClassLevelDef` rewards.

## Infos
- Job track still references old combat ability ids (auto-patch available).
- Class thresholds referencing factions/tags disabled in current world.

