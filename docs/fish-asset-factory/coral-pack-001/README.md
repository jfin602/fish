# FAF-CORAL-001–004 first coral pack

**Candidate record, 2026-11-10.** Twelve original coral assets form four deterministic families with A/B/C variants. The [owner reef target](../references/reef-style-target.png) is the primary visual reference. Blender family review and isolated Godot 4.7.2 Forward+ import/native proof passed. Live P2 reef placement, gameplay camera readability, density, collision and final owner style approval remain open. The live habitat and movement were not changed.

## Editable source and four generator APIs

- [Blender master](../../../assets/faf/coral/coral_001_to_004_family.blend): 12 origin-pivot source meshes in separate family collections and 12 spaced linked review copies. The source meshes have identity transforms and are excluded from review presentation; the copies are excluded from export.
- [Generator implementation](../../../tools/asset-factory/blender/coral/families.py): `branching_geometry(BranchingSpec)` / `create_coral(..., "branching", ...)`; `rounded_geometry(RoundedSpec)` / `create_coral(..., "rounded", ...)`; `tube_geometry(TubeSpec)` / `create_coral(..., "tube", ...)`; and `fan_geometry(FanSpec)` / `create_coral(..., "fan", ...)`. Each returns a single faceted mesh, uses a local seeded random stream and keeps a static Z-up contact pivot. The four specs expose only form controls: size, count, spread, verticality, taper where relevant, asymmetry, lean or curvature where relevant, and seed. Changing seed changes the construction layout without changing a family into another shape.
- [Pack recipe and export entry point](../../../tools/asset-factory/blender/coral/build_pack_001.py): `RECIPES` freezes the twelve reviewed configurations, palette choices and seeds. Run in Blender background Python with `--root REPO`; existing outputs require `--overwrite`. The master, manifest and 12 GLBs are build outputs. This host's deliverables were generated through the source-controlled functions in Blender MCP because a separate background Blender process stalled before running Python during the earlier FAF rock task; the standalone coral command was not separately qualified.
- [Machine-readable candidate manifest](../../../assets/faf/coral/coral_001_to_004_manifest.json) records every ID, type, generator version, dimensions in Blender X/Y/Z metres, triangle/material counts, zero bones/Actions, origin/rotation/scale, palette, complete generator configuration and seed, and GLB path. Every static asset uses one matte material, no texture, no transparency, no armature, no Action and no collision mesh.

The source geometry is reproducible: two in-session runs of each A recipe produced matching SHA-256 geometry digests. A Blender library load found one saved coral scene with twelve source mesh names. Each GLB's glTF payload contains one mesh, one material and the intended canonical mesh node.

## Twelve measured outputs

Dimensions are Blender X × Y × Z metres. All source transforms are location/rotation zero and scale one; the Godot import rotates the dimensions to X × Z × Y while keeping root and mesh transforms at identity. Each variant has a ground-contact origin. A/B/C are stable variant labels under each family ID.

| FAF ID | Family | Size m | Triangles | Palette | Seed | Separate GLB |
| --- | --- | --- | ---: | --- | ---: | --- |
| FAF-CORAL-001A | Branching | 0.908 × 0.948 × 0.831 | 508 | reef red | 1101 | [001A](../../../game/assets/faf/coral/coral_001a_branching.glb) |
| FAF-CORAL-001B | Branching | 1.374 × 1.580 × 1.038 | 604 | coral pink | 1102 | [001B](../../../game/assets/faf/coral/coral_001b_branching.glb) |
| FAF-CORAL-001C | Branching | 0.759 × 0.705 × 1.072 | 412 | orange | 1103 | [001C](../../../game/assets/faf/coral/coral_001c_branching.glb) |
| FAF-CORAL-002A | Rounded/brain | 0.849 × 0.826 × 0.478 | 640 | orange | 2201 | [002A](../../../game/assets/faf/coral/coral_002a_rounded.glb) |
| FAF-CORAL-002B | Rounded/brain | 1.177 × 1.242 × 0.450 | 730 | warm yellow | 2202 | [002B](../../../game/assets/faf/coral/coral_002b_rounded.glb) |
| FAF-CORAL-002C | Rounded/brain | 0.680 × 0.635 × 0.478 | 550 | muted purple | 2203 | [002C](../../../game/assets/faf/coral/coral_002c_rounded.glb) |
| FAF-CORAL-003A | Tube | 0.650 × 0.619 × 0.655 | 536 | coral pink | 3301 | [003A](../../../game/assets/faf/coral/coral_003a_tube.glb) |
| FAF-CORAL-003B | Tube | 0.911 × 0.889 × 0.772 | 672 | cyan accent | 3302 | [003B](../../../game/assets/faf/coral/coral_003b_tube.glb) |
| FAF-CORAL-003C | Tube | 0.602 × 0.512 × 0.791 | 468 | warm yellow | 3303 | [003C](../../../game/assets/faf/coral/coral_003c_tube.glb) |
| FAF-CORAL-004A | Fan/soft form | 1.047 × 0.293 × 0.762 | 344 | muted purple | 4401 | [004A](../../../game/assets/faf/coral/coral_004a_fan.glb) |
| FAF-CORAL-004B | Fan/soft form | 1.432 × 0.320 × 0.806 | 400 | reef red | 4402 | [004B](../../../game/assets/faf/coral/coral_004b_fan.glb) |
| FAF-CORAL-004C | Fan/soft form | 0.763 × 0.238 × 0.903 | 288 | blue accent | 4403 | [004C](../../../game/assets/faf/coral/coral_004c_fan.glb) |

