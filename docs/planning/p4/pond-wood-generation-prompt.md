# Pond FAF wood and cover generation prompt

**Use:** Standalone implementation prompt for the Pond's first woody-cover pack. It creates original FAF assets and an isolated traversal proof, not the full Pond world or a P4 Gate result. Work in a separate checkout if the fish, creature or foliage packs are running concurrently; coordinate shared FAF ID and helper changes before merging.

## Prompt

Create an original **Pond Fish Asset Factory wood-and-cover pack** for Fish using the owner-supplied `/home/jfin/best-pond-faf.png` as the visual and composition reference. Deliver **three fallen-log shapes, three hollow-trunk/opening variants, two trunk-or-stump forms, and four stick/branch/root clusters**: **12 distinct reusable base assets**. Make editable Blender sources, individual clean GLBs, simple Godot collision wrappers for solid cover, a manifest, focused checks and an isolated native Forward+ traversal scene. The focal piece is a large, naturally grounded hollow log that the current fish and follow camera can actually swim through in both directions.

### Preflight and identity

1. Read `AGENTS.md`, `BOOT.md`, `docs/workflow.md`, `docs/planning/p4/pond-faf-assets.md`, and the focused FAF art-style, asset-ID, modeling, budget, Blender–Godot handoff and visual-review contracts. Inspect the existing rock/terrain families, current fish collision and camera envelope, and P3 route/collision lessons before choosing dimensions. Inspect Git HEAD and working-tree changes; preserve the Ocean asset library and ongoing P3 work.
2. Match the reference's faceted, dark submerged wood, broken warm cut faces, roots and occasional algae/moss accents. Make original geometry and compact reusable materials; avoid photographic bark, tiny modeled noise and a single repeated log rotated everywhere. Record reference/provenance without assuming the image may be redistributed in the public repository.
3. Reserve new stable FAF `WOOD` IDs after checking the live registry. If `WOOD` is not an accepted category yet, add it deliberately and additively to the naming/manifest contract. Tag the pack for Pond, with each asset's role, placement zone, contact pivot, intended orientation, allowed scale range and collision requirement. Do not infer habitat from names. Keep Pond source, GLBs, wrappers and proof scene in their own paths. Coordinate any shared registry/helper edit with concurrent asset packs; never work in the same checkout as another generator task.

### Build the 12 assets

| Family | Required variants | Shape and placement target |
| --- | ---: | --- |
| Fallen logs | 3 | Different long, bent/forked and broken silhouettes with asymmetric ends and grounded underside; usable as bankside structure and submerged cover. |
| Hollow trunks | 3 | Genuine open interior geometry with distinct straight, curved and root-flared/broken forms. At least one hero piece is a broad through-route; the others provide legible open shelter or passages sized and labeled against the current fish envelope. |
| Trunks/stumps | 2 | One rooted upright or leaning trunk and one low cut/broken stump with a convincing root flare/contact footprint; support shoreline or submerged placement. |
| Stick/branch/root clusters | 4 | Distinct sparse sticks, forked branches, tangled roots and heavier branch cover. Vary silhouette, height and negative space; do not make four recolors of one cluster. |

Use broad low-poly planes and visible wood grain direction through geometry/color regions, not dense texture detail. Model in metres with stable pivots at useful ground/contact points, applied transforms and clean normals. Broken ends, bark, exposed interior wood and moss/algae may use a small shared material palette. Give the hero trunk an irregular but continuous floor, side walls and roof; the opening must remain visually and physically open. Avoid a uniform cylindrical pipe, a dark tube simply laid on top of the terrain, abrupt floating ends and roots that appear to hang without support.

### Collision and swim-through contract

- Export display meshes separately from authored simplified collision. For the hero log, assemble collision around the rim, walls, roof and floor **without a single convex hull sealing the opening**. Keep collision near the visible surfaces and leave usable fish and camera clearance across the full route, including entrance/exit bends. Do not turn decorative twig detail into dense collision.
- Every visible hollow opening must agree with its collision: no invisible cap, blocked entrance or solid-looking gap that the fish can pass through. If a smaller opening is intentionally only a refuge, record its clearance and make its intended use visually clear. Check current player collider and follow-camera dimensions from source; do not invent a passable width from a render.
- In the isolated native proof, approach the hero opening from both ends using the real fish controller and follow camera. Record snags, floor/ceiling clipping, camera obstruction or snapping, depth/visibility and successful exit. Also test representative side contact and simple route clearance through the smaller covers. A headless geometry check alone does not prove a comfortable swim-through.
- Use terrain contact points and orientation metadata so placement can embed logs and roots into the future Pond bank. Check support along long spans and at both ends; no floating bridges or buried openings unless explicitly designed as such.

### Export, proof and validation

1. Save first-party `.blend` masters and reproducible generator/recipe code where generation is used. Export **one GLB per variant**, excluding review lights/cameras and unused construction objects. Build separate Godot scene wrappers for any collision-bearing piece. Record FAF ID, family/variant, dimensions, exported triangles/materials, pivot, palette, source/export/wrapper paths, collision method, clearance, placement zone and technical/style review state in a manifest. Measure actual output rather than guessing counts.
2. Create an **isolated** Forward+ proof with a sloped muddy bank, water surface, silt floor and representative olive-water light. Arrange all 12 assets individually and in mixed clusters with rotations and bounded scale variation. Include close, overhead, entrance, interior, exit and actual follow-camera views. Show how one kit can produce abundant cover without obvious repetition or blocked open lanes. Do not modify normal play, Ocean assets, World Select or the shared world compiler in this asset task.
3. Add focused checks for exact inventory, unique Pond-tagged IDs, GLB imports, dimensions/transforms, material count, collision-wrapper presence and visible/collider opening agreement. For a deterministic generator, verify fixed-seed output. Check representative density and draw/triangle cost in native Godot; small sticks can remain noncolliding decoration when their role does not require collision.
4. Capture Blender silhouette/family views and native Godot mixed-density and two-direction swim-through evidence. Record source revision, Blender/Godot versions, player/camera envelope used, measured costs, observed failures, corrections and any remaining limits. Keep technical import, native route and owner style judgments separate.

### Done condition

The pack is technically complete when all **12 distinct assets** have editable sources, correct GLBs and manifest identities; collision-bearing wrappers have simple, aligned shapes; and the hero hollow trunk supports an observed two-direction fish-and-camera swim-through without snags or invisible blockage. The full family must read as varied, grounded natural wood at Pond lighting and gameplay distance. Report passed, failed and unobserved checks honestly. Isolated asset proof does not complete a P4 world Gate.
