# Migration — Move Combat Unlocks from Jobs → Classes (v0.2)

**From Jobs (old)** → **To Classes (new)**

- **Guardian job**: `Taunt`, `Shield Bash` → **Guardian class** (Squire/Guardian line).  
- **Healer job**: `Field Dressing`, `Heal` → **Cleric class** (Acolyte line).  
- **Ranger/Scout job**: `Marked Prey`, `Volley` → **Ranger class** (Scout line).  
- **Warden job**: `Quick Ward (barrier)` → **Warder class** combat barrier; day-job **Wardkeeper** retains **Recharge/Refit Wards** (non-combat colony action).  

**Notes**
- Jobs keep **throughput/quality passives** and **field actions** (non-combat).  
- If a save/mod still grants combat abilities via Jobs, provide a `PatchDef` to remap to **ClassAbilityDef** ids.

