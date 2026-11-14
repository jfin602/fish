# C5 M2 — Bass hatch and controllable fry

**Status, 2026-11-13:** revised implementation and headless T2 checks PASS. The owner played the first M2 build and reported fry phasing, jumpy movement, a camera-reactive distant-water artifact, and an egg/camera jump cut. These are real native FAIL observations for that first build. The corrected build has **not** been observed in native play; owner review is pending. Stop before M3. No P5 Gate is promoted.

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
| Corrected native hatch, shell, feel and water | NOT OBSERVED | Editor MCP disconnected; headless checks do not prove visual continuity or feel. |

## Next manual step

Owner reviews the corrected M2 in the actual Fish window, specifically the shell opening and emergence, continuous camera pan, thin-cover contact, swim feel and distant water while moving the camera. Keep M1 native contact recheck open. Only after review should M3 be considered as a separate manual task; do not execute it automatically.
