# P2 prompt assessment — underwater world

**Assessment date:** 2026-11-09; updated after P2/P1 execution.  
**Current continuation source:** P2/P1 is checkpointed on main at root version `0.2.1`. Its comparison tooling launches the same P1 player/camera with `./tools/dev compare compatibility` and `./tools/dev compare forward-plus` without editing `project.godot`. After a real desktop Forward+ run, the owner selected **Forward+** for the rest of P2.

## Request and player outcome

The owner wants P2 to focus on worldbuilding because the current world looks flat and artificial: multiple ground-like surfaces/test geometry remain visible and the space does not feel like a real underwater environment. P2 should create a natural coral/reef floor, rock tunnels/passages and an attractive environment, make the entire view clearly read as underwater, and add fluid-looking effects around the fish that emphasize movement.

The accepted roadmap change intentionally moves feeding/predators/growth to P3. P2 should make swimming through the world compelling before adding ecology.

## Current source findings at authoring

- `game/project.godot` still selects Godot **GL Compatibility** at the `0.2.1` boundary; P2 must change the tracked default to **Forward+** using the same renderer method proven by the comparison tooling.
- `game/scenes/main.tscn` has a box floor centered around Y -4.2, a box ceiling around Y +4.2, retaining walls around a roughly 19 m square space, a directional light and a `WorldEnvironment` populated at runtime.
- `game/scenes/movement_course.tscn` contains colored target/landmark spheres, two rock collision bodies and a visible camera-check cylinder.
- `StarterWorld.cs` applies Coast Sand to the flat floor, places two Rock Face 01 instances, two Smithsonian Heliopora instances and seven stationary Quaternius fish; it sets a simple background color.
- The existing Smithsonian Heliopora coral is a high-detail specimen (~100k triangles) and should not be naively carpeted across the seabed.
- Before P2 execution, the owner prepared an expanded local environment palette under the repository's ignored asset convention: ambientCG Ground060, Gravel041 and Rock063; Poly Haven Rock Face 02, Coastal Cliff 01 and Boulder 01 alongside existing Rock Face 01; and Smithsonian Acropora cervicornis, Acropora palmata, Meandrina meandrites and Montipora danae alongside Heliopora coerulea. The Smithsonian additions import at roughly 100k–150k triangles each and should be treated as measured hero/instanced sources rather than dense raw coverage. Execution must verify the local inventory/vendor mapping rather than assume an ignored payload exists in every checkout.
- The authoritative P1 boundaries already exist: `FishController`/one `SwimMotor`, `FollowCamera`, `FishVisualMotion`, the display/pause shell and selected assets. P2 must consume those rather than create a second movement/camera authority.
- P2/P1 added `RendererComparison.cs`, `renderer_comparison.tscn`, comparison shaders, and `tools/dev compare compatibility|forward-plus [--effects=off]`. Import/build/scene/effect-isolation checks passed; the automated renderer starts used Xvfb software rendering, so they did not select the renderer.
- **Owner native decision:** the owner ran Forward+ on the real desktop and reported that it runs smoothly, looks perhaps slightly better (not yet dramatically different), and that the added rendering capabilities are worth adopting. This supersedes the earlier open renderer choice. Compatibility remains useful only as a diagnostic comparison path unless Forward+ later shows a concrete blocker.

## Required outcome

1. **Lock Forward+.** Preserve P1's comparison tooling/evidence, record the owner's real desktop Forward+ observation, switch the tracked project default to Forward+, and confirm that normal play/assets/camera/UI still work. Do not require another native Compatibility A/B unless a concrete Forward+ regression calls the choice into question.
2. **Natural normal-play habitat.** Move the P1 movement course out of normal presentation while preserving it as a fixture. Create continuous seabed/topology, natural boundaries, open areas, rock tunnel/tight passage, cover/overhang, coral/reef areas and upper/lower traversal.
3. **Underwater visual stack.** Use depth/distance treatment, top-biased light, underwater color/contrast, caustic-style animation, suspended matter and restrained whole-view water treatment appropriate to the selected renderer. UI remains crisp.
4. **Movement-reactive water.** Read real fish movement state to drive a bounded wake/turbulence/bubble/trail/distortion response. Glide < cruise < acceleration/high effort; hard turns visibly redirect disturbance. No force feedback into the motor.
5. **Native quality bar.** Owner/native traversal must establish that the space feels coherent, clearly underwater and comfortable to swim through, with representative performance recorded.

## Decomposition

| Task | Responsibility | Evidence level |
| --- | --- | --- |
| P1 | Reversible renderer comparison harness + feature vertical slice | T1/T2 source/build/scene checks |
| P2 | Native Forward+ decision record and tracked renderer lock | T3 Forward+ confirmation/lock |
| P3 | New habitat root, continuous seabed, natural limits, regression fixture separation | T2 scene/collision integration |
| P4 | Reef/coral/rock composition, tunnels/arches/cover, vertical/open routes, prototype visual cleanup | T2 traversal topology/resource checks |
| P5 | Underwater atmosphere and post stack on chosen renderer | T2 render-resource/integration checks; native final at P7 |
| P6 | Fish-reactive water presentation with bounded emitters/read-only movement state | T2 state/effect integration |
| P7 | Complete native tuning/owner acceptance of V18–V20 | T3 |
| P8 | Evidence/Gate closeout and documentation reconciliation | T1 + review of T3 evidence |

## Risks and mitigations

- **Forward+ lock exposes a regression not seen in the trial:** lock the project using the already-proven comparison method, run a bounded real Forward+ smoke on representative assets/camera/UI, inspect errors/performance, and stop with a concrete blocker if it materially fails. Do not silently fall back to Compatibility or maintain dual production renderers.
- **Pretty scene but bad traversal:** define broad/tight/vertical routes and collision clearances relative to the accepted fish/camera, then test real traversal. Worldbuilding is gameplay space, not backdrop.
- **Player can escape under/through the world:** use continuous collidable seabed and deliberate natural boundaries; test underside/edge routes rather than relying only on a controller clamp.
- **Camera becomes unusable in tunnels:** tune geometry/collision and use accepted camera obstruction behavior; do not silently redesign P1 camera semantics.
- **Coral/heavy rock/transparent particles destroy performance:** avoid dense raw Smithsonian duplication, treat Coastal Cliff and other heavy meshes as measured hero geometry, bound particles/overdraw, measure representative native frame timing with the real mixed asset palette and tune.
- **Underwater effect becomes a blue filter:** require layered depth/light/caustics/motes and multiple-distance/depth observation.
- **Post effect distorts menus/HUD:** order presentation so interface remains crisp and input/focus ownership stays in the existing shell.
- **Reactive water changes physics:** presentation samples movement state read-only; tests assert no transform/velocity/collision writes from the effect component.
- **Effect backlog on pause/focus:** explicitly stop/clear or freeze emitters and reject giant resume bursts.
- **Asset-license drift:** use existing approved assets first; any new source follows the public-repo provenance/license rules and cannot be paid/account-gated as a hidden dependency.
- **Scope creep into ecology/world generation:** no prey/predators/growth, procedural production system, ocean-surface simulation or fluid solver.

## Success boundary

Successful prompt execution produces a qualified Phase 2 world, not merely authored shaders/scenes. P2 may remain NOT GREEN if the Forward+ lock develops a concrete stability/material/performance blocker, habitat traversal/camera fails, the owner does not find the underwater treatment convincing, the water reaction is distracting/wrong, or representative performance is unacceptable.

P2/P1 is already implemented at root `0.2.1`. Continue from that checkpoint; do not rerun P1 or recreate the `0.2.0` baseline.
