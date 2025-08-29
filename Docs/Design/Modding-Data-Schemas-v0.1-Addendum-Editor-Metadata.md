# Schemas Addendum — Editor Metadata (v0.1)
*Additive to **Modding Data Schemas v0.1 (Draft)**.*

## EditorFormDef@1
```yaml
id: core.EditorForm.Faction
schema: EditorFormDef@1
for_schema: FactionDef@1
widgets:
  - field: name_key         ; widget: "locKey"
  - field: colors.primary   ; widget: "color"
  - field: law_profile      ; widget: "defref" ; expects: LawProfileDef@1
  - field: mutators_any     ; widget: "defmulti"; expects: SettlementMutatorHookDef@1
  - field: banner_icon      ; widget: "asset"  ; accepts: ["png","svg"]
required_fields_any: ["name_key","law_profile"]
hints_any:
  - "Pick a LawProfile to control tariffs/bribes/capital punishment."
```

## FieldWidgetDef@1
```yaml
id: core.Widget.DefRef
schema: FieldWidgetDef@1
type: "defref"
behavior:
  on_unresolved:
    prompt_create: true
    suggest_templates_any: ["core.Template.Faction.Basic"]
```

## TemplateDef@1
```yaml
id: core.Template.Faction.Basic
schema: TemplateDef@1
for_schema: FactionDef@1
defaults:
  law_profile: core.Law.Township
  colors: { primary: "#6b8f00", secondary: "#2b2b2b" }
  mutators_any: []
```

## EditorPolicyDef@1
```yaml
id: core.Editor.Policy.Default
schema: EditorPolicyDef@1
can_edit_core: false
allow_patch_overrides: true
autosave: false
history_snapshots: true
```

## PatchDef@1
```yaml
id: mymod.Patch.Ability.NerfCinderSlash
schema: PatchDef@1
target: core.Ability.CinderSlash
ops:
  - { op: "replace", path: "damage.amount", value: 24 }
  - { op: "add", path: "ui_tags[]", value: "tuned" }
priority: 50
```

## MigrationDef@1
```yaml
id: core.Migration.Ability.v1_to_v2
schema: MigrationDef@1
from: AbilityDef@1
to:   AbilityDef@2
steps:
  - { op: "rename_field", from: "cast.gcd_ms", to: "timing.gcd_ms" }
  - { op: "default", field: "flags[]", value: "migrated" }
```

## PlaygroundScenarioDef@1
```yaml
id: core.Playground.AbilityArena
schema: PlaygroundScenarioDef@1
mode: "ability_arena"
participants:
  - actor: core.Actor.DummyTarget ; count: 3
```

## ModPackageDef@1
```yaml
id: mymod.Package
schema: ModPackageDef@1
name: "My First Mod"
version: "0.1.0"
author: "you"
dependencies_any: ["core>=0.1.0"]
```

