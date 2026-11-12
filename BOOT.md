# Fish Boot Guide

Read [AGENTS.md](AGENTS.md) first. This is a concise handoff; [the roadmap](docs/roadmap.md) owns Phase/Gate progress and [development setup](docs/development.md) owns command availability and tool pins.

## Current handoff — 2026-11-11

| Area | Actual state |
| --- | --- |
| Product | Owner-approved **naturalistic stealth-survival** across Ocean and Freshwater Pond (Pond planned): exposure, real hiding and feeding risk are central; one living fish persists across sessions, death is permanent, and each life produces a factual history. Habitat-native species remain separate. |
| Foundation | P0 is complete on the observed Linux/X11 desktop; the Godot .NET project, developer CLI, and isolated Godot Editor MCP path are implemented and qualified for the foundation scope. |
| Current gameplay | **P1 COMPLETE.** The owner accepted the current post-c1 controls, swimming, third-person camera/body and starter course as sufficient for progression. [P1 closeout](docs/tasks/p1/closeout.md) maps G1.1–G1.4 and evidence limits; [accepted tuning](docs/tasks/p1/qualification/accepted-tuning.md) is P2's exact baseline. |
| Controls correction | [c1-bindings](docs/tasks/c1-bindings/README.md) is implemented and reconciled into P1. Its [closeout](docs/tasks/c1-bindings/closeout.md) retains the partial advanced native matrix and the repaired help-layout failure. Further refinement is deferred. |
| Adaptive Follow | A fifth built-in control preset combines mouse fish steering and W/S depth control with independent camera look while Left Shift is held, then returns smoothly. It is implemented with focused headless checks; native feel remains for owner observation. See [swimming and camera](docs/systems/swimming-and-camera.md). |
| Next Phase | **P4 P12 remains NOT GREEN at `0.4.11`.** P12-R2 adds dark muddy-green depth transmission after the owner rejected pale/shiny trials; fixed native views show brown mud at a real shallow spot, fish fading with depth, and brighter light extending sight. Owner judgment of this final candidate, controlled surface swimming, camera/preset comfort and continuous deep/return routes remain open. [P12 evidence and R1/R2 addenda](docs/tasks/p4/qualification/native-surface.md) own V32; no P4 Gate passed. C3 V25 and P2 G2.3/V20 remain open. |
| Renderer | **Forward+ is the owner-selected and tracked P2 desktop renderer.** Normal main play uses `forward_plus`; [P2 decision evidence](docs/tasks/p2/qualification/renderer-decision.md) records the native lock. Compatibility remains a diagnostic compare path only. |
| Assets | Quaternius Fish3/ClownFish, Rock Face 01, Coast Sand 01, and Smithsonian *Heliopora coerulea* are the P1 starter set. P2 may prepare efficient derivatives/instances and add only provenance-safe free environment assets when the approved habitat cannot be reached with the existing set. |
| Root version | **`0.4.11`** — P12 did not pass or receive owner approval; do not assign `0.4.12` or advance the runner. The scoped Character Select guard is in the prior commit; R1 and R2 are non-checkpoint repairs. |
| Next design phase | **P4 P12 OWNER NATIVE REVIEW PENDING:** [P12 evidence](docs/tasks/p4/qualification/native-surface.md) preserves the original failure, R1/R2 diagnostics and unsampled V32 cells. [Freshwater Pond + World Select](docs/planning/p4/freshwater-world.md) and the [P4 stack](docs/tasks/p4/README.md) retain the acceptance scope. |
| Later gameplay | P5 introduces naturalistic sensing, hiding, predator investigation/pursuit, stealth hunting, feeding and growth; persistence/death P6; Life History P7; integrated qualification P8. No P5 stealth gameplay is yet implemented or qualified. |

## Product contract to preserve

**Every movement is a decision about exposure. Every hiding place is an opportunity to survive. Every feeding opportunity is a reason to take a risk.** Stealth survival with natural mechanics is the primary gameplay identity; [stealth and detection](docs/systems/stealth-and-detection.md) owns the future rules. One fish's life is one run. The fish grows by feeding while avoiding predators, with third person as the primary perspective and first person as an optional view. Closing the game suspends the living run; death ends it permanently. Each dead life produces a short factual recounting retained under **Life History** in the main menu, independent of later runs.

