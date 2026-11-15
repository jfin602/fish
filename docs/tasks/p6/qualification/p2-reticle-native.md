# P6 P2 — Owner native reticle and feeding aim review

**Status: owner-closed for progression / NOT GREEN; R1 repair remains a focused-check candidate with native retry deferred to P12.** This is the isolated Pike Encounter review before normal Pond integration. The owner reported that the reticle can stay lined up with food throughout an approach and the fish still misses. G6.1/V44 remains open. The original failure, earlier stop and later owner progression decision are retained below.

## Session identity and preflight

| Field | Recorded value |
| --- | --- |
| Source branch and entry revision | `main`, `1da76d9590d91d5f2c1910d9f197f8a41990e969` (`0.6.1`); ahead of `origin/main` by one commit |
| Entry dirty state | Clean (`git status --short --branch` showed no changed paths) |
| Version at P2 entry | `0.6.1` |
| Host and renderer | Linux/X11; Godot `4.7.2.stable.mono.official.ed1daf0bf`; Vulkan Forward+ on AMD Radeon 880M (editor startup) |
| Fish editor bridge | Previous Fish editor and listener on `127.0.0.1:19080` were stopped after PID/project verification. New task-owned editor and MCP server use `/home/jfin/dev/fish/game/` and `ws://127.0.0.1:19080`. `godot_health_check` returned `bridge_connected=true`, `editor_status=ready`; `godot_get_server_info` returned server/addon `2026.11.04`, live peer `/home/jfin/dev/fish/game/`, Godot `4.7.2-stable`, and `csharp_project=true`. The unrelated `9080` listener was untouched. |
| Fixture | Third World Select destination, Pike Encounter; baked seed `5266`, world `world-6c98f9e946b4ede8` (from P1 fixture evidence) |
| Actors | Player Bass `FAF-FISH-007`, life identity `pike-5266:player-bass`; prey Bluegill `FAF-FISH-006`; small foods include zooplankton and aquatic nymphs. Prior P1 nymph event `pike-5266:FAF-INSECT-001A:0` is historical test evidence, not a P2 observation. |
| Intended stage route | Egg hatch or skip → larva → juvenile → subadult/adult, as reachable during owner play |
| Control presets to compare | Keyboard Only, Mouse Only, Hybrid 2 (independent orbit), Adaptive Follow (Shift free-look); current saved preset and any changes to be recorded during play |
| Viewport/display | Native `Fish (DEBUG)` window measured `1920×1200` on the `1920×1200`, 60 Hz X11 eDP display; `FISH_DISPLAY_MODE requested=windowed-fullscreen source=world-select effective=Fullscreen`. Owner may change viewport during play; record any change. |
| Native screenshot or pacing capture | None yet |

## Focused preflight

P1's [reticle evidence](p1-reticle.md) reports passing projection, committed-feed and duplicate-consumption checks, plus an agent screenshot. Those checks do not establish owner usability or V44. This P2 session will record its own actual targeted command results below.

| Command | Exact result |
| --- | --- |
| `./tools/dev inspect-p6-reticle` | PASS: `P6_RETICLE_CHECK_OK stages=4 zooms=2 viewport=(1280, 1280) feed_events=1 confirmations=1 seed=5266`; fixture loaded seed `5266`, nymph `pike-5266:FAF-INSECT-001A:0` awarded nutrition `2`. This is a headless check. |
| `./tools/dev inspect-auto-feed-boost` | PASS: `AUTO_FEED_BOOST_CHECK_OK food=auto-once ordinary=1.400 boosted=2.520 energy=0.71`; fixture loaded seed `5266`, nymph IDs `:0` through `:6`, then Bluegill larva `pike-5266:bluegill-larva:0`, with subadult growth. This is a headless check. |
| `./tools/dev run --display-mode=windowed-fullscreen` | Build succeeded with `0` warnings and `0` errors; launched Godot `4.7.2` Vulkan Forward+ on AMD Radeon 880M. Main World Select reported `FISH_WORLD_SELECT_READY entries=3 habitats=2`; native Fish game window is open and has been left running for owner play. |

## Owner play matrix

