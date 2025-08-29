# Validation — Addendum: Combat Stats & Formulas (v0.1)

**This addendum extends the existing `Validation-Severity-and-Lints.md` without overwriting it.**

## Errors (block save)
- Circular references among `DerivedStatFormulaDef` or between derived stats and Ability coeffs.
- `HitTableDef` order missing a terminal `normal` or probabilities summing > 1 without normalization rules.
- `MitigationCurveDef` without clamp/cap.
- Ability with `cast.gcd_ms < 300` without `flag: extreme_speed_ok`.

## Warnings
- Builds simultaneously exceeding **ArmorDR ≥ 0.70**, **Guard ≥ 0.40**, **BlockPower% ≥ 0.40**.
- CritChance > 0.35 (soft cap) without DR; Haste > 0.30.
- Status with `tick_ms < 300` flagged as heavy; suggest batching.

## Infos
- Derived stat formulas lacking explicit clamp (auto-clamp applied to [0, +∞) with note).
- Abilities with no `status_apply` or utility flag (author reminder).

