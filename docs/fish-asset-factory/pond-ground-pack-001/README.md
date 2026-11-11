# Pond FAF ground-dressing pack 001

**State:** eight Pond-only asset candidates delivered; technical source/GLB/Godot checks passed; isolated Forward+ visual review observed. Owner style lock and actual generated-world placement remain open. No Ocean, live Pond, World Select, or player behavior was changed.

Review date: 2026-11-11. Native captures were taken from the working tree based on `ae18fbe` after the final shared-material binding and slope placement. Blender 5.2.3 LTS and Godot 4.7.2 .NET were used.

The owner-supplied `/home/jfin/best-pond-faf.png` informed the dark faceted earth, modest algae and leaf accents, filtered light, and quiet routes. This local reference is not redistributed by this pack. The manifest is [`assets/faf/pond/ground/pond_ground_001_manifest.json`](../../../assets/faf/pond/ground/pond_ground_001_manifest.json). Each of its eight records contains seed, recipe, exact source/GLB path, placement zone, support footprint, collision rule, dimensions, triangles, materials and review states.

| Asset | Form | Dimensions X × Y × Z (m, Blender) | Triangles | Materials |
| --- | --- | ---: | ---: | ---: |
| FAF-TERRAIN-002A | undercut eroded bank | 3.549 × 1.416 × 0.543 | 62 | 2 |
| FAF-TERRAIN-002B | scarred eroded bank | 2.818 × 1.859 × 0.707 | 80 | 3 |
| FAF-DETAIL-005A | low algae stone | 1.556 × 1.256 × 0.330 | 30 | 3 |
| FAF-DETAIL-005B | upright algae stone | 0.715 × 0.894 × 0.550 | 30 | 3 |
| FAF-DETAIL-006A | scattered pebbles | 1.027 × 0.489 × 0.075 | 90 | 2 |
| FAF-DETAIL-006B | pebble chain | 1.325 × 0.411 × 0.062 | 126 | 2 |
| FAF-DETAIL-007A | broad leaf litter | 1.034 × 0.709 × 0.055 | 24 | 2 |
| FAF-DETAIL-007B | leaf windrow | 1.343 × 0.377 × 0.055 | 36 | 2 |

All object transforms are identity, +Z up in Blender, with ground-contact pivots. The GLB conversion yields +Y up in Godot. `faf_habitat=pond`, full variant ID, seed and generator metadata are in each GLB. Four named palette roles—silt, mud, wet bank and algae stone—have reusable Godot [`material resources`](../../../game/assets/faf/pond/materials/). The proof binds those shared resources to the GLB surfaces at instancing time (68 surface bindings), while the GLBs retain their standalone named materials. The [`basin shader`](../../../game/assets/faf/pond/materials/pond_basin_variation.gdshader) changes silt/mud colour over roughly 30–40 m forms; it uses no image texture or floor tiles. Its parameters are demonstrated only in the isolated fixture, and are not installed in the live generated basin.

## Construction and placement

[`build_pack_001.py`](../../../tools/asset-factory/blender/ground_dressing/build_pack_001.py) is the source-controlled deterministic recipe. It has separate bank wedge, faceted stone, pebble-group and folded-leaf builders. Each variant has a fixed local seed and its own editable `.blend` master. The paired bank shoulders differ in footprint, profile and scar structure. Stone variants differ in mass and height; pebble/leaf pairs differ in count, spacing and arrangement. The two bank [`Godot wrappers`](../../../game/scenes/faf_pond_ground/) supply three conservative stepped collision boxes each. Pebbles, leaves and stones have no collision in the proof.

The [`placement proof`](../../../game/scenes/faf_pond_ground_proof.tscn) builds one continuous basin mesh and matching collision as its only floor. The placement sampler reads the exact finished terrain triangle heights and normals. It checks the centre and four footprint corners, rejects spans exceeding 0.70 m for banks or 0.27 m for small details, and rejects normals below 0.67/0.82 Y respectively. Bank pieces embed 0.10 m and small details 0.018 m, with tilt capped at 0.16 radian. Both hero-log mouths have a 3.7 m plus asset-radius exclusion; the fish route has 2.4 m plus asset-radius clearance. Two deliberately bad placements were rejected by those zones. Normal Pond integration should replace the fixture's protected-zone coordinates with actual `GeneratedWorldRoutes` and entrance data, and query the compiled terrain surface after the basin and geology finish. Use seeded, bounded scale/yaw clusters near banks, wood and stones; retain quiet patches. This contract does not scatter into normal play yet.

## Checks and native review

- Blender 5.2.3 generated eight sources/GLBs. A second clean fixed-seed build produced byte-identical GLBs for all eight.
- [`check_pack_001.py`](../../../tools/asset-factory/blender/ground_dressing/check_pack_001.py) passed 8/8: unique IDs, Pond tags, materials, per-GLB triangle identity, and ground-contact origins. The eight assets total **478 source triangles**.
- Godot 4.7.2 .NET imported all eight GLBs and four shared materials. [`faf_pond_ground_check.gd`](../../../game/dev/faf_pond_ground_check.gd) matched all eight imported triangle/material counts to the manifest and found positive AABBs.
- The native AMD Radeon 880M Forward+ fixture placed **29 instances**, including six on the rising bank, with **0 support failures**, **2 expected route/entrance rejections** and **1,692 instanced detail triangles**. The one-piece basin is 1,914 triangles. In the cluster capture, Godot reported 148 draw calls, 183 rendered objects and 16,906 rendered primitives for the **whole fixture**, including the existing hero log, shadows, water and environment; these are not pack-only costs.

Observed views: [above](evidence/pond_ground_above.png), [underwater](evidence/pond_ground_underwater.png), [actual Fish camera node](evidence/pond_ground_fish.png), [bank pair](evidence/pond_ground_banks.png), [detail group](evidence/pond_ground_details.png), [mixed cluster](evidence/pond_ground_cluster.png), [rising slope](evidence/pond_ground_slope.png). The fish-camera view uses the real `Fish/CameraRig/Camera3D` node and FOV in a frozen review pose; it does not drive the controller. The small pieces sit against the sampled surface, the bank pieces no longer read as floating slabs, the log mouths and open lane remain clear, and no tiled floor/grid pattern appears. The colour transitions are broad and quiet. No clear z-fighting or deep burial appears in these captures.

**Visual limits:** leaf and pebble shapes become subtle at fish-camera distance, which is appropriate for supporting dressing but weak for lone placement. The isolated basin is intentionally simple and its straight outer fixture boundary is visible in wide views; this is not a Pond world shoreline. The broad silt/mud variation is readable mainly in overview/light changes and needs review under final P5 Pond atmosphere. The bank wrappers were not traversed with the controller, and no live-basin generated placement or long-run density/performance test occurred. These are candidate assets, not completion of the Pond world.
