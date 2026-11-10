# c3-visibility implementation plan

**Mode:** Correction 3; root version 0.3.0 unchanged in the inspected GitHub authoring baseline.
**Existing decision:** [Underwater visibility and discovery](../../systems/underwater-visibility.md), S17/C50, validation V25.
**Execution order:** P1 -> P2 -> P3 -> P4 native/manual closeout.

## Entry, safe handoff and source trace

Read AGENTS.md, BOOT.md, docs/workflow.md, docs/phase-runner.md, this README/assessment, the visibility contract, P3 world plan and relevant P2/c2 closeouts. Inspect Git branch/HEAD, local changes, package.json, correct Fish Godot project identity, actual scene architecture and tools/dev commands. Confirm the actual active package version equals this correction's unchanged metadata **before** running the runner. If P3 already made progress, stop and reconcile c3 metadata/version with the proven completed P3 prefix; do not downgrade package.json, reset work, or run two stacks concurrently. Prompt files must not be edited beneath an active runner.

At authoring, the main presentation path is main.tscn -> UnderwaterAtmosphere -> one Forward+ Environment plus one directional sun, overhead surface, caustic decal, ribbon shafts, volumetric treatment, water reaction and below-UI screen treatment. Current tuning spans UnderwaterAtmosphere.cs, LightingSettings.cs, LightingPresetStore.cs and GraphicsPanel.cs; WaterColumnChecks.cs and game/dev/* provide depth/atmosphere checks. P3 may change scene paths/active fish visuals; inspect rather than hardcoding obsolete P2 scene assumptions.

## P1 — stronger distance extinction, same lighting identity

- Trace how Environment.FogDensity, FogLightColor, background/Sky, height fog, volumetric density/length and water surface affect the actual view. Confirm available Godot 4.7.2 .NET API/properties before selecting a response curve. Do not assume a Godot 3 fog-depth API exists in Godot 4.
- Implement one bounded, continuous camera-to-object extinction model that makes the far boundary disappear without turning 0–8 m foreground into opaque water. Favor existing engine fog and scalar updates; if engine fog cannot provide a sufficient near-clear / far-obscured curve, evaluate a minimal adaptation of the **existing** screen shader path rather than adding a second full-screen pass. Keep water surface/shafts coherent if shader-specific fog bypass is necessary.
- Give the atmosphere one explicit visibility-distance base concept (in meters) and a calibrated mapping to effective extinction; label it as an artistic approximate range, not physical measurement. Keep legacy density values usable until P2 migrates settings. Remove/replace the misleading assumption that EffectiveUsefulRange = VolumetricFogLength represents a gameplay view distance.
- Derive effective visibility continuously from the real surface/deep reference used by existing depth code. Preserve world-depth lighting and the existing color/brightness gradient. Avoid discontinuous updates, extra dynamic lights and extra fog-volume stacks.
- Provide clear bounded diagnostics for shallow/middle/deep: input base visibility, effective derived distance/density, depth factor and attenuation sample at near/mid/far distances. Preserve the atmosphere on/off diagnostic and paused GFX behavior.
- Extend focused depth checks to prove deeper water attenuates more, nearby transmittance is greater than far transmittance, values finite/bounded, and original movement/camera/preset bases unaffected.

**Gate:** T2 focused build + atmosphere/depth checks, no qualification claim on look until P4.

## P2 — Visibility Distance (m) GFX and persistent settings

- Add intuitive Visibility Distance (m) slider to the existing paused GFX panel, with direction explained (lower = murkier, higher = clearer). Preserve the existing Volumetric haze control as a distinct scattering/light-shaft setting; prevent a misleading duplicate Distance fog slider if its semantics have changed.
- Define a single validated base setting in LightingSettings and use it in UnderwaterAtmosphere CaptureLighting/ApplyLighting. Effective camera depth must not modify persisted base settings; switching a preset while deep applies the selected base then derives effective values.
- LightingPresetStore is currently schema v1 and includes valid custom settings with DistanceFogDensity values. Define and implement deterministic forward compatibility or versioned migration of valid old presets, including current selection, ID, names, existing caustics/surface/shaft settings, without dropping them or silently converting every custom preset to Natural. If conversion is lossy, record clearly which legacy value is preserved and how its effective new range is derived. Preserve invalid/corrupt-file recovery and atomic writes.
- Preserve Natural and Soft defaults with a reasonable new visibility value, plus Save As/Save/Rename/Delete/Choose/reload, pause/UI focus, no gameplay input bleed. Add focused round-trip tests with fixtures for old custom schema and new schema, malformed values and normal current preset transitions.
- Keep user:// files only on user storage and tests in isolated XDG profiles/temp locations; no committed personal files.

**Gate:** T2 build + focused GFX/store + depth isolation. Existing personal presets demonstrably survive reload.

## P3 — integrated checks and reproducible performance evidence

- Recheck actual P3 world/character-select scene identities, preserving the selected player fish, shared authoritative motor and camera, routes, and all accepted water effects. Do not create new world geometry or alter P3 art in order to pass fog tests.
- Add or extend focused tests/instrumentation for shallow/middle/deep effective values, lighting preset load/selection, toggling from disabled to enabled, pause/reopen, state isolation, finite values and no abrupt numerical changes at depth endpoints.
- Reuse existing game/dev atmosphere_ab_probe.gd and tools/dev measure-atmosphere where useful, but differentiate **new visibility on/off** from **whole atmosphere on/off**. An atmosphere-off comparison cannot isolate the incremental visibility cost. Use the same-source scene/preset/view and deterministic settings, warmed frame samples and display/backend information; produce recordable output/optional image-capture instructions. Do not promise an actual native measurement from headless checks.
- Check near and distant water surface/terrain composition for shader flags bypassing fog, screen treatment ordering and UI placement. Inspect performance cost: avoid added draw calls/full-screen passes where possible and check 0.3.0 root version unchanged.
- Run targeted import/build/depth/atmosphere and relevant main/selected fish smoke checks; inspect tools/dev check composition and avoid duplicate whole-suite runs. Retain exact commands and results in qualification/implementation.md when performed.

**Gate:** T2 integrated source/runtime checks; native judgments still P4.

## P4 — native visibility and owner closeout

Stop for a real playable Godot 4.7.2 .NET Forward+ session on the actual target machine, including normal controls and the current P3 world if completed. If P3 is still incomplete, label that limitation; qualify the available world only, not a future nonexistent P3 environment.

Use a repeatable **real reachable** camera/route, not a position beyond the accepted fish's bounds:
1. Look across open water from two different directions toward opposite map boundaries. Identify if the boundary is recognizable; distinguish fog concealment from rock occlusion.
2. Approach a named distant reef landmark/cave, then retreat; check smooth silhouette -> recognizable shapes -> color/details, with no hard pop/cut.
3. Compare similar clear horizontal viewlines at shallow, middle and deep, then swim shallow -> deep -> shallow; record active settings, depth, useful visibility and color response.
4. Navigate an open swim lane, cave/tunnel entry and shelter; inspect player fish and near collision obstacles, plus sunlight, caustics, shafts, surface and GFX/HUD.
5. Switch Natural/Soft/custom visibility, save/reload an **old** preset profile copy and re-enter normal play; no loss of custom names/values and no camera/motor drift.
6. Compare warm native median frame times at the *same* scene/views/settings for prior/bypass vs new visibility and note draw/primitives, adapter, resolution, current package/source, preset and Godot errors. Investigate repeatable ~10%+ slowdown (not a hard automatic PASS threshold); report explicit owner acceptance if knowingly retained.

Create qualification/native-visibility.md with actual screenshots/logs (retained paths), test actions, expected/observed results, and PASS/FAIL/NOT OBSERVED/BLOCKED. Create a single closeout.md mapping the contract and V25, with owner decision if available. Do not claim P3 G3.3 or the whole Phase GREEN without all applicable original P3 Gates. Preserve historical P2 V18/V19 PASS, G2.3/V20 OPEN. Reconcile owning docs/roadmap/BOOT/MODULES only if actual evidence or completed implementation justifies it; do not edit old prompt/evidence files. Leave final closeout uncommitted for human review.

## Deferred

Large-map occlusion/LOD, true optical simulation, new volumetric zones in every cave, procedural world generation, predators/feeding/growth, persistence, movement/camera redesign, new GFX framework, renderer selection and P3 art/character-selection development are separate work.
