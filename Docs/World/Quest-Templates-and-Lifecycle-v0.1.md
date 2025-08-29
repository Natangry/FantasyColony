# Quest Templates & Lifecycle (v0.1, Docs Only)

> **Update:** Alive-only objectives, capture blocks, and turn-in flow.

## Alive-Only Objectives
- Use `objective.capture { target: ActorId, alive_only: true }`.
- When present, **Quest Card** shows a **Non-Lethal Required** chip and lists tool hints (rope/manacles vendor or recipe).

## Capture Flow Blocks
- **Surrender → Bind → Transport → Turn-In / Parole / Recruit** all map to objective sub-steps with timers and risks.
- **Fail-Forward:** escape attempts spawn a follow-up objective; turn-in failure before TTL spawns a bounty-hunter encounter.

## Turn-In Rewards
- `BountyContractDef` supplies payout & rep deltas; respect issuer `LawProfile` (alive_only or dead_or_alive).
