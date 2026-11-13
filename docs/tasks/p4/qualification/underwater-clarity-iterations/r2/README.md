# Underwater clarity R2 — optical depth and water color

**Disposition: one native candidate for owner review; P12 remains FAIL / NOT GREEN.** This iteration makes an unmistakable color and layer separation change, but the captured Pond still has a broad, fairly uniform overhead field and does not establish either owner acceptance or the full reference look. The root version remains `0.4.11`; no P4 runner, bake, gameplay state, or day/night clock was advanced.

## Source, references, and capture conditions

- Clean pre-edit `main` HEAD `405116b`, `jfin602/fish`. The current candidate is the reviewable working-tree diff in `UnderwaterAtmosphere.cs`, `PondTurbidityField.cs`, `WaterVisualProfile.cs`, `water_surface.gdshader`, and two focused check expectations; source diff SHA-256 `a0f2a4155f9f0c9b5bb182a0967950ef0babd050078d90271349046aa54fefc3`. The [R2 native probe](../../../../../../game/dev/underwater_clarity_r2_probe.gd) has SHA-256 `9548fe92faac95ce3d74786b182c1798d903696326d92c0af2c0fbaadae4c9be`.
- The owner's two freshwater reference images were supplied in the conversation as bright clear shallows and dark submerged wood. They were not present in the available attachment directory, which contained only the task text; no local image path or image copy is claimed. The [R1 packet](../r1/README.md) supplies the prior in-game visual comparison.
- Native Godot `4.7.2.stable.mono`, Vulkan 1.4.318 Forward+ on AMD Radeon 880M, Linux/X11, 1920×1200 exclusive fullscreen, VSync disabled, uncapped. `pond-p4-71`, world `world-5e3b4fee759acbe3`, seed 71, content hash `d76eabbb5b6fff2f91418b49b2de92dd090541d609b46d0725ad9fc85cbda347`, Adult Bluegill `FAF-FISH-006`, Pond Overcast. All fixed views use 12:00 and a paused game clock. The [baseline manifest](baseline/manifest.json) and [candidate manifest](candidate/manifest.json) confirm all ten fish and camera transforms match exactly; the same native probe provided 24 warmup frames and 90 uncapped timed frames at each benchmarked pose. [Baseline log](baseline/native.log) · [candidate log](candidate/native.log).
- The [baseline contact sheet](baseline/contact-sheet.png) was captured before any optical code change. The fixed-view baseline is current `405116b`, including R1 and c4 day/night work already in HEAD. The moving baseline was later reproduced from exactly those four `HEAD` optical source files, with a temporary rebuild, then the candidate files were restored and rebuilt. Both route manifests have the same seed, input pattern, camera type and fish endpoint. The first exploratory launch accidentally used a non-.NET Godot binary and failed to load C#; its log was replaced by the valid mono baseline and it produced no usable capture.

## Root cause and controlled diagnosis

R1's [isolation matrix](../r1/diagnostics/diagnostic-matrix.png) showed the global and local volumes accumulating over most of the frame. R2's [optical comparison matrix](diagnostics/optical-matrix.png) held the Bluegill, camera and noon light fixed at ordinary, shallow and deep poses. Its `D1` changes water/sky/fog hue alone; `D2` changes distance extinction alone; `D3` combines them; `D4` also halves global and local volume. [Diagnostic manifest](diagnostics/manifest.json) and [log](diagnostics/native.log) record the exact temporary state. Only `D4` clearly exposes more near and middle structure while changing the broad color. These probe changes were restored between views and were not saved to the game.

The green-field appearance came from three reinforcing optical choices: warm olive colors in distance fog, volume and the overcast underside; a broad seeded local FogVolume added on top of global volume; and weak distance extinction (`0.58`) leaving distant surfaces tinted yet still present. Nearby material lighting had contrast, but the overlapping green scatter compressed it. The horizon-sized overhead region is mostly the water plane's underside in these cameras, so changing the distant sky palette alone cannot fix it. The 0.04-pixel screen warp and 0.003 color variation are too small to explain a full-frame blur; the screen pass remained unchanged. Terrain, bark, ambient and solar lighting still affect how the water reads, and the fixed poses cannot isolate every interaction.

## Implemented optical candidate

The Pond distance fog now starts at **4.5 m** instead of 3 m, uses **0.78** extinction instead of 0.58 and a **1.55** curve instead of 1.2. This retains a clearer immediate fish zone while moving middle and far geometry toward the water color smoothly; the saved Overcast visibility end remains 18 m before camera-depth shortening. Pond-only global volume multiplier is **0.19** rather than 0.38, and the single seeded field's volume-density scale is **0.05** rather than 0.11. The field texture, world anchor, normalized sediment samples and world-space pattern are unchanged. No new volume, render pass, texture or draw was added. Sediment still has no light, shadow or clock input.

The Pond palette now moves from muted blue-green shallows to darker blue-green at depth across fog, volume, sky and background. Existing warm ambient/direct light, mud and bark materials and the local olive sediment albedo retain shaded/silty warmth. The underside of the water plane gets its own muted blue-green scatter (`0.085, 0.16, 0.14`), while the air-facing transmission keeps the approved dark olive scatter (`0.055, 0.085, 0.035`) and reflection branch. Pond sky fog affect reaches 0.35 only while immersed and fades to zero in air. Ocean colors, Ocean fog and its water underside remain unchanged. The screen shader, surface transmission, baked world, fish motor, collision, presets, environment clock and solar-state ownership were not changed.

