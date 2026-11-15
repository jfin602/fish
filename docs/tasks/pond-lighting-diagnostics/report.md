# Pond lighting instability: native diagnosis

**Status:** DIAGNOSIS COMPLETE — AWAITING OWNER REPAIR DECISION. No shipping scene, shader, script, graphics preset, version, Gate, or bake was changed for this investigation.

## Executive finding

The severe green/dark sweep over the sediment at the requested low fish/high camera pose is a **camera-distance fog boundary over a sloped floor**. The Pond's depth fog begins 4.5 m from the camera, reaches its configured end at 15.968 m in this pose, and blends toward a dark green fog color with density 0.78 and curve 1.55. As the normal follow camera moves, the same distance blend intersects different sediment. This creates the impression of a large shadow travelling with the fish. The world-space basin color pattern and the global/local volumetric fog add contrast and darkness. The real Sun shadow map creates localized fish/log/vegetation shade but is **not** the broad band: the measured transition remains with shadows disabled and disappears at the sampled column with depth fog disabled.

The most convincing matched comparison is [baseline / shadows off / depth fog off / volumetric fog off / both fog layers off](evidence/01_ground_material.png). The [controlled camera comparison](evidence/03_camera.png) shows the band moving with the camera even when real shadows are off. This is an undesirable visual boundary produced by intentional visibility effects; it is not evidence of a corrupt shadow map. A distinct rapid shadow flicker elsewhere remains unconfirmed by this route.

## Preflight and native conditions

- Repository `jfin602/fish`, branch `main`, HEAD `08810c654ec84709c320006c899047ce63ade95d` at start. The tree was already dirty: `BOOT.md`, `MODULES.md`, `game/project.godot`, `game/scripts/PikeEncounter.cs`, `tools/dev`, audio/docs files and new audio assets. All were preserved. This task added only diagnostic documentation and temporary ignored artifacts; the probe was removed from `game/dev` after captures.
- Godot `4.7.2-stable` .NET/C#, Vulkan 1.4.318 Forward+, AMD Radeon 880M (RADV GFX1150), Linux/X11, 1920×1200. VSync disabled and frame rate uncapped for cost samples. GPU-only timing was not exposed by the probe. `./tools/dev build` passed with zero warnings and errors before captures.
- Actual `res://scenes/main.tscn`, baked Pond seed 71 (`world-5e3b4fee759acbe3`), adult Bluegill `FAF-FISH-006`, Pond Overcast, noon unless noted. The middle Pond area has sediment, a sloping bank, plants, and two nearby logs.
- The fish was moved downward by `CharacterBody3D.move_and_collide` until it **collided with the ground StaticBody3D**: contact `(-30.11457, 3.814294, 0.025251)`, normal `(0.261526, 0.963474, -0.05764)`, fish center `(-30, 4.294959, 0)`. The real `FollowCamera.SnapToFish` placed the camera at `(-30, 6.294959, 3.8)`, 2.0 m above and 3.8 m behind. The [contact image](evidence/01_ground_material.png) shows this ordinary in-game angle. The collision is verified at the start; later swimming is controlled by the actual fish motor and may leave that contact point.
- The abandoned bridge on port 19080 was closed with the owner's permission. A fresh Godot editor and direct editor bridge were launched and identified as the Fish project before native work. The bridge reported addon `2026.11.04`, Godot 4.7.2, and the Fish path. The captures used the same project in native Forward+ debug runs. No editor scene was saved.
- The prior [R2 optical candidate](../p4/qualification/underwater-clarity-iterations/r2/README.md) introduced the Pond's 4.5 m / 0.78 / 1.55 depth fog settings; [R3 lighting](../p4/qualification/underwater-clarity-iterations/r3/README.md) introduced the one-map shadow and basin sunlight variation. Both remain owner-unapproved candidates in the [roadmap](../../roadmap.md); P4/P12/V32 remain **NOT GREEN**. Historical R3 cost evidence is context, not a substitute for the matched measurements below.

The ground-pose [settings manifest](evidence/ground-fog-settings-manifest.json) records the live values. The Sun used mode `0` (orthogonal), 28 m maximum shadow distance, fade start `0.8`, 4096 directional shadow size, bias `0.1`, normal bias `2`, blur `1`, and noon energy `0.567`. The overhead water surface and shaft mesh are explicitly excluded as shadow casters in `UnderwaterAtmosphere.cs`; solid cover and fish can cast real shade.

## Lighting pipeline and coordinate ownership

