# P6 task P9 — solitary Bass rivals

**Disposition: implemented and focused T2 candidate at assigned `0.6.9`; G6.5/V48 remains NOT GREEN pending native fairness review.** This is the owner-approved ordinary seed-5266 Pond route, not the legacy direct Pike Encounter fixture. P7 durable life, P8 history and platform P9 qualification were not started.

## Entry and live editor

- Entry was clean `main` at `e8c29c445a3abf4561145f7b8b9d6f6e7ee1aad0`, root `0.6.8`, local branch ahead of `origin/main` by one commit. The implementation is an uncommitted dirty tree. No reset, rebase, commit or push was performed.
- A fresh task-owned Fish Godot MCP server and .NET editor were connected on `ws://127.0.0.1:19080` after recovery. The live `godot_health_check` returned `bridge_connected=true`, `editor_status=ready`; `godot_get_server_info` returned project path `/home/jfin/dev/fish/game/`, Godot `4.7.2-stable (official)`, `csharp_project=true`, `csharp_supported=true`, matching server/addon `2026.11.04b2`. The separate editor on port `9081` was left untouched. The direct task MCP client remained connected after focused validation. The built-in connection had closed during recovery; it was not used as a substitute for the verified direct client.

## Implemented behavior

- Normal Pond spawns exactly three FAF-FISH-007 rivals: juvenile `world-6c98f9e946b4ede8:bass-rival:juvenile:0` at `(-16, 7.543, 22)`, subadult `...:subadult:1` at `(-10, 1.961, -1)`, and adult `...:adult:2` at `(-5, 1.351, -15)`. Each has its own growth/body fit, collider, imported visual/animation, movement, local cover, sight memory, feeding ledger and stable world-scoped ID. None belongs to a Bluegill school. Spawn probes reject solid overlap, insufficient depth, shore proximity, and nearby actors. The older direct fixture remains at zero rivals.
- Registered stage length and a 0.38 prey-to-predator length cap allow subadult/adult Bass to hunt larval/juvenile Bass only. World-space sight, solid/soft cover, short search memory, bounded turning/swimming and forward physical mouth contact govern pursuit. Player and NPC prey share the size gate, with separate per-actor ledger and exactly-once predation events. The player can eat an eligible smaller rival with the existing automatic mouth pathway. Invalid or consumed targets drop from selection. Pause holds motion; leave/retry clears target and sight state.
- Adult Bass collision is radius `0.223 m`, height `1.052 m`, fitted to the registered adult silhouette. Three rivals add at most four target sensors per actor; the controlled check observed 94 total samples. This is an actor-count observation, not a native frame-time budget.

## Focused results

| Command | Observed result |
| --- | --- |
| `./tools/dev build` | Exit 0; `Fish.dll`, 0 warnings, 0 errors. |
| `./tools/dev inspect-p6-bass-rivals` | Exit 0; `P6_BASS_RIVALS_CHECK_OK` on seed 5266. Tests registered size/diet negatives, no school membership, spawn/model/collider fit, solid occlusion and sight recovery, 5.64 m juvenile motor escape, finite search, adult movement and one NPC catch (`nutrition=6`, one event), adult mouth catch of a larval player (`nutrition=8` total), pause, and retry with the same rival IDs and a new player life. [Full retained trace](p9-bass-rivals-t2.txt). |
| `./tools/dev inspect-growth` | Exit 0; `GROWTH_CHECK_OK`, legacy fixture `bass_rivals=0`, adult player `nutrition=32`, 11 feed events. |
| `./tools/dev inspect-world-select` | Exit 0; `WORLD_SELECT_CHECK_OK` and `POND_SOLID_FOOD_CHECK_OK`. Two choices, ordinary Pond seed 5266 with three rivals, and existing Ocean default `FAF-FISH-001`. |
| `./tools/dev inspect-bluegill-schools` | Exit 0; `BLUEGILL_SCHOOL_CHECK_OK`, 3 schools and 16 fish in the legacy direct fixture, no rivals. |
| `./tools/dev inspect-p6-bluegill-threat` | Failed before its threat assertions: the older check expects a seed-71 `CharacterSelect` after choosing Pond; P8 now enters the seed-5266 Pond directly (`Pond selection missing`). This is a historical route mismatch, not a P9 Bass result. |
| `./tools/dev inspect-world-isolation` | Failed before its isolation assertions: the older check requires the Pike Encounter to remain a third menu destination (`encounter became a habitat`); P8 owner-approved two direct destinations. Current route check above passes. |
| `git diff --check` | Exit 0; no whitespace errors. |

The diagnostic's NPC event was `world-6c98f9e946b4ede8:bass-rival:adult:2:ate:world-6c98f9e946b4ede8:bass-rival:juvenile:0`. Its player event targeted `pond-world-6c98f9e946b4ede8:bass-life:cec5c12bad2e4fe99f27a2d2b2b50546`; retry life was `pond-world-6c98f9e946b4ede8:bass-life:b7bdba830afe4197bafc305de91a08c3`. The test explicitly advances controlled `now` values while scene physics is held, so the diagnostic outcome line's `seconds=0.00` is not a gameplay duration measurement.

## Limits

The test places an exposed juvenile and player near the adult, then drives their motors and sensing. It establishes physical eligibility, escape, contact, event and retry behavior. It does not measure unassisted encounter frequency, difficulty, visible adult silhouette or sustained frame pacing in owner play. No T3 fairness or complete V48 PASS is claimed. The two historical route checks need migration if they are used again as current Pond regressions; their failures do not qualify those older scenarios. P10 multi-species NPC competition remains separate.
