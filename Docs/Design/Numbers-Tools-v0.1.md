# Numbers — Tools & Equipment (v0.1 Defaults)

## Tier Modifiers
- **Loaner:** work_speed −10%, error +10%, fatigue_accum +5%
- **Common:** baseline (0%)
- **Fine:** work_speed +5%, output_quality +2%, error −2%
- **Superior:** work_speed +10%, output_quality +4%, error −5%
- **Masterwork:** work_speed +15%, output_quality +6%, error −10%
- **Enchanted:** same as Masterwork **+ affix** (data-driven)

## Durability (base per class)
- Hatchet **100**, Pick **120**, Hoe/Sickle **90**, Hammer/Saw **100**, Mallet/Chisel **110**, Smith’s Hammer **120**, Chef’s Kit **80**, Focus Rod **100**, Physicker’s Kit **80**, Bow/Traps **90**, Fishing Rod/Net **80**
- **Loss model:** −1 per N actions (class-tuned), or −X per work-second on continuous stations.

## Repairs
- **Repair Kit** (generic): 1× plank + 1× iron nail + 1× resin → restores **+40 durability** (10s at Workbench)
- **Metal Repair** (forge): 1× ingot → restores **+80 durability** (12s at Forge)
- Minimum: cannot exceed tool’s **max durability**.

## Gates
- Forge requires **Smith’s Hammer ≥ Common**
- Hearty Meal requires **Chef’s Kit ≥ Fine**
- Ward tuning bonus at **Focus Rod ≥ Superior**

## Auto-Assign
- **Tool Rack pickup radius:** **12 tiles**
- Sort key: **tier > condition > distance**
- If none found, use **Loaner**

> Values are first-pass knobs and expected to iterate.
