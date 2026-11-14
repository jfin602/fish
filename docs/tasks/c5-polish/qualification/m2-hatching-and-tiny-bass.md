# C5 M2 — Bass hatch and controllable fry

**Status, 2026-11-13:** revised implementation and headless T2 checks PASS. The owner played the first M2 build and reported fry phasing, jumpy movement, a camera-reactive distant-water artifact, and an egg/camera jump cut. The owner then reported that the camera could not rise above the surface, the shell opening faced away from the emerging fish, and an unwanted black dot floated to the right. A later owner screenshot confirmed the shell still opened away from the fry after the first orientation change; its check had assumed the wrong visible axis. These are native FAIL observations. After the shell correction, the owner asked for a denser shallow shoreline nursery and the egg moved into it, then reported that the egg floated above the bed and asked for a clear fry exit. Those follow-ups are implemented and checked locally; owner play of the latest source remains **NOT OBSERVED**. Stop before M3. No P5 Gate is promoted.

## Follow-up corrections on current source

- At this follow-up start, `main` and `origin/main` were at `d6e8d2d06946ce115e79e953c20fbc39311ba680` (`M2`), root package `0.5.12`, and the worktree was clean. The earlier M2 implementation below is now in that commit; this follow-up leaves only local edits. No commit, push, reset or rebase was made.
- `FishController.ConstrainPondAscent` now accepts a body-scale approach band. Ordinary adult Pond movement retains the 35 cm band, while the fry uses 3 cm. This lets normal upward swimming bring the follow camera into air in finite time while the authoritative fish collider remains under its center limit. The camera itself is not constrained to the water; the change applies to any future or current Pond fish body scale.
- The Bass egg prop now faces the visible notch of the opened shell along the larva's emergence path, using the +Z prop axis shown by the owner's screenshot. The previous -Z assumption passed the headless orientation check but failed visually. The export's offset embryo mesh is hidden from the start, leaving the two animated shell halves visible. The shell remains after hatch and is consumed only by the existing one-time bite.
- New headless `inspect-hatch` assertions check the screenshot-derived visible opening axis, exactly two visible shell halves, hidden embryo mesh, and a 240-frame real-input fry ascent whose camera crosses the Pond waterline while the fish collider does not. `inspect-pond-surface`, `inspect-pond-camera-immersion`, `inspect-growth`, and `inspect-world-isolation` also PASS. A Fish Forward+ CLI render in Xvfb/Mesa llvmpipe now shows the opened slit toward the fry after the intro, with the shell still present (`artifacts/logs/egg-visual-forward-22s.png`, ignored). This is a local software-rendered frame, not owner play. Corrected native feel and water remain NOT OBSERVED.

## Owner-directed shallow shoreline nursery follow-up

- On `main` at `d6e8d2d06946ce115e79e953c20fbc39311ba680`, root package `0.5.12`, the owner asked for denser grass/reeds, small sticks and some logs along the shore, with the egg moved into that habitat. Existing local M2 corrections were preserved.
- The egg/nest moved from `(-18, 26)` at about 2.0 m depth to `(-24, 26)` at about 1.1 m depth and 2.4 m inside the actual Pond shoreline. The near-nest grass clump expanded, with plant positions checked against the submerged bed. The larva exit and edible shell still share the nest.
- `PikeCoverFixture` now places a separate, deterministic `ShoreGrowth` band around both sides of the baked encounter shore, using registered Pond plants and wood. Irregular clump spacing and scale produce 113 grasses/reeds/pond plants, 32 small fallen branch pieces and 7 scaled hollow logs in the fixture seed. Wood has actual authored collision and stays at least 2.5 m from the egg. Depth, terrain support and plant waterline are checked at placement. The original 47-item P5 route, ordinary Pond, Ocean and baked worlds are unchanged.
- `tools/dev build`, `inspect-hatch`, `inspect-growth`, `inspect-pike-cover`, `inspect-world-isolation` and `git diff --check` PASS on the latest source. `inspect-hatch` now checks shore depth, both-side density, real wood collision and hatch/retry/skip invariants. A local Forward+ Xvfb/Mesa llvmpipe frame (`artifacts/logs/shore-growth-final.png`, ignored) shows the shallow nest among denser reeds and grasses after hatch, with no engine errors. This is not an owner-machine playthrough; actual look, fry movement and performance remain for owner review.

## Grounded egg and clear fry exit follow-up

- The owner reported that the moved egg floated above the bed and asked that the fry not clip through it during the cutscene. At the current nest, measured shell mesh bounds put its lowest point 0.028 m above the local bed. `GroundEggShell` now places the actual transformed shell bounds within 0.001 m of that bed after aiming its opening toward the fry. The shell remains grounded in idle, hatched and skipped states.
- The former emergence route made the fry visible at the egg center and carried it through the shell. The first visible fry pose is now just beyond the measured shell lip, with its actual larva mesh rear clear by 0.003 m, then moves along the opening to the unchanged gameplay start. The same route runs at normal or accelerated skip speed. This prevents visible mesh overlap while the hatch animation remains on the authentic egg rig.
- `tools/dev build` and `tools/dev inspect-hatch` PASS after this correction. The focused check samples every visible watched and skipped emergence frame, compares transformed fry and shell mesh bounds, and checks bed contact in all three shell states. Local Forward+ software-rendered frames at 5, 6 and 7 seconds (`artifacts/logs/egg-emergence-after-*.png`, ignored) show the egg on the bed and fry clear of the opened shell. This does not establish the final appearance on the owner's native machine; that remains **NOT OBSERVED**.

