# ADR-0005 — Major Sagas & Boss Arcs

**Decision:** Treat large narrative/threat structures as **Major Sagas**: world-scale tags with states, clocks, branches, and data-driven **BossEncounter** + **WorldEffect** outcomes.

**Rationale:** Keeps high-level narrative emergent, moddable, and paced via dials; supports fail-forward branches.

**Implications:** Director requires saga-specific actions; UI needs **Saga Journal**; validation must prevent Apex pileups and dead-end branches.
