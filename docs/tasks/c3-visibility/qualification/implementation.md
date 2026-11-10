# C3/P1 — depth-aware visibility extinction (2026-11-10)

**Result:** Implemented and T2 source/runtime checks pass at unchanged root `0.3.6`. Native appearance and V25 remain unqualified for P4. Pre-task `main` HEAD was `d3d22b2330b22f366c39c59a82e48e9a7d12fe2a`. The already dirty P3 Character Select/world-review files were preserved. No commit was made; the runner owns its checkpoint.

The tracked Fish project is `game/project.godot`, named Fish, with `forward_plus`. `tools/dev doctor` found Godot `4.7.2.stable.mono.official.ed1daf0bf` and .NET SDK `10.0.403`. The Editor MCP server was reachable but its bridge had no connected editor, so no editor mutation or native visual review was performed. The installed GodotSharp 4.7.2 API documentation confirms `Environment.FogModeEnum.Depth`, `FogDepthBegin`, `FogDepthEnd`, `FogDepthCurve` and the depth-mode meaning of `FogDensity`.

## Source behavior

`UnderwaterAtmosphere` now uses the existing Forward+ Environment's artistic depth fog: a 5 m start, 0.96 maximum fog intensity, 1.2 curve and a depth-dependent end distance. It keeps the existing volumetric fog extent/density for scatter and shafts, the world-height fog anchor, depth colors, sun, caustics, sky shader and fog-disabled overhead water surface. The existing screen shader and UI ordering were not changed; no additional full-screen pass was added. Sky fog affect is zero, preserving the directional sky/overhead cue.

The exported artistic `VisibilityRangeMeters` defaults to 35 m. Until C3/P2 migrates GFX/preset storage, the existing saved `DistanceFogDensity` remains the base control: `base = clamp(VisibilityRangeMeters * sqrt(0.0035 / max(legacyDensity, 0.0001)), 18, 60)` meters. Eased camera depth from the existing surface/deep references then gives `end = base * (1 - 3/7 * depth01)`. `EffectiveUsefulRange` reports that **engine fog end distance**, not measured optical sight distance or `VolumetricFogLength`. The diagnostic retention samples approximate only the depth-fog curve; volumetric and height fog also affect actual pixels.

With the default base, the focused runtime probe sampled:

| Camera level | Fog end | Approximate scene retention at 8 / 15 / 25 m | Volumetric density |
| --- | ---: | --- | ---: |
| Surface | 35.0 m | 0.94 / 0.74 / 0.41 | 0.024 |
| Middle | 27.5 m | 0.91 / 0.64 / 0.17 | 0.039 |
| Deep reference | 20.0 m | 0.86 / 0.41 / 0.04 | 0.054 |

These are source-model estimates, not visually accepted thresholds. `WaterColumnChecks` covers near-to-far monotonic attenuation, shorter range with depth, finite bounds, sky/surface consistency, atmosphere toggle restoration, legacy density mapping, and unchanged fish/camera/preset base identity. The atmosphere probe now counts gameplay viewport lights only, excluding P3 Character Select preview SubViewports. Focused probes use isolated ignored XDG user storage so a personal GFX preset cannot change their baseline.

## Checks actually run

| Command | Outcome |
| --- | --- |
| `./tools/dev build` | PASS after final source edit: 0 warnings, 0 errors, 2.97 s. |
| `./tools/dev inspect-atmosphere` | PASS: `ATMOSPHERE_CHECK_OK` and `ATMOSPHERE_MOVEMENT_OK equal-physics-route enabled=disabled steps=30`. The expected optional-missing-screen fixture logged a warning. |
| `./tools/dev inspect-depth` | PASS: `DEPTH_CHECK_OK surface=10.39 deep=-7.61 base_visibility=35.0` with the sampled values above; `presets=isolated state=unchanged`. |
| `git diff --check` | PASS, no output. |

Initial `inspect-atmosphere` failed because its old one-light assertion included P3 preview lights. Initial `inspect-depth` failed after reading a pre-existing personal preset; the next isolated run exposed an overly strict shallow 25 m estimate threshold (0.41 vs 0.40). The viewport scope, storage isolation and sample threshold were corrected before the passing runs. The first unisolated Godot launches also logged inability to write `user://logs`; isolated runs did not.

Exact diagnostic markers from those runs:

```text
ATMOSPHERE_CHECK_FAILED extra real light or excessive screen warp
ERROR: DEPTH_CHECK_FAILED System.Exception: shallow lighting changed from c2 base
ERROR: DEPTH_CHECK_FAILED System.Exception: distance retention lost progressive near-to-far extinction
ATMOSPHERE_CHECK_OK forward_plus=required fog=bounded surface=shoreline sky=sunlit shafts=6 ribbon_mesh_surfaces=1 shadowless caustics=1 deterministic=gaps drift=paired-pause-toggle motes=low-poly ui=above-screen warp=0.1px resize=responsive motor=unchanged
ATMOSPHERE_MOVEMENT_OK equal-physics-route enabled=disabled steps=30
DEPTH_CHECK_OK surface=10.39 deep=-7.61 base_visibility=35.0 shallow=(end:35.0,volume:0.024,t8:0.94,t15:0.74,t25:0.41) middle=(end:27.5,volume:0.039,t8:0.91,t15:0.64,t25:0.17) deep=(end:20.0,volume:0.054,t8:0.86,t15:0.41,t25:0.04) presets=isolated state=unchanged
```

