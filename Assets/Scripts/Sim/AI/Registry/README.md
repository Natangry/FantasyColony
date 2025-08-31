# AI Registry

Maps action IDs from data (XML BrainDefs) to concrete C# classes.

Usage:
- Register built-in actions during boot (e.g., in a BrainBootstrap).
- Optional: allow code mods to register additional actions in the Mods stage.

Do not place gameplay logic here—only mapping/registration.
