# C4 water clarity — P1 implementation candidate

**2026-11-11.** Source base `69a5e3d85de605bb0dabb5e89ddcde65ad91b065` on clean `main`, root `0.4.11`. This is an uncommitted direct implementation candidate. P12 V32, G4.2 and C4 remain **NOT GREEN**; P2 owns swimming, appearance and frame-pacing qualification. Historical [P12 R1–R3 evidence](native-surface.md) is unchanged.

## Implementation

`PondTurbidityField.cs` samples two seeded low-frequency 3D noise fields in world coordinates. The existing seed-71 Pond basin supplies shoreline bounds, water height, floor height and near-bed sediment influence. Shallow open water receives less sediment; selected shoreline pockets and water immediately above mud receive more. The result is a bounded, static 64×16×64 R8 texture, made once at Pond load. Its 64 KiB texel payload and one box `FogVolume` use Godot's existing Forward+ volumetric fog path. The box covers 151.42×15.22×129.16 m for the installed Pond. No new bake/schema, fog planes, fullscreen pass, particles, lights or gameplay state were added.

`UnderwaterAtmosphere.cs` attaches the volume only to Pond. The density field remains fixed when the camera swims, turns or zooms; camera immersion smoothly scales its contribution for waterline crossing. The built-in Pond distance-fog extinction changes from `0.70` to `0.58`; its 3 m start and preset distance ends (18 m Overcast, 24 m Brighter Shallows) remain. Pond global volumetric density is 72% of its previous preset-driven value, while the new volume adds at most `0.11 × field sample` (normal sampled range `0.220–0.572` at 2 m below the seed-71 surface). The fog color and R2 surface shader/transmission are unchanged. Ocean still uses its independent `0.96` distance fog, 5 m start and original volume scaling. Sun, shadow, ambient, caustic and shaft controls remain illumination inputs; no shadow or sun value enters the sediment field.

The development-only `--pond-clarity=off` argument disables the local field at load without changing the baked Pond or saved GFX presets. `SetPondClarityDiagnosticEnabled` permits an in-session fixed-view compare. The R8 texture is created once, with no per-frame generation or movement.

## Checks and observations

Godot `4.7.2` .NET Forward+ and .NET build ran on Linux/X11. `tools/dev import`, `build`, `inspect-pond-atmosphere`, `inspect-pond-surface`, `inspect-pond-camera-immersion` and `inspect-pond-scenery` passed. Build: 0 errors, two `NU1900` restore warnings because vulnerability metadata was unavailable. The Pond atmosphere check loaded seed 71 → Ocean seed 42 → Pond seed 71; profiles, saved lighting settings, fog/material isolation and fish state passed. Its 191 sample locations measured field values `0.220–0.572`, repeated exactly for the same seed and changed by less than `0.08` across a 0.25 m sample step. A bed-proximity probe, camera-anchor probe and diagnostic on/off restore passed. The new texture build took 25.97 and 19.20 ms in the two headless Pond loads.

A bounded native fixed-view smoke used the installed seed-71 Pond, Godot Forward+, AMD Radeon 880M, X11 1920×1200 at 60 Hz, the existing P12 diagnostic primitive fish and identical poses with clarity on/off. The [underwater on](spatial-clarity-evidence/pond-on.png) and [underwater off](spatial-clarity-evidence/pond-off.png) images show the local field changes distant cover while retaining the near fish and log mouth. The [air-on overview](spatial-clarity-evidence/air-on.png) retains the dark muddy-green surface; on/off air image mean absolute RGB difference was `0.055/0.043/0.037` of 255. Native logs contained no `ERROR:` or `SCRIPT ERROR:` after the texture-layout repair. The first attempted native probe accidentally loaded Ocean and produced missing Pond-landmark errors; a second attempt exposed an incorrectly ordered 3D texture slice layout. Both were corrected before the clean on/off runs.

| Fixed view | Clarity on median / p95 | Clarity off median / p95 | Draws on/off |
| --- | --- | --- | --- |
| Underwater near cover | 16.66 / 17.09 ms | 16.66 / 17.25 ms | 195 / 195 |
| Middle | 16.66 / 17.02 ms | 16.67 / 17.87 ms | 133 / 133 |
| Deep | 16.65 / 17.32 ms | 16.66 / 17.67 ms | 141 / 141 |
| Air overview | 16.64 / 16.98 ms | 16.65 / 17.12 ms | 205 / 205 |

These are capped, stationary 90-frame samples, not GPU cost or moving-frame pacing proof. The fixture fish is a diagnostic primitive rather than the selected Bluegill. P2 still needs real swimming through clear, ordinary and silty regions; near-field species/cover readability in shadow; waterline crossing/orbit/zoom; Ocean native comparison; and route-level uncapped performance. The 4–6/2–4/1–2 m visual targets are unmeasured artistic aims, not an asserted result. No owner visual acceptance was recorded.
