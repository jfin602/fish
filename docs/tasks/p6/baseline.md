# P6 `0.6.0` development baseline — 2026-11-15

**Disposition: accepted development checkpoint; P6 Living Pond implementation and G6.1–G6.8 are NOT STARTED.** The root `package.json` is `0.6.0`. This version records a reproducible starting tree, not a P5/C5/P6 Gate pass or a public release. [The roadmap](../../roadmap.md) remains the Phase/Gate status authority.

## Included state and provenance

- The checkpoint follows the published P6 S27 planning commits and the preserved local audio commit replayed as `e990a12` after resolving its `BOOT.md` overlap. The original local audio commit was `fd31517`; its source, assets, review log and historical evidence were retained. No P6 gameplay was added by this checkpoint.
- Concurrently published P6 assessment/plan and P1–P13 execution-brief commits were merged into the final `main` tree after the version checkpoint. The [P6 task stack](README.md) is authored and its prompt grammar validated; no P6 implementation prompt was run.
- The ordinary Ocean and Pond remain separate baked worlds; Pike Encounter remains the isolated seed-5266 regression destination. The existing C5 Bass hatch/fry, staged feeding/growth, automatic mouth-contact feeding, finite boost, Bluegill schools, Pike targeting and blood FX are present in the fixture. Pond cover/collision, dynamic waterline, follow camera, wake and positional audio/splash/bubble work are preserved. See [C5](../c5-polish/README.md), [wake](../c5-wake/README.md), [audio](../../systems/audio.md) and [swimming/camera](../../systems/swimming-and-camera.md) for implementation boundaries.
- Historical P5 P12 actually used `0.5.12`; assigned P13 `0.5.13` was not applied. [P5 closeout](../p5/closeout.md) stays owner-closed for progression / NOT GREEN. P2/P3/P4/c4 failures and accepted gaps remain in their original records. M8 owner-native C5 qualification and W3 wake qualification remain open. P7 persistence/permanent death and P8 Life History are deferred.

## Checks run for this checkpoint

The preflight found `main` in an interrupted rebase with a `BOOT.md` conflict. With owner authorization, the existing rebase was completed, retaining the P6 planning handoff and the local bubble-audio changes. Before validation, `main` was at `e990a12`, one commit ahead of `origin/main`; the remote was `https://github.com/jfin602/fish.git`. `tools/dev` composition was inspected so import/build were run once each. Local Linux/X11 used Godot `4.7.2.stable.mono.official.ed1daf0bf`, .NET SDK `10.0.403` and Forward+ project settings.

| Check | Result | Evidence / limit |
| --- | --- | --- |
| Godot import and C# build | **PASS** | `./tools/dev import`; `./tools/dev build` completed with zero warnings/errors and `AUDIO_PATHS_OK` for ten runtime files. |
| Main Ocean entry | **PASS** | `./tools/dev smoke-world` loaded baked Ocean seed 42 and emitted `FISH_SMOKE_OK`. Headless only. |
| Ocean, Pond and Pike entry/isolation | **PASS** | `./tools/dev inspect-world-isolation` emitted `WORLD_ISOLATION_CHECK_OK` for two Ocean/Pond/Pike cycles and three fixture retries, with encounter predator, feeding and cover absent from ordinary worlds. Headless only. |
| Automatic feeding and boost | **PASS** | `./tools/dev inspect-auto-feed-boost` emitted `AUTO_FEED_BOOST_CHECK_OK food=auto-once`, with ordinary and boosted speeds and remaining energy. Fixture-only, headless. |
| Positional audio and splash | **PASS** | `./tools/dev inspect-audio` emitted `AUDIO_CHECK_OK` for deep/shallow loops, mono sparse bubbles, pause/retry cleanup and motion/turn/boost/splash events. Audible mix quality is unobserved. |
| Dynamic waterline and wake path | **PASS** | `./tools/dev inspect-dynamic-waterline` and `./tools/dev inspect-wake-path` emitted their OK markers for moving surface sampling and four Bass stages. Headless checks do not establish native appearance or feel. |
| P6 prompt grammar | **PASS** | `npm run fish:phase:validate -- p6` accepted all thirteen authored briefs; the runner was not executed. |
| Editor MCP identity/live play | **BLOCKED** | Godot MCP `2026.11.04` reported `bridge_connected=false`; port `19080` was already in use. No editor mutation or live MCP play was attempted. |
| Owner visual/gameplay/performance qualification | **NOT OBSERVED** | No owner M8 playthrough, complete P5 three-zone route, C5 wake W3 review, sustained Forward+ frame-pacing run, or P6 V44–V51 scenario was run for this checkpoint. Earlier native evidence remains historical. |

The version and documentation were checked for consistency and local Markdown links; the final staged diff and whitespace check were reviewed before commit. The new version changes only the development baseline. The next implementation step is the approved P6 feeding reticle and truthful fish-mouth heading cue in the ordinary Pond, while preserving the fixture and all open native qualification debt.
