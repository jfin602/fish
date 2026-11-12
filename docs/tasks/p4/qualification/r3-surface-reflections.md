# P4 P12-R3 — Pond surface reflection candidate

**Disposition: implemented experiment; P12/V32 remain NOT GREEN at `0.4.11`.** The owner requested a clean checkpoint, then a serious attempt to bring the Pond surface toward three supplied reference screenshots. This record does not advance P13, create a runner checkpoint, or replace the preserved [P12/R1/R2 evidence](native-surface.md).

## Checkpoint and source

| Item | Recorded identity |
| --- | --- |
| Pushed source before the experiment | [`cc8cbb99de16f3b6138a046272721d986274a058`](https://github.com/jfin602/fish/commit/cc8cbb99de16f3b6138a046272721d986274a058) |
| Saved rollback branch | [`checkpoint/water-surface-2026-11-12`](https://github.com/jfin602/fish/tree/checkpoint/water-surface-2026-11-12), created at that exact commit |
| Candidate branch | [`experiment/pond-water-reflections`](https://github.com/jfin602/fish/tree/experiment/pond-water-reflections) |
| Implemented shader/build source | [`e93ecf5e6bc2d00d6738fce9766d98f17585a4ca`](https://github.com/jfin602/fish/commit/e93ecf5e6bc2d00d6738fce9766d98f17585a4ca); actual-Pond captures add only the recorded probe capability-guard repair, with unchanged shaders |
| Concurrent documentation integration | Main `614b44d821b794b82c37882df2652d3378bfbc43` adds authored c4 day/night planning; merged without runtime changes. Its S22/C67/C68 identities are preserved; this surface decision is S23/C69. |
| Root version | `0.4.11`; no `0.4.12` or Gate promotion |
| Native target | Tracked seed-71 Pond, Adult Bluegill `FAF-FISH-006`, Godot `4.7.2` .NET, Forward+ |

The remote checkpoint preserves the pushed repository. It does not establish that any unpushed or dirty work on the owner's laptop was saved. Inspect and preserve that work before switching branches. Compare or use a separate worktree; do not reset or clean away local changes.

## Visual target and bounded change

The references show dark green water, recognizable reflected banks and sky, broad irregular ripple distortion, calmer patches, and stronger reflection at low viewing angles. They are visual direction, not evidence of the reference application's rendering technique or performance.

The R2 air branch sampled the opaque scene through water and attenuated it with depth; it had no source of reflected shoreline geometry. R3 adds that missing contribution while keeping the existing absorption model and the established underwater treatment.

| Source | Candidate responsibility |
| --- | --- |
| `game/shaders/pond_water_surface.gdshader` | Pond air-side normal variation, bounded screen-space reflection, sky fallback, Fresnel composition and guarded refraction |
| `game/shaders/pond_air_sky.gdshaderinc` | Shared Pond air sky and reflection fallback with broad smooth cloud variation |
| `game/shaders/pond_sky.gdshader` | Calls the shared helper for Pond air sky while retaining its underwater sky treatment |
| `game/scripts/UnderwaterAtmosphere.cs` | Selects the new surface and sky shaders only for the Pond profile |
| `game/dev/PondAtmosphereChecks.cs` | Adds source/material isolation assertions to the existing integration check |
| `game/dev/p12_surface_reflection_probe.gd`, `tools/dev` | Reproducible native comparison through `review-pond-reflections` |

The original `water_surface.gdshader` and `underwater_sky.gdshader` remain byte-identical to the checkpoint. The new Pond surface shader retains its original vertex displacement and underwater fragment treatment. Broad world-space irregular slopes affect the air-facing normal. Fish transforms, movement, collision, world generation, bake identity and saved lighting preferences are not new rendering authorities.

### Reflection and transmission

The candidate reflects a view ray from the perturbed surface normal and traces against the existing opaque depth image. It clips the ray before the near plane and screen boundary, uses perspective-correct interpolation, refines a valid depth crossing, and checks the resulting world height and depth residual. Underwater fish, mud and cover must not become reflected objects.

Missing or unreliable scene hits fade into the matching procedural air sky. A water Fresnel term blends reflection with the existing transmitted/scattered water: low viewing angles reflect more, while a downward view retains nearby fish and shallow mud. R2's `exp(-0.65 * optical_path / daylight)` attenuation remains the optical basis; this is a continuous fade, not a four-meter clipping plane.

Refraction samples that land on dry or foreground geometry first fall back to the unbent pixel. This guards against wave distortion punching holes around reeds. The surface remains clipped to the existing shoreline mesh and keeps its established edge mask.

The maximum reflection trace is **34 depth reads**: one initial sample, 24 coarse samples, eight refinements and one final verification. Transmission has additional depth/color reads. The trace runs only where its air-side contribution is appreciable. It adds no second camera, scene render, reflection capture or history buffer. These bounds are implementation facts, **not a measured claim that the effect is cheap on the Radeon 880M**.

### Known limits

- Screen-space reflection cannot recover off-screen objects, geometry hidden behind the nearest depth layer, or ordinary transparent geometry absent from the opaque depth buffer.
- Thin reeds can be missed by the finite trace. Increasing thickness tolerance can instead create false broad reflections; judge both cases in motion.
- Edge/range fades can change reflection strength during camera turns. The sky fallback must prevent black gaps, long streaks and edge smearing.
- The existing camera-immersion transition blends the visible atmosphere around the waterline. Review the reflected sky, air/water transition and grazing ripple facets together.
- Patchy ripple strength changes the surface normal. It does **not** implement the [S21 spatial water-clarity contract](../../../systems/underwater-visibility.md#freshwater-pond--spatially-variable-clarity-s21-design-not-implemented), sediment variation, dynamic silt or predator sensing.

## Verification and its limits

| Check | Actual status / scope |
| --- | --- |
| Remote rollback branch | **PASS:** created at the exact pushed source above |
| Original Ocean water and sky shaders | **PASS, source comparison:** unchanged from the checkpoint |
| Inherited Pond vertex/underwater shader portions | **PASS, source comparison:** identical before the new air-facing branch |
| Pinned project import and build | **PASS:** Godot `4.7.2.stable.mono.official.ed1daf0bf`, SDK `10.0.403`; full Fish build has zero warnings and zero errors |
| Pond atmosphere, scenery and camera-immersion checks | **PASS:** all three affected C#/state checks after approved local texture recovery; includes Pond→Ocean→Pond state/material isolation and five control-preset immersion assertions |
| Ocean equal-input check | **PASS:** after exact-hash Fish3 recovery; atmosphere enabled/disabled routes have equal physics over 30 steps. This is not Ocean visual qualification |
| Isolated Forward+ renders | **PASS for the diagnostic scope:** actual opaque bank/marker reflections, irregular surface ripples and retained shallow fish were observed; sky-only control removes reflected scene shapes |
| Same-sky underwater comparison | **PASS for the fixture:** candidate versus original water shader under the same Pond sky has zero changed pixels; this is not full-game equivalence |
| Native helper and CLI syntax | **PASS:** initial syntax/argument guards plus successful pinned .NET execution after replacing the invalid `mono` OS feature check with verified `CSharpScript` capability detection; initial failure retained |
| Actual baked-Pond fixed-pose rendering | **CAPTURED AND REVIEWED FOR THIS SCOPE:** fifteen images, five poses × legacy/candidate/sky-only; pinned .NET, Forward+, software llvmpipe, 960 × 540, no engine/shader/script errors or warnings |
| Actual-Pond underwater comparison | **NEAR-IDENTICAL IN THIS FIXED VIEW:** legacy→candidate differs in 962 of 518,400 pixels, maximum channel delta 1/255; the full-game images are not byte-identical |
| Owner-controlled play, motion stability and camera comfort | **NOT OBSERVED** |
| Radeon 880M incremental GPU cost and route pacing | **NOT OBSERVED** |
| P12/V32 acceptance | **NOT GREEN** |

The isolated renderer diagnostic uses standard Godot `4.6.3`, Forward+ and software llvmpipe. It contains actual FAF assets and a normalized Bluegill, with a synthetic bank, an above-water T-shaped marker and a submerged red diagnostic cube. It can demonstrate shader compilation, sampled reflected geometry, foreground rejection and a controlled underside comparison. It is not the tracked baked Pond, the .NET game, a native owner play session or a representative hardware benchmark.

The retained [diagnostic instructions](native-surface-evidence/r3/diagnostic/README.md), [metadata and hashes](native-surface-evidence/r3/diagnostic/metadata.json), fixture and render logs make these sampled comparisons reproducible:

| Comparison | Observation |
| --- | --- |
| [Checkpoint surface/sky](native-surface-evidence/r3/diagnostic/captures/baseline-above.png) → [candidate](native-surface-evidence/r3/diagnostic/captures/candidate-above.png) | The T marker, banks, logs and plants have recognizable inverted reflections, distorted by broad irregular ripples. The dark water body remains. |
| [Sky-only control](native-surface-evidence/r3/diagnostic/captures/sky-only-above.png) | Reflected bank/marker silhouettes disappear with scene reflection disabled, distinguishing actual sampled geometry from the procedural sky fallback. |
| [Shallow fish](native-surface-evidence/r3/diagnostic/captures/candidate-fish.png) | The normalized Bluegill and submerged diagnostic cube remain readable; no mirror copy of the submerged cube is visible in this sampled view. |
| [Low grazing view](native-surface-evidence/r3/diagnostic/captures/candidate-grazing.png) | Large stump reflections remain continuous after the refinement repair below. Thin reeds and screen-space visibility remain limited. |
| [Original surface under current sky](native-surface-evidence/r3/diagnostic/captures/surface-baseline-below.png) versus [candidate underwater](native-surface-evidence/r3/diagnostic/captures/candidate-below.png) | Pixel-identical in this fixed fixture. Full historical baseline includes a different sky path; use the same-sky comparison to isolate the surface. |

An earlier candidate showed horizontal combing in substantial stump reflections at grazing angles. A controlled 4→6→8 binary-refinement comparison held coarse steps, thickness, scene, light and camera fixed. Six refinements repaired nearby stumps but left a farther one striped; eight removed the observed broad-stump gaps. The final shader adopts eight refinements and preserves the earlier failure in the [refinement diagnostic](native-surface-evidence/r3/diagnostic/refinement-diagnostic.json). This costs four additional depth reads only when a crossing is found. Final retained render logs contain no engine, shader or script error. This repair does not prove all angles or moving-camera stability.

## Pinned runtime and actual-Pond evidence

The later [pinned validation record](native-surface-evidence/r3/pinned/README.md) contains the official tool archive hashes, build and state-check logs, initial asset failures, recovered-input provenance, fifteen captures and measured frame intervals. No runtime pins, baked world files, texture pixels or material settings were edited for the reconstruction. Coast Sand maps and the legacy Fish3 model match their tracked historical hashes. The ambientCG `Ground060`, `Gravel041` and `Rock063` 2K-JPG sets came from the approved official versioned packages, but their historical local-byte equality remains **unverified** because the original ignored inventory is absent. All modes in this comparison use the same restored inputs.

The actual seed-71 Pond loaded its tracked bake with zero generator invocations, 1,101 FAF placements and the real Bluegill `FAF-FISH-006`. The run used 30 warmup frames and 30 measured intervals per capture, a fixed 12-second surface/sky time, an uncapped 960 × 540 virtual X11 display, and the Overcast profile. The shader sources are identical to `e93ecf5`; only the probe's .NET capability check was repaired. The [source record and hashes](native-surface-evidence/r3/pinned/actual-pond/analysis.json) identify that repair and every retained image.

| Actual Pond comparison | Observed result |
| --- | --- |
| [Low bank legacy](native-surface-evidence/r3/pinned/actual-pond/captures/low_bank_legacy.png) → [candidate](native-surface-evidence/r3/pinned/actual-pond/captures/low_bank_candidate.png) | Stumps, roots and reeds have recognizable inverted reflections; the old surface has no corresponding reflected shore structure. No broad-stump combing was visible in this sampled view. |
| [Oblique candidate](native-surface-evidence/r3/pinned/actual-pond/captures/oblique_bank_candidate.png) and [sky-only control](native-surface-evidence/r3/pinned/actual-pond/captures/oblique_bank_sky_only.png) | Scene-reflected silhouettes disappear in the control. Shallow water retains muted olive transmission and floating-pad detail. |
| [Fish at 0.5 m](native-surface-evidence/r3/pinned/actual-pond/captures/fish_050cm_candidate.png) and [4 m](native-surface-evidence/r3/pinned/actual-pond/captures/fish_400cm_candidate.png) | The shallow Bluegill remains visible; the deeper fish is strongly attenuated. Two fixed depths do not complete the continuous shallow→deep route requirement. |
| [Underwater legacy](native-surface-evidence/r3/pinned/actual-pond/captures/underwater_legacy.png) → [candidate](native-surface-evidence/r3/pinned/actual-pond/captures/underwater_candidate.png) | The green underside and fish silhouette are preserved in the sampled view, with maximum 1/255 channel difference. Other poses and transitions still require review. |

This improves the reflective response, but does not establish parity with the reference screenshots. Fish's current open bank and overcast sky produce paler low-angle reflections than the dense canopy scenes. Thin reeds, off-screen objects and moving reflection stability retain the limits above.

On this **software renderer**, low-bank legacy/candidate median intervals were 125.32/132.91 ms; oblique-bank medians were 130.74/158.93 ms. The [complete median/p95 table](native-surface-evidence/r3/pinned/actual-pond/REVIEW.md) includes every mode and pose. These short paused runs include scheduling and presentation, so they provide no Radeon 880M frame-rate or incremental GPU-cost claim. The observed unchanged draw counts do not imply zero shader cost.

## Native comparison ready to reproduce

Follow [the native handoff](../manual/R3-surface-reflections-native.txt) on the configured Fish desktop. `./tools/dev review-pond-reflections` performs import and build, creates a fresh ignored profile/output directory, then runs the actual main scene with the tracked Pond and Bluegill.

The probe captures five fixed poses in three modes: `legacy`, `candidate`, and `sky_only`. The poses cover low and oblique bank views, fish centers at 0.5 m and 4 m, and the underwater side. All three modes use the **current shared sky**, so this isolates surface treatment; it is not a complete pre-R3 atmosphere comparison. Compare the saved checkpoint separately if judging the old sky.

Each capture includes 30 warmup frames and 90 measured frame intervals by default. The JSON report records supplied source identity, shader/manifest hashes, renderer/adapter, viewport, pose, lighting settings, draw/primitive summaries and timing samples. A completion marker means the captures completed; its visual disposition remains `NOT_JUDGED`.

The paused fixed-pose samples include presentation, scheduling and possible vsync. They are not isolated GPU timings or proof of movement feel. Preserve the generated `comparison.json`, images and log at a durable task evidence path before cleaning ignored output.

### Remaining acceptance work

1. Reproduce the recorded candidate on the owner's configured desktop, verifying actual source identity and local asset inputs. The pinned build and four affected state checks passed here; use the handoff's commands when preparing that desktop run.
2. Review real Pond bank reflections, shore/reed edges, nearby fish detail and continuous depth fade in both Pond lighting presets.
3. Play low/high-effort surface swimming, wake/glide, orbit/zoom, repeated air/water crossings, pause/refocus and Pond→Ocean→Pond return. Cover the existing control-preset matrix and actual near-bank clearance.
4. Complete the previously open continuous shallow→middle→deep→return route and covered-route observations; fixed poses do not satisfy them.
5. Compare candidate/legacy incremental cost at a fixed resolution, with uncapped or GPU timing where available; report frame-time median/p95 and visible pacing rather than inferring headroom from a 60 Hz cap.
6. Record the owner's explicit visual/camera judgment and each remaining V32 result. Only actual qualifying evidence can change status; this experiment leaves `0.4.11`, P12 and the runner progression unchanged.
