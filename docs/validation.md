# Validation and evidence

This document defines scenarios. [The roadmap](roadmap.md) alone owns current Phase/Gate results. Foundation B01–B04 passed on Linux/X11 in the [P0 native playtest](tasks/p0/qualification/native-playtest.md); later gameplay scenarios remain planned. [Repository-bootstrap evidence](tasks/repository-bootstrap/qualification.md) covers the earlier documentation and runner checks only.

## Evidence standard

For each claimed result, record the source revision and relevant dirty-state identity, platform/tool versions, scenario or seed, actual actions, expected and observed outcomes, artifacts, and limitations. A planned check is **NOT OBSERVED**. Use **PASS** and **FAIL** only for executed checks; **BLOCKED** identifies a specific missing dependency.

Headless compilation/smoke, editor inspection, runtime state, screenshots and live input prove different things. A static screenshot alone does not prove that input works; a headless state change does not prove camera readability. Use the smallest combination supporting the Gate. Preserve failures alongside later repairs.

## Foundation scenarios

| ID | Required observation | Gates |
| --- | --- | --- |
| B01 | Inspect actual .NET engine/SDK and coherent pins; import/build the actual Fish main scene; execute C# assertions within a bounded smoke; fail on missing marker, script errors or timeout. | G0.1 |
| B02 | Invoke CLI outside repository CWD; inspect real saved scene; create/change/save/reload/assert/remove a disposable scene; missing scene fails; no lingering fixture/cache is tracked. | G0.1 |
| B03 | Verify selected MCP versions, ignored local config, Fish bridge isolation and actual live peer path; inspect and save/reload a disposable real scene; retain Space Factory settings. | G0.2 |
| B04 | Open editor, play the real 3D scene, show horizontal/vertical input and release, inspect live state, capture running-game pixels, check output and close cleanly. | G0.3 |

P0 can record configuration success before a client reload, but B03/B04 remain unobserved until real tools and the actual Fish editor are connected. A TCP listener or addon file on disk is not a substitute.

## Gameplay scenarios

