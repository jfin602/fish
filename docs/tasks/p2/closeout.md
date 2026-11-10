# P2 / P8 — Underwater world closeout review

**Disposition: BLOCKED; Phase 2 is NOT GREEN.** This P8 review cannot close P2 because the P7 candidate has not received direct owner judgment and several native observation cells remain open. V18–V20 and G2.1–G2.3 are therefore not qualified. Keep the last valid implementation version, `0.2.6`; the assigned `0.2.7` P7 and `0.2.8` P8 versions are not evidence or applied checkpoints. [Roadmap](../../roadmap.md) remains the Phase/Gate authority.

## Review boundary and evidence applicability

Reviewed `main` at `ed38cb8b24558a78b6108bcc35aa97c067f9c5cd` with the uncommitted P7 seabed blend, spherical bubble repair, focused-check updates, and P7 evidence/documentation listed by `git status`. This is the same source candidate described in [native-world](qualification/native-world.md) and [candidate environment identity](qualification/accepted-environment.md). The current hashes below match that record. No gameplay or presentation source changed after the P7 captures; the documentation edits in this review do not invalidate them.

| Identity | Current SHA-256 / setting |
| --- | --- |
| Normal `main.tscn`; P2 habitat; reef composition | `019681b0cb0509db4504b42c5ee4fdce078d73a51de961e6b0ba39610d1a1093`; `ad2e2c9be6a7441ece279483c60ecbfe0e11a7e37f4bf54d9becffe5ee350f79`; `9e5415cef2ec8c5ca2f8e636952c609280ff99119bf941bf1a33326a3b454eb4` |
| Tracked project | `0631a97120ec12db639784733e0b1020d298708a3db4c394addfefb8f8dd6957`; `run/main_scene="res://scenes/main.tscn"`, `renderer/rendering_method="forward_plus"`, `Forward Plus` feature. P7 native stdout also reported Vulkan `method=forward_plus` on the AMD Radeon 880M. |
| Habitat palette, surface, blend shader | `HabitatPalette.cs` `60f91ee887f6ead2c5c96673579171357ede60dd7906dea0f290963f49a0c8a3`; `SeabedSurface.cs` `87924135ddf1bc835efafb8f8879cee22389a5ebc6aaa8049706c475b6b7122d`; `seabed_blend.gdshader` `1aed79d2ba353a74ec8eccddf1591a58a5388502be0abac87184469a68559b52` |
| Reef setup; atmosphere; screen shader | `ReefComposition.cs` `f5c93fcb87fc55f951f30b5faef377b15104e4f447cc000e096bf7b144f677f1`; `UnderwaterAtmosphere.cs` `89c50049528e24a3f3a35e408770d0b7e2865adf3f8cd50255a483a00c8f643b`; `underwater_screen.gdshader` `5e47def2c631c0fbdbee8fbe2bacccda732f3a315c1d74d8d0772529dfcf7bae` |
| Water reaction | `WaterReaction.cs` `ce34142b6b9e40d21f0bf6ec73f72f4898c03736b3ff579414c84a53b1930c58`; 16 + 16 ribbon and 12 spherical bubble slots, capped at 44 live slots. |
| Accepted P1 tuning and fixture | `swim_tuning.tres` `6d97d70d7b280d2491d08378d1c3e54cdeca1ca14aa245055bec967943e8e2d7`; `p1_course_fixture.tscn` `8461741933c12ced142ac26fc43cff6a391df7328d8c6e4ccd9872cc4434f403`, matching the [P1 accepted baseline](../p1/qualification/accepted-tuning.md). Fish controller, motor and camera source remain at the P1 accepted hashes. |
| Prepared P2 asset mapping | Ignored [inventory](../../../artifacts/assets/source/p2-environment/inventory.json) `391099f0febe9ce04daa9be8d7614ae32b90f89d6c25f12f57aeda9cdee681d0`; 10 entries and all 34 mapped files exist under `game/assets/vendor-local/p2-environment/`. The P1 Coast Sand 01, Rock Face 01 and Heliopora sources are recorded in the [starter manifest](../../assets/p1-starter-manifest.md); the additional rocks, terrain sets and corals are mapped in the [P2 manifest](../../assets/p2-environment-manifest.md). No vendor payload is tracked. |

