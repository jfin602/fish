# Pond FAF wood and cover pack 001

**Status (2026-11-11):** Twelve original Pond-tagged wood assets passed GLB and Godot 4.7.2 import checks. The hero hollow trunk passed an isolated native Forward+ two-direction traversal with the existing `FishController` and `FollowCamera`. Visuals remain candidate quality pending owner review and a richer Pond composition. This pack does not alter Ocean assets, normal play or the P3 route system.

## Inventory

Each item has its own editable `.blend`, GLB and scene wrapper under `assets/faf/pond/wood/`, `game/assets/faf/pond/wood/` and `game/scenes/faf_pond_wood/`, respectively. The [manifest](../../../assets/faf/pond/wood/pond_wood_001_manifest.json) gives exact paths, deterministic seeds, recipe parameters, dimensions, anchors, placement limits and review states. Dimensions below are Blender X/Y/Z metres. The ground-contact origin is at zero, +Z is up, and the long axis is Blender Y / Godot Z. Export transforms are identity.

| ID | Silhouette / role | Dimensions m | Triangles | Materials | Collision |
| --- | --- | ---: | ---: | ---: | --- |
| FAF-WOOD-001A | Bent fallen log / bank cover | 2.233 × 6.003 × 1.264 | 298 | 4 | Conservative cylinder |
| FAF-WOOD-001B | Forked fallen log / branching cover | 3.135 × 5.201 × 1.566 | 374 | 4 | Cylinder + two major-fork capsules |
| FAF-WOOD-001C | Thick broken log / exposed end | 2.894 × 4.505 × 1.815 | 336 | 4 | Cylinder + one major-fork capsule |
| FAF-WOOD-002A | Hero hollow / two-way passage | 7.707 × 10.220 × 5.339 | 818 | 4 | 16 wall, floor and roof slabs; no end cap |
| FAF-WOOD-002B | Curved hollow / limited refuge | 5.245 × 5.419 × 2.860 | 492 | 4 | 12 open radial slabs |
| FAF-WOOD-002C | Root-flared hollow / narrow fish passage | 5.551 × 4.989 × 3.221 | 492 | 4 | 12 open radial slabs |
| FAF-WOOD-003A | Tall rooted leaning trunk | 3.302 × 3.081 × 4.395 | 218 | 4 | Conservative cylinder |
| FAF-WOOD-003B | Low broken rooted stump | 3.857 × 3.598 × 2.198 | 218 | 4 | Conservative cylinder |
| FAF-WOOD-004A | Sparse sticks, open gaps | 4.012 × 3.204 × 1.442 | 160 | 4 | Decorative; none |
| FAF-WOOD-004B | Forked branch cover | 4.377 × 4.387 × 2.123 | 160 | 4 | Decorative; none |
| FAF-WOOD-004C | Tangled low roots | 5.344 × 4.882 × 1.301 | 224 | 4 | Decorative; none |
| FAF-WOOD-004D | Heavy branch cover | 5.599 × 5.062 × 1.741 | 160 | 4 | Decorative; none |

The [generator](../../../tools/asset-factory/blender/wood/build_pond_wood_001.py) uses four family constructions and fixed, meaningful recipes for length/height, radius, bend or lean, branch count, spread and seed. The [wrapper writer](../../../tools/asset-factory/blender/wood/write_wrappers.py) regenerates the twelve collision scenes from the manifest. Bark, warmer broken faces and restrained olive algae use one shared four-material vocabulary. No photographic maps or alpha forest is involved.

## Passage and collision qualification

The existing player collider is a sphere of radius **0.48 m**. The existing camera probes at radius **0.22 m** and follows at 3.8 m rear / 2.0 m height in open water. The hollow visual meshes have connected inner and outer walls and open annular ends. Their Godot collision is made of independent tangential boxes around the circumference, with no convex hull across the opening. Nominal collision-clear diameters are 3.943 m for 002A, 1.823 m for 002B, and 2.083 m for 002C; the exact faceted free space varies slightly around these values.

The [focused import check](../../../game/dev/faf_pond_wood_check.gd) instantiated all twelve actual GLB wrappers, compared Godot dimensions/orientation, contact pivot, triangles and material surfaces to the manifest, checked 8 collision-bearing wrappers and 4 decoration-only pieces, sampled the fish-sized centerline through all three hollows and confirmed side, roof and floor contact. All checks passed. The smaller 002B is a refuge: it has a fish-sized opening but is **not** qualified for comfortable follow-camera traversal. 002C is a narrow fish passage; its centerline clears the fish sphere, while a full native camera traversal remains unobserved.

