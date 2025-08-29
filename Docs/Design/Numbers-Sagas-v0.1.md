# Numbers — Sagas, Branching & Bosses (v0.1 Defaults)

## Concurrency & Pacing
- **Max concurrent Major Sagas:** 2 (only **1 Apex** at a time).
- **Tick cadence:** inherits Director (6h); saga clock also ticks on **quest outcomes**.

## Saga Clock (per state)
- **Dormant → Stirring:** +1/day baseline; +1 on related tag intensity ≥2; −2 if player clears 2+ related quests in 3d.
- **Stirring → Ascendant:** +1/day; +2 if synergy tag present; −3 on special counter-quest success.
- **Ascendant → Apex:** gate requires **≥2 milestones** cleared or **time ≥ X days** (default 6–9) and pressure index ≥ threshold.
- **Apex → Aftermath:** on boss outcome. Success → apply `WorldEffect.Success`; Fail → apply `WorldEffect.Fail` and optionally regress to **Ascendant** with new branch.

## Branch Weights (first pass)
- Base branch selection **40/30/30**; +15 to branches whose synergy tags are within ≤1 hop.

## Boss Tuning
- **Phases:** 3 baseline; HP thresholds 70% / 40%; optional **prep breakers** remove a phase or disable an ability.
- **Loot Tiers:** Boss drops **T3–T4** uniques; chance for **WorldEffect token** (e.g., settlement boon).
- **Arena:** 2–3 hazard props; weather lock chance 30%.

## World Effects (magnitudes)
- **Route Safety:** bandit encounter weight −25% (regional).
- **Trade Prices:** silk −20% (regional 7d); tyrant taxes +10% (global until countered).
- **Climate Bias:** global temp ±1–2°C; storm frequency ±10–15%.
- **Resource Nodes:** +2–4 temporary nodes; decay 7–10d unless renewed by followup quest.
