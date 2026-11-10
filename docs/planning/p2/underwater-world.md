# P2 — Build the underwater world

**Decision date:** 2026-11-09  
**State:** Owner-selected and planned; implementation/evidence NOT STARTED.  
**Execution:** [docs/tasks/p2](../../tasks/p2/README.md) after P1 closes and the deliberate root `0.2.0` entry baseline is prepared.

## Outcome

Turn the P1 swimming laboratory into a compact environment that is enjoyable to explore even before feeding or predators exist. Normal play should show a natural reef-like seabed, coral areas, rock formations and tunnels/arches/caves with open pockets and meaningful vertical routes. The whole scene should read unmistakably as underwater, and the water should visibly respond to the fish's movement.

This Phase is worldbuilding/presentation. It intentionally comes before ecology so future prey/predator behavior can use real cover, routes and visibility conditions instead of a flat test box.

## Confirmed requirements

- Replace the visible flat/test-course presentation in normal play. Do not leave a second walkable/swimmable ground layer, exposed developer shapes, colored target spheres, camera-check posts or obvious box-room boundaries as the finished environment.
- Build a natural coral/reef floor with varied elevation and usable 3D space.
- Include rock formations that create at least one tunnel/tight passage plus broader open routes, overhang/cover and landmarks at different depths.
- Make the scene look submerged through a layered water presentation, not only a uniform blue tint.
- Add movement-reactive water effects around the fish so ordinary swimming, acceleration, gliding and stronger turning have visibly different disturbance.
- Keep rendering and water reaction presentation separate from authoritative movement. Effects read fish state; they do not push the fish or own velocity.
- P2/P1 provides the Compatibility/Forward+ comparison. After running Forward+ on the real desktop and observing smooth performance, the owner selected **Forward+** for P2. P2/P2 records/locks that choice; Compatibility is not a parallel production target.
- Preserve P1's reproducible movement course as a development/qualification fixture even when it stops being the normal game world.
- Keep fullscreen/windowed-fullscreen and the accepted P1 camera/input behavior working in the new habitat.

## Current starting point

At planning time, `game/project.godot` explicitly selects `gl_compatibility`. `main.tscn` contains a flat floor and ceiling at roughly Y ±4.2, retaining walls around a 19 m square space, a sun and a `WorldEnvironment` whose resource is populated at runtime. `movement_course.tscn` adds target/landmark spheres, two rock collision bodies and a camera-check cylinder. `StarterWorld.cs` applies Coast Sand to the flat floor, places two Rock Face 01 instances, two Heliopora coral instances and seven stationary Quaternius fish, and sets a simple background color.

That composition was appropriate for P1 movement qualification. It is not the P2 target. The executor must inspect the actual post-P1 checkout because P10/c1-bindings or owner fixes may change source before P2 begins.

## World structure

Prefer a **small authored modular habitat** for this first worldbuilding Phase. Use reusable rock/reef/seabed pieces and deliberate traversal composition rather than introducing a production terrain/procedural-world system prematurely. The exact mesh-authoring technique is an implementation choice.

The habitat should provide:

- a continuous seabed with visible height/shape variation;
- coral/reef coverage that reads as a living ocean floor rather than isolated props;
- broad open water for comfortable cruising;
- rock walls/formations and at least one tunnel/arch/cave-like passage;
- an overhang or sheltered pocket;
- upper/lower landmarks that encourage pitch-based travel;
- routes wide enough for the accepted P1 camera, plus at least one tighter passage that demonstrates deliberate navigation;
- natural-looking world limits that prevent swimming under the floor or escaping the playable slice without showing a rectangular test cage.

Do not delete the P1 course merely to remove its visuals. Move normal play to the habitat while retaining a dedicated movement-test scene/fixture or equivalent route for regression work.

## Asset strategy

Before execution, the owner prepared an expanded local P2 environment library. `artifacts/assets/source/p2-environment/inventory.json` is the local source/provenance inventory and `game/assets/vendor-local/p2-environment/` contains the Godot-ready working assets. The intended palette is Coast Sand 01 + ambientCG Ground060, Gravel041 and Rock063 for seabed variation; Rock Face 01 + Poly Haven Rock Face 02, Coastal Cliff 01 and Boulder 01 for terrain structure; and Smithsonian Heliopora coerulea + Acropora cervicornis, Acropora palmata, Meandrina meandrites and Montipora danae for coral silhouettes.

