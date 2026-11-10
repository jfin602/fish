# FAF-TERRAIN-001 modular seabed family

**Candidate record, 2026-11-10.** Six original broad-polygon seabed chunks provide an open sand base, gentle rolls, a reef shelf, a slope transition, a drop-off, and a rocky reef-floor patch. The [owner reef target](../references/reef-style-target.png) guided the quiet sand field, irregular outlines, and cool stone accents. The editable Blender master, separate GLBs, per-asset manifest, Blender composition review, and isolated Godot 4.7.2 Forward+ proof are complete. No live P2 habitat or gameplay file was changed.

## Construction and modular strategy

- [Generator](../../../tools/asset-factory/blender/terrain/patch.py): `terrain_geometry(TerrainSpec)` and `create_terrain(name, spec, palette_materials, collection)` make one 13-spoke, four-ring, low-poly closed patch per configuration. The six `profile` choices are distinct macro elevation functions in the same reusable builder, not six unrelated meshes. Seeded changes affect outline and broad contour; there is no per-vertex noise API.
- [Recipe/export entry](../../../tools/asset-factory/blender/terrain/build_001.py): `RECIPES` fixes the six reviewed specs. Meaningful controls are `footprint`, `elevation`, `slope_direction`, `roughness` (macro contour strength), `edge_profile` (outline irregularity), `reef_coverage` (broad material region), and `seed`. The script can rebuild in background Blender using `--root REPO`; an existing deliverable requires `--overwrite`. These delivered files were generated through those source-controlled functions in Blender MCP so the open Yellow Tang session could be preserved; the separate background entry was not independently exercised on this host.
- [Editable master](../../../assets/faf/terrain/terrain_001_family.blend): six canonical source meshes, six spaced linked review copies, and a three-piece A+C+D overlap composition. The source meshes have zero location and rotation, unit scale, Z-up metre dimensions, and a repeatable origin at the centre of the zero-height outer edge. The rim extends 0.09 m downward from that anchor. The review/composition copies are excluded from selected-object export.
- [Manifest](../../../assets/faf/terrain/terrain_001_manifest.json): each asset's FAF ID, complete generator spec and seed, dimensions, triangles, material names/count, transforms, edge/skirt, and output path.

**Placement method:** irregular perimeter aprons have a common top height of Z=0. Author a broad base floor with 001A, overlap a raised/rolling patch into it, and place the added patch's edge at or slightly above the base surface. The proof uses A at -0.04 m and C/D at 0 m. Rotate and offset pieces; cover the most visible contacts with boulders, coral, grass, or sand/rock dressing. This is an authored overlap system, not a mathematically seamless tiling or an infinite terrain generator. Each chunk carries visual geometry only; collision remains a separate scene-authoring decision.

## Six measured assets

Dimensions are Blender X × Y × Z metres. Godot imports as X × Z × Y. Every chunk is 128 triangles, opaque and flat-shaded, with no texture, skeleton, animation, or collision.

| FAF ID | Role | Dimensions m | Materials | Seed | GLB |
| --- | --- | --- | ---: | ---: | --- |
| FAF-TERRAIN-001A | Open sandy floor | 11.302 × 9.665 × 0.115 | 1 | 6101 | [A](../../../game/assets/faf/terrain/terrain_001a_open_sand.glb) |
| FAF-TERRAIN-001B | Gently rolling seabed | 5.581 × 4.854 × 0.286 | 1 | 6102 | [B](../../../game/assets/faf/terrain/terrain_001b_rolling.glb) |
| FAF-TERRAIN-001C | Raised reef shelf | 5.438 × 4.841 × 1.014 | 3 | 6103 | [C](../../../game/assets/faf/terrain/terrain_001c_reef_shelf.glb) |
| FAF-TERRAIN-001D | Sloped transition | 5.804 × 4.444 × 0.597 | 2 | 6104 | [D](../../../game/assets/faf/terrain/terrain_001d_slope.glb) |
| FAF-TERRAIN-001E | Drop-off / edge | 4.806 × 5.145 × 1.101 | 3 | 6105 | [E](../../../game/assets/faf/terrain/terrain_001e_drop_off.glb) |
| FAF-TERRAIN-001F | Rocky reef-floor patch | 5.098 × 5.316 × 0.478 | 3 | 6106 | [F](../../../game/assets/faf/terrain/terrain_001f_rocky_floor.glb) |

Material region names are `mat__terrain_sand_warm_light` (FAF warm sand), `mat__terrain_sand_rock_transition` (FAF warm stone), and `mat__terrain_reef_cool_slate` (FAF cool stone). A/B use sand only; D uses sand/transition; C/E/F use all three. The transition colors are intentionally broad face regions, not texture noise.

## Visual review, import, and limits

Blender MCP evidence: [family elevated](evidence/01-family-elevated.png), [A+C+D overlap](evidence/02-overlap-composition.png). The first pass had regular concentric color rings and raised pieces that read as perched mounds; the shared generator was corrected with asymmetric macro contours, face-region assignment, and wider shoulders, then rebuilt. The final six have different useful heights and silhouettes. A's large quiet surface leaves visual room for living accents. The close neutral-lit overlap still shows the patch perimeter and occasional abrupt contact; it does not resemble a seamless terrain sheet. Repeated all-around placement of the same shelf/drop-off profile would look stamped without rotation and dressing.

Godot 4.7.2 imported all six GLBs. The [focused check](../../../game/dev/faf_terrain_001_check.gd) verified one mesh per asset, identity root/mesh transforms, exact 128-triangle identity, 1–3 named material surfaces, Blender-to-Godot dimensions within 0.03 m, a -0.09 m skirt below the anchor, and upward-facing surface normals. The [isolated proof scene](../../../game/scenes/faf_terrain_001_proof.tscn) overlaps A+C+D while showing B/E/F nearby over a simple fixture sand floor, with an existing FAF rock, coral, and grass clump for scale. The [native Forward+ capture](evidence/03-godot-native.png) used the AMD Radeon 880M and showed grounded orientation, broad faceting, materials, no holes in the composed area, and visible edge/color seams. The full fixture rendered 17 draw calls in one captured frame (six chunks, floor, three props); this is a sanity observation, not a production performance ceiling. The Godot Editor MCP bridge was offline, so the pinned native executable supplied the import and visual proof.

**Technical validation: passed. Blender review: passed for six candidate shapes. Native isolated composition proof: passed with visible seam limitation.** The pack is ready as candidate geometry for a style-proving authored scene. It is not approved for direct live habitat replacement. That next scene must judge contacts from the actual gameplay camera under Fish's depth lighting/caustics, add deliberate seam dressing, and author simplified collision and camera clearance. These fixtures do not establish P2 Gate evidence or production performance at habitat density.