| Component | Source | Position/time relationship | Diagnostic result |
| --- | --- | --- | --- |
| Sun and real shadow map | `game/scripts/UnderwaterAtmosphere.cs` lines 233–241, 382–396, 511–521 | Solar clock rotates the light; one 28 m orthogonal map has camera-relative coverage; light energy also responds to camera immersion/depth. | Local cast shade, not the broad band. Disabling it removes 89 draws at the ground pose but leaves the band. |
| Pond basin albedo | `game/assets/faf/pond/materials/pond_basin_variation.gdshader` lines 11–30 | `MODEL_MATRIX * VERTEX` anchors broad/alluvial/slope colors in world space. | Fixed color variation increases perceived contrast. Flattening it changes the floor color but does not alone identify the travelling edge. |
| Basin sunlight pools | Same shader lines 31–42; `UnderwaterAtmosphere.cs` line 362 | World-coordinate water-entry calculation plus presentation `surface_time`; solar direction/energy set by shared clock. | Small temporal modulation; freezing or disabling pools leaves the broad band at the same central row. |
| Distance/depth fog | `UnderwaterAtmosphere.cs` lines 207–224, 457–476 | Fog begins/ends by **camera distance**, with parameters updated from camera immersion and depth. | Primary color-boundary source. Depth fog off removes the green-to-brown crossing at the measured floor column, while local shadows remain. |
| Global volume and Pond turbidity | `UnderwaterAtmosphere.cs` lines 225–230, 331–351, 477–488 | Camera-view volumetric integration; turbidity density field is world anchored. | Each adds broad dark/green attenuation; removing both fog layers gives the clearest sediment and cover. No independent rapid flicker confirmed here. |
| Caustic decal and shafts | `UnderwaterAtmosphere.cs` lines 361–373, 700–765 | Projected in world space; drift with presentation time. | Minor/subtle at this pose; disabling either does not remove the band. |
| Water surface and screen shader | `UnderwaterAtmosphere.cs` lines 361–364, `game/shaders/underwater_screen.gdshader` lines 10–21 | Surface motion is animated; screen warp/shimmer uses `SCREEN_UV`. | Small visual modulation, not the floor-wide boundary. Screen-off pair is almost identical. |
| Environment clock | `game/scripts/EnvironmentClock.cs` lines 16–18, 64–68, 103–119 | One shared 900 s day, noon start; debug acceleration up to 120×. | At fixed noon solar direction is steady. Accelerated clock changes real solar lighting and turns shadows off near twilight by design. |

The ordinary follow camera is in `game/scripts/FollowCamera.cs` lines 61–83, 131–171. Its movement reads the fish; it does not advance the solar clock or edit basin material coordinates. The visual coupling comes from camera-relative fog distance and the camera following the fish.

## Four native observation tests

### A. Stationary fish and camera, fixed noon, presentation running

Eleven 1920×1200 frames were captured at about one-second intervals over ten seconds. The central green-to-brown crossing at image column `x=800` was `y=476` at 0, 5, and 10 s. At `x=400` it moved from 404 to 419 px, and at `x=1200` from 600 to 617 px, reflecting smaller animation/scene changes away from the central sample. In the floor ROI `(100,350)–(1800,1100)`, 0-to-10 s mean absolute RGB difference was `(1.78, 1.38, 0.52)` on 0–255 pixels. The dominant band did not sweep across a stationary scene. [0/5/10 s contact sheet](evidence/02_stationary.png).

The controlled basin-time follow-up held the same fish/camera/solar pose. With the pool material cloned and frozen, the central crossing was still 476 px at 0 and 10 s and ROI change fell to `(0.45, 0.45, 0.21)`; with pool energy zero it was `(0.46, 0.44, 0.20)`. Thus animated pools account for modest temporal pixel change, not the dominant moving band. [Normal/frozen/off 10 s comparison](evidence/09_pool_time.png). One-second sampling cannot rule out brief sub-second flicker; no discrete pop was observed in these frames.

### B. Controlled camera movement with fish and solar time fixed

At the ground pose, the camera was translated laterally by five 0.8 m steps, then rotated in four 5° steps, then returned. At `x=800`, the normal crossing moved from `y=476` to `y=598` over a 4.0 m camera displacement: **122 screen pixels**. The same test with real shadows disabled moved it from `476` to `597`: **121 pixels**. With depth fog disabled, pixels at `x=800` from `y=400` through `650` remained sediment-brown (`R-G > 0`) at both ends, so there was no comparable green-to-brown crossing. The logs shift with perspective as expected; this is not a world-motion measurement. [Camera contact sheet](evidence/03_camera.png) and [manifest](evidence/ground-camera-v2-manifest.json).

