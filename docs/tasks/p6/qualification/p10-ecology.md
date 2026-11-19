# P6 task P10 — independent Pond ecology

**Disposition:** implemented and focused T2 candidate at assigned root `0.6.10`. G6.6/V49 remains **NOT GREEN** pending unassisted owner-native Forward+ observation and frame pacing at P12. P7 durable lives, P8 history, and final platform P9 were not started.

## Identity and editor preflight

- Entry was clean `main` at `23a0a6158fa355566d6e7a4a8e152eebc3990d53`, root `0.6.9`. The change is intentionally uncommitted for runner checkpoint review. No reset, rebase, commit, or push occurred.
- Replaced only the stale Fish P9 MCP client/server/editor on `ws://127.0.0.1:19080`; the other-project connection was untouched. The fresh Fish server and `./tools/dev editor` reported `godot_health_check bridge_connected=true`, editor ready, server/addon `2026.11.04b2`. `godot_get_server_info` identified live project `/home/jfin/dev/fish/game/`, Godot `.NET 4.7.2-stable`, `csharp_project=true`, `csharp_supported=true`. The bridge was rechecked during implementation and before final validation. Native editor GPU was AMD Radeon 880M, Forward+; no owner play or native frame pacing was measured.
- The current two-choice World Select routes ordinary Pond Bass into the seed-5266 Pond configuration of `p5_pike_encounter.tscn`; legacy direct-fixture checks retain their historical mode. The older `PondBassSession` seed-71 route and third menu card are no longer the active owner-approved Pond route.

## Implemented contract

- `PondEcology` records live individual actor ID, species, stage, registered length, physical radius/RID, actual world node/position and alive/player distinction. A spatial cell roster refreshes at most every 0.2 game seconds, holds at most 64 actors, and returns at most eight nearby candidates per hunter. A current target is retained inside that cap while valid. `PondEcologyDiagnostics` exposes actors, queries, candidate checks, sight samples, search states, claims and interactions without a gameplay overlay.
- Ordinary Pond uses a scene game clock even when the player has no new motor step. `MovementDisturbanceField.StepAt` advances cue expiry on that clock without inventing movement. Legacy fixture `Step` behavior remains unchanged. Pike, Bass rivals, adult Bluegill, frog and snake use existing world sight, finite search/pursuit and physical mouth checks against individual fish. Adult Bluegill may take size-eligible Bluegill larvae as well as very small Bass; Bass rivals may take eligible Bluegill or smaller Bass. Pike may take a rival Bass. Frog and snake remain shallow, stage-limited hunters.
- Normal Pond Bluegill and Pike have world-scoped stable identities. All NPC prey catches use one scene-owned prey claim before death, event and presentation. A consumed actor becomes hidden and noncolliding; other hunters discard its target, and the affected Bluegill school receives a local alarm. Bass rival feeding uses its own ledger exactly once; adult Bluegill has a five-second recovery after a catch. Player terminal events remain separate.

## Focused checks and retained trace

| Command | Actual result |
| --- | --- |
| `./tools/dev build` | Exit 0; `Fish.dll`, 0 warnings, 0 errors. |
| `./tools/dev inspect-p6-ecology` | Exit 0, `P6_ECOLOGY_CHECK_OK` on seed 5266. [Full trace](p10-ecology-t2.txt) records five distinct NPC predator IDs and five distinct Bluegill prey IDs: adult Bluegill → larva, snake → larva, adult Bass rival → juvenile, frog → larva, Pike → juvenile. Player stayed more than 20 m from every contact. The check used actual actor sight/motor/contact; it held selected NPC prey at known reachable positions for reproducibility. Frog and Pike close-contact phases were driven separately after three full-scene NPC interactions. |
| P10 claim and lifecycle cells | One failed claim followed by one successful claim and a rejected simultaneous second claim; consumed fish hidden and noncolliding, absent from nearby search. Focus loss held scene game time and counters. Scene exit freed the ecology owner. Invalid scene asset rejected; Pond frog/snake assets rejected for Ocean. |
| P10 bounded-work trace | Initial 22 actors, then 17 after five catches; 4,831 nearby queries, 33,483 candidate checks, 632 sight samples, five claims, 12,058 ms measured check-loop wall time in the retained final trace. These are bounded headless diagnostic counts and wall time, **not** native Forward+ frame-time results. |
| `./tools/dev inspect-p6-bass-rivals` | Exit 0, `P6_BASS_RIVALS_CHECK_OK`: three separate Bass IDs, juvenile escape, adult physical NPC/player catch, exactly-once feeding and retry-clear identities. |
| `./tools/dev inspect-pike-ai`, `inspect-prey`, `inspect-bluegill-schools`, `inspect-pike-disturbance` | Each exit 0 with `PIKE_AI_CHECK_OK`, `PREY_CHECK_OK`, `BLUEGILL_SCHOOL_CHECK_OK`, `PIKE_DISTURBANCE_CHECK_OK` respectively. Legacy fixture and cue-clock contracts remained intact. |
| `./tools/dev inspect-world-select` | Exit 0 with `WORLD_SELECT_CHECK_OK`; two destinations, seed-5266 Pond and Ocean default `FAF-FISH-001`. |
| `./tools/dev inspect-p6-frog`, `inspect-p6-snake`, `inspect-p6-bluegill-threat`, `inspect-world-isolation` | Each exit 1 before actor assertions: the first three still expect a seed-71 Pond `CharacterSelect`, while the last still requires a third Pike menu destination. These are pre-P8 route assumptions, also recorded in P9 evidence; P10's current-route check exercises their actors and two-world species boundaries. They should be migrated before reuse as current-route regressions. |
| `git diff --check` | Exit 0. |

The five retained event IDs are in the trace; for example, the Pike target was `world-6c98f9e946b4ede8:bluegill-juvenile:1`, and the adult Bass predator was `world-6c98f9e946b4ede8:bass-rival:adult:2`. No duplicate prey event, player catch screen, or second nutrition award occurred in the seeded check.

## Limits

The deterministic check verifies independent actor execution with the player distant and physical contacts under controlled reachable prey placement. It does not measure spontaneous encounter frequency, long-session population balance, visible fairness, owner control feel, or real Forward+ frame pacing. The older route-dependent checks remain failed at navigation assumptions. G6.6/V49 therefore stays NOT GREEN; P12 owns native observation and pacing.
