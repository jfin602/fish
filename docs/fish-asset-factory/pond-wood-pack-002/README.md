# Pond large wood and cover expansion — FAF-WOOD-005–009

**Isolated asset candidate, 2026-11-12.** Five original large Pond families expand the earlier [wood pack](../pond-wood-pack-001/README.md) into longer hard cover, forked routes, root undercuts, broken hollow entrances and a layered branch crown. This pack is not wired into normal Pond or Ocean generation and does not implement predator sensing, pursuit, hiding state or other stealth logic. The [owner Pond reference](../references/pond/pond-style-target.png) informed the faceted, muddy and root-rich structure; owner style approval remains open.

## Sources, identity and reproduction

The ten editable `.blend` masters are under [`assets/faf/pond/wood/`](../../../assets/faf/pond/wood/); each master contains its own identity-transform mesh and four shared Pond bark/end/algae material roles. The ten individual Pond-tagged GLBs are under [`game/assets/faf/pond/wood/`](../../../game/assets/faf/pond/wood/). The [manifest](../../../assets/faf/pond/wood/pond_wood_002_manifest.json) records every exact source, GLB and wrapper path, stable ID, generator configuration/seed, metres, triangles, material and collision counts, role, and classified opening. Blender +Z is up, Godot +Y is up, and the origin is a mud-contact pivot.

[Family-specific source](../../../tools/asset-factory/blender/wood/pond_cover_expansion.py) shares the first wood pack's faceted beam and four-material vocabulary, then constructs five distinct structural layouts. The [fixed build recipe](../../../tools/asset-factory/blender/wood/build_pond_wood_002.py) writes the ten masters, GLBs and manifest with deterministic seeds. It refuses existing outputs unless `--overwrite` is passed. Regenerate from the repository root with `blender -b --python tools/asset-factory/blender/wood/build_pond_wood_002.py -- --root "$PWD" --overwrite`, then run `python3 tools/asset-factory/blender/wood/write_wrappers_002.py`. The wrapper writer uses the same manifest collision segments and only writes under `game/scenes/faf_pond_wood_002/`.

## Inventory and intended routes

Dimensions are Blender X × Y × Z metres. Each line is one standalone GLB and wrapper. All ten have four opaque material surfaces. Collision uses capped capsules inside substantial visible wood and, for each split hollow, 20 open angular shell panels plus one outer broken-limb capsule. There is no one-piece convex hull over a hollow or branch maze.

| ID | Form and intended stealth role | Dimensions m | Triangles | Collision shapes | Seed |
| --- | --- | ---: | ---: | ---: | ---: |
| 005A | Straight long tree; elongated hide-behind landmark | 4.365 × 14.902 × 2.276 | 484 | 18 | 5101 |
| 005B | Longer bent tree; bank-side exposure boundary | 5.124 × 17.609 × 2.601 | 484 | 18 | 5102 |
| 006A | Broad low Y fork; lateral route splitter | 7.249 × 10.789 × 2.652 | 294 | 12 | 6201 |
| 006B | Wider high asymmetric fork; raised cover | 9.644 × 10.857 × 3.031 | 294 | 12 | 6202 |
| 007A | Open root ball; undercut refuge and hard visual screen | 6.189 × 10.008 × 2.713 | 524 | 23 | 7301 |
| 007B | Wider 10-root ball; larger hide-behind/undercut | 6.875 × 10.059 × 3.220 | 616 | 27 | 7302 |
| 008A | Split decayed shell; end passage plus side entry | 4.533 × 7.700 × 3.869 | 972 | 21 | 8401 |
| 008B | Longer, broader and more broken shell; multiple exits | 5.283 × 9.200 × 4.525 | 828 | 21 | 8402 |
| 009A | Six-arm sunken crown; lateral and vertical sightline breaks | 8.680 × 10.472 × 4.809 | 408 | 18 | 9501 |
| 009B | Eight-arm, deeper crown; larger layered branch maze | 8.763 × 12.409 × 5.263 | 516 | 23 | 9502 |

One of every variant totals **5,420 source triangles and 193 collision shapes**. The [isolated Forward+ proof](../../../game/scenes/faf_pond_wood_002_proof.tscn) contains all ten individually, four mixed second placements, two slope/irregular-terrain placements, and two existing Pond wood pieces for comparison: 16 new instances, **8,322 instanced source triangles and 313 new wood collision shapes**. Representative native captures used Vulkan Forward+ on AMD Radeon 880M/RADV. These modest mesh costs do not imply that hundreds of physics shapes are appropriate for dense generated placement; runtime density remains to be budgeted.

| Family | Major opening classification | Fish clearance and evidence |
| --- | --- | --- |
| Long tree | Both long sides: hard-cover travel lanes; ends expose the fish | Both sides of A and B crossed with the real Fish controller and follow camera. No tunnel is claimed. |
| Fork | Broad space between outer arms: traversable lateral route; apex: visual gap only | Lateral paths crossed A/B, using the raised opening on B. Its lower cross-limb opening compressed the camera and is not a declared route. Near-apex narrow space is not labeled a passage. |
| Root ball | Central exposed-root undercut: **limited refuge**; small upper root windows: visual gaps only | A/B fish entered roughly 3.5 m from the approach, stopping at the trunk. No straight through-route is claimed. |
| Split hollow | Both broken ends: longitudinal passage; collapsed right wall: side-entry passage; missing roof: peek-through only | A/B passed centreline and side-entry fish-sphere probes and actual Fish-controller runs. Roof holes are not sold as a route. |
| Branch crown | Raised outer arm lanes: traversable lateral routes; raised middle gap: vertical escape route; lower crossings: camera-constrained visual gaps | A/B real-controller raised lateral paths and controlled ascents through a raised gap passed, with no sampled camera hits. Other raised gaps remain visual/clearance candidates. |

