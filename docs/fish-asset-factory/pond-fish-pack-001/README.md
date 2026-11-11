# Pond FAF fish pack 001 — FAF-FISH-006–010

**Candidate state, 2026-11-11:** Five original freshwater species, each with Egg, Larva, Juvenile, Subadult, and Adult: **25 separately rigged, individually exported assets**. Source, GLB, deformation, import, and representative native visual checks pass. The owner has not locked Pond art style, and this isolated pack does not qualify a Pond gameplay world.

## Identity and deliverables

| Stable species ID | Species | Editable Blender master | Stage-specific visual progression |
| --- | --- | --- | --- |
| FAF-FISH-006 | Bluegill | [master](../../../assets/faf/pond/fish/fish_006_bluegill.blend) | Tall compressed sunfish, continuous dorsal, small mouth, juvenile-to-adult bars and dark ear patch; muted gray-green becomes green/gold. |
| FAF-FISH-007 | Largemouth bass | [master](../../../assets/faf/pond/fish/fish_007_largemouth_bass.blend) | Longer predator body, paired dorsal sections, lateral stripe, paired visible gape wedges; beige-green becomes darker olive. |
| FAF-FISH-008 | Channel catfish | [master](../../../assets/faf/pond/fish/fish_008_channel_catfish.blend) | Broad flattened head, long anal fin, small adipose, forked tail, paired barbel groups; gray becomes slate-blue. |
| FAF-FISH-009 | Common carp | [master](../../../assets/faf/pond/fish/fish_009_common_carp.blend) | Deep body, long dorsal, short barbels, broad scale blocks; subdued tan becomes bronze/gold. |
| FAF-FISH-010 | Northern pike | [master](../../../assets/faf/pond/fish/fish_010_northern_pike.blend) | Long narrow ambush body, duckbill, rear dorsal/anal pair; juvenile bars shift toward pale adult spots on olive. |

Each master has five independent source scenes with identity object transforms, stage geometry, a deform armature, and named Actions. The [25-entry manifest](../../../assets/faf/pond/fish/pond_fish_001_manifest.json) records every GLB path, measured X × Y × Z dimensions, triangles, material count, bone names, Action names, loop roles, generator configuration, `faf_id`, `faf_life_stage`, and `faf_habitat=pond`. GLBs live only in [`game/assets/faf/pond/fish`](../../../game/assets/faf/pond/fish/). The owner-supplied `/home/jfin/best-pond-faf.png` guided the earthy, faceted colors and warm-on-olive presentation; its file was not redistributed.

