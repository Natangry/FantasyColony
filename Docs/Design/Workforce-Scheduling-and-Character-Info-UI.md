# Workforce Scheduling Rules & Character Info UI — Spec v0.1
- Only **workforce** jobs appear in station shift UIs; autonomous activities are never schedulable.
- **Character Info** owns: Combat Training priorities, **Abilities** (allowlist/toggles for auto-combat), **Gambits** editor, **Hotbar** loadouts, and per-pawn **Auto-Combat** toggle (Normal/Conservative/Burst).

## Character Info Tabs
Overview · Jobs · **Combat Training** · **Abilities** · **Gambits** · **Hotbar** · Auto-Combat

### Abilities Tab
- Drag learned abilities to the **Ability** sub-bar.
- Toggle **Allow in Auto-Combat** per ability; optional thresholds (e.g., *Heal at <40%*).

### Gambits Tab
- Visual IF/THEN editor; apply **Presets**: Balanced, Guardian, Healer, Ranger, Mage, Berserker.
- Link: `Docs/Design/Gambit-Library-v0.1.md` for full stacks.

### Auto-Combat Tab
- Per-pawn toggle (on/off) and profile (Normal/Conservative/Burst).
- Global defaults summarized in `Docs/Design/Numbers-Combat-AI-v0.1.md`.

## Scheduling Recap
Stations request **workforce** jobs per shift (AM/PM/Night). Free Time breaks are enforced; Overtime is punishing.

