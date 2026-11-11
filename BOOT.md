# Fish Boot Guide

Read [AGENTS.md](AGENTS.md) first. This is a concise handoff; [the roadmap](docs/roadmap.md) owns Phase/Gate progress and [development setup](docs/development.md) owns command availability and tool pins.

## Current handoff — 2026-11-11

| Area | Actual state |
| --- | --- |
| Product | Owner-approved 3D fish survival across **Ocean and Freshwater Pond** (Pond planned): one living fish across sessions, permanent death, factual life recounting and a main-menu Life History archive; marine and freshwater species must stay in their own habitats. |
| Foundation | P0 is complete on the observed Linux/X11 desktop; the Godot .NET project, developer CLI, and isolated Godot Editor MCP path are implemented and qualified for the foundation scope. |
| Current gameplay | **P1 COMPLETE.** The owner accepted the current post-c1 controls, swimming, third-person camera/body and starter course as sufficient for progression. [P1 closeout](docs/tasks/p1/closeout.md) maps G1.1–G1.4 and evidence limits; [accepted tuning](docs/tasks/p1/qualification/accepted-tuning.md) is P2's exact baseline. |
| Controls correction | [c1-bindings](docs/tasks/c1-bindings/README.md) is implemented and reconciled into P1. Its [closeout](docs/tasks/c1-bindings/closeout.md) retains the partial advanced native matrix and the repaired help-layout failure. Further refinement is deferred. |
| Adaptive Follow | A fifth built-in control preset combines mouse fish steering and W/S depth control with independent camera look while Left Shift is held, then returns smoothly. It is implemented with focused headless checks; native feel remains for owner observation. See [swimming and camera](docs/systems/swimming-and-camera.md). |
| Next Phase | **P3 is in progress at `0.3.6`.** Character Select's native route works, but [P3 art review](docs/tasks/p3/qualification/native-world-art.md) and [C3 world generation P7](docs/tasks/c3-world-gen/closeout.md) fail the natural reef presentation. Seed 42 compiled in 51.687 s and baked-loaded in 149 ms without generation; full seeds 43/44 fail route repair. The provisional seed-42 Base World is not accepted. Run [manual C3 M1–M4](docs/tasks/c3-world-gen/manual/README.md) sequentially with owner visual review; repair generated-world visual composition and alternate-seed routes, repeat native passage/perimeter review and obtain owner judgment before c3-visibility. [C3 visibility P4](docs/tasks/c3-visibility/closeout.md) leaves V25 unqualified. P2 G2.3/V20 remains an accepted open gap. |
| Renderer | **Forward+ is the owner-selected and tracked P2 desktop renderer.** Normal main play uses `forward_plus`; [P2 decision evidence](docs/tasks/p2/qualification/renderer-decision.md) records the native lock. Compatibility remains a diagnostic compare path only. |
| Assets | Quaternius Fish3/ClownFish, Rock Face 01, Coast Sand 01, and Smithsonian *Heliopora coerulea* are the P1 starter set. P2 may prepare efficient derivatives/instances and add only provenance-safe free environment assets when the approved habitat cannot be reached with the existing set. |
| Root version | **`0.3.6`** — P3/P6 ambient life and finish checkpoint version, unchanged during C3 corrections. P2 ended at implementation version `0.2.6`; `0.2.7`/`0.2.8` were never fabricated. |
| Next design phase | **P4 is PLANNED, NOT IMPLEMENTED:** [Freshwater Pond + World Select](docs/planning/p4/freshwater-world.md) preserves the existing Ocean as a selectable option, adds Pond, filters Character Select and ambient fish by native habitat, and reuses shared compiler/Forward+/controls. After P3 closeout deliberately enter `0.4.x`; do not bump the current `0.3.6`. |
| Later gameplay | **P3 remains at `0.3.6` while M1–M4 manual C3 repairs and native review are pending.** Original feeding/predators/growth move to P5; persistence/death P6; life recounting/history P7; integrated qualification P8. |

## Product contract to preserve

One fish's life is one run. The fish grows by feeding while avoiding predators, with third person as the primary perspective and first person as an optional view. Closing the game suspends the living run; death ends it permanently. Each dead life produces a short factual recounting retained under **Life History** in the main menu, independent of later runs.

The [decision register](docs/decisions-and-questions.md) separates confirmed Ocean/Pond habitat selection from unresolved native species rosters, feeding balance, long-life pacing and later presentation choices. P2 and P3 establish the Ocean prototype and shared deterministic world compiler; P4 adds Pond without erasing that work.

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
