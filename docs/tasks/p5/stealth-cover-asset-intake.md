# P5 — New Pond stealth-cover expansions and natural encounter areas

**Owner input (2026-11-12):** FAF Run 1 — Pond Vegetation Expansion; FAF Run 2 — Pond Large Wood & Log Expansion. These are detailed **creation briefs**, not a delivery/qualification report. A GitHub tree check at `d7f7b156` found only the earlier original [Pond foliage pack 001](../../fish-asset-factory/pond-foliage-pack-001/README.md) and [wood pack 001](../../fish-asset-factory/pond-wood-pack-001/README.md), not completed **new** expansion manifests/GLBs. Check actual local/unpushed producer work at P2 execution; never guess IDs or paths.

**P2 execution update:** Both later producer packs are present and technically verified at pre-task HEAD `eaad625`; [the P2 intake and fixture record](qualification/p2-cover.md) gives actual IDs, manifest hashes, Godot import checks, scene implementation and native observation limits. The earlier tree check above remains historical, not the current asset state.

## Intake and ownership

The independent FAF creation runs own Blender MCP, source-controlled deterministic generators, editable Blender files, new Pond-tagged unique IDs, individual GLBs, manifest/hash/seed records, collision wrappers and isolated Blender/Godot proofs. **P5 imports real completed outputs only** into the standalone Pike Encounter; it must not generate, rename, overwrite or repair producer art, merge conflicting shared registries uncoordinated, re-bake normal Pond, or touch Ocean. P2 records exact actual family IDs, sources/GLB/hash, dimensions, structural variants, rooted pivots, triangles/materials, wrapper/collision, depth role and visual/technical review status.

**Missing/unverified required families or structural variations = P2 BLOCKED.** Do not set `0.5.2` or proceed to P3 by substituting original pack 001 or placeholder models. Source-file existence does not automatically mean Blender/Godot native proof passed. Original Pond foliage/wood may serve as low-key supplementary background only.

## Five new aquatic plant families (soft visual concealment)

| Family | Structural variation | Natural cover role |
| --- | --- | --- |
| **Tall ribbon grass / eelgrass** | True short 0.5–1m, medium 1–2m, tall 2–3.5+m uneven blade composition; near-surface only if supported by water depth | Shallow nursery, penetrable fields, visible risky field edges |
| **Thick branching waterweed** | Narrow column, spreading bush, broad dense branching silhouette | Midwater sightline screen with visible gaps |
| **Bushy coontail / hornwort** | Compact, tall and horizontal irregular dense bottle-brush clumps | Strong small-fish shelter without a solid foliage wall |
| **Broad-leaved pondweed** | Open layered lance/oval leaves at varied stem height | Broken sightlines at bank/log/feeding transition |
| **Submerged rooted thicket** | Thick forked stalks, larger foliage masses and real rooted interior gaps | Hide-behind, peek-through and hide-inside brush near logs/roots |

Require **at least primary plus a structurally distinct secondary for each** vegetation family, with multiple actual ribbon-grass height bands. No uniform-scale/color variant counted as distinct. Keep planted pivots fixed in mud, Pond low-poly opaque cheap materials, foliage physically passable and only sparse thick-stem/root collision when genuinely structural. Avoid individual leaf colliders and invisible hiding walls.

## Five new major wood families (hard cover and real routes)

| Family | Stealth / collision function |
| --- | --- |
| **Long Fallen Tree** | Long grounded tapered trunk: sight barrier while tracking length; exposed ends require a risk decision |
| **Forked Fallen Log** | Asymmetric Y/V major arms: through fork, around limbs, under/over if genuinely open |
| **Root-Ball Tree** | Exposed tangled roots: small-fish refuge, ambush angles, meaningful limited openings |
| **Split / Hollow Decayed Log** | Real open decayed interior plus broken side entries, at least two meaningful access directions; no false dark hollow or sealed convex hull |
| **Sunken Branch Crown** | Large layered branching 3D maze: lateral/vertical cover and route options, bounded follow-camera clearance |

Each declared opening is classified **passage / limited refuge / visual-only** only after actual Bluegill/Pike/camera clearance. Hard visual obstruction must be backed by matching actual geometry; decorative non-colliding twigs are not impassable physics blockers. Collision must not float past broken bark, cap a hollow or seal apparent passages. Root/wood contact grounded on real mud/bank slopes.

## P2 natural three-zone encounter composition

The encounter uses **every new family meaningfully**, not as ten display pieces:

1. **Shallow nursery:** irregular ribbon grass short/medium and selected near-surface tall stands, broad-leaved pondweed fringe, long fallen tree beside bank. Safe Bluegill start and one exposed lane with escape options.
2. **Intermediate refuge:** dense branching waterweed, coontail/hornwort and rooted thicket, with forked fallen log and root-ball tree for hard+soft mixed concealment, routes around/through branches and plausible Pike investigation.
3. **Deeper escape maze:** split/hollow decayed log with side+end access and sunken branch crown allowing vertical/lateral escape. Reduce plants where actual depth/light demand; retain fish and camera readability.

Use real depth-to-shore, bed slope, sediment, sun clarity and ground-contact pivots; uneven seeded clusters/density with tapered edges, structural variants, plausible grass height vs waterline, negative swim lanes and clear **cover → exposed feeding-risk crossing → cover**. Reject grid stamping, repeated identical orientation, floating roots, heavily buried foliage, green opaque walls and artificial safe circles. Size fixture around real asset dimensions and fish/Pike clearance; do not change normal baked Pond/Ocean.

## Sensing, acceptance and evidence

**Hard cover** blocks real LoS and physical passage only where visible solid geometry supports it; Pike retains finite last-sensed memory. **Soft plant cover** only reduces sight based on actually visible foliage, density/angle, movement, range and murk; no automatic invisible invulnerability or binary named “hiding zone.” Approximation volumes, if used, must match and stay bounded by the real plant placements.

P2 focused checks: all 10 new families/structural variants imported, matched manifest IDs, seeded three-zone placement, waterline-height/root support, open-gap classifications, fish/Pike/camera traversal, LoS routes, and bounded geometry/overdraw cost. P3–P5 perception/AI tests compare plant density vs solid wood vs open water.

P6/P8/P11/P13 manual owner reviews: swim shallow→mid→deep with real FishController and FollowCamera. Test vegetation from side/above, new fork/root/hollow and crown multi-direction passages, natural visual blending, root grounding, actual line-of-sight, camera snags, predator search, overdraw and native frame pacing. Record source/seed/manifest hashes/Godot/renderer/owner judgment, captures only if actually made and honest PASS/FAIL/NOT OBSERVED. Do not claim FAF creation, P4 GREEN or normal-world vegetation integration.
