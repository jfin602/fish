# C3/P4 native visibility qualification

**Disposition, 2026-11-10: FAIL / NOT QUALIFIED for V25.** This is a partial real-window review of the unchanged `0.3.6` candidate, not owner acceptance of discovery fog. The owner reported that collision boxes do not line up with rocks, ground pieces, and walls in several areas. Normal input reproduced a movement stop beneath visibly open water. The required two unobstructed boundary views, full route, landmark approach/retreat, live GFX/preset round trip, and explicit owner fog judgment were not completed.

## Source and native setup

| Item | Observed value |
| --- | --- |
| Repository | `/home/jfin/dev/fish`, branch `main`, clean starting HEAD `1f406a387ae2a2f7f5c33d9b2ea2a77dc8ec1bdd`, ahead of origin by 10; this P4 record is uncommitted. |
| Root version | `0.3.6` before and after native work. |
| Project/engine | Fish `game/project.godot`, Godot `4.7.2.stable.mono`, .NET SDK `10.0.403`; Fish Editor MCP bridge identified `/home/jfin/dev/fish/game/`. |
| Display | Linux/X11 `:0`, 1920 × 1200 at 60 Hz; tracked and active Forward+ on AMD Radeon 880M (RADV GFX1150). Normal play and both probes ran `forward_plus`. |
| Active GFX | The isolated game panel showed **Natural**, Visibility Distance **35 m**, Volumetric haze **0.024** in [GFX capture](evidence/native/gfx-natural.png). The first Character Select/play session used the owner's existing profile; its preset was not independently captured. Probe logs record base visibility 35 m, volumetric 0.037 and height 0.018; they do not identify a named preset. Do not infer equal named presets from equal bases. |
| Source identity | `UnderwaterAtmosphere.cs` SHA-256 `3555137a3682718a689330c845d6112fa0bc4565ef3d8487d3408b3c6cf8034e`; `FafTerrainWorld.cs` `8de1e587b5adb562ab0a3d8ccb6f8250abe052a20583c7f6200d0bdba8af4fad`; `FafGeologyWorld.cs` `d4a2e9370829f1d5e61871853955cbfd2c90de26bba005efe6bbe25d7d225d75`; `FishController.cs` `06fb77c2842fff969bf2fc912ee7166ec9c13a6487989c422537d93a5b8c531c`. Scene hashes: `p2_habitat.tscn` `ad2e2c9be6a7441ece279483c60ecbfe0e11a7e37f4bf54d9becffe5ee350f79`; `p2_reef_composition.tscn` `7a41ef5327143d65886ef0a41bf2be375fa5af32fc17f55398bada140eaa2fc7`. |

The first real game opened Character Select with five choices. Yellow Tang was selected and entered normal play; [spawn capture](evidence/native/spawn-default.png) shows the running follow camera. The isolated second run used the normal Fish main scene, Keyboard Only profile and physical W/D/Up key input through the native display. At approximately world `(0, 0, -10.47)`, continued forward input stopped moving the fish although the view appeared to offer open water beneath a shelf: [blocked forward lane](evidence/native/blocked-forward-lane.png). Rotating about 177 degrees gave the [southward view](evidence/native/south-across-water.png), with a continuous distant terrain rise still recognizable; the exact opposite playable boundary cannot be identified confidently from that image. A westward [view](evidence/native/west-obstructed.png) was blocked by nearby geology and is invalid as the second open-water boundary check. The [upward view](evidence/native/upward-surface.png) shows the overhead turquoise surface and a shaft cue, but does not prove the full lighting/effect stack in motion.

This obstacle result is consistent with source geometry rather than a fog artifact: `FafTerrainWorld` drapes 51 visual chunks over the older collidable seabed and adds no matching terrain colliders; `FafGeologyWorld` places 38 visible pieces but adds only three coarse boxes. `p2_reef_composition.tscn` retains P2 rock collision shapes for rock visuals that P3 hides. `FishController` also clamps X/Z to ±24.9 m, separately from the hidden limit shapes at ±37.6 m in `p2_habitat.tscn`. These are source facts and a single reproduced stop, not a complete collider audit. The owner reported mismatches at multiple rocks, ground pieces and walls. [P3/P7 native review](../../p3/qualification/native-world-art.md) already requires rebuilt geology and continuous collision/camera traversal before P3 acceptance. Repair belongs with that world reconstruction, followed by another C3 clearance review; no collision or fog source was changed in this P4 closeout.

## V25 and visibility contract matrix

