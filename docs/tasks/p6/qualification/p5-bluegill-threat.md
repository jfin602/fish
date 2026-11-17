# P6 P5 — ordinary Pond adult Bluegill threat

**Disposition: implemented T2 candidate at assigned `0.6.5`; G6.3/V46 NOT GREEN.** The owner-native cautious and exposed route comparison is reserved for P8. This is session-local gameplay, not P7 durable death.

## Entry and bridge

- Entry checkout: `main` at `42f19e6e637f29ea803888986dba0497f34f5755`, clean, root `0.6.4`. Previous P4 implementation checkpoint is `e668fdf` and [P4 evidence](p4-food.md) remains distinct. No reset, rebase, commit, push, world bake change or asset overwrite was performed.
- Fish's ignored local configuration selected `/home/jfin/dev/fish/game`, Godot .NET `4.7.2`, and `ws://127.0.0.1:19080`. The `9080` and `39080` listeners belonged to other projects and were left alone. Two stale Fish MCP processes were closed. That also closed this task's initial MCP transport; it was recovered with a fresh Fish-only MCP client on `19080` and `./tools/dev editor` for the same `game/` path. `godot_health_check` then reported `bridge_connected=true`, `editor_status=ready`, server/addon `2026.11.04`; `godot_get_server_info` reported peer path `/home/jfin/dev/fish/game/`, Godot `4.7.2-stable`, `csharp_supported=true`, `csharp_project=true`, `csharp_build=false`. Health was rechecked after the focused and regression checks and remained connected.
- The editor's filesystem rescan created the three new C# UID sidecars. Its incidental reorder of `project.godot` was restored; no project setting changed.

## Implemented behavior

- The normal Pond seed `71`, baked world `world-5e3b4fee759acbe3`, now has two individually identified adult Bluegill in a shallow school outside the 3.2 m nest sight radius, alongside the prior four larvae and two juveniles. All retain `PreyActor` physical movement, `BluegillSchool` routing and stable species/stage IDs. Pond Bluegill model scale now uses the same adult Bass world-length reference as the player; registered lengths and stage rule govern predation. The existing Bass mouth ledger can still consume eligible small Bluegill later.
- Adult Bluegill only hunt a registered Bass stage whose actual length is at most 25% of registered adult Bluegill length. In the current catalog, that includes Bass larva (0.058056 m versus 0.309825 m) and excludes juvenile (0.235077 m), subadult and adult Bass. Juvenile/subadult Bluegill cannot use this attack path. The rule checks species, stage and registered physical size together.
- Each adult has its own `BluegillThreatMind`, world-space sight and movement-cue field; no Pike target store, camera, reticle or screen fog enters the decision. Sight uses solid ray occlusion, seven Pond-water samples, solar light and visible batched plant mesh bounds. The 3.2 m notice range, 0.45 s repeated-sight notice, further 0.28 s strike delay, short loss-of-sight transition and 2.8 s last-evidence search provide a bounded escape opportunity. Fast fry movement can create a local cue; a still fry does not. Foliage attenuates rather than grants an absolute safe flag, and solid wood blocks a strike ray.
- Pursuit passes a remembered destination into the existing fish motor, with visible facing, turn rate and `MoveAndSlide` collision. A catch requires fresh strike readiness, registered eligibility, forward mouth-to-Bass capsule contact and clear body-to-mouth and mouth-to-Bass solid paths. One `PondCatchEvent` binds Bass life ID, predator ID, game time and contact position; the existing caught menu offers a new Bass life. The `_caught` guard prevents a second callback, and retry creates a new life with no retained catch event.

## Focused evidence and regressions

The final `./tools/dev build` passed with **0 warnings and 0 errors**. `./tools/dev inspect-p6-bluegill-threat` passed with `P6_BLUEGILL_THREAT_CHECK_OK` on seed `71`. Its trace used adult `world-5e3b4fee759acbe3:bluegill:adult:6`: exposed sight score `0.217`, three clear rays, soft transmission `1.000`; installed `FAF-PLANT-004C` concealment reduced transmission to `0.200` with three clear rays and prevented detection. It checked stationary versus fast cues, warning/chase/physical motor, loss-of-sight search/disengagement, pause freeze, camera-independent sight, registered size exclusions, solid strike obstruction, old identity rejection, one direct catch callback and a fresh retry life. The retained catch trace in `artifacts/logs/p6-bluegill-threat.log` has event `pond-world-5e3b4fee759acbe3:bass-life:39169b457bd54e23982d31e62491b5cb:caught:world-5e3b4fee759acbe3:bluegill:adult:6` at game time `5.90`; the second callback was blocked.

| Targeted command | Exact final result |
| --- | --- |
| `./tools/dev inspect-p6-pond-entry` | `P6_POND_ENTRY_OK`, seed `71`, 8 ecological Bluegill, 23 food actors, 11 ambient actors; two new life IDs differed. |
| `./tools/dev inspect-p6-bluegill-threat` | `P6_BLUEGILL_THREAT_CHECK_OK`, direct contact and retry trace above. |
| `./tools/dev inspect-bluegill-schools` | `BLUEGILL_SCHOOL_CHECK_OK`, fixture seed `5266`, 3 schools/16 fish, local scatter and regroup. |
| `./tools/dev inspect-prey` | `PREY_CHECK_OK`, fixture seed `5266`, hard cover, soft plant attenuation, fast versus slow cues and flee behavior. |
| `./tools/dev inspect-pike-sight` | `PIKE_SIGHT_CHECK_OK`, fixture seed `5266`, 66 plants and camera-stable world sight. |
| `./tools/dev inspect-feeding` | `FEEDING_CHECK_OK`, 16 food, 2 policy events, wall blocked. |
| `./tools/dev inspect-world-isolation` | `WORLD_ISOLATION_CHECK_OK`, two Ocean/Pond/Pike cycles, no cross-world predator, feeding or cover state. |
| `./tools/dev inspect-p6-food` | `P6_FOOD_CHECK_OK`, seed `71`, 9 patches, 23 actors and fresh retry life. |

The first Pond entry check exposed a `GetMeta` error when the fixture-only plant collector met normal Pond batched nodes; source now reconstructs real bounds from the batched Pond meshes and placements. The first obstruction diagnostic placed its test box over the ray origin, so Godot did not register a hit; the corrected perpendicular barrier was independently ray-verified before the blocked-strike assertion. Both failures and fixes remain in the local command logs; the final commands above passed.

## Explicit limits

The headless diagnostic arranges an exposed sight line and a direct mouth contact, and inserts a temporary solid barrier shaped like a log lip. It proves the sensing, collision and event rules but does **not** establish natural route frequency, visible warning readability, escape feel, foliage accuracy during a full swim, or native Forward+ performance. The adult count, range and timing are first-pass values for owner-native V46 review at P8; the ordinary Pond still lacks frog/snake hunting, competing Bass and independent NPC-on-NPC predation from later P6 briefs. G6.3 remains **NOT OBSERVED** until that native review.
