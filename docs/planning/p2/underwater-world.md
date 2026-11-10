# P2 — Build the underwater world

**Decision date:** 2026-11-09  
**State:** IN PROGRESS. The base P2 habitat/effects exist; the post-P2 water/light correction is owner-green. `c2-depth` P1 now provides the structural depth basin; P2 water-column extinction is next before final P2 qualification.
**Execution:** Historical implementation lives in [docs/tasks/p2](../../tasks/p2/README.md); the next bounded stack is [c2-depth](../../tasks/c2-depth/README.md) at unchanged root `0.2.6`.

## Outcome

Turn the P1 swimming laboratory into a compact environment that is enjoyable to explore even before feeding or predators exist. Normal play should show a natural reef-like seabed, coral areas, rock formations and tunnels/arches/caves with open pockets and meaningful vertical routes. The whole scene should read unmistakably as underwater, and the water should visibly respond to the fish's movement.

This Phase is worldbuilding/presentation. It intentionally comes before ecology so future prey/predator behavior can use real cover, routes and visibility conditions instead of a flat test box.

## Confirmed requirements

- Replace the visible flat/test-course presentation in normal play. Do not leave a second walkable/swimmable ground layer, exposed developer shapes, colored target spheres, camera-check posts or obvious box-room boundaries as the finished environment.
- Build a natural coral/reef floor with varied elevation and usable 3D space.
- The default basin exposes **at least five natural traversable ledges/shelves at distinct elevations**. They are irregular reef shelves connected by slopes, cliffs, channels, drop-offs and alternate ascent/descent routes—not five clean staircase platforms.
- Include rock formations that create at least one tunnel/tight passage plus broader open routes, overhang/cover and landmarks at different depths. Compose sightlines so several depth bands can be visible together from useful viewpoints.
- Make the scene look submerged through a layered water presentation, not only a uniform blue tint. Actual water depth and view distance are separate inputs: world depth changes light/color and view depth/distance changes extinction.
- Add movement-reactive water effects around the fish so ordinary swimming, acceleration, gliding and stronger turning have visibly different disturbance.
- Keep rendering and water reaction presentation separate from authoritative movement. Effects read fish state; they do not push the fish or own velocity.
- P2/P1 provides the Compatibility/Forward+ comparison. After running Forward+ on the real desktop and observing smooth performance, the owner selected **Forward+** for P2. P2/P2 records/locks that choice; Compatibility is not a parallel production target.
- Preserve P1's reproducible movement course as a development/qualification fixture even when it stops being the normal game world.
- Keep fullscreen/windowed-fullscreen and the accepted P1 camera/input behavior working in the new habitat.

## Current starting point

The original P2 implementation replaced the P1 test room. Normal play is `main.tscn -> p2_habitat.tscn -> p2_reef_composition.tscn` on Forward+. `SeabedSurface.cs` retains one continuous 84 × 84 m visual/collision mesh. The c2-depth P1 correction reshaped it into five headless-checked ordered elevation bands, with primary and alternate south-to-north routes. Native visual qualification remains for P4.

The completed water/light correction adds the real overhead visual surface derived from the saved upper collision, procedural sky/sun cue, organic moving caustic projection, caustic-aligned ribbon shafts, restrained screen treatment, GPU motes and a paused GFX panel with bounded saved lighting presets. Preserve that accepted stack. The next correction changes **depth structure and depth response**, not the renderer or the core c2 visual identity.

At the pushed lighting baseline `d292aebd`, the saved upper collision is centered around Y 9.75 and the c2 surface is placed just above its top. `UnderwaterAtmosphere` already owns distance/height/volumetric fog, ambient/sun energy and the c2 effect materials. `seabed_blend.gdshader` already has world position available. These are the intended integration points for c2-depth.

## World structure

Prefer a **small authored modular habitat** for this first worldbuilding Phase. Use reusable rock/reef/seabed pieces and deliberate traversal composition rather than introducing a production terrain/procedural-world system prematurely. The exact mesh-authoring technique is an implementation choice.

The habitat should provide:

- one continuous collidable seabed with **at least five clearly distinct traversable elevation bands** spanning most of the usable water column;
- a shallow sun shelf, upper reef, middle shelf, lower reef and deep shelf as composition roles, with exact Y values tuned to the real surface/limits rather than treated as magic product constants;
- optional deeper trenches/drop-offs between shelves when they remain inside the bounded habitat;
- smooth/irregular transitions, unequal shelf footprints and natural channels so the result reads as geology rather than stairs;
- a primary route that can be swum shallow -> upper -> middle -> lower -> deep and back, plus alternate transitions where practical;
- sightlines that can show foreground/midground/background shelves simultaneously, making water-depth differences visible without a HUD;
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

Forward+ should provide one coordinated water-column treatment:

1. **Real surface reference:** use the actual c2 water-surface/upper-boundary relationship as the top of the water column. Do not invent an unrelated depth origin.
2. **World-space depth:** geometry lower in the water column receives progressively cooler/dimmer presentation even when viewed from a shallower camera. Height fog and existing world-position-aware materials are preferred cheap tools; do not add a stack of real lights or expensive fog volumes to fake this.
3. **View-distance extinction:** far geometry loses contrast and color smoothly. Useful horizontal visibility becomes shorter as the camera descends. Avoid a hard fog wall; the intended transition is detail -> silhouette -> water.
4. **Lighting/color:** bright tropical shallows retain useful local color. With depth, overall illumination falls and warm/red-orange contribution diminishes before blue/green, while gameplay silhouettes remain readable.
5. **Surface light effects:** preserve the owner-green c2 sunlight, organic caustics and ribbon shafts. Caustics are strongest in shallow water and visually fade with depth; shafts weaken/broaden/fade as they descend rather than illuminating the deepest shelf like the surface.
6. **Suspended matter and screen treatment:** keep motes and restrained screen treatment subordinate. UI remains outside world distortion.
7. **Camera-depth modulation:** presentation may read the active camera's Y/depth to derive effective fog/ambient/sun values, but it remains read-only. Camera position never becomes simulation authority.
8. **GFX presets:** saved lighting presets remain **base artistic settings**. Depth modulation derives runtime effective values from those bases; moving through the water column must not rewrite the user's preset or save depth-modulated values.
9. **Performance/tuning:** centralize depth parameters in the existing atmosphere boundary and reuse existing fog/material/effect paths. Prefer one shared depth model and a few scalar updates over extra lights, many decals or broad transparent layers.

The target is "obviously underwater while swimming," not photorealism. A shallow and deep shelf seen in one frame should not look as if they occupy identical water.

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
- the space feels natural/coherent and supports vertical exploration through at least five distinct ledges;
- a shallow-to-deep-to-shallow traverse produces a continuous, reversible change in brightness, warmth, caustic/shaft presence and useful visual range;
- comparable horizontal views at different elevations show progressively stronger distance extinction, and several ledges visible in one frame do not all read as if they receive the same water-column light;
- it clearly looks underwater at multiple distances/depths;
- the fish's movement visibly disturbs the water without becoming distracting;
- camera/collisions remain comfortable through tunnels/overhangs;
- Forward+ and the final effects run acceptably on the observed machine.

## Deferrals

No feeding, prey/predator AI, growth, save/resume, death/history, first person, controller, free look, Movement abilities, production procedural world generation, final biome commitment, weather/ocean-surface simulation, full fluid dynamics, broad graphics-settings UI, audio overhaul or production optimization campaign.

P3 owns ecology. Movement abilities remain a separately accepted future movement enhancement and are no longer the immediate post-P1 roadmap step.
