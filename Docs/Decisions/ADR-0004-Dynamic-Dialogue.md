# ADR-0004 — Dynamic Dialogue & Characters

**Decision:** Dialogue is **template-first**, tokenized, and localized. Personas and phrasebooks shape tone; rules/quests can cast roles, fire barks, or attach conversations.

**Why:** Keeps writing scalable for a dynamic world; supports localization; avoids repetition via variation/cooldowns.

**Implications:** Needs validation (token safety), anti-spam budgets, and a minimal memory system (DataJournal facts).

