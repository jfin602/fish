# P5 P7 — Shared prey sensing and evasive behavior

**2026-11-13 source result: PASS at T2; native owner play: NOT OBSERVED.** Pre-task `main` HEAD was `bfec1c6d5b86bb091fb1f10ccf23d815b0d8d1de`, clean, root `0.5.6`. [P4 closeout](../../p4/closeout.md) authorized P5 progression. [P6 native review](native-m1.md) records the actual manual stop, failed candidates, later catch/escape, and the owner's explicit `0.5.6`/runner-continuation request. P6 remains NOT GREEN. This P7 implementation is uncommitted for the runner checkpoint.

The Fish CLI identified `/home/jfin/dev/fish/game`, Godot `4.7.2.stable.mono.official`, .NET `10.0.403`, and the local `2026.11.04` addon. Editor MCP `2026.11.04` reported `bridge.connected=false` at `ws://127.0.0.1:19080`; no editor mutation or native owner gameplay is claimed.

## Implemented behavior

- The isolated Pike Encounter now contains a physical Largemouth Bass **larva** from registered `FAF-FISH-007`. The verified Pond stage is `0.058056 m` long and has actual Swim/SwimFast clips. `SpeciesCompatibility.TryResolvePondAsset` and the owner export hash gate its use. The larger adult Pond roster remains unchanged; no fabricated species, nutrition or consumption is present.
- The prey receives its own `WorldSightSensor` and `MovementDisturbanceField` against the player, using the same Pond turbidity, solar state, real mesh cover bounds, solid ray occlusion and motor-derived cues as the Pike. Prey awareness stores only recent sensed coordinates, expires after `2.8 s`, and clears on fixture retry/exit. Pausing freezes decisions and motion.
- Physical `CharacterBody3D` steering seeks cover on the live generated bed. It prefers coontail and rooted thicket over sparse pondweed, chooses from actual visible plant bounds, tests each step against collision, turns at a bounded rate and slides around wood. Root-ball and fork passages are physical; sealed split wood blocks the body. The prey is encounter-local and does not change either ordinary habitat.
- Rapid fixture retries exposed a Godot cached-resource handle failure while instancing ordinary Pond dressing. Instanced Pond GLBs and prey GLB now load with cache ignore, the same pattern already used for baked subscene loading. This changes resource loading, not baked content or FAF sources.

## Deterministic evidence

Fixture seed `5266`, Pond world `world-6c98f9e946b4ede8`, baked content SHA-256 `f056c3216989617bb02565ee98a2bbf41d2a72373d11dc0c6a09703e417cd072`. Source SHA-256: `PreyActor.cs` `6db2251c`, `PreyAwareness.cs` `1b2c9690`, `PikeEncounter.cs` `b864e0ed`, `PondFafEnvironment.cs` `5815f6d9`, `PreyChecks.cs` `5525f9ee`, `PikeEncounterChecks.cs` `df8c7c97`, `tools/dev` `2d2a2357`.

| Actual command | Result and limit |
| --- | --- |
| `./tools/dev build` | **PASS**, zero warnings/errors on the final source. |
| `./tools/dev inspect-prey` | **PASS.** Exposed Bluegill sight strength `0.2521`; cautious ribbon-grass and long-tree lines both `hard-cover/0.0000`; a clear line through visible plants loses `0.1573` strength. Fast open movement cue `0.4454`, slow approach cue `0.0000`. Registered larva eligibility, camera-independent query, sight and cue driven flight, memory/pause/reset, 4.14 m physical flight, coontail goal `FAF-PLANT-012A`, no sampled wood penetration, root refuge gap, real fork clearance and sealed split wood all passed. These are scripted positions and movement, not a native stealth verdict. |
| `./tools/dev inspect-pike-ai` | **PASS**, Pike patrol/chase/search, catch and hidden escape regression. Headless outcomes only. |
| `./tools/dev inspect-pike-cover` | **PASS**, unchanged 47 placements, all ten FAF families, Bluegill/Pike hollow traverse and zero sampled camera hits. Scripted paths. |
| `./tools/dev inspect-pike-encounter` | **PASS** after loader repair; three routes/two habitats, three fixture retries, old prey actor freed on retry, return and ordinary Pond/Ocean paths. An earlier run **FAILED** with `Handle is not initialized` during repeated Pond dressing instantiation; retained here as historical failure. |
| `./tools/dev inspect-species` | **PASS**, five selectable fish per habitat and fixture isolation. |
| `git diff --check`; `bash -n tools/dev` | **PASS** after documentation reconciliation. |

The check uses a fixed world pose and synthetic motor input for its careful/fast cue comparison. Native prey visibility, player-controlled stalking, natural route readability, sustained frame pacing, and owner fairness judgment remain **NOT OBSERVED** for P8. P6's M1 gaps, historical P2–P4 failures and all P5 Gates remain open. Root `0.5.7` records passing source behavior only.