**Pack range:** 288–730 triangles; exactly one material per asset. All twelve fit the FAF small-coral starting budget of 100–800 triangles. Color stays within the warm reef field with three selective cool accents. The palette added warm yellow and muted purple swatches to the shared [palette seeds](../../../tools/asset-factory/blender/common/palette.py).

## Visual review and corrections

Blender MCP family screenshots: [branching](evidence/01-branching-family.png), [rounded](evidence/02-rounded-family.png), [tube](evidence/03-tube-family.png), [fan](evidence/04-fan-family.png), [whole pack](evidence/05-pack-overview.png). These show each A/B/C row from a useful front or elevated three-quarter view in material-colored solid shading. The [native Godot representative view](evidence/06-godot-native.png) shows 001A, 002A, 003A and 004A together at metre scale under a blue water backdrop and sand ground.

- **Branching:** chunky forked masses and unequal crowns are legible; C is narrower/taller and B is larger/wider. The stalk intersections are intentionally overlapping solids, and some junction seams remain visible at close range.
- **Rounded:** low faceted lobes suggest brain coral through silhouette rather than grooves or texture noise. B was reduced to seven peripheral lobes to stay inside the small-coral budget, and the asymmetry parameter now visibly changes lobe placement/size. Some overlapping lobe edges are visible in close inspection.
- **Tube:** openings and wall thickness are readable from above; height, count and lean differ. A tilted contact ring initially crossed the ground plane by about 3 cm in B. The generator now flattens the contact underside, and all three reimport at Y=0. Clusters still share a largely radial organization.
- **Fan:** broad opaque webs between a few tapered ribs give a readable fan outline. The first pass appeared as bare rods; the family generator widened solid webs and revised the crown. B remains the densest and most rigid-looking member; it would benefit from additional refinement or gentle sway in a future soft-coral pass.

The pack's broad facets, chunky proportions, simple materials and restrained color accents belong to the same *asset language* as the primary reef target. This isolated fixture does not prove final appearance inside the game's full water-column extinction, caustics and habitat composition.

## Godot proof and decision

Godot 4.7.2 imported all twelve GLBs. The [focused check](../../../game/dev/faf_coral_pack_001_check.gd) reads the candidate manifest and verified exactly one mesh and one material surface, canonical named material, identity root/mesh transforms, dimensions within 0.025 m, Y=0 contact within 0.025 m, and exact triangle identity for each variant. The [isolated scene](../../../game/scenes/faf_coral_pack_001_proof.tscn) and [capture script](../../../game/dev/faf_coral_pack_001_proof.gd) ran in native Forward+ on AMD Radeon 880M, showing geometry, color and grounding for one representative from each family. No live P2 scene, player, movement or other FAF asset was changed.

**Technical validation: passed. Blender visual review: passed for all four families. Native candidate proof: passed for A of each family.** Each generator is ready to create *future coral candidates* through the same review process. This record does not approve live reef placement or a P2 Gate. Before production placement, review rotation/repetition at habitat density, camera-distance readability, water-light color loss, simple collision choices and optional soft-coral sway. The fan family has the most visible hard-edge/overlap weakness; none of the families is a universal coral generator.