The shader's albedo variation is mathematically world anchored, while a distance-fog contour is centered on the eye. The band slides over fixed logs and plants in the camera sequence because the fog/geometry intersection changes. We did not reconstruct the contour's world-space displacement from depth-buffer samples, so only screen displacement is measured.

### C. Normal swimming with the actual follow camera

The diagnostic sent the regular keyboard swim input through the actual fish controller: forward, backward, turn, lateral out/back, turn, shoreward, return. Solar time stayed at noon. The first forward segment moved the fish from `z=0` to `z=-7.233 m` over about 2 s, with camera `z=3.8` to `-3.331 m` and the `x=800` crossing `y=476` to `514` (**38 px, roughly 19 px/s in that segment**). Backward motion brought the crossing near `y=481`. These are screen-space measurements; perspective and changing ground depth prevent treating 19 px/s as a physical shadow speed. The fish remains near the center while logs and the band travel through the frame, matching the owner's description. The 36-frame [animation](evidence/ground-motion.gif), [contact sheet](evidence/04_motion.png), and [motion manifest](evidence/ground-motion-manifest.json) record input segments and transforms. The path starts at verified ground contact; the fish's later vertical coordinate stays 4.295 m and is not asserted to be in continuous collision across varying terrain.

### D. Fixed camera/fish, day/night and accelerated solar time

Noon, 17:00, 18:30, and midnight were captured at the same pose. Real shadows are on at 12:00 and 17:00, off at 18:30 and 00:00. At 120× debug time, sampled solar hours advanced 13.614 → 15.229 → 16.829 → 18.430 → 20.040 → 21.645 → 23.253 → 00.860, with shadows off from the 18.430 sample. These large lighting changes follow the shared solar clock and the intentional direct-light threshold; they are distinct from the noon camera-following band. [Day/night contact sheet](evidence/05_daynight.png) and [manifest](evidence/ground-daynight-manifest.json).

## Controlled ablation and cost

All ablations used the same ground-contact fish, actual follow camera, fixed noon, 24 warmup frames, and 90 uncapped CPU frame-interval samples for benchmarked rows. Each rendering change was restored before the next. Median/p95 are milliseconds; p95 varies with host scheduling and shader/cache work, so a single run is not a GPU benchmark. Draws and rendered primitives are per-frame engine monitors. [Ablation contact sheet](evidence/06_ablation.png), [fog stack](evidence/08_fog_stack.png), and [manifest](evidence/ground-ablation-v2-manifest.json).

| Temporary change | Broad floor band | Median / p95 ms | Draws / primitives |
| --- | --- | ---: | ---: |
| Normal baseline | Present | 5.722 / 9.388 | 234 / 296,714 |
| Real shadows off | Present; local fish/cover shade removed | 4.253 / 7.311 | 145 / 126,566 |
| Basin sunlight energy zero | Present | 5.805 / 8.844 | 234 / 296,714 |
| Shadow range 16 m | Present; local coverage changes | 5.359 / 8.510 | 213 / 294,732 |
| Shadow range 42 m | Present; local coverage changes | 5.495 / 6.263 | 261 / 298,548 |
| Two shadow splits | Present | 5.756 / 9.975 | 289 / 464,084 |
| Four shadow splits | Present | 6.179 / 8.363 | 414 / 695,136 |
| Depth fog off | Green-to-brown crossing removed at measured column; cover shadows remain | 5.720 / 9.355 | 234 / 296,714 |
| Combined minimal: shadows, pools, caustics, shafts, local/global volume, screen, depth fog off | Strong sweep absent; plain sediment | 2.900 / 7.591 | 143 / 126,548 |

Other paired single-component captures remove the caustic decal, shafts, global volume, Pond turbidity volume, directional volumetric contribution, underwater screen effect, and height fog. None alone removes the entire visual stack. Global volume off changes floor ROI by mean absolute RGB `(7.51, 5.50, 5.28)`; Pond turbidity off by `(6.60, 4.75, 4.59)`; depth fog off by `(3.81, 0.64, 1.63)` against the same baseline. These differences measure magnitude, **not** which image is preferable. The depth-fog isolation specifically removes the sampled color crossing; the volume isolations reduce dark haze. Disabling real shadows changes the same broad floor ROI by only `(0.08, 0.06, 0.02)` because it mostly affects local cast-shadow pixels.

