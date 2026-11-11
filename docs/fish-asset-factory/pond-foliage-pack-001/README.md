# Pond foliage pack 001 — FAF-PLANT-004–009

**Candidate record, 2026-11-11.** Thirteen original Pond-only foliage variants across six deterministic families. The primary visual direction is the owner's [Pond target](../references/pond/pond-style-target.png): broad olive lily cover over an open fish route, rooted bank grasses and reeds, and restrained muddy greens and browns. This is an isolated asset proof, not a normal-play Pond placement or an Ocean art change.

## Source and generator APIs

- [Editable Blender master](../../../assets/faf/pond/plants/pond_foliage_001.blend): 13 separate source collections plus review copies. The source objects have identity transforms; the selected-object exports exclude review copies.
- [Six family generators](../../../tools/asset-factory/blender/plants/pond_foliage.py): `lily(LilySpec)`, `grass(GrassSpec)`, `cattail(CattailSpec)`, `reed(ReedSpec)`, `algae(AlgaeSpec)`, and `muck(MuckSpec)`. Radius/depth/notch/lean, blade or stalk count/spread/height, algae paddle count, and muck island count/open fraction are meaningful parameters. Every recipe has a seed. Shared low-poly primitives are subordinate to these distinct family forms.
- [Build recipe](../../../tools/asset-factory/blender/plants/build_pond_foliage_001.py) locks the 13 configurations and swatches, saves the master and manifest, and exports 13 individual GLBs. Run with `blender -b --python tools/asset-factory/blender/plants/build_pond_foliage_001.py -- --root "$PWD"`; existing Pond foliage outputs require explicit `--overwrite`.
- [Manifest](../../../assets/faf/pond/plants/pond_foliage_001_manifest.json) records each stable FAF ID, Pond habitat tag, variant, seed, full configuration, Blender metre bounds/dimensions, measured source/export triangles and materials, components, source/export paths, origin/rotation/scale, placement anchor, and review status. GLB mesh-node extras also carry `faf_id`, `faf_habitat=pond`, family, seed, anchor, and the UV sway convention.

Opaque faceted geometry uses **no alpha cards or textures**. `UV.y=0` at the rooted base and reaches `1` at tips (or upper surface). This prepares a future visual shader to move only tips; no gameplay force, collision motion, or rooted-plant runtime sway is introduced here. The isolated proof applies at most 2.5 mm of slow visual bob to lily **pad meshes only** and floating muck; lily stems and all mud roots stay fixed. Its translucent water reference is scene-only and is not part of a foliage GLB.

## Inventory and measured cost

Dimensions are Blender X × Y × Z metres. Godot maps the source vertical Z to Y. All roots/contact anchors are identity-transform local origins; lily and floating-patch origins are the waterline at `z=0`.

| ID | Form | Dimensions m | Tris | Mats | Placement | Seed |
| --- | --- | --- | ---: | ---: | --- | ---: |
| FAF-PLANT-004A | Lily pad + 0.72 m stem | 0.923 × 0.903 × 0.745 | 114 | 2 | Surface over 0.72 m mud | 6401 |
| FAF-PLANT-004B | Lily pad + 1.08 m stem | 1.177 × 1.247 × 1.105 | 114 | 2 | Surface over 1.08 m mud | 6402 |
| FAF-PLANT-004C | Lily pad + 1.48 m stem | 1.576 × 1.501 × 1.505 | 114 | 2 | Surface over 1.48 m mud | 6403 |
| FAF-PLANT-005A | Tall bank grass | 0.699 × 0.705 × 0.999 | 288 | 1 | Damp or dry bank | 6501 |
| FAF-PLANT-005B | Taller, denser grass | 1.117 × 1.013 × 1.301 | 396 | 1 | Damp or dry bank | 6502 |
| FAF-PLANT-006A | Three-head cattail | 0.465 × 0.546 × 1.389 | 294 | 2 | Shallow mud at bank | 6601 |
| FAF-PLANT-006B | Four-head cattail | 0.790 × 0.732 × 1.667 | 392 | 2 | Shallow mud at bank | 6602 |
| FAF-PLANT-007A | Six-stalk reed | 0.698 × 0.802 × 1.357 | 204 | 1 | Shallow mud at bank | 6701 |
| FAF-PLANT-007B | Eight-stalk reed | 1.063 × 1.040 × 1.814 | 272 | 1 | Shallow mud at bank | 6702 |
| FAF-PLANT-008A | Small attached algae | 0.332 × 0.334 × 0.185 | 168 | 1 | Ground/log/rock contact | 6801 |
| FAF-PLANT-008B | Broad attached algae | 0.594 × 0.507 × 0.293 | 224 | 1 | Ground/log/rock contact | 6802 |
| FAF-PLANT-009A | Five-island muck | 0.958 × 0.618 × 0.017 | 180 | 1 | Water surface, isolated | 6901 |
| FAF-PLANT-009B | Seven-island muck | 1.167 × 1.403 × 0.017 | 252 | 1 | Water surface, isolated | 6902 |

