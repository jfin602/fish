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
