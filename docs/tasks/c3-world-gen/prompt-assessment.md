# c3-world-gen prompt assessment

**Assessment date:** 2026-11-10  
**Type:** Correction 3 at owner-selected unchanged `0.3.6`.  
**Status:** design/prompt assessment only; no implementation evidence.

## Owner problem

The current world does not feel naturally formed:

- terrain/ground pieces visibly float or sit above the actual ground;
- placement reads as individually placed props rather than a caused environment;
- world walls/perimeter do not make physical sense;
- the world should feel like an irregular basin/pond, not a perfect circle or square room;
- the useful deepest region should be near the center;
- caves should be formed from actual geological terrain rather than roofs balanced on rock props;
- the owner accepts explicit generation waits up to roughly two minutes if good worlds can then be baked/saved and load quickly.

## Source diagnosis

The historical/current P2 foundation demonstrates why a correction is needed:

- `SeabedSurface` is an 84 × 84 m square heightfield generated from one Y value per X/Z;
- hidden containment is implemented with four rectangular side boxes plus an upper box;
- the height function was optimized for five lateral bands/route evidence rather than a radial natural basin;
- `ReefComposition` historically uses fixed authored X/Z anchors and snaps Y through `GetHeightAt()`;
- a heightfield cannot represent an enclosed cave/overhang with floor and ceiling at one X/Z;
- the original P3 plan explicitly deferred procedural generation and relied on modular terrain/rock composition.

The owner now supersedes that architecture for P3 world formation.

## Required outcome

Build a deterministic **world compiler**, not an every-load random spawner.

Pipeline:

1. seed/settings/asset catalog -> deterministic compile context;
2. irregular basin + natural perimeter;
3. true 3D geological regions;
4. protected route graph;
5. terrain/environment analysis fields;
6. habitat zones/cluster templates;
7. terrain-aware natural placement;
8. relaxation/repair and world scoring;
9. bake resources + manifest;
10. load baked world without regeneration;
11. optional explicit promotion of good candidates to curated Base Worlds.

## Preserved behavior

Do not change:

- Fish's bright stylized low-poly art style;
- FAF source asset identities;
- Forward+ renderer;
- accepted P2 water/light/depth presentation contracts;
- authoritative FishController/SwimMotor/collision/control/camera behavior;
- five-fish Character Select/session routing;
- P1 fixture/history;
- later feeding/predator/growth/persistence scope.

World generation owns environment construction only.

## Prompt decomposition

| Prompt | Responsibility | Evidence |
| --- | --- | --- |
| P1 | deterministic compiler context, seed/settings/manifest, bake/load boundary | T2 deterministic/unit/integration |
| P2 | irregular basin, center-deep topology, natural perimeter and safety containment | T2 geometry/collision |
| P3 | arbitrary 3D geology: caves/tunnels/arches/overhangs | T2 mesh/collision/clearance |
| P4 | route graph and derived environment/surface analysis maps | T2 connectivity/field checks |
| P5 | cluster/suitability placement, support/embedding, blue-noise spacing, relaxation/scoring | T2 deterministic placement |
| P6 | full compile command, phase timing, bake/reload and Base World promotion | T2 + native-prep measurements |
| P7 | owner native generated-world inspection and final correction disposition | T3 |

## Main risks

- **Building a terrain engine instead of a game feature:** explicitly exclude infinite terrain, voxel editing, runtime destruction and streaming.
- **Generation becomes nondeterministic:** use explicit seed + generator version + settings, deterministic PRNG streams and stable ordering.
- **Small generator change reshuffles everything:** derive independent seeded streams by subsystem/zone where practical.
- **Caves become decorative shells with bad collision:** require actual floor/wall/ceiling mesh, collision and route clearance.
- **Natural boundary still hides a box wall:** fail-safe collision lives outside/behind terrain and cannot be the ordinary perceived boundary.
- **Placement still looks random:** placement starts from habitat zones and suitability fields, not random XYZ.
- **Terrain accents float:** sample multiple support points/actual surface projection; reject or embed unsupported pieces.
- **Routes disappear under dressing:** route graph/exclusion volumes are created before placement.
- **Generation exceeds owner tolerance:** instrument every stage; target ≤120 s for P3-sized candidate and optimize measured hotspots.
- **Baked load secretly regenerates:** loader must be generator-independent and checks should detect generator invocation.
- **Generated assets bloat Git:** candidates remain ignored/user data; only explicit Base World promotion creates reviewable tracked output.
- **Visibility tuned to obsolete geometry:** execute c3-world-gen before c3-visibility.

## Success boundary

Source-level checks alone can establish determinism, support, topology, route connectivity and bake/load behavior. They cannot establish that a world looks naturally formed. P7 must include real native exploration and owner judgment.

A generated world that is technically deterministic but still reads as floating tiles, regular rings, artificial walls, random scatter, unusable caves or repetitive stamping is NOT GREEN.
