# Schemas Addendum — Dialogue & Characters (v0.1)
*Extends the canvas **Modding Data Schemas v0.1 (Draft)**. Pure addendum.*

## DialogueTemplateDef
```yaml
id: core.Dlg.Rumor.Webs.Basic
schema: DialogueTemplateDef@1
context: "Rumor"
text_key: loc.dlg.rumor.webs.basic
tokens: ["{region.name}","{poi.closest}","{tag.SpiderDens.intensity}"]
cooldown_hours: 2
anti_repeat_last: 2
```

## PhrasebookDef
```yaml
id: core.Phrasebook.Spiders
schema: PhrasebookDef@1
context: ["Rumor","Hook","Reminder","Success","Fail"]
variants:
  - { template: core.Dlg.Rumor.Webs.Basic, weight: 60 }
  - { template: core.Dlg.Rumor.Webs.Variant2, weight: 40 }
```

## ConversationGraphDef
```yaml
id: core.Conv.Witch.Rite
schema: ConversationGraphDef@1
nodes:
  - id: start
    say: loc.conv.witch.rite.start
    next: ["ask_rite","decline"]
  - id: ask_rite
    say: loc.conv.witch.rite.ask
    gates: [{ has_tag: core.Tag.WitchRite }]
    options:
      - { text: loc.opt.accept, next: "accept", effects: [{ spawn_quest: core.Quest.BreakTheRite }] }
      - { text: loc.opt.later, next: "end" }
  - id: accept
    say: loc.conv.witch.rite.accept
    end: true
  - id: decline
    say: loc.conv.witch.rite.decline
    end: true
```

## BarkTriggerDef
```yaml
id: core.Bark.QReminder48h
schema: BarkTriggerDef@1
when:
  all:
    - quest_time_left_lt_h: 48
    - not_recently_spoken: { speaker: "scout", context: "Reminder", hours: 2 }
say_any: [ core.Dlg.Reminder.CommonA, core.Dlg.Reminder.CommonB ]
```

## NPCArchetypeDef / PersonaDef / NameBankDef
```yaml
id: core.NPCArchetype.Innkeep
schema: NPCArchetypeDef@1
roles_any: ["quest_giver","rumor"]
```

```yaml
id: core.Persona.Cheerful
schema: PersonaDef@1
tone: "warm"
dialect: "plain"
pronouns: "they"
```

```yaml
id: core.NameBank.ForestFolk
schema: NameBankDef@1
patterns: ["{given} {byname}", "{given} of {place}"]
given: ["Allin","Mara","Tamsin"]
byname: ["Webwatch","Oak-Hands"]
place: ["the Webbed Ford","Elderwood"]
```

## RoleAssignmentDef
```yaml
id: core.Cast.CullBloom.Default
schema: RoleAssignmentDef@1
roles:
  quest_giver:
    archetype: core.NPCArchetype.Innkeep
    persona: core.Persona.Cheerful
    namebank: core.NameBank.ForestFolk
    persistence: "keep"
```

## DataJournalDef (memory facts)
```yaml
id: core.Journal.Default
schema: DataJournalDef@1
facts:
  - id: core.Fact.BrokePromise.Webs
    when: { quest_failed: core.Quest.CullBloom }
    lines_any: [ loc.fact.webs.broke_promise ]
```

## Rule/Quest extensions
- `RuleDef@2.then.post_dialogue: { template: DialogueTemplateId, speaker: ArchetypeId|ActorId }`
- `QuestTemplateDef@2.cast` binds roles to archetype/persona/namebank.

## Validation
- Dialogue templates must reference valid localization keys and only allowed tokens.
- Conversation edges must resolve; option effects must be valid actions.
- RoleAssignment must produce unique persistent NPC ids when `persistence: "keep"`.

