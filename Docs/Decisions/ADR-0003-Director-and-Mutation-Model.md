# ADR-0003 — Director & Mutation Model

**Decision:** Use a **rule-driven Director** that reacts to **World Tags** + **Actors** to spawn quests, mutate threats, and post rumors. Tags may **mutate** (replace) or gain **overlays** (augment) based on proximity, intensity, and time.

**Why:** Supports emergent world evolution, clear modding hooks, and controllable pacing via dials (caps, cooldowns, thresholds).

**Alternatives considered:** hand-authored linear questlines; pure random encounter tables.

**Implications:** Requires provenance UI and robust validation for rule conflicts. Addenda extend schema with `WorldTagDef@2`, `RuleDef@2`, `MutationDef`, `OverlayDef`, `QuestTemplateDef@2`, `ActorDef@2`.