The [decision register](docs/decisions-and-questions.md) separates confirmed Ocean/Pond habitat selection from unresolved native species rosters, feeding balance, long-life pacing and later presentation choices. P2 and P3 establish the Ocean prototype and shared deterministic world compiler; P4 adds Pond without erasing that work.

The isolated [Pond FAF wood and cover pack](docs/fish-asset-factory/pond-wood-pack-001/README.md) has twelve editable variants, Pond-tagged GLBs, simple collision wrappers and a native two-direction hero-log traversal proof. It is an asset candidate only; it is not installed in normal play or the P4 generated Pond.

The isolated [Pond FAF ground-dressing pack](docs/fish-asset-factory/pond-ground-pack-001/README.md) has eight editable variants, four shared materials and a sparse/dense Forward+ placement proof. The generated basin remains the sole floor in that fixture; normal Pond and Ocean play are unchanged.

## P1 completion boundary

P1 is complete under the owner's 2026-11-09 decision: the current controls are good enough to progress, and further control/movement refinement waits for later world/gameplay context. The authored [P10 iterative tuning](docs/tasks/p1/P10-owner-tuning-and-dynamics-lock.txt) was superseded at closeout; [P11 evidence review](docs/tasks/p1/closeout.md) was completed directly. The evidence record distinguishes actual native observations, saved-scene checks and unobserved advanced matrix cells. P0 and P1 are COMPLETE; c1 is reconciled. No old prompt execution or intermediate versions were invented.

Normal play now instances `p2_habitat.tscn`; the original P1 main is preserved as `p1_course_fixture.tscn` for movement/camera/animation checks and renderer comparison. The open seabed is a deterministic generated basin with one visual mesh and matching collision; the former rectangular sides sit outside its banks as fail-safe collision. Five generated geological regions add actual cave/tunnel/arch/canyon floor, walls and ceiling where enclosed. FAF terrain, rocks, coral, plants and details are placed from generated habitat/route fields; the terrain accents drape onto the generated surface with matching collision. The accepted Forward+ depth presentation reads the generated deep core. Historical [P2 implementation](docs/tasks/p2/qualification/implementation.md) and [native evidence](docs/tasks/p2/qualification/native-world.md) describe the earlier accepted authored candidate, not new native qualification of this basin.

## P2 entry and boundary

At the P2 handoff:

1. Inspect branch/HEAD, local changes, [final P1 tuning](docs/tasks/p1/qualification/accepted-tuning.md), current renderer and actual native evidence. Preserve compatible work.
2. Preserve P1's compare tooling/evidence and the completed [P2 native Forward+ lock](docs/tasks/p2/qualification/renderer-decision.md). The owner selected Forward+ after a smooth real-desktop trial. The [water/light correction](docs/tasks/c2-watet-light-fx/closeout.md) is owner-green at pushed source `d292aebd`; preserve its surface, organic caustics, shafts, GFX presets and recorded performance evidence. [c2-depth P1–P3 evidence](docs/tasks/c2-depth/qualification/implementation.md) establishes the structural candidate; [P4 native evidence](docs/tasks/c2-depth/qualification/native-depth.md) remains the failed historical observation. The [R1 repair](docs/tasks/c2-depth/qualification/r1-repair.md) and native closeout now qualify the authored route and owner-accepted depth presentation. Owner decision S16 closes P2 for progression with G2.3/V20 preserved as an accepted gap. Do not fabricate a PASS; revisit V20 only as deferred refinement.
3. Keep rendering/world presentation read-only with respect to authoritative fish movement. A particle wake, post effect, fog change or camera effect must not change the motor, fish identity or simulation time.
4. Do not implement feeding, predators, growth, saves, death/history, controller, first person, or Movement abilities as hidden P2 scope.

Keep detailed execution results in task evidence. Update this guide only when actual implementation/evidence or the selected next action changes.
