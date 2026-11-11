# Asset IDs and names

Assign a stable ID when an original asset is registered for production: `FAF-<CATEGORY>-<NNN>` with categories `FISH`, `CORAL`, `ROCK`, `PLANT`, `TERRAIN`, `DETAIL` and three or more digits, for example `FAF-FISH-001`. Keep an assigned ID even if the display name or generator changes. Never recycle or casually renumber it. The ID describes identity; a variant/revision is separate metadata.

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

The first varied fish pack registers **FAF-FISH-002–005** as Ocellaris Clownfish (`fish_002_ocellaris_clownfish`), Blue-Green Chromis (`fish_003_blue_green_chromis`), Reef Butterflyfish (`fish_004_reef_butterflyfish`), and Blacktip Reef Shark (`fish_005_blacktip_reef_shark`). Their [pack record](reef-fish-pack-001/README.md) and [manifest](../../assets/faf/fish/reef_fish_002_to_005_manifest.json) own source/output paths and technical identity. FAF-FISH-001 remains separately authored and preserved.

Developmental representations retain their species' `FAF-FISH-00X` identity. A non-adult export adds a double-underscore stage suffix to the canonical stem, for example `fish_002_ocellaris_clownfish__juvenile.glb`. Use lowercase machine stage names (`egg`, `larva`, `juvenile`, `subadult`, `newborn_pup`, `early_juvenile`) and record `faf_life_stage` in GLB metadata and the [life-stage manifest](../../assets/faf/fish/life_stages_001_to_005_manifest.json). The unchanged canonical GLB is `adult`; the stage suffix does not allocate a new FAF ID.

The first freshwater Pond fish identities are **FAF-FISH-006–010**: `fish_006_bluegill`, `fish_007_largemouth_bass`, `fish_008_channel_catfish`, `fish_009_common_carp`, and `fish_010_northern_pike`. All five stages, including Adult, use `__egg`, `__larva`, `__juvenile`, `__subadult`, or `__adult` on their file and Action stems. `faf_habitat=pond` is required metadata on these exports. The [Pond pack record](pond-fish-pack-001/README.md) and [manifest](../../assets/faf/pond/fish/pond_fish_001_manifest.json) own the full inventory. None of these IDs or files replace the Ocean families.

The first rock family has canonical ID **FAF-ROCK-001**. Its four reviewed variants use stable A/B/C/D suffix labels (FAF-ROCK-001A through D) and file stems `rock_001a_reef_boulder` through `rock_001d_reef_boulder`. The suffix denotes a member of the family; the family ID remains the registry identity. See the [rock candidate record](rock-001/README.md).

The first four coral families have canonical IDs **FAF-CORAL-001–004**. Each has A/B/C variants and stems such as `coral_001a_branching`, `coral_002a_rounded`, `coral_003a_tube`, and `coral_004a_fan`. The suffix is a stable family-member label; see the [coral pack record](coral-pack-001/README.md) for all twelve IDs and seeds.

The first vegetation families have canonical IDs **FAF-PLANT-001–003**. Sea grass and broad seaweed use A/B/C; reef algae uses A/B. Stems include `plant_001a_sea_grass`, `plant_002a_broad_seaweed`, and `plant_003a_reef_algae`. The [vegetation pack record](plant-pack-001/README.md) lists all eight variants and seeds.

The first Pond-only foliage families reserve **FAF-PLANT-004–009**: lily pad (A/B/C), tall grass (A/B), cattail (A/B), reed (A/B), submerged algae (A/B), and floating muck (A/B). The family ID stays stable while its letter denotes the variant. All GLBs carry `faf_habitat=pond`; [the Pond foliage record](pond-foliage-pack-001/README.md) and [manifest](../../assets/faf/pond/plants/pond_foliage_001_manifest.json) own the 13 stems, seeds, costs and placement anchors. The Ocean 001–003 identities are unchanged.

The first small-detail families have canonical IDs **FAF-DETAIL-001–004**, each with A/B variants. They cover shell, rubble, pebble and sponge forms; stems include `detail_001a_fan_shell` and `detail_004b_cup_sponge`. The [detail pack record](detail-pack-001/README.md) owns the eight candidates and their seeds.