| ID | Scenario and required result | Gates |
| --- | --- | --- |
| V01 | Complete the accepted core movement course in mouse-only mode and the keyboard fallback. Observe quick acceleration, gradual release slowdown, keyboard backward-input slowing, curved turns/turnarounds, gaps, pitch-based travel to upper/lower landmarks, release, pause/resume, collisions and boundaries. Equivalent simulation conditions remain independent of render/view changes. | G1.1 |
| V02 | Observe the same course and several minutes of free swimming in both desktop modes. Verify accepted third-person framing, smooth follow, obstacle visibility, swimming/gliding body motion, turning lean and a steady camera horizon. Record focus/viewport behavior and the owner's judgment of comfort, natural feel, nimbleness and ease of control. | G1.2 |
| V03 | Consume food/prey, repeat callbacks or competing contacts, reject ineligible prey, and account for one removal/reward per entity. | G4.1 |
| V04 | Cross growth thresholds while near geometry; verify continuous identity, visible/body changes, usable shelter changes and a food-chain reversal. | G4.1 |
| V05 | Observe a predator's detection/attack, a legitimate escape and a lethal encounter; each event's cause agrees with authoritative state. | G4.2 |
| V06 | Save during ordinary play and active danger; close/reopen; compare run ID, resources, consumed prey, position, time and pursuit state. Advance wall-clock time while closed without simulation progress. | G5.1 |
| V07 | Record terminal death, close/reopen and use supported recovery; dead run remains unavailable for Continue. Start new life with fresh identity and preserved history. | G5.2 |
| V08 | Inject interruption at each material death/archive persistence boundary and retry finalization; converge to one terminal outcome and one completed-life record with no resurrection. | G5.2 |
| V09 | Exercise unreadable/unsupported/partial active save and archive separately; report actual storage failure, preserve recoverable data, and do not invent an in-world death. | G5.1, G5.2 |
| V10 | Produce accounts for a brief life, eventful life and incomplete optional metadata; trace every claim to events/final state and keep supported species versus individual identity distinct. | G6.1 |
| V11 | Skip death presentation; open Life History from main menu; read same complete account, return, start another life, restart application and revisit without count/text changes. | G6.1, G6.2 |
| V12 | Use empty and representative large archives; reach every saved story, preserve navigation and prevent silent truncation or fictional filler entries. | G6.2 |
| V13 | Toggle first/third person at different sizes and near cover; state/identity/time remain unchanged by viewpoint and controls/readability remain usable. | G7.1 |
| V14 | Play feeding, escape and growth; suspend/resume across sessions; die; read true recap; revisit archive and begin a new hatchling with prior story intact. | G7.2 |
| V15 | Build/export/run each explicitly claimed desktop target; verify the complete named scope in that target environment and record configuration-only targets separately. | G7.2 |
| V16 | In the real P1 course, observe Quaternius `Fish3` / `ClownFish` as the controlled provisional fish plus Rock Face 01, Coast Sand 01 and Smithsonian *Heliopora coerulea*. Check any additional Quaternius fish placed as non-gameplay population. Verify actual provenance/preparation, materials/deformation, scale, collision agreement, representative performance, public-source boundaries and explicit missing-resource behavior. | G1.3 |
| V17 | Observe fullscreen startup and windowed fullscreen in a native game window. Verify saved choice/temporary override, readable viewport layout and camera, then both input modes across both display options with pause, transitions, Alt-Tab/refocus, capture/rearm and independent menu access. Record effective backend behavior and tested display sizes. | G1.4 |
| V18 | Traverse the P2 normal-play habitat through all **five or more primary depth ledges** from shallow -> deep -> shallow, including the broad route and a rock tunnel/tight passage. Verify irregular natural shelf transitions rather than staircase geometry, at least one alternate ascent/descent transition, collision/camera clearance, no escape below the seabed/intended limits, useful simultaneous foreground/midground/background depth composition, and absence of visible P1 test-course presentation. Preserve the separate P1 course fixture. | G2.1 |
| V19 | Preserve the P2/P1 renderer record and owner-green c2 water/light result, then observe the **tracked Forward+** depth-structured habitat from comparable shallow/middle/deep horizontal views and during a continuous shallow -> deep -> shallow swim. Verify: actual world depth progressively cools/dims lower ledges; warm color, caustics and shafts reduce with depth; useful horizontal visibility shortens as the camera descends; far geometry transitions smoothly detail -> silhouette -> water; multiple ledges visible in one frame do not all receive identical water-column treatment; the bright overhead surface remains legible when looking up; HUD/GFX/menu stay crisp; movement outcomes and saved lighting preset bases remain unchanged. Record representative native performance/errors and active preset. A native Compatibility rerun is unnecessary unless Forward+ is challenged by a concrete blocker. | G2.2 |
| V20 | From rest/glide, ordinary cruise, acceleration/high effort and hard turns, observe the fish-reactive water effect. Disturbance scales coherently, stays near believable body/tail space, does not obscure the camera or burst after pause/focus/reset, and cannot alter authoritative movement/collision. Record representative effect counts/performance. | G2.3 |
| V21 | Traverse the accepted five-depth world after the FAF terrain/geology rebuild. Verify all six terrain roles and four rock variants participate in normal play; the world no longer reads as parallel terraces/test basin; two distinct swim-through routes, a cave/shelter chamber and deep corridor are usable with honest collision/camera clearance; P2 depth routes/water remain intact. | G3.1 |
| V22 | Compare shallow/high, upper, middle, lower and deep reef zones while swimming. Verify all twelve coral, eight plant and eight detail variants participate in coherent clusters; shallow/high areas are visibly the lushest, middle bands balance living reef/geology, and lower/deep bands become progressively sparser; open lanes, cave mouths and landmarks remain readable; modular seams/repetition are acceptably hidden. | G3.2 |
| V23 | From the actual gameplay camera, inspect dense shallow near/wide views, a tunnel/cave view, a mid-depth layered view, a deep sparse view and one swim-through beside the FAF reef target and style-proof 001. Record owner style judgment, optional ambient FAF-FISH-001–004 behavior, and representative dense/cave/deep performance. Confirm P2 movement/depth/water authority is unchanged. | G3.3 |

