# Party Loyalty, Temptation, Betrayals & Interjections (v0.1, Docs Only)

> **Update:** Betrayal now **may spawn a Spin-Off Chain** and/or **kick off Propagation** via `on_betrayal_any` in the quest or `spin_off_chain_on_trigger` in the trigger def. Chains create their own loyalty beats later (mentors can intervene once per chain).


**Goal:** Make party composition (Ethos, class/job, traits, relationships) materially affect quests and dialogue: betrayals/refusals, role-gated options, and flavorful interjections. Mod-first, data-driven.

> Shapes defined here are in the schema addendum `Docs/Design/Modding-Data-Schemas-v0.1-Addendum-Party-Quests-and-Dialogue-Mutators.md`. This addendum **extends** the canvas doc **Modding Data Schemas v0.1 (Draft)** (do not overwrite).

---

## 1) Core Model

- **Loyalty (stat):** attachment to the party/leader. Sources: relationship to leader, job satisfaction, favors/debts, fear/respect, promises kept/broken.
- **Temptation (scene rule):** pressure toward deviating from the current plan. Sources: Ethos mismatch with chosen quest path, traits (Greedy, Hot-headed, Zealous), external bribes/offers, target ties, mutators in the region.
- **Check Timing:** At **big choice gates**, **boss phase transitions**, **loot objectives**, **hostage moments**.
- **Outcome:** *Comply*, *Interject/Refuse*, or *Betray* (flip sides, sabotage, steal, flee).

**Roll sketch**
```
score = (Temptation - Loyalty) + SceneDifficulty + Fatigue
if score > random(0..1): trigger betrayal
else if score > random(0..1)*0.66: trigger refusal/interjection
else: comply
```
Modifiers: Party Lead present (−), public setting (+), time pressure (+), *Oath Token* carried (−), high bond with leader (−), rival nearby (+).

---

## 2) Interjections & Role-Gated Options

- **PartyInterjection** inserts lines & optional checks when a pawn/class/trait or relationship condition is true. Examples:
  - *Greedy* → “Skim a cut” option on delivery quests (Evil).
  - *Scholar (Journeyman)* → lore deduction unlocks *Neutral* “Expose the lie”.
  - *Mentor present* → once-per-quest **Override** to stop an apprentice betrayal (consumes a **Bond Token**).
  - *Rivals present* → escalate a clean exit to a **timed retreat** unless Leadership check passes.

- **QuestVariantMutator** adds/changes objectives when conditions are met (e.g., *Artisan@Journeyman* unlocks **Forge Counterseal** to resolve without bloodshed).

---

## 3) Betrayal/Refusal Hooks by Path

- Good path with Evil pawn(s) risks betrayal; Evil path with Virtuous pawn(s) risks refusal.
- Average **party Ethos** biases the **Director** to surface *dark offers* (Evil skew) or *mercy aids* (Good skew).
- **Fail-Forward:** betrayal never hard-locks the quest; it raises stakes, changes rewards, or spawns follow-ups.

---

## 4) Relationship & Class Effects

- **Close Friends**: unlock combo resolutions (e.g., *Mark & Parley*).
- **Rivals**: increase insult/duel branches; higher chance of interjection.
- **Romance**: unlock *Reckless Save* at risk, trading safety for speed.
- **Class Aspects** (virtue/vice) flavor options & lines; e.g., *Ward Knight* adds sanctify outlets, *Dread Warden* offers soul-harvest.

---

## 5) UI/UX

- **Quest Card:** path tabs show **Risk Chips** (“Betrayal risk: Low/Med/High”), **Role Locks** (e.g., *Needs Scholar (Jman)+*), and **Ethos icons** with outcome previews.
- **Interjection Popups:** pause-time choice with speaker name, persona, stakes, and checks.
- **Party Panel:** hover a pawn to see current *Loyalty* and *Temptation contributors* (debug overlay toggle for testing).

---

## 6) Worked Examples

**A) Track the Hunter (Good path)**
- *Poacher Ranger* rolls Temptation on trap-recipe offer. Fail → trained beasts join; success → *Scholar* interjection unlocks Neutral “Expose the Hunter” shortcut.

**B) Sanctify the Webs (Undead branch)**
- *Dread Warden* attempts soul-silk harvest (Evil betrayal) unless Mentor Override triggers or loyalty wins. Aftermath adjusts faction rep and Ethos.

---

## 7) Storyteller Dials

- Global **Betrayal Frequency** (Rare/Occasional/Common).
- **Interjection Intensity** (Flavor-only → Mechanical).
- **Mentor Override Stock** (tokens per week).
- **Public Consequence** scaling (settlement witnesses intensify rep swings).

