# FAF-DETAIL-001–004 — first small reef-floor detail pack

**Candidate record, 2026-11-10.** Eight original static details make four A/B pairs: shells, broken reef rubble, pebbles, and small sponge forms. They are supporting pieces for close and midground dressing beside the approved [reef style target](../references/reef-style-target.png), not new hero coral. Blender MCP family review, individual GLB validation, Godot 4.7.2 import, and one isolated native Forward+ cluster proof passed. Live habitat placement and gameplay-camera style approval remain open.

## Source and reuse

The [editable Blender master](../../../assets/faf/details/detail_001_to_004_family.blend) has eight ground-pivot source meshes and spaced review copies; only sources go to [individual GLBs](../../../game/assets/faf/details/). The [source-controlled builder](../../../tools/asset-factory/blender/details/build_pack_001.py) freezes eight configurations and swatches, while [`details.families`](../../../tools/asset-factory/blender/details/families.py) exposes four small APIs: `shell_geometry(ShellSpec)`, `rubble_geometry(RubbleSpec)`, `pebble_geometry(PebbleSpec)`, and `sponge_geometry(SpongeSpec)`. Shells use a shallow closed radial plate; rubble uses uneven truncated fracture chips; pebbles share only a cheap faceted-lump primitive; sponges use short open polygonal cups with actual recesses. The controls are metre footprint, count, spread, proportion, arch/opening where relevant, orientation/lean, and a local deterministic seed. The [manifest](../../../assets/faf/details/detail_001_to_004_manifest.json) records full configurations and outputs. This is intentionally four narrow families, not a universal debris system.

All source objects have location/rotation zero, scale one, and ground-contact Z=0. Every GLB has one closed faceted mesh, one opaque matte material, no textures, no armature, no Actions, and no collision. Source dimensions below are Blender X × Y × Z metres; Godot import maps them to X × Z × Y.

## Eight candidates and technical cost

| FAF ID | Form and difference within pair | Size m | Triangles | Palette / seed |
| --- | --- | --- | ---: | --- |
| FAF-DETAIL-001A | Broad low fan shell | 0.1593 × 0.1127 × 0.0533 | 80 | shell cream / 1011 |
| FAF-DETAIL-001B | Narrower, longer, tilted ridge shell | 0.1154 × 0.1420 × 0.0646 | 80 | shell sand / 1012 |
| FAF-DETAIL-002A | Three low coral-colored fracture chips | 0.1704 × 0.1164 × 0.0445 | 48 | muted coral / 2021 |
| FAF-DETAIL-002B | Two larger warm-stone reef chips | 0.1733 × 0.0547 × 0.0493 | 32 | warm stone / 2022 |
| FAF-DETAIL-003A | Five compact cool slate pebbles | 0.1279 × 0.1119 × 0.0125 | 90 | slate / 3031 |
| FAF-DETAIL-003B | Four broader, more scattered warm pebbles | 0.1629 × 0.1885 × 0.0107 | 72 | warm stone / 3032 |
| FAF-DETAIL-004A | Low single encrusting sponge cup | 0.1439 × 0.1411 × 0.0316 | 46 | muted ochre / 4041 |
| FAF-DETAIL-004B | Two taller, leaning sponge cups | 0.1630 × 0.0866 × 0.1068 | 92 | dusty rose / 4042 |

**Cost:** 32–92 triangles each, 540 triangles for one of every asset, and one material per asset. These remain below the new provisional 20–150 triangle small-detail budget. The dimensions are roughly one-tenth to one-fifth the width of the first FAF coral/rock forms and comparable to small fish body lengths, so the pieces read as seabed dressing rather than larger structures.

## Visual review

Blender MCP evidence: [whole pack](evidence/01-pack-overview.png), [shell pair](evidence/02-shell-pair.png), [rubble pair](evidence/03-rubble-pair.png), [pebble pair](evidence/04-pebble-pair.png), and [sponge pair](evidence/05-sponge-pair.png). The broad-facet language and subdued cream, rock, coral, and sponge colors are consistent with the reef target. The A/B silhouettes differ in footprint, height, count and orientation, rather than just tint. The first rubble pass looked like pointed mini-boulders; its family geometry was revised to irregular planar fracture tops and lower profiles before export. At very close range, the 002B chips are still simple polygonal slabs; place them partly behind rock/coral bases or revisit that form if it becomes prominent. The shallow pebbles will disappear at longer camera distances, which is appropriate for this supporting role.

The [isolated Godot fixture](../../../game/scenes/faf_detail_pack_001_proof.tscn) places all eight on one small sand patch. Its [native Forward+ capture](evidence/06-godot-native-cluster.png) confirms subdued color, ground contact, and distinct silhouettes under a blue water backdrop. This fixture measured 9 draw calls and 542 rendered primitives (the eight details plus a two-triangle sand plane) on an AMD Radeon 880M; it is not a live-habitat performance measurement. The [focused import check](../../../game/dev/faf_detail_pack_001_check.gd) verified all eight GLBs against the manifest: one mesh/material surface, exact triangle identity, source-to-Godot size mapping within 0.003 m, Y≈0 contact, named material, and identity import transforms. No live habitat, movement, fish, coral, rock or terrain source was changed.

## Placement guidance and decision

Start with **one to three detail assets per square metre** near authored rock/coral bases, using fewer in open sand and more only in close foreground pockets. Because each pebble/rubble asset is already a cluster, avoid laying the same pair in a regular grid; rotate instances, alternate A/B, and leave quiet sand between groups. This is a provisional composition recommendation, not a measured P2 density limit. Check draw calls, visibility and color balance in the real habitat before increasing density.

**Technical validation: passed. Blender and isolated native visual review: passed as supporting candidates.** The four generators are ready to produce additional detail candidates, subject to the same review. Full reef lighting, camera distance, repetition at placement density, and collision needs remain untested. This pack supplies variation only; it does not approve live reef placement or a Roadmap Gate.