The player collision sphere is radius **0.48 m** and the follow-camera probe is radius **0.22 m**. Manifest opening widths are nominal design values; native routes and actual shape probes are the evidence for the tested lines. The [Godot checker](../../../game/dev/faf_pond_wood_002_check.gd) verifies all ten imported costs, bounds and collision counts, plus fish-sized hollow centerline and side openings. The [binary GLB checker](../../../tools/asset-factory/blender/wood/check_pond_wood_002.py) verifies source/GLB/wrapper existence, Pond IDs, opaque materials and measured triangles.

## Blender and Forward+ visual review

Blender MCP was verified healthy before asset work, generated representative family shapes in the connected scene, then reviewed the final editable exports as A/B pairs in side, end, three-quarter and elevated views. Evidence: [long tree](evidence/blender_long_pair.png), [fork](evidence/blender_fork_pair.png), [root ball](evidence/blender_root_pair.png), [split hollow](evidence/blender_split_pair.png), [branch crown](evidence/blender_crown_pair.png), and [older hero hollow beside the new split family](evidence/blender_existing_hollow_comparison.png). Each family also has `blender_<family>_side.png`, `_end.png` and `_elevated.png` in the evidence folder.

Native views: [overview](evidence/pond_wood_002_overview.png), [long tree](evidence/pond_wood_002_long.png), [fork](evidence/pond_wood_002_fork.png), [root ball](evidence/pond_wood_002_root.png), [split shell](evidence/pond_wood_002_split.png), [branch crown](evidence/pond_wood_002_crown.png), [end opening](evidence/pond_wood_002_end.png), [flat top view](evidence/pond_wood_002_top.png), [mild bank](evidence/pond_wood_002_bank.png), [irregular mud](evidence/pond_wood_002_irregular.png), [older wood comparison](evidence/pond_wood_002_comparison.png), and [real Fish in the hollow](evidence/pond_wood_002_passage.png).

The strongest additions are the 14.9–17.6 m fallen-tree cover line, the genuinely broken side-entry hollow, and the crown's high/lateral branches. The fork arms read as two route choices rather than an identical Y stamp. The root ball has real gaps between roots but its frontal cut end is still broad and its root tips remain pointed; softer, more irregular breakage would improve a final style lock. The split shell is visibly less intact than the older 002A clean tube, although repeated angular bark bands are still apparent. New wood rests on flat mud and on two tapered diagnostic terrain patches; the bank trunk follows the mild cross-slope, and root tips touch or partly enter irregular mud. The patches are placement fixtures, not a generated Pond shoreline.

The proof's simple water, light, and silt allow silhouette/clearance inspection but do not reproduce final Pond clarity or lily-canopy lighting. Native static views showed no obvious floating trunk end, giant invisible collision block, sealed hollow opening or solid wall of branches. Camera and collider checks were performed on the named paths only. Close camera movement inside the hollow can reduce camera distance, as expected, but each declared route retained at least 1.22 m of rig separation and zero sampled physical camera hits. Lower fork/crown cross-limb attempts did compress the camera and are excluded from the declared routes. The B crown's lateral route also deflected several metres around a branch, so its line should be refined before production placement. Actual frame draw calls include fixtures and shadows; final overview, bank and comparison captures reported 159, 152 and 113 respectively.

## Verification and remaining boundary

- `python3 tools/asset-factory/blender/wood/check_pond_wood_002.py` → `POND_WOOD_002_GLB_OK assets=10 triangles=5420`.
- Godot 4.7.2 import and `--script res://dev/faf_pond_wood_002_check.gd` → `POND_WOOD_002_GODOT_OK assets=10 triangles=5420`.
- Isolated proof `--faf-check` → `POND_WOOD_002_PROOF_READY assets=10 instances=16 instanced_triangles=8322 collision_shapes=313 renderer=forward_plus`.
- Sixteen declared real-controller route runs, one Fish instance per process, passed, including raised lateral and vertical routes through both crowns. Low cross-limb trials exposed camera compression and informed the narrower route classifications. The final B crown lateral path was rerun after moving its extra arms away from the intended lane. A native Forward+ 008A end passage also passed and produced the Fish-camera screenshot. One-process-per-route avoids a Godot .NET shutdown/finalizer crash observed when many duplicate C# Fish controllers were repeatedly destroyed in one scene session; the crash was a proof-harness issue, not a claimed asset collision result.
- A separate Blender 5.2.3 rebuild produced byte-identical manifest and all ten GLBs. `.blend` masters preserve the same generated scenes but contain save-location data and are not byte-identical across output directories.

Other untested crown gaps, the lower camera-constrained cross-limb lanes, the deflected B-crown lateral line, full all-angle collision inspection, real generated-Pond placement, predator approach/occlusion behavior and performance at production density remain unqualified. Normal Pond generation and Ocean content are untouched by this isolated candidate. No P4/P5 Gate result follows from these asset checks.
