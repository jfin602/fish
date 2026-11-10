# P2 implementation plan — underwater world

## Entry and routing

Read AGENTS.md, BOOT.md, this stack README/assessment, [P2 design](../../planning/p2/underwater-world.md), [roadmap G2](../../roadmap.md#p2--build-the-underwater-world), [validation V18–V20](../../validation.md), [technical direction](../../technical-direction.md), [UX](../../ux-and-presentation.md), [art/assets](../../art-and-assets.md), final P1 accepted tuning/evidence, and affected source only.

Before implementation inspect branch/HEAD/dirty state, `package.json`, final P1/c1 state, `game/project.godot`, main/course scenes, `StarterWorld.cs`, fish/camera/display source, `artifacts/assets/source/p2-environment/inventory.json` plus the local vendor asset mapping when present, and `tools/dev` composition. Require actual P1 close and deliberate root `0.2.0` baseline. Confirm Godot MCP targets Fish before editor mutations.

## P1 — Renderer comparison vertical slice

Keep Compatibility as the current production setting until the native decision. Add the smallest reversible development path that can launch/inspect the same candidate under Compatibility and Forward+ using the actual Godot 4.7.2 capabilities. Inspect the engine CLI/project override rather than inventing unsupported flags. Do not create two permanent gameplay projects.

Create a bounded underwater rendering vertical slice using the existing P1 fish/camera and representative assets from the prepared P2 palette. Verify the full prepared set imports, while keeping the comparison scene small enough for a fair renderer A/B. It should make the renderer comparison meaningful by exercising:
- distance/depth fog or renderer-appropriate equivalent;
- top directional light and basic underwater color treatment;
- one caustic-style animated light/material treatment;
- sparse suspended particles;
- one restrained screen/depth-aware post treatment where supported.

The slice is diagnostic, not the final P2 habitat. Keep the existing main scene runnable. Add focused checks for renderer launch selection/config isolation, material/resource load, finite shader parameters and unchanged movement authority. Record implementation evidence and exact commands. Assigned P1 version: `0.2.1`.

## P2 — Native renderer lock

Mandatory native handoff. Run the same candidate/viewpoints under both renderers on the target desktop where supported. Record Godot/backend, resolution/display mode, source/assets, visual correctness, errors, material parity, fog/post/particle behavior and representative frame timing/FPS. Use real game pixels; an editor viewport/headless process alone is insufficient.

Choose Compatibility or Forward+ from evidence and explain the tradeoff. Apply that choice to the tracked project/configuration, remove/disable comparison-only production ambiguity, update the technical record, and retain a reproducible way to understand the rejected path without shipping dual runtime logic. If one renderer cannot launch or materially breaks the candidate, record that as evidence rather than forcing parity. Write `qualification/renderer-decision.md`. Assigned P2 version: `0.2.2`.

## P3 — Natural habitat foundation

Move normal play away from the visible P1 movement-course box while preserving that course as a dedicated development/qualification fixture. Create a P2 habitat root/scene with:
- continuous collidable seabed with varied elevation;
- a reusable seabed palette using Coast Sand 01, Ground060, Gravel041 and Rock063;
- spawn and enough open water for normal swimming;
- natural-looking enclosing terrain/rock boundaries instead of exposed box walls;
- no route under the seabed or outside intended limits;
- retained access to accepted P1 fish/camera/display/input without duplicating their ownership.

Use reusable scene/resource pieces rather than growing `StarterWorld.cs` into a giant world-construction method. Keep static visual and collision responsibilities explicit. Add scene/collision checks for spawn clearance, underside/edge containment and fixture separation. Assigned P3 version: `0.2.3`.

## P4 — Reef tunnels and world composition

Build the actual exploration space from the prepared palette. Use Coast Sand 01, Ground060, Gravel041 and Rock063 for seabed/material variation; Rock Face 01, Rock Face 02, Coastal Cliff 01 and Boulder 01 for terrain structure; and Heliopora coerulea, Acropora cervicornis, Acropora palmata, Meandrina meandrites and Montipora danae for distinct coral silhouettes. The final normal-play habitat **must** visibly use every prepared model at least once and visibly use every prepared terrain material set. Performance is handled through placement, reusable scenes, instancing, LODs or derived lower-cost runtime resources—not by silently omitting an asset. If a prepared asset proves unusable, P4 stops with that blocker unless the owner explicitly changes this requirement. Compose:
- coral/reef coverage that reads as a floor/ecosystem base rather than two props;
- multiple rock formations;
- at least one rock tunnel/arch/cave-like passage;
- an overhang/sheltered pocket;
- a broad cruising route;
- a tighter navigation route;
- upper/lower landmarks and a vertical route;
- visual variety/orientation/scale that avoids obvious copy-paste repetition.

Remove P1 target spheres/camera-check post/test-course presentation from normal play; preserve them only in the fixture if still useful. Simplified collision may differ from visual mesh but must not mislead tunnel clearance. Do not carpet the scene with raw ~100k–150k-triangle coral or densely duplicate heavy hero rock meshes. The prepared set is intended to be sufficient; add another free/provenance-safe environment asset only for a concrete blocking gap and document it. Assigned P4 version: `0.2.4`.

## P5 — Underwater atmosphere

On the locked renderer, implement a coherent environment stack owned by a small number of resources/components:
- distance/depth attenuation/fog;
- underwater color/contrast;
- top-biased sunlight;
- animated caustic-style illumination across representative sand/gravel/rock/coral surfaces from the actual prepared palette;
- sparse suspended motes;
- restrained whole-view water distortion/noise/grade, depth-aware where the selected renderer supports it;
- tunable parameters for native iteration.

Do not apply a uniform full-screen blue rectangle and call it water. Keep pause/settings/HUD crisp and outside inappropriate distortion. Shaders/effects must tolerate resize/fullscreen changes, pause and missing optional visual resources. Add bounded render-resource/scene checks and prove equal authoritative fish movement under presentation on/off where practical. Assigned P5 version: `0.2.5`.

## P6 — Fish-reactive water

Add a presentation-only component/effect anchored to the accepted player visual/root. Read movement state such as velocity magnitude, acceleration/effort, turn rate and glide state without writing back. Drive a renderer-appropriate combination of wake particles, bubbles, turbulence, ribbon/trail or local distortion.

Tune four observable states: near-rest/glide minimal, normal cruise subtle steady wake, acceleration/high effort stronger response, hard turn directional/asymmetric disturbance. Spawn from plausible tail/body space through pitch/yaw, avoid camera obstruction, cap counts/lifetimes, and clear/freeze safely across pause/focus/reset/scene reload. Add tests that effect updates cannot change fish transform/velocity/collision and that resume does not dump a queued burst. Assigned P6 version: `0.2.6`.

## P7 — Native world tuning and qualification

Mandatory native handoff on the final combined P2 candidate. Traverse V18 broad/tight/vertical routes, including rock tunnel/overhang and seabed/edge containment. Tune composition/collision/camera clearance rather than changing accepted motor semantics to fit the world.

Observe V19 at near/far and upper/lower viewpoints in fullscreen and windowed-fullscreen: water depth, color/light, caustic motion, motes, post effect, UI readability, motion comfort and representative performance. Confirm the accepted normal-play world visibly uses the complete prepared seabed, four-piece rock set and five-species coral set. A missing prepared asset is a failed/incomplete composition requirement, not a performance exception; optimize its representation rather than dropping it. Confirm the locked renderer still matches its decision record after full scene load.

Observe V20 at rest/glide, cruise, acceleration/high effort and hard turn. Tune intensity/origin/lifetime so reaction is visible but not distracting. Exercise pause/focus/resume to catch backlog/burst issues. Record owner judgment that the prototype-course look is gone, the world feels natural/coherent and clearly underwater, and movement reaction improves the sense of swimming. Write `qualification/native-world.md` and `qualification/accepted-environment.md`. Assigned P7 version: `0.2.7`.

## P8 — Final closeout

Review G2.1–G2.3 against source-bound P1–P7 evidence. Reuse valid unchanged observations; rerun only invalidated native criteria. Reconcile roadmap, BOOT, MODULES, task state and owning docs once. Do not mark P2 complete if a Gate remains failed/unobserved or if owner/native quality is missing.

On success, route the next roadmap Phase to P3 feeding/escape/growth. Movement abilities remain separately accepted future work. Leave final closeout changes reviewable. Assigned closeout version: `0.2.8` subject to the runner's closeout/version policy; version metadata is not evidence.

## Test strategy and exclusions

Use T1 for config/resource/unit risks, T2 for scene/collision/effect integration and T3 for renderer/world visual qualification. Inspect `./tools/dev check` composition and avoid redundant builds. Use real Godot scene/runtime checks, `git diff --check`, public-asset boundary review and no `package-lock.json`.

Do not expand into ecology, survival saves, first person, controller, free look, Movement abilities, procedural production generation, surface/ocean waves, buoyancy, general graphics settings, new engine versions, broad refactors or a physically simulated fluid.