**Pack cost:** 3,012 triangles for one instance of each asset; 114–396 triangles and one or two materials per asset. B grass and B cattail exceed the 20–300 triangle *small grass* starting range because they are taller bank silhouettes; they should be used in curated groups, not as dense repeated ground cover. The three lily pad/stem pairs each contain exactly two imported components, and cattails retain distinct stalk, seed-head, and leaf parts. Six opaque swatches are shared: olive, yellow olive, moss, deep moss, stem brown, and seed brown.

## Placement and visual review

The [isolated Forward+ scene](../../../game/scenes/faf_pond_foliage_proof.tscn) lays all 13 on a shallow cross-section with a central open route. Its [script](../../../game/dev/faf_pond_foliage_proof.gd) supports `--faf-family=lily_pad|tall_grass|cattail|reed|submerged_algae|floating_muck|all`, `--faf-view=above|waterline|underwater`, and `--faf-capture=ABSOLUTE_PNG`. Twenty-one native captures covered all six families individually and the mixed scene from all three views. Selected evidence: [mixed above](evidence/mixed_above.png), [waterline](evidence/mixed_waterline.png), [underwater](evidence/mixed_underwater.png); [lily tops](evidence/lily_pad_above.png), [lily stems](evidence/lily_pad_underwater.png), [bank grass](evidence/tall_grass_waterline.png), [cattail heads](evidence/cattail_waterline.png), [reeds](evidence/reed_waterline.png), [algae](evidence/submerged_algae_underwater.png), and [muck](evidence/floating_muck_above.png).

- **Lily:** three broad irregular discs have visibly different radius, notch and hue. Their separate stems meet the underside and touch the authored mud at 0.72, 1.08, and 1.48 m. Tops sit slightly above the fixture waterline; no sunken pad or floating stem root appeared. The discs are still simpler and less layered than the target canopy; repeated placement needs mixed radii, rotations and spacing.
- **Grass, cattail, reed:** grass makes wide pointed bank crowns; cattails have clear brown seed clubs; reeds are taller, thinner open stalks without clubs. They stay out of the central lane. One early family camera cut through the bank, so the fixture camera was moved into open water before final review. The final views show grounded bases.
- **Algae:** low, faceted contact pads and paddles sit on the pond floor. They can be reoriented so local +Z meets a log or rock face. They are correctly subdued and hard to see from above the water; use them as close or midground substrate accents.
- **Floating muck:** two asymmetric groups of separated opaque islands leave water openings. They are readable from above, visually quiet edge-on, and do not form a solid swim-lane ceiling.
- **Together:** the olive/yellow-green/brown range and broad facet sizes fit the owner's earthy target direction, while the thin stems, narrow reeds, and sparse algae keep the water channel open. The scene has only foliage, procedural mud and a scene-only waterline; it does not reproduce the target's logs, depth richness, shafts of light, or final Pond water treatment. Style lock remains an owner decision.

## Technical proof and status

The [binary GLB checker](../../../tools/asset-factory/blender/plants/check_pond_foliage_001.py) verified all 13 binary headers, Pond ID extras, component names, exact exported triangle counts, opaque materials and absence of textures. The [Godot import checker](../../../game/dev/faf_pond_foliage_check.gd) instantiated all 13 GLBs in Godot 4.7.2 .NET and checked component counts, identity transforms, imported dimensions (within 0.035 m), exact triangles/materials, UV presence, and all three stem extents. It printed `FAF_POND_FOLIAGE_OK assets=13 lily_stems=3`. Native Forward+ captures ran on AMD Radeon 880M. The Godot Editor MCP bridge was offline; the Blender MCP session held an unrelated Yellow Tang scene, so the Pond source was built in a separate background Blender process and reviewed through the isolated native fixture without altering that open scene.

**Result:** all 13 variants and lily components import and render in the isolated proof. The pack is technically ready for authored Pond candidate placement. No normal play, Ocean asset, P3 world, or game version was changed. Remaining production checks are real Pond-water lighting, gameplay-scale repetition, tip-sway amplitude, and collision/occlusion against final Pond routes. No performance ceiling or owner style approval is claimed from this fixture.