These IDs are scenario references, not installed test commands. Write executable checks when the actual source exists and a concrete risk merits them. Avoid large suites that repeat the implementation.

**Current P2 result (2026-11-10):** [c2-depth P4 native evidence](tasks/c2-depth/qualification/native-depth.md) remains a historical FAIL. The later [R1 native closeout](tasks/c2-depth/qualification/native-depth.md#r1-addendum--repaired-candidate-native-observation) reaches all repaired authored primary/alternate and tunnel/shelter markers with normal physical-key input, captures comparable native views and warmed performance counters, and records the owner's explicit direct-play acceptance of all requested visual, effect, movement/camera/UI/GFX and smoothness criteria. **V18 and V19 PASS** for that source/display. The owner report is distinct from fixed-image and scripted-route evidence. **V20 remains OPEN** under the [P2 P8 review](tasks/p2/closeout.md); c2-depth does not qualify G2.3 or complete P2.

## Core movement acceptance

**Design and criteria approved — 2026-11-09; execution results remain owned by the roadmap.** The [completed worksheet Q10](planning/p1/swim-controls-worksheet.md#q10--ready-for-gameplay-playtest) accepts these seven criteria for the [core swimming contract](systems/swimming-and-camera.md):

| Criterion | Required owner-play result | Scenario |
| --- | --- | --- |
| 1. Start and slow predictably | Accelerate quickly, approach a chosen landmark, and slow near it with confidence. | V01 |
| 2. Turn through a small course | Circle rocks, reverse direction through a turn, and pass through gaps with the intended slight need to anticipate. | V01 |
| 3. Navigate vertically | Reach landmarks above and below, then return to level swimming without losing your bearings. | V01 |
| 4. Keep the view comfortable | The fish and route remain readable through turns, climbs, dives, and nearby obstacles. | V02 |
| 5. Recover control cleanly | Pause and resume without stuck inputs or unexpected movement. | V01 |
| 6. Use both desktop modes | Complete the same course comfortably with mouse-only controls and the keyboard fallback. | V01, V02 |
| 7. Enjoy swimming itself | Spend several minutes exploring and feel that the fish is natural, nimble, and easy to control. | V02 |

Use one compact course with open space, landmarks at different depths, rocks to circle and a gap/passage. Record its layout, source revision, platform, bindings/tuning, selected mode, actions, outcomes and owner feedback. Each desktop mode needs its own observations; completing the course in one mode does not qualify the other.

Observe slowing on propulsion release in mouse mode and the selected backward-input slowing in keyboard mode. An active mouse brake has no selected binding yet. Intended gradual coasting is not stuck input. Return to level travel through the implemented controls; automatic leveling has not been selected.

V02 observes a camera behind/slightly above at moderate distance, with the whole fish and route ahead visible, smooth direction/depth following, and adjustment near geometry. Body/tail motion eases during gliding and the fish bends/leans into turns. A steady horizon does not require fixed camera pitch.

The owner evaluates several minutes of free swimming in both modes for enjoyment, natural feel, nimbleness and ease. Record this judgment directly; automated movement assertions cannot supply it.

P1 must finish with the dynamics settled. Record the final bindings, motor model and values, pitch/near-rest policy, camera and animation settings, source/configuration identity, selected asset identity and actual owner acceptance. Use the [P1 stack](tasks/p1/README.md) for the tuning loop and evidence records. V16/V17 qualify the selected scene and display behavior; a placeholder-only or unfinished-feel result cannot complete P1.

The implemented core is locked for progression only after required native observations and owner acceptance are recorded for G1.1/G1.2. The owner later selected P2 worldbuilding as the next roadmap Phase; Movement abilities remain approved future work but are not the immediate post-P1 step. Controller support, free look, abilities, growth-dependent handling, feeding and predators remain later scope.

## P2 underwater-world acceptance

P2 qualification is native and integrated. V18 proves the space is a usable natural habitat rather than a hidden set of screenshots; V19 proves **Forward+**, already owner-selected after the P2/P1 real-desktop trial, remains correct on the tracked/full habitat and reads as underwater in motion and at depth; V20 proves water reaction follows the fish without becoming gameplay authority. The renderer-decision record must distinguish P1 automated comparison evidence, the owner's native Forward+ observation, the tracked lock, and final-world performance/limits.

Headless scene checks can prove saved nodes, collision topology, shader/resource validity and bounded emitter configuration. They cannot establish natural composition, comfortable distortion, convincing underwater depth or the visual quality of a wake. Record owner/native judgment for those claims.

## P3 FAF reef-world acceptance

P3 is an art/world-composition qualification, not an isolated asset-import check. V21 proves the finished original terrain/rock kit forms usable geology over the accepted P2 navigation skeleton. V22 proves the completed coral/plant/detail library creates a deliberate depth-based density gradient instead of uniform scatter. V23 proves the running Forward+ world belongs beside the owner target from the gameplay camera, improves materially on style-proof 001, and remains performant enough for progression.

Headless checks can prove asset identity, placement counts, collision routes and deterministic density metrics. They cannot establish that the world looks natural, that caves feel good to swim through, that repetition/seams are visually hidden, or that the target style is achieved. Those require P3 P6 owner/native review.

## Requirement coverage

| Confirmed requirement | Planned evidence |
| --- | --- |
| C01–C04: 3D fish, growth/feeding/predation and camera choice | B04; V01–V05; V13. |
| C05–C07: one life across sessions, permanent death, no offline simulation | V06–V09; V14. |
| C08–C10: brief factual recounting and all stories in main-menu history | V08; V10–V12; V14. |
| C11–C13: reference workflow, Godot/MCP starter and honest setup boundary | Starter content/link/diff and runner checks; B01–B04 after p0. |
| C14–C15: lock core before later abilities; abilities retained but sequencing superseded | V01/V02 and recorded owner acceptance establish the core prerequisite; S10 routes the next Phase to P2 while abilities keep their own future evidence. |
| C16–C17: realistic ocean direction and approved asset shortlist | [Art and assets](art-and-assets.md) selection record and acquisition/import checks; selection alone does not qualify an import or runtime suitability. |
| C18–C26: desktop controls, swimming feel, camera/body presentation, free-look deferral and owner acceptance | V01/V02 in both desktop modes; inspect initial scope and record the owner's actual play judgment. Later controller support needs separate qualification. |
| C27–C29: P1 selected assets, settled dynamics and fullscreen-first delivery | V01/V02 with the final accepted tuning record; V16/V17 on the accepted candidate; all G1.1–G1.4 required for P1 completion. |
| C32–C39: P2 worldbuilding, natural reef, renderer lock, reactive water, visual style, actual water-column extinction and five-level basin | V18–V20 plus the P2 renderer comparison/decision record and c2/c2-depth evidence; G2.1–G2.3 all required for P2 completion. |
| C41–C44: P3 FAF reef rebuild, caves/tunnels, depth-density gradient and complete core environment asset integration | V21–V23 plus P3 implementation/native world-art evidence; G3.1–G3.3 all required for P3 completion. |

IDs refer to [the decision register](decisions-and-questions.md). A coverage mapping is not a passed result.

## Check selection

Use T1 content/link/diff review for documentation, the existing tests for runner changes, and focused C#/scene/rule checks for implementation. Use T2 for shared time, growth/body/camera, and persistence/event boundaries. Use T3 for native controls, threat readability, full life flow and platform claims. Inspect wrapper composition to avoid rebuilding the same unchanged candidate several times.

Automate difficult-to-observe invariants: one reward per consumed entity, no offline advancement, terminal recovery, archive identity, and narrative provenance. Observe actual play for movement feel, camera, fair warning, menu focus, recap pacing and archive readability. Extend validation only for a concrete unresolved risk or required Gate.

## Documentation review

Verify relative links and anchors, current-versus-planned wording, decision provenance, task/model/version grammar and source consistency. Ensure gameplay completion is never inferred from documentation, ported tests, package version or a prior project's evidence. Review the actual diff for unrelated changes and machine-specific secrets/paths before publication.
