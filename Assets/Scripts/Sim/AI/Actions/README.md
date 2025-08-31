# AI Actions

Reusable behavior patterns. Examples:
- UseAbility
- ChaseTarget
- MoveToPoint
- Flee
- Kite
- Flank
- GuardAlly
- Idle

Guidelines:
- Each action follows one contract (Id, Score, Tick, Enter/Exit).
- Parameters come from BrainDef XML (parsed at load).
- No direct references to UI/MonoBehaviours; operate via an AI context.
