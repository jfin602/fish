# FAF-PLANT-001–003 first vegetation pack

**Candidate record, 2026-11-10.** Eight original low-poly vegetation assets form three deterministic families: sea grass (A/B/C), broad seaweed (A/B/C), and small reef algae (A/B). The [owner reef target](../references/reef-style-target.png) is the primary style comparison. Blender family/density review and isolated Godot 4.7.2 Forward+ proof passed. Nothing was placed into the live P2 habitat.

## Source, APIs and handoff

- Editable [Blender master](../../../assets/faf/plants/plant_001_to_003_family.blend) contains eight ground-pivot source meshes in separate family collections and eight linked review copies. The review copies are outside each selected-object GLB export.
- [Generator implementation](../../../tools/asset-factory/blender/plants/families.py) exposes `sea_grass_geometry(SeaGrassSpec)`, `broad_seaweed_geometry(BroadSeaweedSpec)` and `reef_algae_geometry(ReefAlgaeSpec)`, with shared `create_plant(name, family, spec, material, collection)`. Each spec controls height, count, spread, blade/frond width, curvature, lean and seed. These controls change the form directly; no unrelated per-vertex noise or alpha-card forest is used.
- [Pack recipe/export entry point](../../../tools/asset-factory/blender/plants/build_pack_001.py) freezes the eight configurations, seeds and palette swatches. The Blender background command accepts `--root REPO` and requires `--overwrite` for existing outputs. The delivered pack was generated through these source-controlled functions in Blender MCP while preserving the open Yellow Tang scene; the separate background command was not qualified on this host.
- The [candidate manifest](../../../assets/faf/plants/plant_001_to_003_manifest.json) records IDs, type, generator version, full configuration and seed, Blender X/Y/Z metre dimensions, triangles, one material, zero bones/Actions, origin/rotation/scale and GLB path for each asset. All meshes are opaque and flat shaded with no texture or collision. Blender +Z is up; the broad seaweed fronds are upright and can be rotated around that axis for placement.

Every source mesh has one UV coordinate layer. **UV.y is 0 at every base and 1 at each blade tip**, including the broad seaweed. Godot import confirmed this range. It is a reserved height mask for a later cheap, base-anchored visual sway; no sway, solver, force or gameplay movement was added in this pack.

Repeated in-session runs of each A recipe produced identical geometry digests; changing only the seed changed each family digest. The saved Blender source was reopened as a library and contained exactly eight canonically named source meshes. Each exported GLB contains one mesh, one material and one UV set.

## Eight measured assets

Dimensions are Blender X × Y × Z metres. Godot imports these as X × Z × Y with identity root/mesh transforms and ground contact at Y=0. Every variant has location/rotation zero and unit object scale.

| FAF ID | Family | Dimensions m | Triangles | Palette | Seed | Separate GLB |
| --- | --- | --- | ---: | --- | ---: | --- |
| FAF-PLANT-001A | Sea grass | 0.629 × 0.608 × 0.579 | 196 | reef green | 5101 | [001A](../../../game/assets/faf/plants/plant_001a_sea_grass.glb) |
| FAF-PLANT-001B | Sea grass | 0.775 × 0.935 × 0.714 | 224 | deep green | 5102 | [001B](../../../game/assets/faf/plants/plant_001b_sea_grass.glb) |
| FAF-PLANT-001C | Sea grass | 0.433 × 0.475 × 0.445 | 168 | yellow-green | 5103 | [001C](../../../game/assets/faf/plants/plant_001c_sea_grass.glb) |
| FAF-PLANT-002A | Broad seaweed | 0.961 × 0.836 × 1.119 | 208 | deep green | 5201 | [002A](../../../game/assets/faf/plants/plant_002a_broad_seaweed.glb) |
| FAF-PLANT-002B | Broad seaweed | 1.119 × 1.156 × 1.297 | 260 | reef green | 5202 | [002B](../../../game/assets/faf/plants/plant_002b_broad_seaweed.glb) |
| FAF-PLANT-002C | Broad seaweed | 0.571 × 0.584 × 0.924 | 156 | muted teal | 5203 | [002C](../../../game/assets/faf/plants/plant_002c_broad_seaweed.glb) |
| FAF-PLANT-003A | Reef algae | 0.746 × 0.739 × 0.303 | 164 | yellow-green | 5301 | [003A](../../../game/assets/faf/plants/plant_003a_reef_algae.glb) |
| FAF-PLANT-003B | Reef algae | 0.776 × 0.695 × 0.417 | 192 | deep green | 5302 | [003B](../../../game/assets/faf/plants/plant_003b_reef_algae.glb) |

**Pack range:** 156–260 triangles, one material per asset. All eight fit the FAF 20–300 triangle grass/small-plant working range. Palette selection stays in the FAF vegetation family; yellow-green and muted teal were added as restrained seeds beside existing reef and deep greens.

## Visual review and corrections

Blender MCP evidence: [sea grass family](evidence/01-sea-grass-family.png), [broad seaweed family](evidence/02-broad-seaweed-family.png), [reef algae family](evidence/03-algae-family.png), and [repeated staging beside FAF rock/coral](evidence/04-density-with-rock-coral.png). Native Godot evidence: [representative grouping](evidence/05-godot-native.png) and [18 extra repeated clumps](evidence/06-godot-density.png).

- **Sea grass:** six to eight broad-enough tapered blades make clumps readable without a high blade count. A/B/C differ in height, spread and color. Rotations and mixed variants are needed to avoid a repeated crown pattern.
- **Broad seaweed:** three to five tall fronds carry the midground silhouette with several longitudinal segments for later low-cost bending. Some fronds read edge-on from a fixed camera and appear narrow; whole-clump rotation and interleaved instances help, but final placement must check the gameplay camera.
- **Reef algae:** short broad paddles distinguish this family from grass. The first pass looked disconnected at its base. The generator now narrows paddle spacing and adds a low faceted shared anchor; the fix applies to both variants.
- **Together:** the silhouettes and controlled green range sit beside the existing faceted rock and vivid coral without competing for the same focal color. A repeated Blender patch remained legible. In the denser native fixture, close spacing began to mask the coral, so authored placement needs clear gaps and mixed rotations rather than a uniform stamp grid.

## Godot proof and decision

Godot 4.7.2 imported all eight GLBs. The [focused import check](../../../game/dev/faf_plant_pack_001_check.gd) read the candidate manifest and verified one mesh/material surface, canonical material name, identity transforms, Blender-to-Godot dimensions within 0.025 m, ground contact, exact triangle identity, and UV.y from 0 to 1 for every variant. The [isolated Forward+ scene](../../../game/scenes/faf_plant_pack_001_proof.tscn) and [capture script](../../../game/dev/faf_plant_pack_001_proof.gd) showed representative grass, broad seaweed and algae beside FAF rock/coral on AMD Radeon 880M. A second capture added 18 more clumps for density review. The Godot Editor MCP bridge was offline, so native proof used the pinned project executable and the isolated scene.

**Technical validation: passed. Blender visual review: passed. Native candidate proof: passed.** All three family generators are ready for future candidates and curated repeated placement. Dense live-habitat readiness is still conditional on actual gameplay-camera, water-column lighting, draw-call and occlusion measurements; the fixture does not pass a P2 Gate or prove a performance ceiling. No animation or sway was implemented. A later Godot visual shader can displace tips according to `UV.y` (with squared or eased falloff), small amplitude, and a stable phase per instance while leaving roots and collision fixed. Do not apply the sway to authoritative movement or physics.
