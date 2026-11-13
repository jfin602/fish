# Pond stealth vegetation pack 001 — FAF-PLANT-010–014

**Isolated asset candidate, 2026-11-12.** Five original underwater foliage families add soft visual cover to Pond. They are not in normal Pond generation or Ocean, and they do not implement detection, sway, disturbance, or a hidden/safe state. The [owner-supplied Pond style target](../references/pond/pond-style-target.png), [Pond reference set](../references/pond/README.md), and existing [foliage](../pond-foliage-pack-001/README.md), [wood](../pond-wood-pack-001/README.md), and [ground dressing](../pond-ground-pack-001/README.md) informed the forms. The new ribbon grass is taller, narrower, and rooted underwater; the older 005 bank grass remains a separate asset.

## Source, identity, and reproduction

- [Editable Blender master](../../../assets/faf/pond/plants/pond_stealth_vegetation_001.blend) has a source collection for each variant and a separate side-by-side review collection. Every source component has identity transform at its mud root.
- [Five deterministic generators](../../../tools/asset-factory/blender/plants/pond_stealth_vegetation.py) expose height, spread, blade/stalk/branch/whorl/leaf count, lean or horizontal bias, and seed as appropriate. Shared faceted stalk/leaf primitives do not turn the families into shape/color swaps. [The build recipe](../../../tools/asset-factory/blender/plants/build_pond_stealth_vegetation_001.py) fixes thirteen structural configurations and exports one GLB per variant. Run `blender -b --python tools/asset-factory/blender/plants/build_pond_stealth_vegetation_001.py -- --root "$PWD"`; an existing pack requires deliberate `--overwrite`.
- [The manifest](../../../assets/faf/pond/plants/pond_stealth_vegetation_001_manifest.json) records exact specs/seeds, Pond IDs, dimensions, source and output paths, costs, depth role, mud anchor, and technical/style candidate states. Each GLB mesh-node extra has `faf_id`, `faf_habitat=pond`, family, seed, and anchor. The exported meshes use opaque materials without textures or normal maps.
- Blender +Z is up and Godot +Y is up. `UV.y` increases from fixed root to flexible tips. Future presentation sway should leave roots and the thick thicket stalks essentially fixed; leaf tips and grass blades may move. There is **no vegetation collision** in these GLBs. The thicket's thick stalks are visual structure here, not an invisible wall.

The first generation and family-by-family review ran in the healthy repository-configured Blender MCP session (Blender 5.2.3 LTS, socket port 9876). The final deliverables were rebuilt with the same source-controlled recipe in clean background Blender to exclude MCP addon scene metadata and provide canonical mesh names. A second clean build produced thirteen byte-identical GLBs (SHA-256 comparison).

## Inventory and measured export cost

Dimensions are Blender X × Y × Z metres; the last number is vertical height. One line represents one individual GLB and editable source collection. All are **soft concealment with no hard collision**. Technical state for every line: Blender GLB structure **PASS**, Godot 4.7.2 import/identity/bounds/cost **PASS**, isolated Forward+ visual candidate **REVIEWED**; owner style approval and future stealth-system efficacy remain open.

| FAF ID | Family and structural form | Dimensions m | Tris | Mats | Seed | Intended depth/cover role |
| --- | --- | --- | ---: | ---: | ---: | --- |
| 010A | Ribbon grass, short 18 blades | .888 × .756 × .830 | 648 | 1 | 7101 | 0.5–1 m shallows; low visual screening |
| 010B | Ribbon grass, medium 27 blades | 1.982 × 1.394 × 1.738 | 972 | 1 | 7102 | 1–2 m transition bed; broad screen with gaps |
| 010C | Ribbon grass, tall 37 blades | 3.193 × 2.694 × 3.577 | 1,332 | 1 | 7103 | 3.2–3.7 m column; near-surface grass-field cover |
| 011A | Branching waterweed, narrow upright | .787 × .744 × 1.567 | 610 | 1 | 7201 | Intermediate depth; slender mid-water interruption |
| 011B | Branching waterweed, spreading bush | 1.503 × 1.632 × 1.168 | 924 | 1 | 7202 | Intermediate depth; porous mid-water screen |
| 011C | Branching waterweed, broad mass | 2.112 × 2.301 × 1.555 | 1,424 | 1 | 7203 | Intermediate depth; stronger open soft cover |
| 012A | Coontail, compact whorls | .801 × .779 × .837 | 910 | 1 | 7301 | Shallow bottom; small-fish cover pocket |
| 012B | Coontail, tall whorls | 1.191 × 1.083 × 1.578 | 1,488 | 1 | 7302 | Intermediate depth; repeated sightline breaks |
| 012C | Coontail, horizontal growth | 3.115 × 3.256 × .906 | 1,274 | 1 | 7303 | Bottom spread; low, wide cover without a wall |
| 013A | Broad pondweed, open small form | .844 × .833 × 1.088 | 640 | 1 | 7401 | Shallow bank/log edge; broken leaf occlusion |
| 013B | Broad pondweed, taller leaf form | 1.037 × 1.094 × 1.619 | 1,148 | 1 | 7402 | Shallow/intermediate transition; peek-through leaves |
| 014A | Rooted thicket, compact rooted brush | 1.571 × 1.508 × 1.314 | 2,472 | 2 | 7501 | Near wood; small hide-behind/open interior |
| 014B | Rooted thicket, tall rooted brush | 2.270 × 2.034 × 1.879 | 4,256 | 2 | 7502 | Near wood; larger irregular visual barrier |