The material-only isolation replaced static basin albedo with a constant while keeping its moving sunlight formula, direct light, shadows, and fog. It strongly changes overall brightness and leaves some distance darkening, so it cannot by itself prove a material-rooted band. Individual removal of broad sine, alluvial field, and alluvial gain produces smaller fixed color changes and does not eliminate the camera-distance transition. [Material components](evidence/07_material_components.png). The temporary material was restored; no shader file was saved.

## Root-cause assessment

| Component / confidence | Observation and reason for apparent motion | Performance and possible repair |
| --- | --- | --- |
| **Depth fog — CONFIRMED dominant band source.** `UnderwaterAtmosphere.cs:214–220,457–476`. | Fog color is green compared with warm brown sediment. The active camera-distance interval is 4.5–15.968 m; its intersection with sloping ground makes a broad contour. Same contour survives shadow/pool removal and travels with camera; disabling depth fog removes the measured green-to-brown crossing. | No draw-count change when toggled at this pose. Evaluate a smoother, less contrasting Pond-only transition; retain fog visibility intent and verify all depths/times. |
| **Volumetric fog + Pond turbidity — LIKELY secondary darkening.** `UnderwaterAtmosphere.cs:225–230,331–351,477–488`. | Each individual isolation visibly lifts/changes broad dark green attenuation; combined fog-off is clearest. The local density texture is world anchored, but volume integration and sampled depth are view dependent. No discrete temporal reprojection flicker was proven here. | GPU timing unavailable; same draw count is not a zero-cost claim. Tune density/volume color only after depth-fog candidate review. |
| **Static basin color — CONFIRMED fixed contrast contributor, EXCLUDED as sole camera-following cause.** `pond_basin_variation.gdshader:11–30`. | Alluvial and broad colors are based on world coordinates, so the pattern itself cannot follow the camera. Material isolates change patches, while depth fog still creates a distance boundary. | Material tweaks should preserve bank/sediment readability. No draw-count change in the matched material experiment. |
| **Animated basin sunlight pools — CONFIRMED small temporal contributor, EXCLUDED as main band.** `pond_basin_variation.gdshader:31–42`, `UnderwaterAtmosphere.cs:362`. | Freezing pool time reduces 10 s floor pixel change; zero pool energy leaves the band. This is world-projected slow light, not camera-locked. | Pool-off median was within noisy baseline range; no draw change. Lower amplitude only if owner dislikes residual shimmer after fog repair. |
| **Real Sun shadow map — EXCLUDED for the broad band, POSSIBLE for separate local shadow instability.** `UnderwaterAtmosphere.cs:233–241,389–396`. | Shadow-off image keeps the band and its 4 m camera travel. Actual local cast shade around fish/logs vanishes. Range/split changes alter local coverage/cost, not the broad color boundary. This test does not prove every cover-shadow angle stable. | Shadow-off saves 89 draws and 170,148 primitives at this pose. Keep one map for natural cover shadows; investigate only a separately reproduced local flicker. |
| **Caustics, shafts, screen treatment — EXCLUDED as main band; POSSIBLE subtle motion.** `UnderwaterAtmosphere.cs:361–373,700–765`; `underwater_screen.gdshader:10–21`. | Isolating each leaves the broad band. Shaft and screen pairs are near identical at this view; caustics make a small projected change. | Avoid removing them as a band fix. |
| **Solar clock — EXCLUDED at fixed noon.** `EnvironmentClock.cs:64–68,103–119`. | Clock was disabled in fixed tests. Accelerated-time changes are expected and separate from camera motion. | No repair indicated. |

## Direct answers to the owner questions

1. **What creates the large dark bands?** Mainly the Pond's camera-distance depth fog blending dark green over warm sediment on a sloping floor. Global/local volumetric fog deepen the dark side; world-space albedo variation adds quieter patches.
2. **Why at the player's speed?** The ordinary camera follows the fish. Its fog-distance contour therefore moves through the world at camera speed, while the fish stays near screen center. The 4 m fish-fixed camera test reproduces the travel.
3. **Is the 28 m orthogonal shadow map responsible?** No for this broad band. Shadow-off, 16/42 m range, and 2/4 split tests leave it. The map supplies local cover shadows and has measurable cost.
4. **Are animated sunlight pools contributing?** Small, slow luminance variation, yes; not the broad moving boundary. The central edge remains at 476 px in normal/frozen/off ten-second tests.
5. **Caustics, shafts, volumetrics?** Caustics and shafts are minor here. Global and Pond-local volume materially darken the view, but no rapid independent flicker was confirmed in the sampled ground route.
6. **One system or multiple?** One dominant camera-relative depth-fog transition with overlapping volumetric attenuation and fixed basin color contrast. Separate local shadow instability remains an unconfirmed possibility, not an explanation for this band.
7. **Intentional versus artifact?** Depth visibility loss, world sediment variation, sunlight pools, true cover shade, and solar-time movement are designed effects. Their combined, conspicuous camera-following green/dark line is the objectionable visual artifact. No evidence of a broken shadow atlas was found at this pose.
8. **Lowest-risk repair?** Tune the Pond's depth-fog transition and color first, preserving the real Sun shadow map. Judge the result in native near-ground movement and other camera depths before changing the volume or basin pattern.