**Owner requirement:** the P2 normal-play habitat must visibly use every prepared model at least once and visibly use every prepared terrain material set. Performance work should optimize placement, instance counts, LODs or derived runtime resources rather than omit an asset. If a prepared asset proves technically unusable, the affected P2 task is blocked until repaired or the owner explicitly changes this requirement.

Do not densely duplicate high-detail Smithsonian coral or heavy hero rock/cliff geometry without measuring cost. Prefer sparse hero placement, reusable scenes, instancing/LOD or lower-cost derived runtime resources as justified by measured performance. The prepared library is intended to be sufficient; additional third-party environment content should only be sourced for a concrete blocking gap and must remain free, legally suitable and fully provenance-documented.

## Renderer decision

P2/P1 created a reversible renderer/underwater comparison slice using the actual P1 player, materials and camera. Its automated import/build/startup checks exercised both Compatibility and Forward+ under Xvfb/software rendering without changing `project.godot`.

The owner then ran `./tools/dev compare forward-plus` on the real desktop. Forward+ ran smoothly, looked perhaps slightly better at this early stage, and the owner selected it because the additional rendering capabilities are worth using for the underwater world.

**P2 renderer direction is therefore Forward+.** P2/P2 must switch the tracked normal-play project to the exact Forward+ method used by the comparison tooling and record the source/platform evidence. It does not need another native Compatibility A/B. Keep the Compatibility compare command only as a diagnostic path if useful; do not carry dual production rendering implementations. If the full P2 world later reveals a concrete Forward+ blocker, stop and return to the owner rather than silently switching.

## Underwater atmosphere

Forward+ should provide the layered treatment:

1. **Depth/distance:** fog or equivalent distance attenuation so far geometry loses contrast and color.
2. **Lighting:** directional/top light that suggests light entering from the surface.
3. **Color:** restrained blue-green water absorption/grade while retaining useful local color.
4. **Caustic-style motion:** animated light pattern on seabed/rocks/coral, implemented by a bounded Forward+ material/decal/projected/fullscreen technique.
5. **Suspended matter:** sparse motes/plankton-like particles that provide depth/motion cues without visual noise.
6. **Whole-view water treatment:** subtle screen-space distortion/noise/grade where useful, depth-aware when supported. Do not make the image wobble constantly or distort HUD/menu text.
7. **Performance/tuning:** centralize parameters enough to tune the look without scattering magic values through unrelated gameplay code.

The target is "obviously underwater while swimming," not photorealism.

## Fish-reactive water

Use presentation effects rather than a fluid solver. A small component may sample read-only movement data such as velocity magnitude, acceleration/propulsion state, turn rate and glide state. It can drive tail/body wake particles, bubbles, turbulence, ribbons/trails or local distortion using Forward+ presentation primitives appropriate to Godot 4.7.2.

Required response:

- near-rest/glide: little disturbance;
- ordinary swimming: steady subtle wake;
- acceleration/high effort: stronger/denser response;
- hard turn: directional/asymmetric burst or curved disturbance;
- pause/focus/reset: no runaway emitter backlog or giant first-frame burst.

Cap spawn rates/lifetimes and keep the camera clear. These effects must never call the motor to add force or alter collision.

## Validation and finish line

Use [V18–V20](../../validation.md) and roadmap G2.1–G2.3. P2 requires native observation of the complete environment, not only shader compilation or a screenshot.

The owner should be able to spend several minutes swimming through open water, coral floor, elevation changes and rock passages and judge that:

- the prototype-course look is gone from normal play;
- the space feels natural/coherent and supports vertical exploration;
- it clearly looks underwater at multiple distances/depths;
- the fish's movement visibly disturbs the water without becoming distracting;
- camera/collisions remain comfortable through tunnels/overhangs;
- Forward+ and the final effects run acceptably on the observed machine.

## Deferrals

No feeding, prey/predator AI, growth, save/resume, death/history, first person, controller, free look, Movement abilities, production procedural world generation, final biome commitment, weather/ocean-surface simulation, full fluid dynamics, broad graphics-settings UI, audio overhaul or production optimization campaign.

P3 owns ecology. Movement abilities remain a separately accepted future movement enhancement and are no longer the immediate post-P1 roadmap step.
