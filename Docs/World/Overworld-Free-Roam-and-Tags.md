# Overworld — Free-Roam (Option B) + Dynamic Tags

*This page is cross-linked with:*
- **Director, World Tags & Dynamic Quests:** `Docs/World/Director-Tags-and-Quests-v0.1.md`
- **Dialogue & Characters:** `Docs/World/Dialogue-and-Characters-v0.1.md`

SNES-style overworld with fog-of-war and stealth/chase; RTwP combats in **scene-swapped** arenas; return to same map spot.

## Dynamic World Tags & Director
Tags (GLOBAL/REGION/ROUTE/POI/FACTION/ACTOR) with TTL and caps; the **Director** evaluates RuleDef every **6 in-game hours** and on **region entry** to spawn quests, move borders, alter POIs, add tracks, and post rumors.

### Quest Lifecycle (Visited vs Unvisited)
- **Seeded** by rules or POI interactions.  
- **Visited regions:** quests start a **countdown** on creation.  
- **Unvisited regions:** quests remain **Paused** (no timer) until first entry, then start their countdown.