## Identity and boundaries

- Repository `jfin602/fish`, branch `main`, HEAD `cb0d9e8ec83fe501f638cdd7b4284cc7f77e2584`, root package `0.5.12`. At M2 start HEAD was `dec00014d0d078b8fb6e61a9338f5c4a4309fd58`; a concurrent owner M1 commit captured early M2 `PondGrowthState.cs` and `FishController.cs` edits. This task did not commit, push, reset or rebase. The unrelated untracked `assets/sfx/` directory was preserved.
- Godot Editor MCP `2026.11.04` reports `bridge.connected=false`, no selected project or active scene. Godot 4.7.2 .NET CLI checks identify Fish through `game/project.godot`, `Fish.csproj` and the P5 scene. No editor control or corrected native visual result is claimed.
- Pike Encounter remains a third destination with two habitats. The normal generated Pond, Base World, Ocean, FAF exports, P5 prompts/history and historical Gate evidence were not changed.

## Revised behavior

- One `FAF-FISH-007` Bass identity starts as a non-swimming egg at a shallow fixture-only nest. The authentic egg `Idle` and `Hatch` Actions play. The registered larva mesh appears at the opening shell, moves out to the common start position, and the cinematic camera interpolates to the gameplay camera before handoff. Skip accelerates this same sequence and cannot duplicate the fish. The authored egg export is 3.4 mm long versus the 58 mm larva, so only the nest egg prop is enlarged to make physical emergence readable; authoritative fish growth and larva dimensions stay separate.
- The opened shell remains in the world until one reachable Space bite consumes it. It records one stable feed event with zero nutrition, so the shell does not cause an immediate stage jump. M3 still owns actual larval nutrition.
- Runtime mesh bounds showed the larva nose at about -0.036 m and tail at +0.043 m in fish space. The old capsule center (+8% of length) left the visible mouth outside the shape. The center is now +4.5%, with proportional collision recovery margin and eight slide iterations. The fry turn input is smoothed; thrust acceleration and top speed are reduced. The camera collision cast margin and fallback distances now match fry scale.
- The fry camera far plane increased from 50 m to 200 m. The screenshot’s camera-following distant-water cutoff is consistent with the old plane intersecting the Pond surface, which extends farther than 50 m from the nursery. This is a source-based diagnosis; corrected rendered water is NOT OBSERVED.
- Egg, emergence and camera pan hold prey and Pike simulation. Pause/focus/retry and both watch/skip paths preserve identical final stage, nutrition, position and prey population. No save, death or story semantics were added.

## Changed files

M2 source: `game/scripts/PondGrowthState.cs` (captured in concurrent owner commit), `game/scripts/FishController.cs`, `game/scripts/PikeEncounter.cs`, `game/scripts/FollowCamera.cs`, `game/scripts/FeedingLedger.cs`, and focused check adjustment in `game/dev/GrowthChecks.cs`. Focused route: `game/dev/HatchChecks.cs`, `game/dev/HatchChecks.cs.uid`, `tools/dev`. Routing/evidence: `BOOT.md`, `docs/planning/c5-post-p5-ecology-lifecycle.md`, `docs/tasks/c5-polish/README.md`, `docs/tasks/c5-polish/implementation-plan.md`, and this file.

## Verification

| Check | Result | Actual evidence |
| --- | --- | --- |
| `tools/dev import` | PASS (earlier M2 run) | Egg and larva GLBs imported without engine errors. No asset file was changed in this correction. |
| `tools/dev build` | PASS | Godot .NET project, zero warnings/errors after source changes. |
| `tools/dev inspect-hatch` | PASS | Real Hatch Action; larva emerges while shell remains; camera moves continuously; watch/skip converge; pause/focus/retry; one shell bite; mesh inside capsule; 2 cm physical wall blocks fry; bounded fish/camera frame movement. |
| `tools/dev inspect-growth`, `inspect-feeding`, `inspect-prey`, `inspect-pike-cover`, `inspect-pike-encounter`, `inspect-world-isolation` | PASS | Existing growth, food, cover and three-destination/two-habitat checks remain green. |
| `git diff --check` | PASS | No whitespace errors. |
| Owner first-build native play | FAIL | Owner screenshot and report: phasing, jumpy movement, moving distant-water artifact, and jump-cut hatch. |
| Owner screenshot after first orientation correction | FAIL | The opened notch still faces the foreground, away from the fry. The headless check used the wrong prop axis. |
| Fish Forward+ CLI frame under Xvfb/Mesa llvmpipe | PASS | The opened slit points toward the fry and the shell remains after hatch. Captured locally at `artifacts/logs/egg-visual-forward-22s.png`; no engine errors in that run. |
| Corrected owner-native hatch, shell, feel and water | NOT OBSERVED | Editor MCP disconnected; the local software-rendered frame does not establish owner-machine appearance, visual continuity, swim feel or moving-water behavior. |

## Next manual step

Owner reviews the corrected M2 in the actual Fish window, specifically the shallow nest and shore density, small-wood clearance, shell opening and emergence, continuous camera pan, thin-cover contact, swim feel and distant water while moving the camera. Keep M1 native contact recheck open. Only after review should M3 be considered as a separate manual task; do not execute it automatically.
