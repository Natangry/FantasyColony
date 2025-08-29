# Numbers — Storyteller & Pacing (v0.1 Defaults)

## Budget & Costs
- **Daily Budget:** 10 pts (base).  
- **Beat Costs:** Hook 2, Probe 3, QuestBeat 4–6, Festival 3–4, Caravan 2–3, Raid 6–9, Aftershock 5–7, Climax 8–10, Respite 1–2.  
- **Bonuses/Taxes:** +0–4 (tension bonus), −2–4 (recent catastrophe tax), +difficulty modifier (−20% Peaceful … +25% Hardcore).

## Cadence & Respite
- **Respite after Major:** 1.5–2.5 days; **after Medium:** 0.5–1 day.  
- **Min Gap** between hard beats: 0.75d (profile-tuned).  
- **Rate Limits:** ≤1 major raid per 3d; ≤2 medium incidents per 1d.

## Tension
- **Decay:** 0.10/day baseline.  
- **Instant deltas:** Victory −0.15; Festival −0.10; Siege +0.12; Route closure +0.05.  
- **Metric Weights (default Wayfarer, Region scope):** Danger 0.45, Unrest 0.20, Prosperity (inverse) 0.15, Aftershock Density 0.10, Patrol Density (inverse) 0.10.

## Diversity & Focus
- Prefer not to repeat the same beat type more than **2×** in a row.  
- Focus the **current Region/Colony** 70% of the time; allow 30% spillover.

## Validation Hints
- `deescalate_below < escalate_at` for tension hysteresis; EMA 2–7d.  
- Budget never negative; beats must declare a cooldown window.

