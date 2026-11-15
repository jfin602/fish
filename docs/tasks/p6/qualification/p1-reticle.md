# P6 P1 — Feeding reticle and truthful mouth heading

**Disposition: implemented T2 candidate at assigned `0.6.1`; G6.1/V44 NOT GREEN.** P2 owner-native aim review is still required. P3 must attach this reusable renderer to the future normal-Pond Bass session; P1 does not create that session.

## Source and bridge identity

- Entry was clean `main` at `3e85d4bb9aff3b3c8769a8ce4f07cfd595d45783`, tracking `origin/main`; root version `0.6.0`. Remote: `https://github.com/jfin602/fish.git`. No existing source changes were carried into P1.
- Linux/X11, Godot `4.7.2.stable.mono.official.ed1daf0bf`, .NET SDK `10.0.403`, Forward+, AMD Radeon 880M. The task-local Fish `.codex/config.toml` and `tools/local.env` both selected `ws://127.0.0.1:19080` and `/home/jfin/dev/fish/game/`. An older Fish-owned MCP process, PID `122221`, held that port and was stopped after its process environment confirmed the Fish path. The first built-in task MCP connection had `bridge_connected=false` and `bind_error=address already in use`. A fresh task-owned direct MCP client/server on the same Fish project and port then reported `bridge_connected=true`, addon/server `2026.11.04`, Godot 4.7.2, C# project support and live peer `/home/jfin/dev/fish/game/`; health remained ready after the checks. The unrelated listener on `9080` was left alone.
- Fixture world manifest `world-6c98f9e946b4ede8`, seed `5266`, content `f056c3216989617bb02565ee98a2bbf41d2a72373d11dc0c6a09703e417cd072`, `generator_invocations=0`. Player Bass `FAF-FISH-007`, identity `pike-5266:player-bass`; prey Bluegill `FAF-FISH-006`. The verified nymph event was `pike-5266:FAF-INSECT-001A:0`.

## Implemented behavior

- `FeedingReticle` is a reusable viewport-sized, input-transparent Control on layer 9, beneath the fixture menu on layer 10. It draws a small contrast-outlined center cross and, when separated, a distinct projected diamond. It reads the current camera and interpolated fish pose with fitted `BodyForwardReach`; behind-camera and offscreen headings are suppressed and visible positions are smoothed. It never queries food, steers, changes reach or awards nutrition.
- The fixture attaches the renderer and supplies its active-play policy. Egg intro, hatch lock, paused/menu, disarmed/focus-lost, ended and transition states hide it. Only the committed `FeedResult.Consumed` branches for real food and Bluegill call the brief confirmation pulse; the zero-nutrition shell, invalid contact and duplicate attempts do not.
- The renderer's `Attach` entry point takes a fish, camera and session-active predicate so P3 can use it without copying the fixture script. Physical mouth contact still reads the same fitted fish body; only its read-only position accessor was shared.

## Checks and observed results

| Actual command or action | Result and scope |
| --- | --- |
| `./tools/dev import` | PASS; Godot project import completed. Generated C# UID sidecars retained. |
| `./tools/dev build` | PASS; Fish .NET build with zero warnings/errors; audio path check found ten runtime SFX files. Rebuilt after source/check corrections. |
| `./tools/dev inspect-p6-reticle` | PASS, `P6_RETICLE_CHECK_OK stages=4 zooms=2 viewport=(1280, 1280) feed_events=1 confirmations=1 seed=5266`. Tested larva/juvenile/subadult/adult fitted mouths, near/far ray projections, camera divergence, five preset selections, offscreen/behind suppression, viewport size and lifecycle policy. One real nymph consumption awarded nutrition 2 and one pulse; a duplicate attempt awarded neither. The initial attempt failed because the test offset projected offscreen; a later run exposed the zero-size CanvasLayer Control and prompted the viewport-size fix. Final check passed. |
| `./tools/dev inspect-auto-feed-boost` | PASS, `AUTO_FEED_BOOST_CHECK_OK`; one automatic food feed, growth to subadult, one Bluegill consumption, ordinary speed `1.400`, boosted `2.520`, energy `0.71`. Existing physical feeding and boost behavior remained intact in the fixture. |
| `./tools/dev inspect-hatch` | PASS, `HATCH_CHECK_OK`; watched and skipped intro, stable Bass identity, zero-nutrition shell, pause/retry cleanup, camera continuity and mobile fry. |
| `./tools/dev inspect-control-presets` | PASS, `CONTROL_PRESET_CHECK_OK` for five built-ins and corrupt-profile recovery. Its expected corrupt-file warnings appeared during negative cases. |
| `./tools/dev inspect-input-lifecycle` | PASS, `INPUT_LIFECYCLE_OK`; Ocean input/focus/capture route. |
| `./tools/dev smoke-world` | PASS, `FISH_SMOKE_OK`; baked Ocean seed 42 loaded independently. |
| `./tools/dev inspect-world-isolation` | PASS, `WORLD_ISOLATION_CHECK_OK`; two Ocean/Pond/Pike cycles and three fixture retries retained separate predator, feeding and cover scope. |
| Live Fish editor play and screenshot | A Forward+ Pike Encounter game window loaded; Enter completed the hatch flow. The initial capture revealed a zero-size reticle, which was fixed. The later live UI probe reported `visible=true`, rect `1280×800`; a captured `1920×1200` game image showed the center cross and separate heading diamond. Local ignored capture: `artifacts/p6-reticle-native.png`. This is an agent visual sanity check, not the P2 owner playthrough. |

Logs for the executed CLI checks are under ignored `artifacts/logs/`. The task-owned MCP connection was healthy after the final check. `git diff --check` was clean before checkpoint review.

## Open limits

- P2 owner must judge whether the cue is readable and useful during sustained real feeding, follow lag, Hybrid 2 orbit, Adaptive Follow free-look, surface crossing, viewport changes and bright air/murky water. The limited screenshot and headless projection checks cannot establish that feel or full contrast range.
- Normal Pond does not yet host a Bass survival run or this reticle; P3 owns that integration. G6.1/V44 remains incomplete and no other P6 Gate changes state.