[P1 closeout](../p1/closeout.md) records G1.1–G1.4 PASS. [P2 implementation evidence](qualification/implementation.md) records executed P1–P6 checks and repairs, while the [renderer decision](qualification/renderer-decision.md) records the owner's desktop Forward+ trial and selection. P7's actual Fish game window used the tracked Forward+ renderer, a temporary ignored input/capture probe, and native route/effect logs under `artifacts/p7-native/`. Those observations are reusable for this unchanged candidate. They establish marker reachability, rendering and counters, not an owner's hand-controlled comfort or visual approval. The failed square seabed transitions, square bubble sprites and unsaved dense-fog trial remain documented in P7 evidence.

## G2.1 — Natural habitat and traversal / V18

| Criterion | Result | Evidence and remaining boundary |
| --- | --- | --- |
| Normal play uses P2 habitat; P1 course is separate | PASS | `main.tscn` instances `p2_habitat.tscn`; `p1_course_fixture.tscn` retains the accepted course. P7 native frames show no P1 target spheres or camera-check post. |
| Continuous varied seabed, reef/coral areas, broad route, tunnel, cover and vertical route | PASS for structure and marker traversal | [P3/P4 checks](qualification/implementation.md) and P7 real-window open, tunnel, shelter and lower-to-upper marker runs passed. The blended surface replaced a visible square material seam without changing collision. Hand-controlled route comfort remains unobserved. |
| Complete prepared palette visibly used | NOT OBSERVED | Resource and placement checks find Coast Sand 01, Ground060, Gravel041, Rock063; Rock Face 01/02, Coastal Cliff 01, Boulder 01; and Heliopora coerulea, Acropora cervicornis, Acropora palmata, Meandrina meandrites, Montipora danae. Ten rocks and eight coral anchors instantiate all nine model IDs; all four terrain map sets are bound in the blended shader. P7 route frames show mixed regions but do not prove each model is visually identifiable and each terrain set visibly reads in play. Owner review is required; no prepared asset may be silently omitted. |
| No underside/edge escape; collision and camera understandable | NOT OBSERVED for full native use | `HABITAT_CHECK_OK` checks underside, rim and upper containment; `REEF_CHECK_OK` checks accepted fish collider and camera probes through the tunnel. The outer edge and underside were not swum normally in P7. Tunnel/vertical camera comfort also awaits owner judgment. |
| Natural, coherent world without a visible test-room edge | BLOCKED | The far and upper P7 views expose a prominent rock rim. Only a direct normal-play review can decide whether this reads as a cove or a test-room boundary. |

**G2.1: BLOCKED. V18: BLOCKED.** Structural and scripted native route evidence is valid, but required visible palette use, naturalness, perimeter and hand-controlled clearance are unqualified.

## G2.2 — Forward+ underwater atmosphere / V19

| Criterion | Result | Evidence and remaining boundary |
| --- | --- | --- |
| Owner-selected, tracked Forward+ renderer | PASS | The [P1 comparison and P2 lock](qualification/renderer-decision.md) include the owner's real desktop trial and explicit selection. `project.godot` tracks `forward_plus`; final P7 logs report AMD Vulkan Forward+ without engine/resource errors. No concrete blocker has challenged the choice, so a Compatibility rerun is unnecessary. |
| Depth/distance, color/light, caustic motion, motes and screen treatment | PASS for installed/rendered components; NOT OBSERVED for quality | `UnderwaterAtmosphere.cs` uses distance/height and Forward+ volumetric fog, a top light, moving caustic projection, 128 suspended motes and a restrained screen shader below UI. P7 native near/far and upper/lower captures show the stack. The owner has not judged its underwater readability or comfort during movement. |
| Crisp UI and stable final habitat | PASS for captured integration; NOT OBSERVED for owner comfort | P7 pause/fullscreen captures show readable text; native mode 3 → 4 → 3 worked and logs had no engine errors. Actual focus away/back and subjective readability under play remain open. |
| Movement unaffected | PASS | `ATMOSPHERE_MOVEMENT_OK` compared equal-intent movement with presentation on/off; fish position, velocity and steps matched. |
| Representative native performance | PASS for measurement; NOT OBSERVED for acceptability | P7 route medians were 54 FPS open, 53 tunnel, 49 shelter and 41 upper on the recorded Radeon 880M/1920 × 1200 system. These short samples exclude startup and do not define a production budget; owner acceptability is unrecorded. |

**G2.2: BLOCKED. V19: BLOCKED.** The renderer lock is sound, but final underwater appearance, motion comfort and owner performance judgment are missing.

## G2.3 — Fish-reactive water / V20

