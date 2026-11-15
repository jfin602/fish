# /prompt-plan — C5 Wake

**State:** S26 design and manual W1–W3 execution plan, not code implementation.

## Narrow source trace

Start with AGENTS.md, BOOT.md, docs/workflow.md, docs/planning/c5-wake.md, then actual `git status`, branch, revision and dirty files. Inspect `game/scripts/WaterReaction.cs`, `PlayerBodyGeometry.cs`, `FishController.cs`, `PondGrowthState.cs`, `Main.cs`, `PikeEncounter.cs`, `UnderwaterAtmosphere.cs`, `PondSurfaceGeometry.cs`, `game/shaders/water_surface.gdshader` and relevant waterline/camera checks. Verify the local moving-water sampler introduced after the remote baseline, including time ownership, world coordinates, shoreline damping and physics/presentation safety.

## W1 — source geometry and history

Read actual playable Bass model transforms, rest-pose bounds, orientation, current stage body and head/shoulder/side/tail coordinates; use an appropriate stable approximation for animated contact rather than binding to cosmetic fin flaps. Sample the current local water height for the same wave phase as gameplay/rendering. Store a small, fixed-capacity ring buffer of world X/Z, heading, left/right origin, body width, age and motion intensity. Insert samples by distance/time with bounded sampling; no new samples when deep, motionless, blocked or hatch-locked. Retain prior path during turns, and break/clear it at teleport/growth/retry. Development-only debug lines show anchors/height/history, not final art.

## W2 — rendering

Represent each side's historical path as a low-cost, reusable curved strip or comparable normal/highlight distortion. New fronts start at contacting body regions; older ones spread outward, soften and fade. Prevent rigid V translation, squared joins, harsh opacity, z-fighting, floating/disappearing waves and shore spill. Include modest bow pressure and tail micro-turbulence. Sample each displayed point's **shared** moving surface height; use actual water palette, lighting and night response. Keep underwater particles separate. No new expensive fluid simulation or per-frame world-mesh recreation.

## W3 — tuning and lifecycle

Actual speed, effort, yaw/turn and finite active boost tune real wake width/lifetime/strength; boost held while blocked or deep cannot produce a giant wake. Scale across larva/fry, juvenile, subadult and adult; never generate a swim wake in the egg cinematic. Test normal Pond and Pike Encounter, disable/low effects, pause/resume/focus/retry/scene exit, moving wave heights and camera comfort. Keep authoritative swimming/collision, environment clock, predator sensing, boost energy and Ocean underwater behavior unmodified.

## Targeted checks / native owner gate

Inspect `./tools/dev` before use. Applicable focused checks include `./tools/dev import`, `./tools/dev build`, `./tools/dev inspect-pond-surface`, `./tools/dev inspect-pond-scenery`, `./tools/dev inspect-growth`, `./tools/dev inspect-auto-feed-boost` and `./tools/dev inspect-pike-encounter`; extend targeted wake/scene tests as needed rather than running the whole suite blindly. Assert finite history, stage anchors, correct shared Y, genuine motion sampling, turn history, no deep/blocked emission, bounded boost increase, pause/growth/retry cleanup and one shared effect instance per scene.

Native same-seed/clock/preset/route overhead and underwater footage must compare ring baseline to replacement, growth stages, turns, boost, pause and wave extrema. Record Forward+ frame pacing, draw/primitive counts and available GPU timing honestly. **W1 and W2 stop for owner review. W3 ends by launching the actual game for owner T3 playthrough**. Preserve each prior outcome even if a later repair is required.

No automatic commit/push, package-version bump, history rewrite or Gate GREEN.