Enter the third **Pike Encounter** destination. The center cross is a view reference; the original separate diamond shows projected mouth heading when it diverges. In R1, a nearby eligible food ring, mouth-path dot and distance label replace that diamond while a food candidate is guided. Food is consumed by actual forward mouth contact, with no bite key. Record the active preset, viewport, stage, food/prey ID if visible in logs, and whether the food was physically reachable. Leave expected and observed cells blank until the owner tests each row.

| Scenario | Expected | Observed |
| --- | --- | --- |
| Hatch normally; skip hatch; pause, resume and refocus |  |  |
| Tiny larva and later adult: readability in bright air and murky water |  |  |
| Centered follow, camera lag, mouse/keyboard swim and zoom |  |  |
| Hybrid 2 independent camera orbit |  |  |
| Adaptive Follow Shift free-look and return |  |  |
| Surface crossing and viewport change |  |  |
| Larval Bass approaching tiny food; valid forward contact and pulse |  | Owner report: “the reticle doesnt help with guiding the fish to the food. you can keep it lined up the whole time and still miss.” Follow-up identified the **larva** stage and **tiny food**. Exact food kind, control preset, target position, obstruction and feed log were not supplied, so this establishes misleading aim guidance, not a proved failure of a physically eligible contact. |
| Food at camera center but away from mouth heading; edge/side/behind targets |  |  |
| Food or Bluegill behind true solid obstruction |  |  |
| Later size-eligible Bluegill through the front mouth |  |  |
| Repeat contact with the same consumed food/prey; nutrition, event and pulse count |  |  |
| Native frame pacing during sustained swimming and feeding |  |  |
| Return to World Select; ordinary Ocean/Pond routing |  |  |

## Owner decision

- Reticle alignment/readability and false-positive/false-negative experience: **FAIL for aim usability.** The owner can maintain visual alignment and still miss food. Exact physical eligibility of those misses remains unmeasured.
- Native frame pacing: **awaiting owner observation**.
- Proceed to P3 normal-Pond integration or STOP for bounded reticle repair: **STOP under this prompt's failure rule.** The owner has not accepted the aiming interface or authorized P3 progression.
- P2 manual checkpoint/version: **not applied**.

The fixture is ephemeral and cannot qualify ordinary Pond V44 by itself. The game launch, automated checks and any agent screenshot cannot stand in for owner swimming and feeding.

## Bounded repair plan

1. Reproduce one aligned-but-missed **larval tiny-food** approach in the same seed `5266` fixture. Capture the preset, viewport, target stable ID and physics-frame fish mouth/forward, target position, camera pose, obstruction and actual `FoodReachable` result. Record whether the miss is depth/parallax, a moving target, insufficient forward contact range, a solid obstruction or an eligibility timing issue; do not assume the feeding rule is wrong from screen overlap alone.
2. Replace the single fixed-distance mouth-heading diamond as the sole aim aid. It currently projects only `mouth + forward * max(0.8 m, 4 × body reach)` and smooths that screen point. A point aligned with a food image cannot establish the food's 3D position or contact. Design a short depth-aware mouth path or candidate-relative offset using only stage-eligible, actually visible food; show no positive contact cue until the unchanged authoritative `FoodReachable`/ledger path can consume. Suppress cues for occluded, behind-camera, unavailable and duplicate targets. Keep the reticle read-only and input-transparent.
3. Extend the targeted reticle check with a **larval** visually aligned but physically off-path tiny-food target at different depth, a genuinely reachable larval front target, side/behind/blocked targets, camera orbit/lag and stage-sized mouths. Assert that visual alignment never claims feed eligibility and one committed feed yields one pulse. The current P1 check projects all four stages but switches to **juvenile** before its one nymph feeding assertion; it does not cover the owner's failed larval approach. Preserve the existing auto-feed/boost and fixture checks.
4. Rebuild and repeat the focused checks, then relaunch native Forward+ at the same viewport for the owner to retry the exact failed approach plus Hybrid 2 and Adaptive Follow. Record observed misses and native pacing. Resume P3 and apply `0.6.2` only after explicit owner acceptance and a legitimate reviewed manual checkpoint.

