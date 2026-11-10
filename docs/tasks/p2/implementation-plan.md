# P2 implementation plan — underwater world

## Entry and routing

Read AGENTS.md, BOOT.md, this stack README/assessment, [P2 design](../../planning/p2/underwater-world.md), [roadmap G2](../../roadmap.md#p2--build-the-underwater-world), [validation V18–V20](../../validation.md), [technical direction](../../technical-direction.md), [UX](../../ux-and-presentation.md), [art/assets](../../art-and-assets.md), final P1 accepted tuning/evidence, and affected source only.

Before continuation inspect branch/HEAD/dirty state, `package.json`, final P1/c1 state, P2/P1 implementation evidence, `game/project.godot`, main/course/comparison scenes, `RendererComparison.cs`, `StarterWorld.cs`, fish/camera/display source, `artifacts/assets/source/p2-environment/inventory.json` plus the local vendor asset mapping when present, and `tools/dev` composition. Require the real P2/P1 `0.2.1` checkpoint. Confirm Godot MCP targets Fish before editor mutations. **Forward+ is the owner-selected P2 renderer from this point forward.**

## P1 — Renderer comparison vertical slice — COMPLETE

P1 is historical input for the remaining stack; **do not rerun it**. At root `0.2.1`, it added the reversible `tools/dev compare compatibility|forward-plus [--effects=off]` path, `RendererComparison.cs`, `renderer_comparison.tscn` and comparison shaders without changing normal play or `project.godot`. Import/build/scene/prepared-asset/effect-isolation checks passed; automated renderer starts used Xvfb/software rendering. The owner's subsequent real-desktop Forward+ trial supplies the native selection input for P2.

## P2 — Native Forward+ lock — COMPLETE

The renderer choice is no longer open. The owner ran `./tools/dev compare forward-plus` on the real desktop, reported smooth performance and slightly improved appearance, and explicitly chose Forward+ because its additional rendering capabilities are worthwhile for P2. The tracked lock and post-lock checks are in [the decision record](qualification/renderer-decision.md).

The lock uses the exact `forward_plus` method proven by the comparison path. The decision record covers the normal-play Fish3, seabed, rock/coral, camera, pause/UI and display checks, source/platform identity, runtime errors and a bounded idle frame sample. No native Compatibility rerun was needed. If Forward+ materially fails later, record the concrete blocker and return to the owner rather than silently falling back.

Keep `tools/dev compare compatibility` only as a development diagnostic if it remains useful; production/main should have one renderer: Forward+. Update the technical record and write `qualification/renderer-decision.md` with the P1 automated comparison boundary, the owner's real desktop Forward+ selection, the tracked lock, validation results and limitations. Assigned P2 version: `0.2.2`.

## P3 — Natural habitat foundation — COMPLETE

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

On **Forward+**, implement a coherent environment stack owned by a small number of resources/components. Use Forward+ capabilities deliberately instead of carrying Compatibility-safe fallbacks. Prefer volumetric/depth-aware fog and GPU-driven presentation where they materially improve the result:
- Forward+ volumetric fog and/or depth-aware distance attenuation, tuned for near-route readability and distant water depth;
- underwater color/contrast;
- top-biased sunlight;
- animated caustic-style illumination across representative sand/gravel/rock/coral surfaces from the actual prepared palette;
- sparse GPU-driven suspended motes/particles;
- restrained whole-view water distortion/noise/grade using Forward+ depth/screen information where useful;
- tunable parameters for native iteration.

Do not apply a uniform full-screen blue rectangle and call it water. Keep pause/settings/HUD crisp and outside inappropriate distortion. Shaders/effects must tolerate resize/fullscreen changes, pause and missing optional visual resources. Add bounded render-resource/scene checks and prove equal authoritative fish movement under presentation on/off where practical. Assigned P5 version: `0.2.5`.

## P6 — Fish-reactive water

Add a presentation-only component/effect anchored to the accepted player visual/root. Read movement state such as velocity magnitude, acceleration/effort, turn rate and glide state without writing back. Drive a **Forward+** combination of GPUParticles3D, bubbles/turbulence, particle trails/ribbons and/or local distortion as appropriate. No Compatibility fallback path is required.

Tune four observable states: near-rest/glide minimal, normal cruise subtle steady wake, acceleration/high effort stronger response, hard turn directional/asymmetric disturbance. Spawn from plausible tail/body space through pitch/yaw, avoid camera obstruction, cap counts/lifetimes, and clear/freeze safely across pause/focus/reset/scene reload. Add tests that effect updates cannot change fish transform/velocity/collision and that resume does not dump a queued burst. Assigned P6 version: `0.2.6`.

## P7 — Native world tuning and qualification

Mandatory native handoff on the final combined P2 candidate. Traverse V18 broad/tight/vertical routes, including rock tunnel/overhang and seabed/edge containment. Tune composition/collision/camera clearance rather than changing accepted motor semantics to fit the world.

Observe V19 at near/far and upper/lower viewpoints in fullscreen and windowed-fullscreen: water depth, color/light, caustic motion, motes, post effect, UI readability, motion comfort and representative performance. Confirm the accepted normal-play world visibly uses the complete prepared seabed, four-piece rock set and five-species coral set. A missing prepared asset is a failed/incomplete composition requirement, not a performance exception; optimize its representation rather than dropping it. Confirm **Forward+** remains smooth/stable enough on the full scene and still matches the owner-selected renderer decision after full scene load. If the full habitat reveals a material Forward+ blocker, stop and escalate instead of silently switching renderer.

Observe V20 at rest/glide, cruise, acceleration/high effort and hard turn. Tune intensity/origin/lifetime so reaction is visible but not distracting. Exercise pause/focus/resume to catch backlog/burst issues. Record owner judgment that the prototype-course look is gone, the world feels natural/coherent and clearly underwater, and movement reaction improves the sense of swimming. Write `qualification/native-world.md` and `qualification/accepted-environment.md`. Assigned P7 version: `0.2.7`.

## P8 — Final closeout

Review G2.1–G2.3 against source-bound P1–P7 evidence. Reuse valid unchanged observations; rerun only invalidated native criteria. Reconcile roadmap, BOOT, MODULES, task state and owning docs once. Do not mark P2 complete if a Gate remains failed/unobserved or if owner/native quality is missing.

On success, route the next roadmap Phase to P3 feeding/escape/growth. Movement abilities remain separately accepted future work. Leave final closeout changes reviewable. Assigned closeout version: `0.2.8` subject to the runner's closeout/version policy; version metadata is not evidence.

## Test strategy and exclusions

Use T1 for config/resource/unit risks, T2 for scene/collision/effect integration and T3 for Forward+/world visual qualification. Inspect `./tools/dev check` composition and avoid redundant builds. Use real Godot scene/runtime checks, `git diff --check`, public-asset boundary review and no `package-lock.json`.

Do not expand into ecology, survival saves, first person, controller, free look, Movement abilities, procedural production generation, surface/ocean waves, buoyancy, general graphics settings, new engine versions, broad refactors, dual production renderer support or a physically simulated fluid.
