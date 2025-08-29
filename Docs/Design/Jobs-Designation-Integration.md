# Jobs × Building Designations — Integration Notes (v0.1)

- Each `BuildingDesignationDef` emits **ShiftNeeds** (e.g., Bar → 1× Bartender PM; Inn → 1× Innkeep PM).
- The **Assignment Board** aggregates these into **workforce job slots** at dawn; stations show them per shift.
- Closing a building (requirements unmet or hours) removes its shift needs; reopening re-adds them.
- Designations can reference **Stock/Adjacency** requirements (e.g., Kitchen access). When unmet, the building may run **Limited** service.
