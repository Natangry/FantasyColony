# Simulation / AI

Central home for runtime AI logic. Do not place UI logic here.

Subfolders:
- **Actions/** — reusable behavior blocks (Chase, Flee, Kite, Flank, UseAbility, …)
- **Brain/** — BrainConfig (data) and BrainInstance (decision loop)
- **Registry/** — ActionRegistry mapping string IDs to action classes

Notes:
- One C# class **per action type** (small, reusable).
- Brain reads data (from XML BrainDefs) and uses the registry to instantiate actions.
- Keep files small and focused. Avoid giant switch files.
