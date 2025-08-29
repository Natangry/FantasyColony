# Numbers — World Conditions & Thresholds (v0.1 Defaults)

## Hysteresis & Smoothing
- Default **hysteresis**: escalate 0.70, de-escalate 0.50.
- Default **EMA windows**: 2–3d for volatile (Sanitation/Traffic), 5–7d for slow (Corruption/ApexSaga).

## Suggested Thresholds
- Region **Danger**: `0.7 / 0.5`
- Settlement **Sanitation**: `below 0.4 / above 0.6 (2d window)`
- Route **Traffic** slump: `< 0.2` and **Ambushes3d ≥ 3`
- **Arcane Saturation**: `> 0.8` leak; clear `< 0.6`
- Faction **Corruption**: `> 0.6` (EMA 5d) → corruption hooks
- Party **AftershockDensityNearby**: `≥ 2` → pause Apex saga seeds

## Cooldowns & TTLs
- Condition cooldowns: **2–6d**, defaults **3d**.
- Tag TTLs from conditions: **3–7d** typical.

## UI
- Chip shows metric value & trend arrow (↑/→/↓).
- Inspector lists **Top Drivers** (e.g., “Ambush rate +, Patrol −”).

## Director Safety Caps
- Per-region active conditions cap **4** (warn otherwise).
- Global cap on Apex saga seeds while **AftershockDensityNearby ≥ 2**.
