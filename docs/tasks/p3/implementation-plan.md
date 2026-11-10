# P3 implementation plan — FAF reef world rebuild

## Entry and routing

Read `AGENTS.md`, `BOOT.md`, workflow/runner docs, this task README/assessment, [P3 design](../../planning/p3/reef-world-rebuild.md), FAF art style/index, terrain/rock/coral/plant/detail/fish candidate records, [style proof 001](../../fish-asset-factory/style-proof-001/README.md), final P2 closeout/depth evidence, `MODULES.md`, and affected source only.

At execution inspect branch/HEAD/dirty state, root version, accepted P2 normal-play scene, depth/collision authority, route markers, P2 water/light/depth resources, FAF manifests/GLBs and `tools/dev` checks. Require P2 GREEN and root `0.3.0`.

## P1 — FAF runtime integration

Verify and expose all 38 environment variants through the smallest stable normal-play catalog/wrapper boundary:

- terrain A–F;
- rock A–D;
- coral 001–004 A/B/C;
- plant 001–003 variants;
- detail 001–004 A/B.

Do not rename/re-export valid GLBs or create a generic asset framework. Preserve canonical FAF IDs, source paths and import orientation/pivots. Static decorative assets get no gameplay collision by default.

Prepare optional plant UV.y sway material hooks, but no physics. Add/extend one focused runtime-load check proving all 38 IDs resolve through the P3 path.

Assigned version `0.3.1`.

## P2 — Depth-world visual rebuild

Preserve the accepted P2 five-depth navigation/water-column skeleton. Rebuild the visible seabed/ledges with FAF-TERRAIN-001A–F.

Preferred if final P2 still uses it:

- retain accepted continuous `SeabedSurface` / `GetHeightAt()` as hidden collision/depth-query authority;
- hide/subordinate the technical surface;
- overlap A/B/C/D/E/F as visible authored geometry;
- rotate/offset/scale within sensible bounds;
- keep visible path surfaces close enough to collision to remain honest.

Use A/B for quiet sand/rolls, C shelves, D transitions, E drop-offs and F rocky floor. Break long terrace lines, rings and perimeter reads. Create authored seam zones for P3/P4 dressing.

Assigned version `0.3.2`.

## P3 — Caves, tunnels and geology

Use FAF terrain structural pieces plus FAF-ROCK-001A–D to build large-scale geology.

Target:

- two distinct swim-through routes;
- one cave/shelter chamber;
- one deep canyon/corridor;
- overhang/shelf transitions;
- major landmarks readable from multiple routes;
- reconnecting alternates.

Use terrain shelf/drop-off pieces as large structural masses so formations read continuous. Rocks provide facets, edges, filler and landmark variation. **Do not copy style-proof 001's weak simple boulder-lintel arch.**

Simplified collision exists only where geology constrains navigation. Validate fish/camera clearance, cave turn radius, obstruction recovery and no world escape.

Assigned version `0.3.3`.

## P4 — Depth-based reef dressing

Integrate all twelve coral, all eight plants and all eight details with deterministic authored clusters.

Depth treatment:

- shallow/high: densest plants/coral/details, strongest warm accents;
- upper: lush but open-lane aware;
- middle: moderate living density balanced with geology;
- lower: sparse plants, isolated coral/details;
- deep: minimal tall vegetation, broad rock/terrain negative space and small accents.

Use details around rock/coral bases, contact seams and close foreground pockets; keep open sand quiet. Avoid uniform scatter.

Dress terrain/rock seams without masking cave mouths or routes. Implement gentle plant sway from UV.y only if useful; roots fixed, no physics.

Use shared instances/MultiMesh where it materially reduces cost without making placement nondeterministic.

Assigned version `0.3.4`.

## P5 — Ambient reef life and finish

Use FAF-FISH-001–004 for bounded presentation-only schools/local loops when practical:

- more visible in shallow/upper;
- occasional middle groups;
- quieter deep zones;
- no collision/ecology/event/persistence state;
- current player unchanged.

Reserve FAF-FISH-005 shark for later ecology.

Perform a full repetition/intersection/color cleanup: break identical clusters, fix floating/buried assets, improve foreground/midground/background layering, preserve landmarks/routes, remove visible transitional third-party art where FAF now replaces its role, and tune colors under actual P2 depth lighting.

Assigned version `0.3.5`.

## P6 — Native style and performance tuning

Mandatory native owner checkpoint.

Swim shallow→upper→middle→lower→deep→return, alternate route, both tunnel routes, cave chamber and deep corridor.

Compare actual gameplay views against the FAF reef target and style-proof 001. The live world should materially improve on the proof's weak arch, visible seams, sparse layering and finite-diorama read.

Review:

- all 38 environment variants as real composition;
- shallow→deep density gradient;
- route/cave readability;
- faceting/palette/layering;
- plant sway/ambient fish if implemented;
- P2 water/depth identity;
- dense shallow, cave/middle and deep-sparse performance.

Write `qualification/native-world-art.md` and `qualification/accepted-world-art.md`. Owner must explicitly accept the world-art direction.

Assigned version `0.3.6`.

## P7 — Final closeout

Map G3.1–G3.3 against P1–P6 evidence. Confirm P2 remains valid, all required variants participate, caves/tunnels work, shallow/high density visibly exceeds lower/deep, target style is owner-accepted and representative performance applies to the accepted candidate.

Reconcile roadmap/BOOT/task/art/module docs once. Route successful P3 to feeding/escape/growth.

Assigned closeout version `0.3.7`.

## Hard boundaries

No new core FAF family generation by default; no third-party asset hunting; no ecology/feeding/predator/growth behavior; no save/death/history; no renderer change; no motor/camera authority change; no procedural world generation.

Preserve the P1 fixture, accepted controls/presets, Forward+, final P2 water/light/depth and water-reaction identity.
