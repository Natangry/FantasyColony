# AI Brain

Holds the decision runner and data model used by NPCs:
- **BrainConfig** — in-memory representation of a BrainDef after XML + rules/overrides are applied.
- **BrainInstance** — runtime loop: sense → score → pick/keep → tick action.

Keep this layer independent of rendering and UI. It should only depend on:
- Action interfaces
- AI context (sensing/commands)
- Def registries (to resolve referenced abilities, etc.)