No `import` was run because no resource or shader file changed. Headless checks establish load and numerical behavior, not whether opposite boundaries disappear or near coral and cave entries read comfortably on a real display. C3/P2 still owns the explicit Visibility Distance GFX control and schema-compatible preset migration; C3/P4 owns native visual/performance acceptance. P2 G2.3/V20 remains open.

## C3/P2 — Visibility GFX and preset compatibility (2026-11-10)

**Working-tree result:** implemented from clean `main` HEAD `0d6ae25d13c302372b5f3fdc684e32d15cd2a784`, the completed C3/P1 runner checkpoint, at unchanged root `0.3.6`. P3 P1–P6 are present; P7 world-art review remains open. No commit was created by this task.

The paused GFX panel now shows **Visibility Distance (m)** from 18 to 60 m in 1 m steps, with lower values described as murkier and higher values as clearer. This is the artistic shallow-water depth-fog end distance, not a measured optical sight range. The existing Volumetric haze slider remains separate. Moving the visibility slider previews immediately through `ApplyLighting`; the panel stays focusable while the game tree is paused. `LightingSettings.VisibilityDistanceMeters` is the validated saved base. `UnderwaterAtmosphere` captures/applies that base, then derives only the runtime end distance from camera depth: `effective = base * (1 - 3/7 * easedDepth)`. The retained `DistanceFogDensity` field is compatibility metadata and does not drive live fog. Diving, ascending and switching presets do not rewrite the base.

New writes use schema **v2** in the existing `user://lighting-presets.json` path and retain atomic replacement and corrupt-file backup. Schema v1 custom records load with their IDs, names, active ID and other settings intact. Their old density maps through the exact P1 response: `clamp(35 * sqrt(0.0035 / max(density, 0.0001)), 18, 60)` meters. This is lossy at the bounds: density 0 maps to 60 m, and 0.02 maps to 18 m. The original density is retained as metadata in v2 for later inspection/round trips. A missing density in an otherwise valid v1 record uses the historical 0.0035 default; v2 requires an explicit meter field. Invalid values and malformed JSON still back up and recover to Natural, separately from valid migration. Natural and Soft keep their other defaults and use a 35 m base.

The focused isolated-profile fixture creates two old custom presets, selects one, converts its saved JSON to schema v1, and removes the legacy density from the second to exercise the old default. It reloads and verifies IDs, names, current choice, caustics, wave speed, shaft count, legacy density and converted meter values. It saves schema v2, renames and updates the selected preset to 44 m, restarts the store, chooses and deletes the other custom preset, then checks a 61 m value and malformed JSON as distinct recovery cases. The paused GFX probe checks 18–60 m slider bounds, both visibility and haze labels, live fog-end response, restoration, and unchanged fish position. Native visual quality, physical input feel and full P4 V25 judgment remain pending.

| Check | Result |
| --- | --- |
| `./tools/dev build` | PASS, 0 warnings and 0 errors. |
| `./tools/dev inspect-depth` | PASS, `LIGHTING_PRESET_CHECK_OK v1=migrated v2=round-trip invalid=recovered` and `DEPTH_CHECK_OK`; expected corrupt-fixture warnings appeared. |
| `./tools/dev inspect-atmosphere` | PASS, `ATMOSPHERE_CHECK_OK` and `ATMOSPHERE_MOVEMENT_OK`; its optional-missing-screen warning is expected. |
| `git diff --check`; `package.json` | PASS; root version remains `0.3.6`. |

## C3/P3 — integration and diagnostic handoff (2026-11-10)

**Working-tree result:** implemented from clean `main` HEAD `02545dd77870d3afb29c1a017206f12b62b8212f`, the C3/P2 runner checkpoint. Root version remains `0.3.6`. Changes are uncommitted for the runner. P3 P1–P6 remain present; P7 native reef art repair remains separate and open.

Normal `main.tscn` still has one `WorldEnvironment` and one `UnderwaterAtmosphere`, with `p2_habitat.tscn` under Habitat and the P3 FAF terrain/dressing and ambient fish at runtime. The existing `WaterEffects` group contains one overhead surface, one caustic decal, one shaft mesh and GPU motes; the screen treatment is CanvasLayer 0 and the Help/PlayShell interface is layer 10. Standard player, terrain and reef materials use engine fog. The sole `fog_disabled` spatial shader is `water_surface.gdshader`: this intentional overhead transmission exception keeps the visible sunlit surface from becoming an opaque horizontal lid; its edge fades and depth cue remain in the shader. Shafts and caustic shaders do not opt out of fog. No new render pass, volume, draw geometry, geography or physics was added.