| Criterion | Result | Evidence and remaining boundary |
| --- | --- | --- |
| Distinct rest/glide/cruise/acceleration/hard-turn response | PASS for numeric mapping; NOT OBSERVED for visual benefit | P7 native effect log records near-zero rest, reduced glide, steady cruise, stronger acceleration and left/right asymmetric turn ratios. Captures show round bubbles after repair. The owner has not judged whether the states are visibly distinct and improve swimming. |
| Body/tail origin, spatial coherence, camera clearance | NOT OBSERVED | Emitters are anchored at bounded fish-space tail offsets and inherit pitch/yaw; P6 checks cover configuration. P7 did not observe a combined pitch-and-turn effect beside close geometry or judge camera clutter under hand control. |
| Bounded count and pause/focus/reset cleanup | PASS for recorded scripted checks; NOT OBSERVED for actual desktop focus | Three GPU emitters cap at 44 live slots. P7 pause/resume, synthetic focus notification and restart cleared ratios/emitters without backlog. A real window-manager focus loss/refocus was not completed. |
| No movement/collision authority | PASS | `WATER_REACTION_OK` compared 45 equivalent-intent ticks with effects on/off and found equal fish position, velocity, basis, input count, motor steps, tuning and collision. |
| Representative performance | PASS for shared-scene measurement; NOT OBSERVED for acceptability | P7 route counters include the effect in the full habitat; no separate production effect budget or owner judgment exists. |

**G2.3: BLOCKED. V20: BLOCKED.** Numeric response and authority isolation pass; native visual quality, spatial coherence and real focus handling remain unqualified.

## Handoff and version

The owner needs to swim this exact saved candidate in the normal game window and record habitat naturalness, underwater readability, tunnel/vertical comfort and whether the wake helps. During that review, check the exposed far/upper rim, visually identify every prepared model and terrain set, attempt the intended outer-edge/underside route, and exercise actual focus loss/refocus. Capture only the missing combined pitch-and-turn reaction near the tunnel wall and any affected criterion after a repair. Record the platform, scene/source identity, observations and any dissent in `qualification/native-world.md`; update `qualification/accepted-environment.md` only when accepted. Then apply the P7 checkpoint/version under the runner's policy and re-evaluate this P8 table. Only after every G2 Gate passes may the roadmap mark P2 COMPLETE, set the P8 `0.2.8` closeout version and route execution to P3. Movement abilities remain approved future work without a P2 schedule.

## Later c2-depth R1 evidence — 2026-11-10

The P8 table above is the historical review of its earlier source candidate. The subsequent [c2-depth R1 native closeout](../c2-depth/qualification/native-depth.md#r1-addendum--repaired-candidate-native-observation) tested `main` HEAD `1ca8c4f123646dbe472fd18198f9a13c5d7b0019` plus the uncommitted R1 repair at root `0.2.6`. Real Forward+ normal input reached all repaired authored depth markers in both directions, alternate lane, tunnel and shelter. The owner separately confirmed direct hand-play and accepted natural terrain/perimeter, depth visibility, deep upward surface, c2 effects, movement/camera/UI/GFX and smoothness. Five focused checks passed with an isolated default input profile. The original P4 failure and the P8 baseline above remain historical evidence.

For this later candidate, **G2.1/V18 and G2.2/V19 PASS** as recorded in the [roadmap](../../roadmap.md). **G2.3/V20 remains OPEN:** the owner has not yet qualified fish-reactive wake/bubble state changes, combined pitch/turn near close geometry, and actual desktop focus/refocus as a separate water-reaction review. P2 remains IN PROGRESS / NOT GREEN; no version bump or P3 entry follows from c2-depth. The next bounded work is only that G2.3 native qualification and subsequent P2 Gate reconciliation.


## Owner progression override — 2026-11-10

**Disposition for progression: OWNER CLOSED / NOT GREEN.** After the runner refused P3 because root was still `0.2.6`, the owner explicitly authorized the deliberate `0.3.0` phase-entry transition.

This override does **not** change the evidence table above:

- G2.1 / V18 remains PASS on the repaired depth candidate.
- G2.2 / V19 remains PASS on the repaired depth candidate.
- G2.3 / V20 remains OPEN / NOT GREEN: direct owner wake/bubble quality judgment, combined pitch/turn near geometry, and actual desktop focus/refocus were not fully qualified.

The owner accepts that G2.3 evidence gap for progression. P2 is therefore closed **for roadmap execution only**, not reclassified as fully GREEN. Future work may revisit V20 without invalidating the P3 transition. Root `0.3.0` is authorized as the P3 baseline.
