# Asset IDs and names

Assign a stable ID when an original asset is registered for production: `FAF-<CATEGORY>-<NNN>` with categories `FISH`, `CORAL`, `ROCK`, `PLANT`, `TERRAIN` and three or more digits, for example `FAF-FISH-001`. Keep an assigned ID even if the display name or generator changes. Never recycle or casually renumber it. The ID describes identity; a variant/revision is separate metadata.

| Item | Convention and example |
| --- | --- |
| Folder / file stem | Lowercase category, number and slug: `fish_001_yellow_tang`; source `fish_001_yellow_tang.blend`, runtime `fish_001_yellow_tang.glb`. |
| Blender meshes/parts | `fish_001_yellow_tang__body`, `__tail_fin`, `__eye_l`; no Blender `.001` suffixes in export selection. |
| Armature object | `fish_001_yellow_tang__armature`; deform bones use the [rig roles](rigging-standards.md). |
| Materials | `mat__fish_yellow`, `mat__eye_black` or another stable palette/role name; share where practical. |
| Actions | `fish_001_yellow_tang__Swim`, `__Glide`, `__SwimFast`; keep the role spelling consistent. |

The [Python naming helpers](../../tools/asset-factory/blender/common/naming.py) validate canonical IDs and build stems, object names and Action names. They do not allocate numbers or operate a registry. Record assigned IDs in a future manifest alongside the actual source and output paths.

## First registered fish

The owner's **FAF-001 Yellow Tang** prototype alias is preserved in its history. The first original fish has canonical ID **FAF-FISH-001** and stem `fish_001_yellow_tang`. Its [candidate record](fish-001/README.md) identifies the source, runtime GLB, evidence and review state. It is separate from the third-party Sea life Yellow Tang candidate.

The first rock family has canonical ID **FAF-ROCK-001**. Its four reviewed variants use stable A/B/C/D suffix labels (FAF-ROCK-001A through D) and file stems `rock_001a_reef_boulder` through `rock_001d_reef_boulder`. The suffix denotes a member of the family; the family ID remains the registry identity. See the [rock candidate record](rock-001/README.md).

The first four coral families have canonical IDs **FAF-CORAL-001–004**. Each has A/B/C variants and stems such as `coral_001a_branching`, `coral_002a_rounded`, `coral_003a_tube`, and `coral_004a_fan`. The suffix is a stable family-member label; see the [coral pack record](coral-pack-001/README.md) for all twelve IDs and seeds.