`WaterColumnChecks` now asserts continuity at both clamped water-column endpoints, deep paused preset preview, and depth-fog-only bypass/restoration while height/volumetric fog stay enabled. Existing checks cover shallow/middle/deep effective ranges and near/mid/far retention, finite bounds, v1/v2 custom preset migration/reload, disable/re-enable, fish/camera identity and unchanged motor state. The selected-player `--character-check` checks all five visual IDs and select/play/return/select/play routing. These are source and headless runtime assertions, not native pixel evidence.

### Native comparison prepared for P4

Run `./tools/dev measure-visibility` on a real Forward+ display. `game/dev/visibility_ab_probe.gd` instances the normal `main.tscn`, keeps the entire atmosphere enabled, and toggles only depth-fog extinction through `SetVisibilityDiagnosticBypass`. This is a **visibility treatment bypass vs new** comparison, distinct from `measure-atmosphere`'s whole-atmosphere on/off diagnostic. At `Routes/DepthShallow` and `Routes/DepthDeep`, it fixes the same camera eye and target for both treatments, warms 120 frames per sample, samples 180 frames and prints median frame milliseconds, draw calls, primitives, effective depth/range, adapter, renderer, viewport resolution and current base setting. The log ends in `VISIBILITY_AB_OK`; retain the complete `artifacts/logs/visibility-ab.log`, including any Godot errors. Record the selected GFX preset name from the paused panel next to the log; the probe prints the actual loaded meter base but does not infer a preset name from it. Compare on the same adapter/resolution/source and note any repeatable roughly 10% or greater slowdown for owner judgment. No native median or cost claim exists yet.

For visual screenshots, launch normal Character Select, choose a fish, enter the world and record source revision, fish ID, GFX preset, visibility meter value, adapter and resolution. Use the current route markers under `Habitat/Composition/ReefComposition/Routes`, with camera-relative/player-accessible checkpoints: `DepthShallow` at local `(3,0,21)`, `DepthMiddle` at `(3,0,0)`, `DepthDeep` at `(3,0,-21)`; the marker height is a route reference, so keep the fish about 1.5 m above its local floor and record actual world Y. First capture a near reef/obstacle and the `UpperLandmark` from the shallow lane, then approach `OpenMid` `(3,-1,-7)` and `OpenFar` `(3,-1.5,-18)` while facing along the lane; capture the same landmark receding on the return. Repeat a clear horizontal view at middle and deep, then shallow → deep → shallow. From the open lane, turn toward two different opposite playable boundaries and note whether a boundary is identifiable through water rather than merely hidden by rock. Finish at `TunnelEntry` and `ShelterEntry` for nearby navigation, and include player fish, surface/caustics/shafts and HUD/GFX in screenshots. These are authored reference points to verify in native play, not claims of completed traversal or view quality. Do not use the historical unreachable Z ±27/28 points.

### T2 results and limits

| Check | Actual result |
| --- | --- |
| `tools/dev` composition | Inspected: `check` runs import, build, smoke and many unrelated suites; targeted commands used. |
| `./tools/dev import` | **Blocked by sandbox**: Godot .NET editor could not open its local TCP listener/MCP socket and could not save global editor settings. File scan/import progressed, but `tools/dev` correctly rejected the engine errors. The new GDScript UID sidecar was generated and retained. |
| `./tools/dev build` | PASS, 0 warnings, 0 errors. |
| `./tools/dev inspect-atmosphere` | PASS, `ATMOSPHERE_CHECK_OK` and `ATMOSPHERE_MOVEMENT_OK`; expected optional-missing-screen fixture warning. |
| `./tools/dev inspect-depth` | PASS, `LIGHTING_PRESET_CHECK_OK` and `DEPTH_CHECK_OK`; expected corrupt-preset fixture warnings. |
| `./tools/dev inspect-faf` | PASS, all 38 environment and five fish catalog sources load. |
| Headless `--character-check` on `character_select.tscn` with isolated XDG data | PASS, `CHARACTER_CHECK_OK count=5 route=select-play-return-select-play`. |
| `./tools/dev measure-visibility` | **NOT OBSERVED**: X11 and Wayland displays unavailable in this sandbox, before the native sample can run. A headless script invocation parsed and reported `VISIBILITY_AB_FAILED native Forward+ adapter/window required` as intended. |
| `./tools/dev inspect-scene res://scenes/main.tscn` | Scene reported `SCENE_OK` but command failed due to pre-existing unwritable default `user://logs`; focused runtime checks used isolated XDG profiles. |
| `git diff --check`; `package.json`; tracked vendor/lock inspection | PASS; no whitespace errors, version `0.3.6`, no package-lock or vendor source injection. |

P4 still needs native screenshots and playable judgment for boundary concealment in two unobstructed directions, gradual near/landmark/far discovery, shallow/middle/deep response, near fish/obstacle/cave readability, surface and UI composition, actual preset reload, and matched native performance/errors. V25 and P3 G3.3 remain **NOT QUALIFIED**; P2 G2.3/V20 remains open. Route this source to C3/P4 native visibility closeout and the separate P3 P7 art repair review.
