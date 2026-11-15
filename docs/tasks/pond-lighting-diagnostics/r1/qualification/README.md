# R1 native qualification and limits

## Source and setup

- Base revision: `7866d7b500cbd6c148c99cfb30753f2efd9709f6` on `main` (`smooth camera + sfx`), clean before this task. Candidate source and evidence files are uncommitted; [measurements](measurements.json) record SHA-256 hashes for the initial four-file fog candidate and the later two-file owner shadow follow-up.
- Modified production source: `game/scripts/UnderwaterAtmosphere.cs`, `game/scripts/WaterVisualProfile.cs`. Focused check expectations changed in `game/dev/PondAtmosphereChecks.cs` and `game/scripts/PondCameraImmersionChecks.cs`. Documentation and images are under this R1 folder plus concise BOOT/roadmap/system routing.
- Native system: Godot 4.7.2 .NET, Forward+, AMD Radeon 880M Vulkan, Linux/X11, 1920×1200. Pond seed 71 (`world-5e3b4fee759acbe3`), adult Bluegill `FAF-FISH-006`, Pond Overcast, fixed solar noon. The baseline was captured **before** the production edit in this task. Both sides used the same diagnostic probe and a real `StaticBody3D` ground contact at `(-30.11457, 3.814294, 0.025251)`. The normal `FollowCamera.SnapToFish` placed the eye at `(-30, 6.294959, 3.8)`.
- Godot Editor MCP version `2026.11.04` and its live project identity as this checkout's `game/` were verified after an older Fish-only bridge listener was released. The isolated native capture probe ran in the actual game renderer; it did not save scenes or modify the authoritative world. Temporary probe files, machine paths, and full-resolution run output are ignored artifacts.

## Fog configuration

| Setting | Before, matched ground pose | R1 candidate, matched ground pose |
| --- | ---: | ---: |
| Mode | Depth | Exponential |
| Density | `0.7800` maximum blend | `0.10146` exponential density |
| Fog color, linear RGB | `(0.0874, 0.1991, 0.1697)` | `(0.1397, 0.1859, 0.1262)` |
| Depth begin/end/curve | `4.5 m / 15.968 m / 1.55` | Inactive in exponential mode |
| Above-water density | Depth mode, maximum blend `0.94` over 27–105 m | Exponential `0.01`; blended continuously across camera immersion |
| Global volume and local turbidity | Existing values | Unchanged |

