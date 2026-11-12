# Pond air-side surface reflection experiment — 2026-11-12

**Disposition: candidate for owner visual review; P12/V32 remains FAIL / NOT GREEN at root `0.4.11`.** This rendering experiment follows the clean recovery checkpoint, rebased on `main` as `736233c9638eff9b2705b49f03983b63b8278a98` (original checkpoint `02a96ddb28473d21a646ae8fb723f17a81d06ac7`). The checkpoint contains the preceding C4 P1 spatial-clarity work. The surface candidate is committed for review without qualifying a Gate.

## Diagnosis and approach

The checkpoint surface already had procedural displacement, refraction, depth-based Pond extinction/scatter, Fresnel, and an irregular shoreline mask. Its air-facing branch largely colored the transmitted opaque scene and added a small water tint. It had no reflected bank or sky sample, so the overcast Pond read as an almost uniform olive sheet even at grazing angles. The underwater branch is separate.

The candidate retains that transmission path and composes it with a new reflected contribution. A bounded 12-step screen-space ray uses the existing Forward+ opaque color/depth buffers to find above-water bank pixels. A procedural overcast sky supplies reflection where the ray misses or exits the screen. A water Fresnel term increases reflection at grazing angles; layered broad, medium and fine directional slopes distort the reflection. The reflected light is subdued so bank silhouettes remain dark. Existing extinction still removes fish and bottom detail with water path length. Existing air fog and shoreline fade remain in place. The profile exposes four Pond controls: reflection strength, distortion, ripple strength and fine-normal strength. Ocean reflection strength is zero; its front face remains discarded. No additional camera, render target, mesh density or gameplay state was added.

## Matched native views

Both sets used the same `pond-p4-71` bake, Bluegill, camera coordinates, 1920×1200 native Godot `4.7.2` .NET Forward+ on AMD Radeon 880M, and Pond Overcast. The **before** set temporarily loaded the exact checkpoint shader, then restored the candidate shader. The capture probe is `game/dev/pond_reflection_probe.gd`; it allows the camera immersion blend to settle after each teleport. Earlier captures made before that correction are excluded from this comparison.

| View | Checkpoint | Candidate | Observation |
| --- | --- | --- | --- |
| Near fish, 0.5 m below surface | [before](surface-reflection-evidence/before-near_fish_050cm.png) | [after](surface-reflection-evidence/after-near_fish_050cm.png) | Fish outline and color remain readable at a short water path; broken, dark and bright sky bands replace the flat sheet. |
| Near fish, 1 m and 3 m below | [before 1 m](surface-reflection-evidence/before-near_fish_100cm.png), [before 3 m](surface-reflection-evidence/before-near_fish_300cm.png) | [after 1 m](surface-reflection-evidence/after-near_fish_100cm.png), [after 3 m](surface-reflection-evidence/after-near_fish_300cm.png) | Fish becomes less legible with depth; 3 m is strongly absorbed. |
| Low grazing and distance | [before](surface-reflection-evidence/before-grazing_distance.png) | [after](surface-reflection-evidence/after-grazing_distance.png) | Reflection dominates the long view, with dark water under softened overcast sky streaks. |
| Vegetated shore | [before](surface-reflection-evidence/before-shoreline.png) | [after](surface-reflection-evidence/after-shoreline.png) | Bank stumps, reeds and mud form distorted silhouettes; irregular shore edge and shallow lily pad remain intact. |
| Shallow mud | [before](surface-reflection-evidence/before-shallow_mud.png) | [after](surface-reflection-evidence/after-shallow_mud.png) | Nearby bottom remains visible through the water; farther path turns murky. |
| Underwater looking up | [before](surface-reflection-evidence/before-underwater_up.png) | [after](surface-reflection-evidence/after-underwater_up.png) | No material visual change was observed in this fixed underside view. |

The [later shoreline frame](surface-reflection-evidence/after-shoreline-motion.png) shows changing breakup after motion resumes. The material clock was unchanged during pause (`0.000`) and advanced after resume (`1.117`). Native logs: [before](surface-reflection-evidence/before.txt), [after](surface-reflection-evidence/after.txt). Neither emitted a shader/compiler error, black/magenta sample, or visible disappearance in these fixed views. These stills and one later frame do not establish a full moving swim-route result or exclude transient flashes in unobserved camera paths.

## Cost and regression boundary

With VSync on, both matched runs had 16.6–16.7 ms fixed-view median frame intervals, so the display cap hides the shader cost. The separate 36-frame-per-view uncapped comparison used the same candidate shader with reflection strength on/off. Representative medians: near fish 0.5 m **5.03 vs 4.75 ms**, grazing **4.25 vs 3.80 ms**, shoreline **3.64 vs 3.13 ms**, shallow mud **4.29 vs 3.89 ms**; draw counts were unchanged. Across these fixed views the measured median increment was about **0.28–0.51 ms**. See [reflection on](surface-reflection-evidence/uncapped-reflection-on.txt) and [off](surface-reflection-evidence/uncapped-reflection-off.txt). These are short whole-frame intervals on one desktop, not isolated GPU shader timing or moving-route pacing; p95 varied substantially.

Focused checks passed after the change: `tools/dev import`, `tools/dev build` (0 errors; two NU1900 metadata-fetch warnings), `tools/dev inspect-pond-surface` (including ascents/waterline), `tools/dev inspect-pond-atmosphere` (Pond→Ocean→Pond material/profile disposal, reflection disabled in Ocean), and `tools/dev inspect-pond-camera-immersion` (crossing, pause, re-entry and unchanged fish state). The native before/after logs confirm Forward+ and no shader compile errors. No full native Ocean visual route was recorded for this experiment.

## Limits and next review

Screen-space bank reflection only sees opaque scenery within the current camera image. Off-screen objects, transparent foliage and distant shore detail cannot reflect faithfully; the sky fallback covers misses and can look generic. The present Pond is sparsely dressed under a flat overcast sky, so this does not match the dense canopy or dramatic highlights in the supplied river reference. The fixed shore view has some thin dark edge noise and a near-shore lily pad that is brighter than the surrounding water. The surface still needs owner judgment while swimming, turning and crossing it in normal play, including GFX presets and sustained frame pacing. Preserve P12/V32 as open and keep root version `0.4.11` until that review decides the next step.
