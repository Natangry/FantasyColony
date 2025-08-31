# Monsters (MonsterDef)

Content definitions for monsters/creatures.

Typical fields:
- id, label
- Stats (hp, speed, intelligence, defense, …)
- BrainRef (which BrainDef to use) + optional param overrides
- Abilities (list of AbilityRef)
- LootRef, Faction, VisualRef

Files here are XML and use stable string IDs. Prefer one file per monster.
