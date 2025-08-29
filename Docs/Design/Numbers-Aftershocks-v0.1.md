# Numbers — Aftershocks (Spin-Off Chains & Propagation) v0.1

## Spin-Off Chains
- Base spawn chance on seed: **100%** for flagged “Major” betrayals/mutators; **50%** for “Minor”.
- Max concurrent **Aftershock Chains**: **2**; queue additional seeds (oldest drops if over cap).
- Step timer defaults: **3–5 days**; fail-forward edges preferred.

## Propagation
- Default hops: **1** settlement, **2** routes; density **0.5** per hop.
- TTLs: settlement **5–7d**, route **4–6d**; decay **0.15–0.25/day**.
- Global caps: **Routes:** 4; **Settlements:** 3 for a given propagation family.
- Resistance tags halve density; chapel wards reduce TTL by **−2d**.

## UI
- Risk chip thresholds for “Spreads Quickly” badge if density × hops ≥ **1.0**.
- Map overlay fades as TTL approaches 0.
