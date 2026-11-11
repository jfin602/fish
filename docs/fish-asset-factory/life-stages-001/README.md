# FAF-FISH-001–005 life-stage asset family

**Candidate state, 2026-11-10:** Twenty new developmental assets surround the five unchanged canonical Adults, for **25 representations** total. The assets are source-controlled, deterministic, rigged where swimming, exported as individual GLBs, and imported in an isolated Godot Forward+ viewer. This is an asset/style checkpoint, not gameplay growth approval or a P4 implementation.

## Identity, source, and inspection

Life stage is metadata on an existing `FAF-FISH-00X` identity. Non-adult file stems use `fish_00x_species__life_stage`; the five existing adult stems and files remain unchanged. Each new mesh or armature exports `faf_id` and `faf_life_stage` GLB extras. The [source master](../../../assets/faf/fish/life_stages_001_to_005.blend) contains the twenty identity-transform source scenes; the separate [review master](../../../assets/faf/fish/life_stages_review.blend) instances them with the five canonical adults. Its display-only scaling normalizes each species row, and eggs are enlarged ninefold for visibility. Neither staging transform enters a GLB. The [manifest](../../../assets/faf/fish/life_stages_001_to_005_manifest.json) owns exact generator parameters, source and GLB paths, bones, materials, and loop checks.

The source recipe is [`fish.life_stages`](../../../tools/asset-factory/blender/fish/life_stages.py), built by [`build_life_stages.py`](../../../tools/asset-factory/blender/fish/build_life_stages.py). Species-specific adult loft/fin recipes remain in `reef_families.py`; the Tang uses a separate juvenile loft because its canonical adult was hand authored. Development profiles independently change head length/fullness, peduncle, depth, width, eye ratio, fin and caudal extent, physical size, cadence, and wave amplitude. The four bony-fish larvae use fewer body rings and eye facets. The palette is explicitly staged: subdued protective olive/sand/rust/gray-teal/slate early colors progress to the existing adult yellow, orange, blue-green, cream/yellow, and slate. Clownfish gain one juvenile head bar then full subadult bars; Butterflyfish gains a juvenile eye band before the adult pattern. No shark egg or egg case is authored; the live-bearing species starts at `newborn_pup`.

## Inventory and measured source cost

Dimensions are source Blender metres, **X × Y × Z**. The new assets were measured from source geometry; adult values are the existing canonical records. Every swimming entry has `Glide`, `Swim`, and `SwimFast`, with stage-specific wave/cadence and no animated gameplay root. `—` means a mesh-only egg. Each new GLB is at `game/assets/faf/fish/<stem>.glb`.

| FAF identity | Stage | Dimensions (m) | Triangles | Materials | Bones |
| --- | --- | ---: | ---: | ---: | ---: |
| 001 Yellow Tang | Egg | .0030 × .0022 × .0034 | 40 | 2 | 0 |
| 001 | Larva | .0046 × .0299 × .0107 | 332 | 2 | 14 |
| 001 | Juvenile | .0158 × .0923 × .0443 | 660 | 3 | 14 |
| 001 | Subadult | .0315 × .1695 × .0978 | 660 | 3 | 14 |
| 001 | **Adult, canonical** | .0563 × .2370 × .1450 | 1,100 | 6 | 14 |
| 002 Ocellaris Clownfish | Egg | .0024 × .0018 × .0027 | 40 | 2 | 0 |
| 002 | Larva | .0057 × .0210 × .0079 | 320 | 2 | 14 |
| 002 | Juvenile | .0168 × .0613 × .0282 | 720 | 3 | 14 |
| 002 | Subadult | .0392 × .1159 × .0702 | 912 | 3 | 14 |
| 002 | **Adult, canonical** | .0553 × .1540 × .1020 | 912 | 3 | 14 |
| 003 Blue-Green Chromis | Egg | .0020 × .0015 × .0023 | 40 | 2 | 0 |
| 003 | Larva | .0047 × .0209 × .0054 | 328 | 3 | 13 |
| 003 | Juvenile | .0144 × .0582 × .0198 | 404 | 3 | 13 |
| 003 | Subadult | .0325 × .1080 × .0475 | 424 | 3 | 13 |
| 003 | **Adult, canonical** | .0473 × .1450 × .0710 | 404 | 3 | 13 |
| 004 Reef Butterflyfish | Egg | .0026 × .0020 × .0029 | 40 | 2 | 0 |
| 004 | Larva | .0049 × .0295 × .0103 | 352 | 3 | 14 |
| 004 | Juvenile | .0151 × .0855 × .0391 | 896 | 3 | 14 |
| 004 | Subadult | .0289 × .1554 × .1035 | 896 | 3 | 14 |
| 004 | **Adult, canonical** | .0380 × .2010 × .1590 | 896 | 3 | 14 |
| 005 Blacktip Reef Shark | NewbornPup | .2150 × .4761 × .0948 | 944 | 3 | 16 |
| 005 | EarlyJuvenile | .3334 × .7254 × .1609 | 944 | 3 | 16 |
| 005 | Juvenile | .4608 × .9809 × .2419 | 972 | 3 | 16 |
| 005 | Subadult | .6156 × 1.2763 × .3460 | 972 | 3 | 16 |
| 005 | **Adult, canonical** | .7800 × 1.5700 × .4600 | 876 | 3 | 14 |