## Ranked repair strategies for owner selection — analysis only

| Rank | Proposed change | Expected result / cover shadows | Cost, complexity, regression risk | Confidence |
| --- | --- | --- | --- | --- |
| **1. Minimal optical transition** | Pond-only adjustment of depth fog begin/end/curve and fog-light color to reduce the abrupt warm-to-green boundary at 4.5–15.968 m. Keep the shadow map and local cover. | Directly targets the confirmed camera-relative crossing without removing fish/log/plant cast shade. | No new pass or draw call expected; low code complexity. Risk: distant visibility, underwater mood, and bank reading may weaken; needs native route and night/Ocean checks. | High for band reduction; medium for final look. |
| **2. Coordinated fog stack retune** | After option 1, modestly rebalance global volume and Pond turbidity density/color against the new depth fog. | Could reduce remaining dark haze while keeping cover shadows and world sediment patches. | More variables and native review; volumetric GPU cost may change but no GPU timer was available. Risk: flattening depth/stealth readability or changing surface transitions. | Medium. |
| **3. Rework optical model and basin contrast** | Redesign distance extinction and sediment albedo contrast together, possibly a more physical world-depth treatment. | Largest control over the full look; real cover shadows can remain. | Highest implementation/art risk and qualification cost; could affect multiple camera depths and visual presets. | Low to medium until a candidate is rendered. |

Changing shadow range/split count is not a ranked band repair: it leaves the crossing and adds substantial rendering work at two/four splits.

## Reproduction and evidence

From the repository root, use the ignored diagnostic probe retained at `artifacts/pond-lighting-diagnostics/pond_lighting_diagnostic_probe.gd`. The local `tools/local.env` owns the Godot .NET executable as `GODOT_CMD` on this host. The probe is a one-shot native runner that instantiates the ordinary main scene, selects the baked Pond/Bluegill via debug flags, forces ground collision, and records images/monitors; it does not save a scene.

```bash
source tools/local.env
cp artifacts/pond-lighting-diagnostics/pond_lighting_diagnostic_probe.gd game/dev/pond_lighting_diagnostic_probe.gd
mode=ablation
outdir="$PWD/artifacts/pond-lighting-diagnostics/replay-$mode"
mkdir -p "$outdir"
env XDG_DATA_HOME="$PWD/artifacts/pond-lighting-diagnostics/profile" \
  "${GODOT_CMD[@]}" --path game --rendering-method forward_plus \
  --script res://dev/pond_lighting_diagnostic_probe.gd -- \
  --pond-surface-probe --environment-time-debug \
  "--lighting-mode=$mode" --lighting-hour=12 "--lighting-output=$outdir"
rm game/dev/pond_lighting_diagnostic_probe.gd game/dev/pond_lighting_diagnostic_probe.gd.uid
```

Modes used: `ground`, `stationary`, `stationary_pool_frozen`, `stationary_pool_off`, `camera`, `ground_motion`, `daynight`, `ablation`, and `material`. Run one mode per native process with a fresh output directory. The exact source probe and complete per-mode PNGs, manifests, and stdout logs remain in ignored `artifacts/pond-lighting-diagnostics/`. The [evidence directory](evidence/) contains review-size paired screenshots, contact sheets, animation, path-normalized manifests, and [native log excerpts](evidence/native-log-excerpts.txt). The initial broader baseline and high-camera ablations in ignored artifacts were exploratory; the findings above use the user's actual follow-camera/ground-contact pose.

**Remaining unknowns:** no GPU-only timing; one AMD/Linux Forward+ host; no owner hand-play acceptance of a candidate; no depth-buffer reconstruction of band world speed; no conclusive reproduction of the separately reported high-frequency local shadow flicker. These limits do not change the matched isolation of the broad band. The next action is an owner choice among the repair strategies, followed by a separate authorized implementation and native review.