Silhouette and marking cues were cross-checked against [Bluegill species notes](https://animaldiversity.org/accounts/Lepomis_macrochirus/), [largemouth bass identification](https://nas.er.usgs.gov/queries/FactSheet.aspx?SpeciesID=401), [channel catfish anatomy](https://freshwater-aquaculture.extension.org/wp-content/uploads/2019/08/Channel_Catfish_Life_History_and_Biology.pdf), [common carp anatomy](https://www.usgs.gov/labs/fish-health-program/science/koi-cyprinus-carpio-koi-fhp), and [northern pike identification](https://www.dnr.state.mn.us/minnaqua/speciesprofile/northern_pike.html). These are form references; all meshes and markings were built as original low-poly geometry.

All stages are independently authored from each species' station, fin, head, mouth, tail, and palette recipe. The shared loft, rig, and animation functions do not instantiate a scaled adult mesh. Early stages change head fullness, body depth/width, peduncle, eye size, fin extent, wave amplitude, and cadence. Larvae retain small species-specific mouths, bills or barbels where relevant. Eggs are species-sized, faceted shells with a visible embryo and a separate egg rig.

## Measured technical summary

Source dimensions are metres. The Y extent is the nose-to-tail axis in Blender; Godot import maps it to Z. The full exact measurements are in the manifest.

| FAF ID | Egg diameter class | Larva length | Juvenile length | Subadult length | Adult length | Triangle range | Materials | Bones |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| 006 Bluegill | .0035 | .0248 | .0997 | .2094 | .3098 | 160–796 | 2–5 | 4 egg / 13 swimmer |
| 007 Largemouth bass | .0041 | .0581 | .2351 | .4953 | .7337 | 160–808 | 2–5 | 4 / 13 |
| 008 Channel catfish | .0040 | .0638 | .2580 | .5436 | .8052 | 160–868 | 2–5 | 4 / 13 |
| 009 Common carp | .0048 | .0587 | .2376 | .5003 | .7410 | 160–816 | 2–5 | 4 / 13 |
| 010 Northern pike | .0038 | .0854 | .3451 | .7261 | 1.0752 | 160–776 | 2–5 | 4 / 13 |

Swimmers export `root → spine_01 → spine_02 → spine_03 → tail_01 → tail_02 → tail_fin`, pectoral L/R, dorsal, anal, and bilateral `gill_L` / `gill_R` deform controls. The head and eye geometry remain on the stable front bone. All 20 swimming assets have distinct `Idle`, `Glide`, `SwimSlow`, `Swim`, `SwimFast`, `TurnLeft`, and `TurnRight`. The five locomotion/neutral clips loop with matching seam poses; turns are one-shot. Species and stage tune both cadence and wave, with progressively stronger tail motion. No clip animates game-root travel. Gill bones have localized independent weights and are neutral in swimming Actions, ready for a later runtime breathing driver.

Each egg has a true four-bone deform rig (`root`, `shell_l`, `shell_r`, `embryo`) and separate Actions: looping `Idle` deforms shell/embryo, while one-shot `Hatch` opens the two shell halves and moves the embryo. Eggs have no swim Action. Imported Godot clip settings are owned by the 25 `.glb.import` files.

## Verification and visual review

- The [GLB/manifest checker](../../../tools/asset-factory/blender/fish/check_pond_pack_001.py) passed all 25 files: GLB metadata, counts, skin joints and exported egg/gill vertex weights on both sides, bone names, Actions, root/head stability, looping seams, stage growth, and file identity. Range: **160–868 triangles**, **2–5 materials**.
- The [Blender deformation checker](../../../tools/asset-factory/blender/fish/check_pond_deformation.py) passed all five masters and 25 stages. Its [per-species reports](evidence/deformation_006.json) cover egg Idle/Hatch mesh motion; neutral source equivalence; left and right localized gill movement; SwimFast, opposite strong turns, and stable eyes/head on each swimmer.
- Godot 4.7.2 imported all 25 GLBs. The [focused Godot checker](../../../game/dev/faf_pond_fish_check.gd) passed **25/25** actual instances: orientation and identity root, measured dimensions after axis conversion, exact triangle/material/bone counts against the manifest, every imported Action and loop setting, moving tail/egg controls, stable head, and no animated game root. The [isolated Forward+ viewer](../../../game/scenes/faf_pond_fish_proof.tscn) switches all species, stages, actions, near/game distances, and side/top/three-quarter/front views without loading normal play.
- Blender produced [side/top/three-quarter and deformation captures](evidence/faf_fish_006_adult_idle_side.png) for every species. Native examples include [bluegill side](evidence/godot-006-adult-swim-side-near.png), [catfish side](evidence/godot-008-adult-swim-side-near.png), [pike top](evidence/godot-010-adult-glide-top-near.png), [carp turning at game distance](evidence/godot-009-juvenile-turnleft-three_quarter-game.png), and [egg Hatch front](evidence/godot-006-egg-hatch-front-near.png). The native fixture uses olive distance fog, a warm surface light, and subdued ambient fill to approximate the Pond reference for asset inspection.

The species silhouettes read differently in side and top views. The bluegill/carp deep-body contrast, bass/pike longer forms, and catfish broad head and barbels are visible. Early stages lose the adult's stronger markings and use subdued camouflage. A first render pass exposed an inward body front cap and insufficient larval mouth cues; both were fixed in the shared recipes before final export. The native viewer's original near clip hid eggs; it was corrected and Hatch was recaptured.

Run the viewer scene directly in the Godot .NET editor. `Q/E` changes species, `A/D` changes stage, `1–7` selects the stage's Action (Egg uses `1–2`), `C` cycles side/top/three-quarter/front, and `V` toggles close inspection versus the current Fish fixed 3.8 m camera distance. The viewer never loads the player or Pond gameplay.

**Open visual limits:** Mouth/lip profiles and thin fin planes remain simplified at close range. The viewer's fixed game-distance mode uses the current Fish **3.8 m** follow distance from `SwimTuning`; at that distance juveniles are small and eggs/larvae nearly invisible. Pond-specific growth camera framing is not yet defined, so near mode is needed to judge early stages. The simple fixture does not include a complete Pond canopy, silt, wood cover, or moving gameplay camera, so in-world palette and motion approval remains unobserved. Native captures show selected poses rather than a human real-time playback session; the imported Action tracks and control motion were verified programmatically. These are production candidates, pending owner style review in a future integrated Pond context.

## Rebuild scope

The [Pond family recipe](../../../tools/asset-factory/blender/fish/pond_families.py) and [pack builder](../../../tools/asset-factory/blender/fish/build_pond_pack_001.py) own only FAF-FISH-006–010 Pond outputs. Rebuild from the repository root with `blender -b -t 2 --python tools/asset-factory/blender/fish/build_pond_pack_001.py -- --root "$PWD" --overwrite`, then run the checkers, `configure_pond_imports.py`, and Godot import proof. Source and GLB regeneration resets review status until checks are repeated. Ocean fish/GLBs, World Select, normal play, gameplay movement, and the project version were not changed.
