# P2 prompt assessment — underwater world

**Assessment date:** 2026-11-09  
**Authoring source:** main documentation commit `7c428c403cb9bafdcad1491a9cc404d0f6da171e`; gameplay source remains the P1 direct-P9 candidate at root version `0.1.9`. P1 native closeout is still pending, so execution must re-inspect the actual post-P1 tree.

## Request and player outcome

The owner wants P2 to focus on worldbuilding because the current world looks flat and artificial: multiple ground-like surfaces/test geometry remain visible and the space does not feel like a real underwater environment. P2 should create a natural coral/reef floor, rock tunnels/passages and an attractive environment, make the entire view clearly read as underwater, and add fluid-looking effects around the fish that emphasize movement.

The accepted roadmap change intentionally moves feeding/predators/growth to P3. P2 should make swimming through the world compelling before adding ecology.

## Current source findings at authoring

- `game/project.godot` selects Godot **GL Compatibility**.
- `game/scenes/main.tscn` has a box floor centered around Y -4.2, a box ceiling around Y +4.2, retaining walls around a roughly 19 m square space, a directional light and a `WorldEnvironment` populated at runtime.
- `game/scenes/movement_course.tscn` contains colored target/landmark spheres, two rock collision bodies and a visible camera-check cylinder.
- `StarterWorld.cs` applies Coast Sand to the flat floor, places two Rock Face 01 instances, two Smithsonian Heliopora instances and seven stationary Quaternius fish; it sets a simple background color.
- The Smithsonian coral is a high-detail specimen (~100k triangles) and should not be naively carpeted across the seabed.
- The authoritative P1 boundaries already exist: `FishController`/one `SwimMotor`, `FollowCamera`, `FishVisualMotion`, the display/pause shell and selected assets. P2 must consume those rather than create a second movement/camera authority.
- The user-reported local checkout may advance beyond this snapshot during P1 tuning/c1-bindings. Every prompt must inspect actual source and preserve compatible changes.

## Required outcome

1. **Renderer choice from evidence.** Build a reversible comparison slice and observe Compatibility versus Forward+ on the same post-P1 candidate/desktop. Pick one; do not choose Forward+ merely because it exposes more effects.
2. **Natural normal-play habitat.** Move the P1 movement course out of normal presentation while preserving it as a fixture. Create continuous seabed/topology, natural boundaries, open areas, rock tunnel/tight passage, cover/overhang, coral/reef areas and upper/lower traversal.
3. **Underwater visual stack.** Use depth/distance treatment, top-biased light, underwater color/contrast, caustic-style animation, suspended matter and restrained whole-view water treatment appropriate to the selected renderer. UI remains crisp.
4. **Movement-reactive water.** Read real fish movement state to drive a bounded wake/turbulence/bubble/trail/distortion response. Glide < cruise < acceleration/high effort; hard turns visibly redirect disturbance. No force feedback into the motor.
5. **Native quality bar.** Owner/native traversal must establish that the space feels coherent, clearly underwater and comfortable to swim through, with representative performance recorded.

## Decomposition

| Task | Responsibility | Evidence level |
| --- | --- | --- |
| P1 | Reversible renderer comparison harness + feature vertical slice | T1/T2 source/build/scene checks |
| P2 | Native same-candidate renderer comparison and lock | T3 native renderer decision |
| P3 | New habitat root, continuous seabed, natural limits, regression fixture separation | T2 scene/collision integration |
| P4 | Reef/coral/rock composition, tunnels/arches/cover, vertical/open routes, prototype visual cleanup | T2 traversal topology/resource checks |
| P5 | Underwater atmosphere and post stack on chosen renderer | T2 render-resource/integration checks; native final at P7 |
| P6 | Fish-reactive water presentation with bounded emitters/read-only movement state | T2 state/effect integration |
| P7 | Complete native tuning/owner acceptance of V18–V20 | T3 |
| P8 | Evidence/Gate closeout and documentation reconciliation | T1 + review of T3 evidence |

## Risks and mitigations

- **Renderer migration breaks imports/materials/MCP:** compare the actual P1 candidate before lock, preserve a reversible path until P2 native decision, inspect output/errors and imported fish/rock/coral/sand under both.
- **Pretty scene but bad traversal:** define broad/tight/vertical routes and collision clearances relative to the accepted fish/camera, then test real traversal. Worldbuilding is gameplay space, not backdrop.
- **Player can escape under/through the world:** use continuous collidable seabed and deliberate natural boundaries; test underside/edge routes rather than relying only on a controller clamp.
- **Camera becomes unusable in tunnels:** tune geometry/collision and use accepted camera obstruction behavior; do not silently redesign P1 camera semantics.
- **Coral/transparent particles destroy performance:** avoid dense raw Heliopora duplication, bound particles/overdraw, measure representative native frame timing and tune.
- **Underwater effect becomes a blue filter:** require layered depth/light/caustics/motes and multiple-distance/depth observation.
- **Post effect distorts menus/HUD:** order presentation so interface remains crisp and input/focus ownership stays in the existing shell.
- **Reactive water changes physics:** presentation samples movement state read-only; tests assert no transform/velocity/collision writes from the effect component.
- **Effect backlog on pause/focus:** explicitly stop/clear or freeze emitters and reject giant resume bursts.
- **Asset-license drift:** use existing approved assets first; any new source follows the public-repo provenance/license rules and cannot be paid/account-gated as a hidden dependency.
- **Scope creep into ecology/world generation:** no prey/predators/growth, procedural production system, ocean-surface simulation or fluid solver.

## Success boundary

Successful prompt execution produces a qualified Phase 2 world, not merely authored shaders/scenes. P2 may remain NOT GREEN if the renderer comparison is blocked, habitat traversal/camera fails, the owner does not find the underwater treatment convincing, the water reaction is distracting/wrong, or representative performance is unacceptable.

The stack is authored only. Root version stays `0.1.9` until P1 is complete and the explicit `0.2.0` entry transition is made.