The Chromis remains one of the least expensive swimming families. The shark stages cost slightly more than the canonical adult because four visible, broad gill-slit marks are added on each side. Egg cost is 40 triangles with no swimming skeleton.

## Rig, Actions, and breathing

Swimming stages retain `root → spine_* → tail_* → tail_fin`, species-appropriate pectoral/dorsal/anal branches, and a stable head/eye region. The generated bony fish use localized `gill_L` and `gill_R` surface controls parented to `spine_01`. The new shark stages use the same predictable side labels parented to `spine_02` behind the eye, with local surface weighting and four dark slit marks on each side. These are **exposed gill-region controls**, not operculum plates. The existing adult shark is preserved with its documented `exposed_gill_slits_pending` status and 14-bone rig; breathing-complete adult shark remains a future correction. Eggs have no rig or Actions.

Each stage Action name includes the stage suffix, for example `fish_002_ocellaris_clownfish__juvenile__Swim`. All 16 new swimming stages export three positive-length loops. Godot `.glb.import` sidecars set linear looping for those 48 clips. No Action moves the gameplay root. The [deformation measurements](deformation-report.json) checked all new stages: neutral source equivalence, changing body pose in `SwimFast`, localized left/right gill response, and stable eye geometry. No breathing Action or gameplay driver was created.

## Technical and visual evidence

- Blender 5.2.3 LTS generated source and GLBs. [`check_life_stages.py`](../../../tools/asset-factory/blender/fish/check_life_stages.py) passed 20 new GLB identity/metadata/triangle/material/skin/Action checks and confirmed 25 total representations. [`check_life_stage_deformation.py`](../../../tools/asset-factory/blender/fish/check_life_stage_deformation.py) passed all 16 swimming stages. [`configure_life_stage_imports.py`](../../../tools/asset-factory/blender/fish/configure_life_stage_imports.py) owns the new loop import settings.
- Godot 4.7.2 Forward+ imported every GLB. [`faf_life_stages_check.gd`](../../../game/dev/faf_life_stages_check.gd) passed all 25 instances, including meshes, materials, skeleton roles, gill roles, three looping clips per swimmer, tail motion, stable head, and no imported translation tracks. The isolated [`faf_life_stages_proof.tscn`](../../../game/scenes/faf_life_stages_proof.tscn) selects species/stage and near or wider camera distance without touching normal play.
- Blender side, three-quarter, and swimming-pose renders exist for [all five rows](evidence/overview_side.png), including [clown progression](evidence/faf_fish_002_side.png) and [shark progression](evidence/faf_fish_005_side.png). Native examples: [clown juvenile](evidence/godot-clown-juvenile.png), [Chromis larva](evidence/godot-chromis-larva.png), [Butterflyfish juvenile](evidence/godot-butterfly-juvenile.png), [shark pup](evidence/godot-shark-pup.png), and [wider clown distance](evidence/godot-clown-juvenile-game-distance.png).

The lineup reads as five continuous species families rather than size-only copies. The shark pup is unmistakably a shark; Tang and Butterflyfish gain their taller adult silhouettes; clown bands and each species' adult coloration arrive gradually. Broad facet scale and simple materials match the FAF reference direction. **Style approval remains open:** eggs and larvae are necessarily hard to inspect at actual game distance, the earliest bony larvae have weak species-specific markings, and the flat fin polygons are conspicuous in close views. Gill motion is intentionally subtle and may need a later camera-scale presentation adjustment. The review viewer uses simple blue water and light, not the full integrated reef, so owner/native reef-context approval remains separate.

Canonical adult Blender files, adult GLBs, adult import options, player, Character Select, habitat, growth systems, and all gameplay behavior were not replaced by this task.
