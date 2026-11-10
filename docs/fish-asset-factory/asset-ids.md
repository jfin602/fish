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

## Current experiment

The owner's **FAF-001 Yellow Tang** label is already attached to local ignored experimental Blender filenames. Preserve that label as an experiment alias; the current checkout contains no committed production ID assignment or source `.blend`. It is a modeling prototype only. Register a canonical category ID after source/provenance and review are ready, without silently renaming the local experiment or confusing it with the separate Sea life Yellow Tang candidate.