**Source-based hypothesis, not a native diagnosis:** the P1 check proves the diamond projects its own chosen 3D endpoint, and one separately placed nymph feeds. It does not test whether a food image visually aligned with that endpoint lies on the physical mouth path. The fixture's food rule requires the food center within `1.5 × MouthRadius` of the current mouth, a forward dot product of at least `0.65`, and an unobstructed path. The cue has no target-depth or contact-range information. This mismatch can explain the owner's report, but the specific missed contacts were not logged.

## R1 addendum — 2026-11-15 larval tiny-food repair and owner retry

### Entry and live bridge

R1 began on `main` at `1da76d9590d91d5f2c1910d9f197f8a41990e969`, ahead of `origin/main` by one commit, root `0.6.1`. The existing dirty `BOOT.md`, `docs/roadmap.md`, `docs/tasks/p6/README.md` and this untracked P2 evidence file were preserved. The prior Fish-only editor/server pair (`153085` / `153043`) on `19080` was identified by process environment and stopped; the unrelated `9080` listener was untouched. This task started its own Fish MCP server from the project-local configuration and `./tools/dev editor`. `godot_health_check` reported `bridge_connected=true`, `editor_status=ready`, server/addon `2026.11.04`. `godot_get_server_info` reported live path `/home/jfin/dev/fish/game/`, Godot `4.7.2-stable`, `csharp_project=true`, `csharp_supported=true`, `csharp_build=false`. The bridge was rechecked after the focused work and remained connected.

### Reproduced geometry and repair

The seed `5266` Pike Encounter headless fixture reproduced a larval visual alignment miss in a controlled physics-frame sample. At physics frame `9`, game time `12.100 s`, player `pike-5266:player-bass` with preset `builtin:adaptive-follow` and `1280×1280` viewport faced approximately `(0, 0, -1)` from mouth `(0, 5.65905, 19.962847)`. Zooplankton `pike-5266:pond:zooplankton:0` was at `(0, 5.694297, 19.662848)`. Camera origin was `(0, 5.85905, 22)`, pitched toward the fish. The food image and old diamond aligned to under `0.1 px`, but the food was `0.3021 m` from the mouth against `0.0180 m` contact reach, with a `14.2 px` separation between the food and the projected mouth path at that food's depth. The forward dot was `0.9932`; a solid-obstruction ray was clear. `TryBiteFood` returned `out of reach or blocked` without a feed. Before the `12 s` yolk threshold it separately returned `food is not available at this stage`; at `12.100 s` the diet award was `1`. The test froze actor motion for this measured frame, so it establishes a projection/range explanation for an aligned miss, not the exact cause of the owner's earlier moving native approach.

The reticle now uses a nearby stage-eligible food candidate to draw a restrained ring at the actual food image, a dot at the mouth path at that food's depth, a connector when they separate, and a mouth-distance label. The fixed-distance diamond remains only when no candidate is guided. Candidate selection is limited to available, unconsumed, visible-in-tree food within `1.2 m`, forward of the mouth, on screen, in front of the camera and clear of solid obstruction from both mouth and camera. The cue is neutral; only a committed feed triggers the existing pulse. Motor, camera controls, food placement/drift, diet, contact, clock and ledger were not changed. The `--p6-reticle-check` fixture now starts the real larval diet instead of juvenile, without changing normal Pike Encounter startup.

### Focused commands and outcomes

`tools/dev` composition was inspected first: `build` checks local audio paths and builds `Fish.csproj`; the three `inspect-*` commands launch only the Pike Encounter headless scene with their targeted flag; `run` builds once and launches the real project. No broad aggregate ran.

