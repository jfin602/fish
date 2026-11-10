# P3 — Rebuild and dress the reef with FAF assets

**Decision date:** 2026-11-10  
**State:** Owner-selected and planned; implementation/evidence NOT STARTED.  
**Execution:** [docs/tasks/p3](../../tasks/p3/README.md) only after P2 is actually closed and the deliberate root `0.3.0` Phase-entry baseline is prepared.

## Outcome

Rebuild the current varying-depth P2 world into the first **style-proving Fish reef** using the completed Fish Asset Factory library.

P3 does not invent a new navigation topology from scratch. It preserves the accepted P2 five-depth route/water-column skeleton, Forward+ atmosphere, movement and camera, then replaces the visible technical/test-world language with authored low-poly terrain, geology, caves, tunnels, coral gardens, vegetation, reef-floor detail and bounded ambient life.

The target is the authoritative [FAF art style](../../fish-asset-factory/art-style.md) and [reef target](../../fish-asset-factory/references/reef-style-target.png). The isolated [FAF style proof 001](../../fish-asset-factory/style-proof-001/README.md) is a useful composition baseline and warning record: its stacked boulder arch, visible terrain apron seams, sparse reef layering and diorama perimeter are **not** the live-world finish to copy.

## Entry boundary

P3 may be authored while P2 remains open, but **must not execute until P2 is GREEN**.

Before P3 execution:

1. P2 G2.1–G2.3 must be closed with the final accepted source/evidence. Current authoring-time root remains `0.2.6`; P3 does not waive V20.
2. Preserve Forward+, the owner-green water/light/depth identity, water reaction, accepted P1 movement/camera/controls and the P1 fixture.
3. Deliberately transition root `package.json` to **`0.3.0`**. P3 prompts then target `0.3.1` through `0.3.7`.
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

### Ambient fish

FAF-FISH-001–004 may provide presentation-only reef life. FAF-FISH-005 Blacktip Reef Shark remains reserved for later ecology/predator work unless the owner explicitly changes that boundary.

## Preserve the depth skeleton, rebuild the visible world

The accepted P2 depth structure remains the navigation skeleton:

- shallow / sun shelf;
- upper reef;
- middle shelf;
- lower reef;
- deep shelf;
- primary/alternate shallow-to-deep return;
- accepted tunnel/shelter route identities.

The accepted analytic seabed/collision query may remain hidden underneath if it is still the safest route/depth authority. The visible world should be rebuilt with FAF terrain chunks and geology while preserving honest collision clearance.

Use the terrain pack as authored overlapping macro surfaces, not as a tile grid:

- A open sand;
- B rolling seabed;
- C raised reef shelf;
- D slope transition;
- E drop-off;
- F rocky reef floor.

Rotate/offset/overlap them and dress contacts. Avoid visible patch aprons, parallel shelf edges, concentric bands, high test-room rims, perched mounds and Z-fighting.

## Geological structure

P3 must turn the depth world into memorable playable geology.

Target at least:

- two distinct swim-through tunnel/arch routes;
- one cave/sheltered chamber large enough to enter, turn and leave;
- one deeper canyon/rock corridor;
- several overhang/shelf transitions;
- major landmark formations readable from multiple routes;
- alternate passages that reconnect rather than becoming decorative traps.

Use terrain shelves/drop-offs as major structural mass and FAF rocks as formation/edge/seam language. **Do not recreate the weak three-boulder lintel from style-proof 001 as the main landmark.** Large formations should read geologically continuous from gameplay scale.

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
- Prefer deterministic authored transforms and reusable scene/MultiMesh grouping where useful.

## Ambient life

If used, FAF-FISH-001–004 remain presentation only:

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
- open swim routes remain obvious;
- the live Forward+ view improves materially on style-proof 001 and belongs beside the FAF reef target;
- seams/repetition are not immediately obvious;
- representative dense scenes remain acceptably smooth;
- P2 movement/depth/water authority remains unchanged.

## Deferrals

No feeding, prey/predator AI, growth, save/resume, death/history, first-person implementation, new movement physics, procedural world generation, new core FAF family production, production audio pass or final whole-game biome/content scale.

The next gameplay phase after successful P3 closeout is feeding/predators/growth.