All GLBs live under [`game/assets/faf/pond/plants/`](../../../game/assets/faf/pond/plants/), named `plant_<number><variant>_<family>.glb`; their source, exact spec and output path are individually recorded in the manifest. One instance of every delivered variant totals **18,098 triangles**. The thicket exceeds the small-plant starting budget deliberately as an occasional structural cover piece, not a dense-scatter item.

## Blender and Forward+ review

Blender MCP family views: [ribbon grass](evidence/blender_ribbon_grass.png), [branching weed](evidence/blender_branching_waterweed.png), [coontail](evidence/blender_coontail.png), [broad pondweed](evidence/blender_broad_pondweed.png), [rooted thicket](evidence/blender_rooted_thicket.png). The views show structural A/B/C differences, not uniform scale or recolor. The tall grass bends and fans more than the old bank grass, though repeated tall stands could still look stamped without mixed yaw and spacing.

The [isolated Forward+ proof scene](../../../game/scenes/faf_pond_stealth_vegetation_proof.tscn) uses a shallow-to-intermediate mud slope, a scene-only water plane, existing-style Pond wood, a single-asset gallery, and mixed patches. It instances 32 plants (42,092 instanced source triangles and 36 material slots), with bounded 0.89–1.05 scale and about ±0.4 rad yaw variation. The `010C` bed rooted at `x=-16,z=-8` reaches about `y=3.02 m` against a `y=2.90 m` waterline; tips can meet or slightly break the surface without a scaled-up mesh. Native views: [short grass](evidence/pond_cover_grass_short.png), [medium grass](evidence/pond_cover_grass_medium.png), [tall bed](evidence/pond_cover_grass_tall.png), [near-surface grass](evidence/pond_cover_grass_surface.png), [branching waterweed](evidence/pond_cover_waterweed.png), [coontail](evidence/pond_cover_coontail.png), [broad leaves](evidence/pond_cover_pondweed.png), [thicket beside wood](evidence/pond_cover_thicket.png), [mixed cover](evidence/pond_cover_mixed.png), and [controller traversal](evidence/pond_cover_traverse.png).

Observed on AMD Radeon 880M/RADV, Godot 4.7.2 .NET Forward+: roots meet the generated mud, mixed clumps leave open water between them, and no obvious leaf/root floating or dense transparency overdraw appeared. The real Fish controller moved from `z=7` through the weed/coontail corridor to `z=-13.03`; the proof recorded **zero physical camera hits** and a 4.29 m minimum camera-rig distance. The traversal screenshot shows foliage overlapping the fish silhouette without a solid wall. Captured frame draw calls ranged from 53 in the thicket view to 157 in the gallery/controller view; the mixed close view reported 75. These are fixture-frame observations, not a production density or frame-time budget. The native proof uses its own simple water/fog and mud, so final Pond clarity, exposure readability and predator investigation have not been qualified.

The most distinct silhouettes are the tall ribbon bed, broad leaves, and rooted thicket. Branching weed offers branching openings; coontail's compact/tall/horizontal whorls make tighter near-ground cover, though at a distance the upright form can resemble waterweed. Close views show faceted, inexpensive foliage rather than needle-level realism. Broad leaves interrupt sightlines without hiding a full adult fish; dense coontail and thicket are intended for smaller fish or layered placements. A single large thicket is too costly for indiscriminate scattering.

## Verification and remaining boundary

- [Binary GLB checker](../../../tools/asset-factory/blender/plants/check_pond_stealth_vegetation_001.py): `POND_COVER_GLB_OK assets=13 triangles=18098 grass_heights=[0.83008, 1.738, 3.57745]`.
- [Godot import checker](../../../game/dev/faf_pond_stealth_vegetation_check.gd): `POND_COVER_GODOT_OK assets=13 triangles=18098`; it verifies identity, no collision nodes, components, UVs, materials, root/bounds and exact counts.
- Proof scene headless check: `POND_COVER_PROOF_READY assets=13 instances=32 instanced_triangles=42092 renderer=forward_plus`; headless real-controller traversal **PASS**, repeated in native Forward+ with screenshot. Scene is standalone; normal Pond/Ocean scenes and runtime catalogs are untouched.

Later work owns subtle sway/disturbance, P5 sensing/occlusion behavior, species/life-stage cover tuning, and depth-aware generated placement. None is inferred from the visual proof. An owner style lock and a real generated-Pond swim review are still open.
