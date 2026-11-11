# P3 — Rebuild and dress the reef with FAF assets

**Decision date:** 2026-11-10  
**State:** Owner-selected and READY TO EXECUTE at root `0.3.0`; implementation/evidence NOT STARTED.  
**Execution:** [docs/tasks/p3](../../tasks/p3/README.md) under owner transition S16. P2 G2.3/V20 remains an accepted evidence gap rather than a fabricated PASS.

## Outcome

Rebuild the current varying-depth P2 world into the first **style-proving Fish reef** using the completed Fish Asset Factory library.

**S18 supersedes the original authored/modular world assumption.** P3 now compiles a deterministic finite reef world: irregular basin, true 3D geology, protected routes, terrain analysis, natural FAF habitat placement and baked output. It preserves the accepted Forward+ atmosphere, movement/camera authority and overall shallow-to-deep gameplay intent, but it does not preserve obsolete fixed P2/P3 anchor topology when that topology causes artificial world structure.

The target is the authoritative [FAF art style](../../fish-asset-factory/art-style.md) and [reef target](../../fish-asset-factory/references/reef-style-target.png). The isolated [FAF style proof 001](../../fish-asset-factory/style-proof-001/README.md) is a useful composition baseline and warning record: its stacked boulder arch, visible terrain apron seams, sparse reef layering and diorama perimeter are **not** the live-world finish to copy.

## Entry boundary

P3 is now explicitly authorized to execute. The owner accepted the remaining P2 G2.3/V20 evidence gap for progression; P2 is not relabeled GREEN.

Before P3 execution:

1. P2 G2.1/G2.2 are accepted; owner decision S16 waives G2.3/V20 **only as a progression blocker**, while preserving it as OPEN/NOT GREEN evidence.
2. Preserve Forward+, the owner-green water/light/depth identity, water reaction, accepted P1 movement/camera/controls and the P1 fixture.
3. Root `package.json` is deliberately **`0.3.0`**. P3 prompts target `0.3.1` through `0.3.8`.
4. Inspect current source before assuming the authoring-time `SeabedSurface` / reef-composition layout survived P2 closeout.

## Core FAF library

P3 consumes the completed tracked original outputs. It does not generate replacement families by default.

### Environment — required normal-play coverage

- **FAF-TERRAIN-001A–F:** six terrain roles.
- **FAF-ROCK-001A–D:** four boulder variants.
- **FAF-CORAL-001–004 A/B/C:** twelve coral variants.
- **FAF-PLANT-001–003:** eight vegetation variants.
- **FAF-DETAIL-001–004 A/B:** eight small reef-floor detail variants: shells, rubble, pebbles and sponges.

That is **38 required environment variants**. Every one must participate meaningfully in normal play by P3 closeout. This is not permission to hide checklist instances off-route.

### Selectable player fish and ambient fish

P3 adds a **Character Select** page using all five finished FAF fish as selectable player visuals:

- FAF-FISH-001 Yellow Tang;
- FAF-FISH-002 Ocellaris Clownfish;
- FAF-FISH-003 Blue-Green Chromis;
- FAF-FISH-004 Reef Butterflyfish;
- FAF-FISH-005 Blacktip Reef Shark.

This is a visual/player-body choice only for P3. All five use the same existing authoritative `FishController`, collision footprint, SwimMotor, controls, camera, tuning and water-reaction semantics. Per-character presentation transforms may normalize source scale/orientation into one shared player envelope; selecting the shark does **not** make the gameplay collider larger or add predator stats/behavior.

Fresh launch enters Character Select before normal play. The page shows an animated 3D preview and the fish name, lets mouse/keyboard users browse all five choices, and starts the reef session with the selected visual. Selection is session-local for P3; it is not a saved-life or species-stat system.

Pause/Game Menu adds **Return to Character Select**. It clears held/captured input, unloads the current ephemeral play session and returns to the selection page with a visible pointer. It does not count as death, archive a life, save/resume a run, or establish the later abandonment policy. The most recently selected fish may remain highlighted in memory for convenience, but no disk persistence is required yet.

FAF-FISH-001–004 may additionally provide presentation-only ambient reef life. FAF-FISH-005 remains excluded from ambient predator presentation by default even though it is selectable as the player visual; later ecology will decide shark threat behavior.

## Procedural world formation

The authoritative formation contract is [procedural-world-generation.md](procedural-world-generation.md), implemented by [c3-world-gen](../../tasks/c3-world-gen/README.md).

The generated world owns:

- an irregular non-circular basin with useful deepest terrain near the central region;
- natural outward-rising perimeter rather than visible rectangular/four-wall limits;
- arbitrary 3D geological cave/tunnel/arch/overhang meshes;
- generated protected shallow/middle/deep/return/alternate/cave routes;
- terrain/environment analysis fields;
- habitat zones and terrain-aware cluster placement;
- support/embedding and relaxation/repair;
- deterministic bake/load and Base World promotion.

The six FAF terrain variants are no longer floor tiles. They may appear as localized embedded features when supported by generated terrain, while the generated basin/geology owns playable ground.

