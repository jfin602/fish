# FAF-ROCK-001 reef boulder family

**Candidate record, 2026-11-10.** FAF-ROCK-001 is the first original procedural reef-rock family. A is the medium reference boulder; B/C/D test low, tall, and leaning formations. The [owner reef target](../references/reef-style-target.png) is the primary style comparison. Technical validation and an isolated native Godot Forward+ proof passed. Full style approval beside the live reef and from the gameplay camera remains open; the P2 habitat was not changed.

## Source and generator

- Editable master: [rock_001_reef_boulder_family.blend](../../../assets/faf/rocks/rock_001_reef_boulder_family.blend). `FAF-ROCK-001 Source` contains four meshes at ground-contact origins with identity object transforms. `FAF-ROCK-001 Review` contains spaced linked copies for side-by-side inspection; these are excluded from GLB export.
- Reusable construction: [boulder.py](../../../tools/asset-factory/blender/rocks/boulder.py), API `create_reef_boulder(name, BoulderSpec(...), material, collection)` and `boulder_geometry(spec)`. A low-resolution closed volume receives broad seeded directional massing, a contact/base cut, a sloped crown cut and one to three angled side cuts. Shallow triangles are dissolved into unequal broad planes. No independent vertex noise or texture detail drives the shape.
- Family recipe and export entry point: [build_001.py](../../../tools/asset-factory/blender/rocks/build_001.py), generator identity `rocks.boulder.v1`. `seed` is deterministic; repeated A runs produced an identical geometry fingerprint, while changing seed or `facet_scale` changed the result. The source-controlled script supports `--reference-only` and full family builds from Blender Python. The final family was generated and exported through these functions in Blender MCP; a separate background Blender launch stalled before running its script, so the standalone command-line entry point was not qualified on this host.
- One matte `mat__rock_cool_slate` material uses the FAF rock palette seed. No texture images, armatures, Actions or collisions are included.

The exposed `BoulderSpec` controls have visible purposes: `size` is horizontal metre scale; `depth` sets footprint aspect; `verticality` sets height; `asymmetry` sets broad directional mass; `facet_scale` sets plane-cut count and dissolve threshold; `base_width` sets contact footprint; `lean` offsets the upper mass; and `seed` selects reproducible mass/cut orientation. These are family controls rather than species or scene-specific coordinates.

## Variants and measured output

All dimensions are Blender X/Y/Z metres, with Z up. Imported Godot dimensions become X/Y/Z with Y up; the [isolated check](../../../game/dev/faf_rock_001_check.gd) compares each at 0.02 m tolerance. Every source mesh has location and rotation zero, scale one, a Z=0 contact plane, one material, a closed manifold surface, and a distinct selected-object GLB with one mesh and one node. The family ID is `FAF-ROCK-001`; A/B/C/D are stable variant labels, displayed as FAF-ROCK-001A through D.

| Variant | Role | `size`, `depth`, `verticality` | `asymmetry`, `facet_scale`, `base_width` | `lean`, seed | Dimensions m | Triangles | GLB |
| --- | --- | --- | --- | --- | --- | ---: | --- |
| A | Medium reference | 1.55, 0.84, 0.66 | 0.68, 0.55, 0.82 | (0.14, -0.06), 1049 | 1.561 × 1.291 × 0.886 | 240 | [A](../../../game/assets/faf/rocks/rock_001a_reef_boulder.glb) |
| B | Low/wide | 1.85, 1.10, 0.38 | 0.42, 0.35, 0.96 | (-0.015, 0.015), 2083 | 1.850 × 2.035 × 0.572 | 186 | [B](../../../game/assets/faf/rocks/rock_001b_reef_boulder.glb) |
| C | Tall/irregular | 1.25, 0.78, 1.27 | 0.66, 0.72, 0.72 | (0.10, 0.015), 3109 | 1.223 × 0.975 × 1.392 | 248 | [C](../../../game/assets/faf/rocks/rock_001c_reef_boulder.glb) |
| D | Leaning/angular | 1.45, 0.73, 0.83 | 0.64, 0.60, 0.78 | (0.26, -0.09), 4093 | 1.464 × 1.041 × 1.035 | 222 | [D](../../../game/assets/faf/rocks/rock_001d_reef_boulder.glb) |

## Visual review and proof

Blender MCP views of A: [front](evidence/01-a-front.png), [side](evidence/02-a-side.png), [top](evidence/03-a-top.png), [low three-quarter](evidence/04-a-three-quarter-low.png), [elevated three-quarter](evidence/05-a-three-quarter-elevated.png). Family views: [front](evidence/06-family-front.png), [elevated](evidence/07-family-elevated.png). Native Godot: [family proof](evidence/08-godot-native-family.png) in the [isolated scene](../../../game/scenes/faf_rock_001_proof.tscn).

The first contour-band approach looked like a truncated dome; the uniformly beveled block looked manufactured. The accepted construction uses directional masses and angled cuts, with shallow triangulation merged into larger planes. A now has an irregular footprint, grounded underside, asymmetric crown and readable facets. B is visibly flatter, C taller, and D leans toward a cleaved end face. The four share a palette and facet language without reading as scaled copies. Under the native fixture light, their blue-gray faces sit coherently against a quiet sand color and blue water.

Godot 4.7.2 imported all four GLBs. The check observed one mesh, one named material, identity root and mesh transforms, ground-contact Y≈0, dimensions within 0.02 m and exactly 240/186/248/222 triangles. A native Forward+ capture on the AMD Radeon 880M confirmed the material and silhouettes at metre scale. The proof is a deliberately small fixture, not a live habitat placement or P2 Gate result.

## Decision and remaining weaknesses

**Technical validation: passed. Visual review: candidate approved for controlled rock generation; live reef-context style approval pending.** The generator is ready to make further boulder *candidates* with deterministic seeds and the established FAF review process. It is not a universal cliff/arch generator. Several cut faces remain broad and nearly oval, particularly D; additional form refinement may be warranted for hero placement. Native proof used simple lighting and one instance of each; gameplay-camera readability, collision shapes, repetition at habitat density and real reef lighting were not tested. No live P2 habitat, movement, coral, plant or terrain asset was changed.
