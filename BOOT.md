# Fish Boot Guide

Read [AGENTS.md](AGENTS.md) first. This is a concise handoff; [the roadmap](docs/roadmap.md) owns Phase/Gate progress and [development setup](docs/development.md) owns command availability and tool pins.

## Current handoff — 2026-11-10

| Area | Actual state |
| --- | --- |
| Product | Owner-approved 3D fish survival: one living fish across sessions, permanent death, a brief factual life recounting, and a main-menu Life History archive. |
| Foundation | P0 is complete on the observed Linux/X11 desktop; the Godot .NET project, developer CLI, and isolated Godot Editor MCP path are implemented and qualified for the foundation scope. |
| Current gameplay | **P1 COMPLETE.** The owner accepted the current post-c1 controls, swimming, third-person camera/body and starter course as sufficient for progression. [P1 closeout](docs/tasks/p1/closeout.md) maps G1.1–G1.4 and evidence limits; [accepted tuning](docs/tasks/p1/qualification/accepted-tuning.md) is P2's exact baseline. |
| Controls correction | [c1-bindings](docs/tasks/c1-bindings/README.md) is implemented and reconciled into P1. Its [closeout](docs/tasks/c1-bindings/closeout.md) retains the partial advanced native matrix and the repaired help-layout failure. Further refinement is deferred. |
<<<<<<< HEAD
| Adaptive Follow | A fifth built-in control preset now combines mouse fish steering with hold-C independent camera look and smooth return. It is implemented with focused headless checks; native feel remains for owner observation. See [swimming and camera](docs/systems/swimming-and-camera.md). |
| Next Phase | **P2 — Build the underwater world** is IN PROGRESS / NOT GREEN. The owner selected a stronger water/light target on 2026-11-10, so [c2-watet-light-fx](docs/tasks/c2-watet-light-fx/README.md) is now AUTHORED / NOT EXECUTED and is the next bounded implementation before returning to native P2 acceptance. G2.1–G2.3 remain open; do not start P3. |
=======
| Adaptive Follow | A fifth built-in control preset combines mouse fish steering and W/S depth control with independent camera look while Left Shift is held, then returns smoothly. It is implemented with focused headless checks; native feel remains for owner observation. See [swimming and camera](docs/systems/swimming-and-camera.md). |
| Next Phase | **P2 — Build the underwater world** is IN PROGRESS / NOT GREEN. [P8 closeout review](docs/tasks/p2/closeout.md) reuses the source-matched P7 Forward+ observations but finds G2.1–G2.3 BLOCKED on owner judgment and missing native checks. The next action is the [P7 owner checkpoint](docs/tasks/p2/qualification/native-world.md), not P3 execution. |
>>>>>>> 6f2b73c (FAF Baseline)
| Renderer | **Forward+ is the owner-selected and tracked P2 desktop renderer.** Normal main play uses `forward_plus`; [P2 decision evidence](docs/tasks/p2/qualification/renderer-decision.md) records the native lock. Compatibility remains a diagnostic compare path only. |
| Assets | Quaternius Fish3/ClownFish, Rock Face 01, Coast Sand 01, and Smithsonian *Heliopora coerulea* are the P1 starter set. P2 may prepare efficient derivatives/instances and add only provenance-safe free environment assets when the approved habitat cannot be reached with the existing set. |
| Root version | `package.json` remains `0.2.6` while P7 acceptance and P8 closeout are blocked; `0.2.7`/`0.2.8` were not applied. The P2 phase-entry baseline was `0.2.0`; no `0.1.10`/`0.1.11` checkpoints were fabricated. |
| Later gameplay | Feeding/predators/growth move to P3. Persistence/death becomes P4, life recounting/history P5, and integrated qualification P6. [Movement abilities](docs/planning/p1/movement-abilities.md) remain accepted future work but are no longer the immediate post-P1 roadmap step. |

## Product contract to preserve

One fish's life is one run. The fish grows by feeding while avoiding predators, with third person as the primary perspective and first person as an optional view. Closing the game suspends the living run; death ends it permanently. Each dead life produces a short factual recounting retained under **Life History** in the main menu, independent of later runs.

The [decision register](docs/decisions-and-questions.md) separates confirmed behavior from unsettled final player species, production biome, balance, long-life pacing and later presentation choices. P2 establishes a convincing reef-like underwater environment for the current slice without claiming the final production biome or world-generation strategy.

## P1 completion boundary

P1 is complete under the owner's 2026-11-09 decision: the current controls are good enough to progress, and further control/movement refinement waits for later world/gameplay context. The authored [P10 iterative tuning](docs/tasks/p1/P10-owner-tuning-and-dynamics-lock.txt) was superseded at closeout; [P11 evidence review](docs/tasks/p1/closeout.md) was completed directly. The evidence record distinguishes actual native observations, saved-scene checks and unobserved advanced matrix cells. P0 and P1 are COMPLETE; c1 is reconciled. No old prompt execution or intermediate versions were invented.

Normal play now instances `p2_habitat.tscn`; the original P1 main is preserved byte-for-byte as `p1_course_fixture.tscn` for movement/camera/animation checks and renderer comparison. The basin has a continuous collidable seabed, four prepared material sets blended on one visual surface, safe spawn, hidden limits and a reef composition using all four prepared rock models and five coral species. [P4 evidence](docs/tasks/p2/qualification/implementation.md#p2-p4--reef-tunnels-and-world-composition) covers open, tunnel, shelter and vertical route geometry. [P5 evidence](docs/tasks/p2/qualification/implementation.md#p2-p5--underwater-atmosphere) covers the Forward+ atmosphere, and [P6 evidence](docs/tasks/p2/qualification/implementation.md#p2-p6--fish-reactive-water) covers fish-reactive GPU particles. [P7 evidence](docs/tasks/p2/qualification/native-world.md) records native route/counter/effect checks and the remaining owner handoff; [P8 review](docs/tasks/p2/closeout.md) maps each open Gate criterion.

## P2 entry and boundary

At the P2 handoff:

1. Inspect branch/HEAD, local changes, [final P1 tuning](docs/tasks/p1/qualification/accepted-tuning.md), current renderer and actual native evidence. Preserve compatible work.
2. Preserve P1's compare tooling/evidence and the completed [P2 native Forward+ lock](docs/tasks/p2/qualification/renderer-decision.md). The owner selected Forward+ after a smooth real-desktop trial; the tracked project now uses it. Execute the authored [c2-watet-light-fx correction](docs/tasks/c2-watet-light-fx/README.md) against the current P2 candidate, then return to native P2 owner acceptance. Another Compatibility A/B is unnecessary unless Forward+ develops a concrete blocker.
3. Keep rendering/world presentation read-only with respect to authoritative fish movement. A particle wake, post effect, fog change or camera effect must not change the motor, fish identity or simulation time.
4. Do not implement feeding, predators, growth, saves, death/history, controller, first person, or Movement abilities as hidden P2 scope.

Keep detailed execution results in task evidence. Update this guide only when actual implementation/evidence or the selected next action changes.