## Matched visual result

| Evidence | Observed result |
| --- | --- |
| [Baseline](baseline/contact-sheet.png) · [candidate](candidate/contact-sheet.png) · [all ten matched pairs](comparison/matched-pairs.png) | The ordinary, shallow and middle water shift from broad yellow-olive to muted blue-green. Nearby mud and bark stay brown/olive; plants and distant stumps remain silhouettes. The change is visible at contact-sheet size. |
| [R1 / R2 baseline / R2 candidate](comparison/r1-r2-matched.png) | R1 and R2 baseline share the same green-field limitation; R2 makes a larger color change than R1's small density adjustment. |
| [Fish detail](comparison/fish-detail.png) · [foreground, middle, far zones](comparison/optical-zones.png) | Bluegill outline, fins and near bark stay recognizable. Bark edges and midground vegetation separate better from the water, though several distant silhouettes and the waterline remain fairly abrupt. |
| [Shallow / deep](comparison/shallow-deep.png) | Shallow water is cooler and lighter than the dark deep floor. The deep overhead region still looks too even; this is a visible limitation, not a claimed full optical-depth solution. |
| [Eight-second normal-input route](comparison/motion-route.png) | The real keyboard-swim/follow-camera path has no sampled color jump. Both fish endpoints are exactly `(-10.63327, 7.79, -0.000227)`. [Baseline route](baseline/motion/manifest.json) · [candidate route](candidate/motion/manifest.json). |
| `A10_air` in [metrics](comparison/metrics.json) | Matched above-water image is pixel-identical (mean absolute RGB difference `0, 0, 0`), supporting preservation of the accepted reflection and transmission at this pose. |

At `A2_near`, mean absolute full-image RGB change is `6.66, 6.41, 26.08`/255, dominated by the intended color shift. This is substantially larger than R1's reported 1–6/255 changes. Pixel difference is an image measure, not an artistic score. No pale fog, white haze, extra screen blur or solid background was introduced in the captured views; the overhead plane remains too uniform.

## Performance and regression

All timing is elapsed native frame interval, **not GPU-only timing**. The warmed 90-frame fixed-view medians/p95 are in milliseconds. Draws and primitives are identical before/after at each pose; [full measurements](comparison/metrics.json) retain every sample summary and image difference.

| Fixed view | Baseline median / p95 | Candidate median / p95 | Draws / primitives |
| --- | ---: | ---: | ---: |
| Spawn | 3.93 / 5.02 | 4.13 / 6.21 | 179 / 127,302 |
| Ordinary | 4.04 / 5.75 | 4.27 / 6.65 | 206 / 129,978 |
| Hollow log | 4.20 / 6.60 | 3.87 / 5.67 | 210 / 130,664 |
| Middle | 3.74 / 5.15 | 3.90 / 5.27 | 143 / 125,284 |
| Deep | 3.72 / 5.70 | 3.72 / 6.02 | 146 / 127,176 |
| Air | 5.35 / 6.44 | 5.56 / 8.04 | 205 / 126,522 |

The matched 8-second route measured **4.976 / 7.332 ms** baseline median/p95 over 1,363 frames and **4.983 / 8.483 ms** candidate over 1,333 frames, with **202 draws and 182,240 primitives** in both. The route p95 increased by 1.15 ms in this single sample, while the median was effectively unchanged. This remains below a 16.67 ms 60 Hz interval, but one run cannot separate renderer cost from frame-pacing variation; repeat native route timing before performance acceptance. GPU timestamps were unavailable. The optical code adds no draw or shader pass. Baseline/candidate world-load and volume-build times vary with startup/cache state and are not used as speed claims.

The final import and .NET build passed with zero warnings/errors. [Pond atmosphere and preset/Ocean isolation](diagnostics/inspect-pond-atmosphere.log), [surface](diagnostics/inspect-pond-surface.log), [camera immersion and unchanged fish state](diagnostics/inspect-pond-camera-immersion.log), [solar lighting and solar water in both habitats](diagnostics/inspect-solar-lighting.log), and [Ocean depth/preset checks](diagnostics/inspect-depth.log) passed. The two expected invalid-preset recovery warnings in the Ocean depth check are test inputs, not candidate failures. All final fixed and route native logs have no engine/shader error marker. The world ID/content hash and sampled fish endpoint are unchanged. Headless checks do not establish owner camera comfort or full continuous route/crossing quality.

## Remaining defects and R3 focus

The broad overhead region still appears too even, especially in deep water; its waterline/bank transition can read as a horizontal band. Middle and distant silhouettes could fade more naturally in varied light without losing nearby navigation. The darker submerged-wood condition preserves a recognizable opening but remains dim, and the Brighter Shallows GFX preset was not separately photographed. Night, repeated crossings, full hollow-log traversal and the owner's two reference images were not directly compared by native hand-play here. R3 should focus on **the surface underside's spatial/light variation and the far bank transition**, then assess both illumination conditions and the moving waterline with the owner. Keep the single Pond optical model and avoid a new blur pass or blanket darkening.

**Owner decision pending:** Review this R2 candidate in native play against the supplied images. No visual self-approval, P12 Gate change, c4 approval, runner progression, version bump, commit or push is claimed.