In the isolated [Forward+ proof scene](../../../game/scenes/faf_pond_wood_proof.tscn), the hero log sits on silt and is approached from both ends. A duplicate of the **real** `Fish` subtree from `main.tscn` supplies the existing controller, sphere collider and follow camera; a scripted W key hold propels it through each direction. The second approach turns the real controller before thrust. The measured passes were `+8.2 → -5.83 m` and `-8.2 → +5.83 m` on the log axis. Both entered and exited, with zero sampled camera probe hits. Minimum camera-to-fish distances were **3.62 m** and **3.53 m**. No invisible end barrier, snag or camera snap was observed on these centred paths. The traversal fixture uses the prototype fish visual carried by `main.tscn`; this check qualifies the movement envelope, not a Pond fish skin.

## Native visual review

The proof includes all 12 in an individual gallery plus seven more in authored mixed cover, using 0.88–1.12 scale and ±0.32 rad rotation. The flat silt route stays open. Terminal bark/root contact vertices for the six logs and hollows lie between -0.12 and +0.011 m against the flat floor, so no end is floating in this fixture. There are **20 wood instances** and **6,400 instanced wood triangles** in the complete fixture, with four material surfaces per source asset. Godot's Forward+ primitive monitor reported **18,686** at the gallery capture and **19,570** at the entrance capture; those frame counters include lighting/shadows and the proof floor, so they are not a wood-only triangle count. The renderer was Vulkan Forward+ on AMD Radeon 880M.

Reviewed native evidence: [gallery](evidence/pond_wood_gallery.png), [top](evidence/pond_wood_top.png), [hero entrance](evidence/pond_wood_hero_entrance.png), [first-direction interior](evidence/pond_wood_first_inside.png), [return interior](evidence/pond_wood_return_inside.png). These are actual Godot captures. The dark faceted bark, muted olive accent and open hero silhouette fit the Pond palette. The branch clusters retain visible gaps and the large trunk is easy to identify as cover at gameplay distance. The initial hollow surface winding hid the inner wall; normals were corrected and the final captures use the corrected meshes.

**Visual weaknesses:** the hollow members still look more regularly tubular than the owner reference's broad, eroded root arch. Some branches and roots end in sharp spikes, and the four shared materials create similar bark patch rhythms in repeated instances. The isolated floor/bank lighting lacks the lily canopy and layered sediment that make the target reference rich. The large log is grounded on a flat test floor; embedding on future sloped banks and contact at both ends has not been qualified. Thin rootlets and decorative clusters have no collision and could visually overlap a fish grazing them. No owner style lock is claimed.

## Reproduction and boundaries

- Generate masters, GLBs and manifest with Blender 5.2.3: `blender -b --python tools/asset-factory/blender/wood/build_pond_wood_001.py -- --root "$PWD"`. Regeneration resets review-state fields until checks are rerun.
- Generate wrappers: `python3 tools/asset-factory/blender/wood/write_wrappers.py`.
- Validate GLBs: `python3 tools/asset-factory/blender/wood/check_pond_wood_exports.py` (12/12, 3,950 source triangles).
- Blender 5.2.3 reopened all twelve editable `.blend` masters and found one intended mesh object in each.
- Import in Godot, then run `Godot --headless --path game --script res://dev/faf_pond_wood_check.gd` (12/12) and the isolated scene with `-- --faf-traverse` for the native passage. The scene also accepts `--faf-view=gallery|top|hero_entrance|hero_inside|hero_exit` and `--faf-capture-dir=<ignored output directory>`.

The source reference was the owner-supplied local `best-pond-faf.png`, used for visual direction only. It is not redistributed as geometry or a texture. There is no normal-play hookup and no P4 world Gate claim.

At final verification, the concurrent P3 edit to `game/scripts/GeneratedBasin.cs` made the unrelated C# build fail with CS0136 (`depth` redeclared at line 183). The wood GLB/import checks and both real-controller passage runs passed using the previously compiled assembly. This pack makes no change to that P3 file; a clean whole-project build must be rerun after its owner resolves the conflict.