| Exact command | R1 result |
| --- | --- |
| `./tools/dev build` | PASS, final C# build `0` warnings and `0` errors. |
| `./tools/dev inspect-p6-reticle` | PASS, `P6_RETICLE_CHECK_OK stages=4 zooms=2 viewport=(1280, 1280) larval_feed_events=1 feed_time=12.100 confirmations=1 seed=5266`. The test covers yolk suppression, aligned off-path miss, orbit/follow offset, behind-camera/offscreen, side/behind, bed-blocked, unavailable and duplicate food; stage-sized mouth path at four stages and two zooms; one automatic larval zooplankton feed awarded `1` nutrition and one pulse. |
| `./tools/dev inspect-auto-feed-boost` | PASS, `AUTO_FEED_BOOST_CHECK_OK food=auto-once ordinary=1.400 boosted=2.520 energy=0.71`. The juvenile nymph and later Bluegill sequence remained intact. |
| `./tools/dev inspect-feeding` | Initial run reached `FEEDING_CHECK_OK` but the command failed on two leaked headless audio playback objects at immediate shutdown. The focused test now silences its headless feed player before the scene bites. Final PASS: `FEEDING_CHECK_OK food=16 juvenile_nutrition=8 policy_events=2 wall=blocked`. No production audio behavior changed. |
| `./tools/dev run --display-mode=windowed-fullscreen` | PASS launch: build `0` warnings/errors, Godot `4.7.2` Vulkan Forward+ on AMD Radeon 880M, `FISH_DISPLAY_MODE requested=windowed-fullscreen source=world-select effective=Fullscreen`, `FISH_WORLD_SELECT_READY entries=3 habitats=2`. The real native game window was left open at World Select for owner play. |

Intermediate reticle attempts exposed the prior juvenile-only feeding assertion and the same immediate-shutdown headless audio leak after the new larval feed marker; both were corrected in the targeted checks. The final reticle and feeding commands exited cleanly. The test clock was advanced to `12.100 s` after first asserting yolk-period ineligibility, then the actual scene physics fed the relocated zooplankton once; this headless setup does not establish native feel.

### R1 owner-native review and progression

The owner should enter the third Pike Encounter destination, retry larval tiny food first, then inspect centered follow, Hybrid 2 orbit, Adaptive Follow free-look, zoom, surface crossing, pause/refocus, later food and Bluegill if reachable. Record actual alignment, misses, successful feeds, viewport/preset and frame pacing here when reported. **No R1 owner play result or acceptance has been received yet.** The original owner failure above remains valid history. If aiming remains misleading, stop with that observation and a narrower repair. P3 normal-Pond integration is stopped; G6.1/V44 remains NOT GREEN; root stays `0.6.1`, and `0.6.2` and a manual runner checkpoint await owner acceptance and legitimate review. No commit, push or runner resume was performed. Unobserved native cases remain the owner matrix above, including adult/Bluegill, camera feel, surface/focus behavior and pacing.

### Owner progression decision — 2026-11-15

After reviewing the R1 candidate and its focused evidence, the owner explicitly chose to defer further larval tiny-food aim repair and native retry until the end of the P6 runner phases, and authorized the `0.6.2` checkpoint so P3 can proceed. This supersedes the earlier P2/R1 stop for **progression only**. The original native usability failure is still open; the R1 candidate is retained in source, but the owner has not reported a repaired native feed, accepted its aim quality, or supplied frame-pacing results. G6.1/V44 remains **NOT GREEN**. Revisit the larval tiny-food approach in the final native P12 review, repair again if it remains misleading, and keep P13 closeout evidence-based. P3 may integrate the current presentation candidate into normal Pond without treating this deferral as a feeding Gate PASS. The exact `0.6.2` checkpoint is a direct owner-authorized manual runner handoff; no P3 execution is included in this decision.

Before the manual checkpoint, the Fish MCP bridge was rechecked: `godot_health_check` remained connected and ready at `ws://127.0.0.1:19080`; `godot_get_server_info` still identified `/home/jfin/dev/fish/game/`, server/addon `2026.11.04`, Godot .NET `4.7.2`, and a C# project. `npm run fish:phase:validate -- p6` passed the 13-prompt grammar with P2 `0.6.2` and P3 `0.6.3`; `git diff --check` passed. This validation checks runner metadata, not native aim.

At the deferred handoff, the previously launched native Fish game process was no longer running; no R1 owner play observation had been supplied. The Fish editor and MCP bridge remained connected through the checkpoint preflight and can be retired after the commit so P3 establishes its own verified bridge.
