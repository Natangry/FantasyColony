# ADR-0006 — Faction Mutators & Settlement Visit Scenes

**Decision:** Factions can gain **Mutators** that change prices, contracts, patrols, law, and **settlement generation** on visit (scene swap). Settlements are built from **templates + modules** parameterized by faction/mutators/tags/quests.

**Why:** Keeps towns reactive to world state and player actions; strong flavor with minimal bespoke code; highly moddable.

**Implications:** Director gains mutator actions; economy/route systems read mutator modifiers; UI shows mutator chips and law meters; persistence for settlement layouts & cast.