| Criterion | Status | Native observation and limit |
| --- | --- | --- |
| Opposite boundary concealed by fog in two reachable, unobstructed directions | **NOT OBSERVED** | One across-water turn was captured, but the exact boundary identity is uncertain; the other direction is hidden by a nearby shelf. No two valid views establish fog as the cause of concealment. The southward image leaves a visually recognizable distant rise, so the desired appearance is not demonstrated. |
| Distant reef/cave landmark emerges and recedes continuously, without hard pop or flat fog wall | **NOT OBSERVED** | No full approach, turn and retreat at a stable landmark was completed. Fixed captures and headless extinction values cannot substitute. |
| Shallow → middle → deep → shallow on reachable routes; matched horizontal comparisons; cooler/dimmer and shorter range deeper | **BLOCKED** | The native route stopped at a visual/collision mismatch. [P1–P3 depth checks](implementation.md) establish bounded numerical depth response, and the probe samples shallow/deep fixed cameras, but neither is a continuous playable route or matched native screenshot sequence. |
| Near fish, coral, terrain, cave mouth and obstacle/camera clearance | **FAIL** | Player fish and some terrain remained visible in the captures. The fish stopped against geometry that did not visually explain the obstruction; the owner independently reported rock/ground/wall mismatch in several locations. Cave and tunnel passages remain **NOT OBSERVED**. |
| Overhead surface, sun, shafts, organic caustics, motes, water reaction, clean GFX/UI | **NOT OBSERVED** | Upward capture shows surface and a shaft cue; paused GFX capture shows readable controls. Caustic motion, motes, fish-reactive water, sustained sun/shaft quality and full UI lifecycle were not judged together in native motion. Existing c2/P2 acceptance remains separate historical evidence. |
| Visibility Distance live changes and Natural/Soft/custom switches | **NOT OBSERVED** | Natural at 35 m was visible in paused GFX. Input attempts did not produce a controlled, saved switch or repeatable live before/after view. Earlier focused GFX tests are in [implementation evidence](implementation.md). |
| Old valid custom preset load/save/reload and deep/shallow base preservation in isolated user storage | **NOT OBSERVED** | A read-only copy of the owner's valid custom data was placed under ignored isolated XDG user storage, transformed to schema v1, and a native game was launched. The isolated file remained schema v1 after the attempted UI session; no confirmed Save/reload occurred. The owner's live `lighting-presets.json` was not changed. P2's isolated fixture proved schema v1→v2 migration/round trip in code, but P4 native proof is absent. |
| Five fish choices and Return to Character Select in this P3 build | **NOT OBSERVED for full matrix** | Five entries were visible and Yellow Tang entered play. This P4 run did not exercise all five or return routing; [P3 native selection evidence](../../p3/qualification/character-select.md) is separate. |
| Same-scene Forward+ fog bypass/new cost, draw and primitive counts, errors | **PASS for measured probe** | Both modes kept the entire atmosphere enabled and changed only depth-fog extinction. Fixed camera/depth/scene/device/resolution, 120 warm frames and 180 sampled frames per mode. See table/logs below. No engine errors in the successful logs. This does not prove playable feel or owner performance acceptance. |
| Owner visual/feel judgment and C3 acceptance | **BLOCKED** | The owner's direct native feedback is negative for collision agreement. No explicit owner acceptance of fog/discovery, GFX feel or overall C3 result was given. Reviewer images do not stand in for that decision. |

## Matched visibility cost

The [capped run](evidence/native/visibility-ab-capped.log) used `./tools/dev measure-visibility` on the real X11/Forward+ display. The [uncapped run](evidence/native/visibility-ab-uncapped.log) used the same source probe and pinned .NET engine with `--disable-vsync`. Values are median frame milliseconds, draws and primitives; percentages compare **new** to **bypass** within each view/run. Capped results are constrained by the 60 Hz display. Draws and primitives were identical in each pair; fog did not cull world geometry.

| Run/view | Bypass ms | New ms | Delta | Draws both | Primitives both |
| --- | ---: | ---: | ---: | ---: | ---: |
| Capped shallow | 16.66 | 16.67 | +0.1% | 224 | 98,586 |
| Capped deep | 16.66 | 16.65 | -0.1% | 46 | 39,830 |
| Uncapped shallow | 5.07 | 4.66 | -8.1% | 224 | 98,586 |
| Uncapped deep | 6.33 | 6.35 | +0.3% | 46 | 39,830 |

No repeatable ≥10% regression was demonstrated in these samples. The uncapped run is one pass, so this is a bounded diagnostic, not a production benchmark or owner cost acceptance. The probe reported shallow/deep effective distances 33.2/20.2 m at base 35 m. No comparable bypass/new screenshot pair was captured; image artifact comparison remains **NOT OBSERVED**. The logs end `VISIBILITY_AB_OK` and have no successful-run `ERROR:` lines.

## Checks and next qualification boundary

`./tools/dev doctor` identified the actual Fish project and pinned environment. `./tools/dev build` passed with zero warnings/errors. No gameplay or rendering source was changed during P4, so the [P1–P3 focused checks](implementation.md) remain the source regression evidence. The native captures were made in a real playable Godot window; the fixed-camera probe is identified separately wherever used.

P3 world-art repair needs visible geometry and collision to agree along normal routes, cave/tunnel entrances and walls. Repeat the full hand-controlled route and two open-water boundary directions after that repair, then capture a landmark approach/retreat, matched depth views, controlled live GFX changes, isolated native preset save/reload, and an explicit owner fog/feel decision. V25 cannot be GREEN on the present record.
