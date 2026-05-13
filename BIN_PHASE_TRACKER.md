# Bin Migration Phase Tracker

Status key: `TODO` / `IN_PROGRESS` / `BLOCKED` / `DONE`

## Current baseline (2026-05-13)
- Server is in a **test-server-ready** state after map/item/storage stabilization work.
- We keep strict rule: no hidden DB fallback for migrated static domains.
- Next phase focus is chosen by highest static DB removal value + gameplay impact.

## Next bin target
## `Npc.bin` (next up)
Status: `IN_PROGRESS` (next active phase)

### Why this is next
- It removes a large remaining static DB surface (`Asset_Npc*` queries).
- It directly affects live gameplay interactions (shops, warehouses, portals, NPC services).
- It is the cleanest next cut after item/storage stabilization.

### Completion definition
- [ ] `Npc.bin` loader finalized with client-accurate layout/padding.
- [ ] All runtime NPC consumers switched to bin-backed providers.
- [ ] Legacy static `Asset_Npc*` read paths removed from runtime query handlers.
- [ ] Client-parity behavior verified for:
  - [ ] NPC shop open/buy/sell flows
  - [ ] Warehouse/cash warehouse access flows
  - [ ] Portal/warp interaction gating
  - [ ] Other NPC service handlers currently keyed by DB NPC assets
- [ ] Boot/runtime validation passes with no NPC static DB dependency.
- [ ] Drop-list prepared for NPC static tables that are no longer used.

## Following target (after `Npc.bin`)
## `Quest.bin` + `MasterCard.bin`
Status: `TODO`

### Notes
- Keep `ItemList.bin` parity hardening in parallel only if a bug is found during NPC phase.
- Do not move to next phase until `Npc.bin` completion definition is fully checked.
