Fantasy Colony — Stage Lifecycle (scene-free)
===========================================
Purpose
Central, explicit, and scene-free app lifecycle with deterministic ordering.
Keep stages minimal; prefer domain events & cadences for frequent gameplay signals.
Stages
AppBoot → Intro → WorldSetup → WorldSim → EmbarkPrep → MapGen → Gameplay → (RestartRequested/Shutdown)
Usage
• Register a static method with:
[OnStage(LifecycleStage.Intro, StageMoment.Enter, order: 0, runOnce: false)]
static void BringUpIntroUI(StageContext ctx) { /* ... */ }
• Methods may be void M() or void M(StageContext ctx)
• Use [OrderBefore(typeof(MyType))] / [OrderAfter(typeof(OtherType))] to express ordering constraints.
• Handlers marked runOnce=true will execute only once per process lifetime.
Rules of thumb
• DO: Move scene-coupled boot code to stages; UI buttons advance stages via StageRunner.Instance.AdvanceTo(...)
• DO NOT: Gate systems on “intro visible” — gate on stages instead (e.g., show HUD on Gameplay Enter).
• DO: Keep stages rare and global. For gameplay events (season changed, pawn spawned), use a separate Domain Events layer.
Boot behavior
• Registry builds at domain load and logs a Lifecycle Report with ordered handlers per stage/moment.
• StageRunner auto-enters AppBoot. Advancing to other stages is explicit (e.g., Intro Start → WorldSetup).
Notes
• This patch establishes the lifecycle core only. Existing files should be migrated in follow-up changes.
