# Correction 2 P4 — native water/light checkpoint

Date: 2026-11-10. Source candidate: uncommitted correction on `main` at `a2e7d46`; root `0.2.6`, saved and runtime renderer `forward_plus`. Device: AMD Radeon 880M (RADV GFX1150), Vulkan 1.4.318, X11, 1920 × 1200 exclusive fullscreen, Godot 4.7.2 .NET. Godot Editor MCP bridge had no connected peer; the actual native game and diagnostic windows were used. The ignored `artifacts/c2-native/` folder contains local logs and screenshots for human inspection.

## Owner input and visual limits

The owner played the earlier P4 candidate and said **“it looks good”**, then requested more wave detail, a moving land contact, drifting ground lines, more caustic-aligned shafts, an edge-on ribbon shape with diagonal fade, and GFX sliders with saved lighting presets. The revised source implements those requests, but **the owner has not yet played or judged the revised build**. No six-item PASS statement or explicit acceptance of its measured performance has been reported.

The revised normal main scene opened in a real Forward+ window without a runtime/shader error. A native spawn screenshot showed clear fish, rock and sand silhouettes and blue-water distance falloff. A real pause menu and GFX panel remained crisp over the world. In that panel, changing Wave detail from 0.80 to 0.32 updated the live slider, Save As created a named preset and wrote `lighting-presets.json`, Delete removed it and returned selection to Natural, and Back returned to pause settings. The resulting local file has `currentId: builtin:default` and an empty custom-preset list. These UI observations do not prove swimming motion or subjective surface/shaft/caustic quality. The earlier normal-play screenshots covered upper look, upper climb, pause and resume before the owner's revised requirements; they are **not reused as final visual qualification**.

| Revised-build item | Native finding | Owner judgment |
| --- | --- | --- |
| Overhead surface / sun-above read | Revised wave and larger plane compiled and rendered in native play; moving shoreline contact and sun read at the upper route were not directly observed after revision. | **NOT OBSERVED** |
| Volumetric / ribbon shaft look | Six active caustic-anchored ribbons loaded; edge-on shape, softness and diagonal fade need revised native route inspection. | **NOT OBSERVED** |
| Organic non-grid caustics | Generated texture remained deterministic with lit/dark gaps, and the focused check placed every active ribbon on a lit ridge. Several seconds of revised near/mid ground motion were not observed. | **NOT OBSERVED** |
| Underwater color, depth and readability | Native spawn screenshot showed readable fish/reef/sand silhouettes and water depth, but the full mid-depth/open/upper route judgment remains outstanding. | **NOT OBSERVED** |
| Motion comfort and stable camera image | Headless equal-physics route passed and native pause/GFX were crisp; a revised open+tunnel+upper swim and pause/resume comfort judgment were not completed. | **NOT OBSERVED** |
| Representative native performance | Same-run on/off counters were collected below. Capped run is near 60 FPS; uncapped cost exceeds the 10% investigation threshold and awaits explicit owner decision after tuning. | **NOT OBSERVED** |

The final owner checkpoint needs a revised Forward+ swim through open water, one tunnel/shelter, and the upper route, with upward and multi-angle shaft views, several seconds of moving caustics, and one pause/resume. A still image cannot qualify animation or comfort.

## Same-revision performance A/B

`./tools/dev measure-atmosphere` uses the same normal scene instance, device, resolution and fixed OpenFar/UpperLandmark views for effects on/off. Each mode/view warms for 120 frames and samples 180 frames. The revised default has six active ribbon shafts in one mesh surface, one caustic decal, 128 low-detail motes, and a 48 × 48 surface. The capped native run recorded:

| View | Effects | Median FPS | Median wall frame ms | Draws | Primitives |
| --- | --- | ---: | ---: | ---: | ---: |
| Open | On | 59 | 16.64 | 9 | 233,440 |
| Open | Off | 60 | 16.65 | 6 | 222,492 |
| Upper | On | 60 | 16.68 | 17 | 402,020 |
| Upper | Off | 60 | 16.66 | 13 | 391,056 |

That is +3/+4 draw calls and +10,948/+10,964 primitives. The display cap obscures headroom. Two uncapped native runs on the same 64-slice fog configuration reported Open **111 vs 131 FPS, 8.55 vs 6.92 ms**, then **66 vs 106 FPS, 15.73 vs 8.21 ms**; Upper **105 vs 124 FPS, 9.26 vs 7.49 ms**, then **69 vs 87 FPS, 16.11 vs 11.47 ms** (on vs off). System timing varied strongly between runs, so these do not yield a stable single cost percentage. Both nevertheless cross the ~10% investigation guardrail. An earlier three-run capped candidate with high-detail motes held 60 FPS but added about 543k primitives; reducing mote mesh detail is the successful bounded repair. A trial of 48 × 48 volumetric fog resolution yielded 106 vs 137 FPS Open and 111 vs 130 Upper uncapped, without meaningful improvement; it was reverted to engine defaults. There is no explicit owner acceptance of the remaining larger cost. **Performance is not green.** Historical P7 values (54/53/49/41) were not used as the same-run baseline.

## Failed attempts and verification

- An exploratory uncapped command accidentally invoked the system's non-.NET Godot binary, causing C# script-loader errors and a timeout. It was rerun with the pinned Godot .NET executable; the failed output is in `updated-ab-uncapped-1.log` only until the successful retry replaced that filename, so the exact failed console transcript is retained in this task conversation, not claimed as a valid sample.
- An earlier overlapping native launch produced X11 `NO GRAB` when two game windows competed for mouse capture. It was stopped and the subsequent single-window normal play showed no such error.
- The component isolation probe produced unstable frame/FPS readings as screen/fog/motes/surface/shaft/decal visibility changed; it was used only to identify investigation targets, not as acceptance data. Its log is `artifacts/c2-native/updated-components.log`; the temporary script was deleted.
- `./tools/dev build` passed with zero warnings/errors; `./tools/dev inspect-atmosphere` passed its focused structural and equal-movement checks. The initial build had two nullable warnings, both repaired before the passing build. Native normal play and GFX preset interaction ran without shader/runtime errors. The optional missing-screen-shader warning in the focused negative case is expected.

All screenshots and logs are local ignored artifacts, not committed acceptance evidence. No package version or P2 roadmap Gate was advanced.
