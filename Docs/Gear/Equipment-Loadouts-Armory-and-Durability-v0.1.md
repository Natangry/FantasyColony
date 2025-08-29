# Equipment, Loadouts, Armory & Lockers, Quality/Durability (v0.1, Docs Only)

**Intent:** A moddable, transparent equipment system that supports *Sound the Alarm*, day-jobs, crafting, and raids. Authors can define slots, categories, materials, quality/durability, encumbrance, sockets/affixes, repair/salvage, and storage policies without code.

*Additive to the canvas **Modding Data Schemas v0.1 (Draft)** (see `ItemDef/RecipeDef/BuildingDef`).*

---

## A) Slots & Categories

**Equip slots:** `mainhand`, `offhand`, `twohand` (virtual), `armor`, `helm`, `cloak`, `ring`, `trinket`, `quiver`, `focus`, `tool`.

**Two-hand rules:** Items with `tags: ["weapon.2h"]` occupy `mainhand` + lock `offhand`. Shields require `offhand` and conflict with `weapon.2h`.

**Categories & tags (examples):**
- Weapons: `weapon.sword|axe|spear|bow|staff`, ranged get `weapon.ranged`, magic foci get `focus.magic`.
- Armor: `armor.light|medium|heavy`.
- Tools: `tool.hoe|hammer|tongs|saw` etc. (works with job proficiency).
- Materials: `material.wood|leather|iron|steel|mythril`.
- Synergies: `class.guardian`, `role.ranger`, `elem.Ember` etc.

---

## B) Quality & Durability

**Quality tiers:** *Poor, Common, Fine, Exceptional, Masterwork* → quality scalar ranges (+/− to base stats & affix budgets).  
**Condition (durability 0–100):** soft penalties begin < 40; heavy < 15. Condition decays from **hits**, **blocks**, **usage at stations** (tools). Repair restores condition (see §E).

---

## C) Encumbrance & Carry

Each item has **weight**. Pawn **Capacity** scales with STR/VIT and pack bonuses. Encumbrance bands affect **MoveSpeed**, **Stamina regen**, **Evasion**:

- *Light* (≤ 60% capacity): no penalties.  
- *Normal* (≤ 100%): minor stamina drain.  
- *Heavy* (≤ 130%): −Move, −Evasion, +Stamina drain.  
- *Overloaded* (> 130%): large −Move, −Actions gated (warnings/auto-drop if extreme).

Warden wards may temporarily offset penalties (buff).

---

## D) Loadouts & Auto-Equip

- **LoadoutTemplates** per role/class/stance (e.g., Guardian.Defense, Ranger.Field).  
- **EquipScore** ranks items by tags, quality, material, condition, class/trait synergy; used by *Sound the Alarm* and the **Loadout Manager**.  
- **Reservation policy:** **Personal Lockers → Armory → World** to avoid tug-of-war.  
- **Quick-swap sets:** Each pawn may keep **Field** and **Defense** sets; *Stand Down* can revert gear.

---

## E) Sockets, Affixes & Gems (optional light system)

- **Sockets:** zero or more; colorless by default; optional element sockets (ember/frost/etc.).  
- **AffixDef:** small stat lines with rarity weights and incompatibility rules.  
- **GemDef:** pluggable affixes or small procs; remove/replace at a bench (cost/scaler).

---

## F) Repair, Salvage & Upkeep

- **RepairRecipeDef:** Field Repair (quick, low cap) vs Bench Repair (full). Inputs scale with material & quality.  
- **Upkeep policies:** auto-queue repairs under thresholds; **Maintenance** job executes.  
- **SalvageTableDef:** scrapping returns materials/parts; quality/condition affect yields.

---

## G) Storage: Armory & Lockers

- **Armory (room/building module):** “combat-only” filters, *Alarm* integration to reserve best items; theft risk during raids; Warden/Guardian secure on Lockdown.  
- **Personal Lockers:** per-pawn storage for owned gear; assignment UI; ownership reduces churn.

---

## H) UI/UX

- **Loadout Manager:** per pawn (and batch): preview best set, deltas, encumbrance bar, conflict badges (2H vs shield), missing slots; **Apply** uses reservation rules.  
- **Locker/Armory panels:** filters, reserve toggles, repair queue view.  
- **Tooltips:** quality ribbon, condition %, affixes/sockets, material, weight, source (crafted/vendor/drop).

---

## I) Acceptance (v0.1)

- Clear slot map & conflicts; encumbrance penalties visible; quality & condition impact stats.  
- *Alarm* equips to best viable set using lockers/armory; optional **Stand Down** reverts.  
- Repair/salvage flows documented; UI concepts solid.  
- Everything is **data-driven** and editable in the Definition Editor (Steps 25–26).