The previous 4.5 m clear foreground and steep finite depth interval created the measured color contour on sloping sediment. The candidate uses an unbounded exponential response, so there is no distance at which a depth-fog interval begins or saturates. Density is `0.09 × 18 / EffectiveUsefulRange`; the existing 18 m Pond Overcast visibility control and `DeepRangeFactor=0.68` set shallow-to-deep attenuation, and the Brighter Shallows visibility setting still changes this result. At the default preset, surface density is `0.09`, deep density is about `0.132`. Ocean remains in Depth mode with its original `0.96`, `5 m`, and `1.2` curve. Pond fog color changes from green/blue-green to muddy olive at ordinary and deep depths so distance absorption does not look like a separate green layer. Night fog still blends to the existing night palette. Godot documents [exponential mode as density-based fog](https://docs.godotengine.org/en/stable/classes/class_environment.html); depth begin/end/curve apply only to Depth mode.

## Bounded candidate selection

The unchanged diagnostic probe first compared three finite-interval candidates with earlier onset, lower curves, wider intervals, and warmer color. They reduced the sampled ground-pose red/green gradient from `0.204` to `0.149`, `0.165`, and `0.121` but retained a visible finite floor contour. Three exponential trials at densities `0.10`, `0.14`, and `0.18` removed the finite edge. The latter two made nearby cover too dark, so the `0.10` ground-pose trial guided the final depth-scaled `0.10146` candidate. [Trial sheet in ignored artifacts](../../../../../artifacts/pond-lighting-diagnostics/r1/exponential-trials.png) is local evidence, not a shipped image.

## Matched observations

The [ground and camera comparison](../comparison/ground-translation.png) holds fish and Sun fixed. At center column `x=800`, averaging `x=780…820` and measuring 50 px smoothed red-minus-green gradients across `y=350…850`:

| Frame | Red/green crossover | Strongest gradient (RGB levels/px) |
| --- | ---: | ---: |
| Baseline stationary | `y=476` | `0.209` |
| Candidate stationary | `y=630`, lower contrast | `0.113` |
| Baseline, camera +4 m | `y=600` | `0.131` |
| Candidate, camera +4 m | no crossover in sampled interval | `0.071` |

The baseline's strong crossing travels **124 px** with the camera (the previous investigation measured 122 px with its sample method). The candidate's stationary crossover is not a new end distance: exponential fog has no finite end, and the measured color change is about 46% less steep. Some broad darkening remains in the distance and can still move with viewpoint because global volume, Pond turbidity, and world-space sediment are unchanged. The native frames show no distinct replacement ring at the measured near-ground route; the owner must judge that claim during free swimming.

The [normal route](../comparison/swim-route.png) used actual keyboard thrust with the follow camera: forward, backward, turns, lateral movement, shoreward and return. The capture is wall-clock timed, so baseline/candidate endpoints differ slightly. The [stationary sequence](../../../../../artifacts/pond-lighting-diagnostics/r1/candidate-stationary/manifest.json) and [camera rotation sequence](../../../../../artifacts/pond-lighting-diagnostics/r1/candidate-camera/manifest.json) are retained locally. [Shallow](../candidate/shallow.png), [deep](../candidate/deep.png), [twilight](../candidate/twilight.png), [midnight](../candidate/midnight.png), and [vertical waterline](../candidate/vertical-crossing.png) native frames show stronger attenuation at depth, nearby fish/cover silhouettes, preserved night darkness, and a continuous immersion color blend in scripted camera movement. The [air-side frame](../candidate/air.png) keeps the existing surface transmission and reflection geometry. Compared with the prior diagnostic's same fixed air pose, its full-frame mean absolute RGB difference is `(0.156, 0.256, 0.420)` RGB levels on a 0–255 scale; that earlier frame is from a different source revision, so this is a limited visual check rather than a strict pixel-isolation proof.

## Checks and cost

- `./tools/dev import` and `./tools/dev build`: passed; C# build had zero warnings/errors.
- `./tools/dev inspect-pond-atmosphere`, `inspect-pond-camera-immersion`, `inspect-pond-surface`, `inspect-solar-lighting`, `inspect-depth`, and `inspect-world-isolation`: passed. These cover depth profile, presets, smooth immersion/re-entry, Ocean depth mode and Pond→Ocean→Pond state isolation. The immersion check initially failed because it asserted the old `0.78` Pond value; its assertion was updated to test the new depth-scaled coefficient, and the rerun passed. Expected invalid-preset recovery traces in `inspect-depth` are test inputs.
- Native candidate logs for fixed views, camera translation/rotation, forward/reverse swim, stationary, day/night, vertical crossing and matched performance contain no shader or rendering error marker. The vertical camera sequence is scripted presentation movement with a fixed fish; actual fish ascent and prolonged owner play remain unsampled.
- A fixed-pose ABBA comparison reconstructed the baseline's exact depth-fog values in the candidate process, changing only Environment fog properties. Four 90-frame native samples, VSync disabled and FPS uncapped: legacy medians **5.396 / 5.274 ms**, candidate **5.062 / 5.088 ms**; both had **234 draws / 296,714 primitives**. P95 values were `8.362 / 6.859` versus `11.196 / 7.452 ms`. One candidate tail spike means this sample does not prove a speed gain. GPU-only timing was unavailable. The normal-input route measured median/p95 `5.427 / 9.298` ms baseline and `5.356 / 8.495` ms candidate; route lengths and visible draw sets differ slightly, so this is context rather than an isolated fog cost.

## Owner review boundary

The real World Select → Freshwater Pond → adult Bluegill route was launched through the identified Fish Godot Editor MCP connection. The live scene tree contained `Main`, `Fish`, `UnderwaterAtmosphere`, and the normal Pond habitat; [the running-game frame](../candidate/normal-play.png) records that route. After final C# checks, the editor play session stopped for recompilation. A fresh native Forward+ `main.tscn` process was then launched with the Pond Bluegill selection flag `--pond-surface-probe`; its output confirms the baked seed-71 Pond, adult Bluegill, waterline, and atmosphere. The owner then supplied a screenshot showing a remaining hard local shade edge, and reported that the first filter-blur correction left it visible. The [R1 follow-up](../README.md#owner-screenshot-follow-up--hard-local-shade-edge) reproduces the jagged left-log cast shadow and records the revised angular-penumbra comparison and measured cost. The revised candidate remains **NOT GREEN** pending the owner's normal Pond swim at that location. The owner should check the low camera angle, forward/reverse/turning motion, denser vegetation and log cover, surface crossings, and deeper water. A broad dark region remains where the fixed volumetric stack and terrain meet; any objection to that residual darkness belongs in R2 only after owner direction. No R2 tuning, version advance, Phase/Gate closeout, or assertion of owner approval is made here.
