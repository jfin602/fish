# P6 P3 — Normal Pond Bass survival entry

**Disposition: implemented T2 candidate at assigned `0.6.3`; G6.1–G6.8 and V44–V51 remain NOT GREEN.** This checkpoint integrates the existing Bass lifecycle into ordinary Freshwater Pond. It does not qualify the full living food web or owner play.

## Source and live bridge

- Entry: clean `main` at `1e269998c1435b43ad7d1accfa2f8f54cdbaedf0`, root `0.6.2`; no owner changes were present. All P3 source and documentation changes remain uncommitted. No runner execution, commit or push occurred.
- The task's initial MCP connection was disconnected because a stale Fish server held `127.0.0.1:19080`. After identifying process ownership, the other MCP servers/listeners were closed as requested by the owner. A fresh Fish server and Godot .NET editor were started for `/home/jfin/dev/fish/game/` on `ws://127.0.0.1:19080`. `godot_health_check` returned `bridge_connected=true`, `editor_status=ready`, addon/server `2026.11.04`; `godot_get_server_info` returned the exact Fish project path, Godot `4.7.2-stable`, C# project/support true. Health was rechecked after focused checks and remained connected. The editor used Forward+ on the Linux/X11 AMD Radeon 880M host. This proves bridge identity, not native play quality.
- The pinned normal Pond load reported world `world-5e3b4fee759acbe3`, seed `71`, content `d76eabbb5b6fff2f91418b49b2de92dd090541d609b46d0725ad9fc85cbda347`, `generator_invocations=0`. The selected Bass is `FAF-FISH-007`; session life IDs have prefix `pond-world-5e3b4fee759acbe3:bass-life:` and a new GUID per retry. Example food ID: `world-5e3b4fee759acbe3:food:pond:zooplankton:0`; physical prey IDs use `world-5e3b4fee759acbe3:bluegill:larva:0` and registered Bluegill `FAF-FISH-006`. The selected nest was `(-53.561985, 9.355044, 4.599852)` after actual terrain, shore, waterline and collision probes.

## Implemented behavior

- World Select → Freshwater Pond → Bass now opens a session on the existing baked Pond. The egg hatch, skip, larva camera handoff, fitted stage art/body/tuning, auto-mouth feeding, Space boost, pause, caught screen, return and new-life retry are available without instancing `PikeEncounter`. A new life has its own identity, feeding ledger, food/prey state and clock; pause and closed-game time do not advance it. No durable save or permanent-death record is claimed.
- `BassHatchPresentation` shares registered egg construction and shell geometry between the fixture and normal Pond. `PondGrowthState`, `FeedingLedger`, `PondFoodActor`, `PreyActor`, `PlayerArt`, `FishController` and the read-only feeding reticle are reused. The normal session owns a single ledger. Food and prey consumption require stage eligibility, forward mouth reach and clear physical path; a committed result alone pulses the reticle.
- P3 seeds eight zooplankton, eight aquatic nymphs and six physical Bluegill near the nursery. Bass selection removes Bluegill from the presentation-only ambient roster, avoiding a parallel visual Bluegill identity. Other Pond demo fish retain the original 12-actor ambient presentation, and Ocean and Pike Encounter retain their routes. P4 owns habitat-wide food patches, depletion and bounded renewal; P5 onward owns predator and independent ecology.
- The new `inspect-p6-pond-entry` runs through the actual selection pages and checks bake identity, hatch/skip, reticle policy, valid physical feeding and stage transitions, caught state, and retry into a fresh egg with zero nutrition. Its relocation and clock controls are diagnostic only.

## Focused checks

| Command | Observed result |
| --- | --- |
| `./tools/dev import` | PASS; Godot project import completed. |
| `./tools/dev build` | PASS; final build had 0 warnings and 0 errors. |
| `./tools/dev inspect-p6-pond-entry` | PASS, `P6_POND_ENTRY_OK`; seed 71 loaded generator-free, hatch retained the same life ID, nutrition 6/14/26 advanced juvenile/subadult/adult, caught state stopped that life, retry produced a different egg identity with zero nutrition. Count: 16 food, 6 physical prey, 11 ambient visuals. |
| `./tools/dev inspect-pond-creatures` | PASS, `POND_CREATURE_CHECK_OK`; five Pond choices and 25 stage exports, 12 ambient actors for art demos, 11 for Bass survival. The old check's adult Bass assumption was corrected after an initial failure. |
| `./tools/dev inspect-hatch` | PASS, `HATCH_CHECK_OK`; the extracted egg helper preserved fixture hatch/skip behavior. |
| `./tools/dev inspect-growth` | PASS, `GROWTH_CHECK_OK`; existing staged growth rules. |
| `./tools/dev inspect-feeding` | PASS, `FEEDING_CHECK_OK`; existing ledger/contact behavior. |
| `./tools/dev inspect-auto-feed-boost` | PASS, `AUTO_FEED_BOOST_CHECK_OK`; existing fixture auto-mouth and boost. |
| `./tools/dev inspect-p6-reticle` | PASS, `P6_RETICLE_CHECK_OK`; fixture cue and committed-feed regression. |
| `./tools/dev inspect-world-select` | PASS, `WORLD_SELECT_CHECK_OK`; selection routing. |
| `./tools/dev inspect-world-isolation` | PASS, `WORLD_ISOLATION_CHECK_OK`; Ocean/Pond/Pike session isolation. |
| `./tools/dev inspect-world-catalog` | PASS, `WORLD_CATALOG_CHECK_OK`; the older diagnostic first failed because it loaded the Pond habitat off-tree, then passed after attaching it under `Main` before validation. No bake was changed. |
| `./tools/dev smoke-world` | PASS, `FISH_SMOKE_OK`; default Ocean still loaded. |
| `npm run fish:phase:validate -- p6` | PASS; all thirteen P6 prompt briefs satisfy runner grammar, with P3 assigned `0.6.3`. This did not execute the stack. |
| `git diff --check` | PASS; no whitespace errors. |

The target check asserts its physical mouth rules by diagnostic relocation, not by normal human input. No native gameplay window was played for this P3 run, no owner judgment or frame pacing was captured, and the P2 larval reticle usability failure remains deferred. Food remains concentrated near the nursery with no renewal; later P6 prompts must establish habitat distribution, fair threats, competing Bass and deeper exploration. Existing P5/C5 native gaps are unchanged.
