# Core (infrastructure services)

This folder is the home for cross-cutting services used by the whole game.
Keep gameplay rules out of here—only infrastructure lives in Core.

Suggested contents:
- **Clock/**: fixed-step simulation clock & time scale/pause APIs
- **EventBus/**: typed pub/sub for decoupled systems
- **Save/**: versioned SaveService and migration helpers
- **Config/**: startup options, mod selection, world seed

Principles:
- Plain C# (no scene singletons). Registered during boot stages.
- Deterministic where possible (fixed-step ticks).
- Small interfaces, testable implementations.