Good generated candidates may be promoted to curated Base Worlds after owner review. Normal play loads a baked world without rerunning generation.

## Depth-based composition

Living density is deliberately non-uniform.

### Shallow / highest areas

- densest vegetation and coral;
- tall broad seaweed and sea-grass patches;
- mixed branching/rounded/tube/fan coral gardens;
- strongest warm/pink/orange living accents;
- more small ambient reef fish if enabled;
- small details around foreground rock/coral bases;
- still preserve clear sandy lanes.

### Upper / middle depths

- rich but structured reef clusters;
- moderate vegetation;
- stronger balance of coral with caves/rock;
- visible negative water and navigation corridors;
- details concentrated around close/midground focal clusters.

### Lower / deep areas

- increasingly sparse vegetation;
- little tall seaweed;
- isolated coral/details rather than continuous gardens;
- broader exposed terrain/rock;
- larger cave mouths, drop-offs and silhouette-driven geology;
- quieter ambient fish population;
- preserve P2 cooler/dimmer/shorter-visibility treatment instead of compensating with bright clutter.

This is a stylized visual-density gradient, not a biological depth simulation.

## Underwater visibility and discovery refinement

**Owner-selected 2026-11-10; not yet implemented or qualified.** The [visibility contract](../../systems/underwater-visibility.md) requires distant reef geometry and opposite playable map boundaries to disappear smoothly into water rather than exposing the whole world at spawn. Keep nearby coral, fish, terrain, hazards, cave mouths and swim lanes recognizable. Midground forms soften and distant geology becomes a silhouette before merging with water as the fish moves; avoid a sharp fog wall or sudden object pop-in.

Preserve the accepted P2 Forward+ depth response, water surface, shafts, caustics, movement-reactive effects and colorful FAF art. Tune visibility shorter at deeper camera elevations, with an initial **middle-depth useful range around 25–30 m** and separate shallow/deep targets (not fixed or validated cutoffs). Shared distance extinction should be calibrated before considering small local cave/tunnel fog regions. The existing P3 execution prompts were authored before this refinement; coordinate it as a separate bounded visibility correction at a safe runner checkpoint rather than silently rewriting an in-flight P3 stack.

## Dressing and repetition rules

Richness comes from curated clusters and layered composition.

- Mix silhouette families within clusters while leaving quiet sand/rock.
- Avoid even spacing, random scatter, repeated grids or identical crown orientations.
- Use bounded rotation/scale variation.
- Use details at authored bases and foreground pockets; follow the detail-pack guidance of sparse use rather than carpeting the floor.
- Break terrain seams with rocks, coral, plants, rubble/pebbles/shells/sponges and overlapping aprons.
- Keep coral saturation concentrated on living focal areas.
- Preserve broad swim lanes, cave mouths and camera sightlines.
- Use plant UV.y height masks for optional cheap visual sway; roots remain fixed and physics-free.
- Prefer deterministic generated cluster transforms and reusable scene/MultiMesh grouping where useful.
- Never place normal world assets from random XYZ alone; use terrain suitability, real surface projection, route exclusions and support tests.

## Ambient life

Character selection is separate from ambient population. If used, FAF-FISH-001–004 remain presentation only:

- deterministic local loops/schools;
- more visible in shallow/upper zones;
- occasional middle-depth groups;
- quieter deep areas;
- no collision, prey, predator, feeding, health, event or save state;
- current player identity unchanged.

## Performance

P3 may be visually dense, but density is measured rather than assumed.

- Decorative plants/coral/details normally have no expensive collision.
- Prefer shared meshes/materials and instancing for repeated static assets.
- Measure at least dense shallow reef, cave/tunnel cluster and deep sparse view.
- Tune repeated density, visibility and ambience before dropping required variants.
- Do not infer a whole-game production budget from this phase.

## Finish line

P3 is complete when the owner can swim the full accepted five-depth world and judge that:

- it no longer reads as a technical basin or terrace stack;
- FAF terrain/rocks create natural shelves, caves, tunnels, corridors and landmarks;
- shallow/high areas visibly carry more vegetation and living density than lower/deep areas;
- all 38 core environment variants are coherently integrated;
- details make close/midground reef bases richer without noise;
- open swim routes remain obvious at nearby playable distances while distant map bounds and landmarks remain concealed until approached (visibility refinement pending native qualification);
- the live Forward+ view improves materially on style-proof 001 and belongs beside the FAF reef target;
- seams/repetition are not immediately obvious;
- representative dense scenes remain acceptably smooth;
- P2 movement/depth/water authority remains unchanged;
- Character Select presents all five FAF fish with animated previews, starts play with the selected visual, and Pause → Return to Character Select cleanly returns without death/save/history semantics.

## Deferrals

Procedural world generation is now **in scope** through the finite compiler/bake architecture. Still deferred: infinite streaming terrain, voxel/destructible terrain, runtime erosion/cave carving, procedural ecology, feeding/predator AI, species stats, growth, save/resume, death/history, first-person implementation, new movement physics, new core FAF family production, production audio pass and final whole-game biome/content scale. Character Select remains session routing/presentation only.

The next gameplay phase after successful P3 closeout is feeding/predators/growth.
