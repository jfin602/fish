# FAF-FISH-001 Yellow Tang candidate record

**Status, 2026-11-10:** The first original FAF fish is technically validated through Blender, GLB export, Godot import, Action playback and a native Forward+ proof. It is a reference **candidate**. Final style approval still needs a view beside the accepted reef pieces and from the gameplay camera, as required by [visual review](../visual-review.md). The current Fish player and movement are unchanged.

| Field | Result |
| --- | --- |
| `faf_id` / category / species | `FAF-FISH-001` / `FISH` / Yellow Tang; former prototype alias `FAF-001` |
| Blender source | [`assets/faf/fish/fish_001_yellow_tang.blend`](../../../assets/faf/fish/fish_001_yellow_tang.blend) |
| Runtime GLB | [`game/assets/faf/fish/fish_001_yellow_tang.glb`](../../../game/assets/faf/fish/fish_001_yellow_tang.glb) |
| Dimensions | Blender bounding box 0.0563 × 0.2370 × 0.1450 m (X/Y/Z); Godot imported bounds 0.0563 × 0.1450 × 0.2370 m (X/Y/Z) |
| Geometry / materials | 1,100 triangles; 8 mesh parts; 6 named materials |
| Rig | 1 armature, 14 deform bones including `gill_L` / `gill_R`; root at body pivot; mesh and armature scales 1 |
| Actions | `fish_001_yellow_tang__Glide`, `__Swim`, `__SwimFast`; 24 FPS; 2.04 s, 1.29 s, 0.79 s respectively; each imported as a linear loop |
| Tooling | `tools/asset-factory/blender/fish/rig.py`, `animation.py`, `yellow_tang_001.py`; common selected-object GLB export |
| Technical validation | Passed Blender skin and Action checks, GLB structure inspection, Godot import and automated scene check, and native Forward+ scene capture |
| Style review | Blender views reviewed against the [reef target](../references/reef-style-target.png); native isolated proof reviewed; reef-context approval pending |

## Model and rig

The prototype's recognizable oval, laterally compressed body, pointed snout, forked tail, dorsal/anal fins and restrained facets were retained. No geometry or proportion change was justified. The mesh and armature rest data were rotated from prototype +X to FAF Blender -Y forward, then canonically renamed. The source remains centered at the body pivot with applied identity object transforms. The eyes are rigidly weighted to `spine_01`.

The exported hierarchy is `root → spine_01 → spine_02 → spine_03 → spine_04 → tail_01 → tail_02 → tail_fin`. `gill_L` and `gill_R` now branch from `spine_01`; `pectoral_l` and `pectoral_r` branch from `spine_02`; `dorsal` and `anal` branch from `spine_03`. Body weights follow species-specific longitudinal stations; fin-root weights stay attached. Both strong-tail directions were inspected without visible gaps or tail-base collapse. The extreme bend does reveal a sharp low-poly angle at the peduncle, so it should remain outside ordinary swim amplitudes.

The three Actions use a traveling wave with a stable front body and progressively larger tail motion. Root translation is unanimated. First/last rotation differences were zero in Blender. The native Godot check confirmed a changing `tail_02` pose during `Swim` and all three imported clips playing as loops. The new gill bones remain neutral in these swimming Actions.

## Breathing rig correction — 2026-11-10

The Yellow Tang is classified `operculate_bony`. Its existing source geometry, materials, dimensions and three Actions were preserved. The new `gill_L` and `gill_R` deform bones are children of stable `spine_01`, placed behind the eyes and ahead of the pectoral roots. The original sculpt's left side is −X, so `opening_rotation_z` supplies opposite signed local Z rotations to open each side outward. `fish.yellow_tang_001` now creates this rig during the initial recipe and can upgrade the existing source through `add_breathing_to_existing`; the shared `fish.breathing` module owns deterministic localized weight transfer and validation. Ten body vertices per side carry gill influence. Eyes, snout and tail groups remain unchanged. Neutral evaluated mesh error was below 0.00000001 m; subtle and strong test poses displaced body vertices by at most 0.00063 m and 0.00139 m respectively, with zero eye movement. The exported GLB has 14 skin joints, including both correctly parented gill bones, and retains the three swimming clips.

Blender MCP viewport evidence: [neutral side](evidence/08-gill-neutral.png), [right strong side](evidence/09-gill-right-strong-side.png), [both strong front](evidence/10-gill-both-strong-front.png). The cover flare is very subtle against the Tang's bright yellow facet pattern; readability at the actual gameplay-camera distance remains unproven. This is a deform skeleton for future Godot-driven breathing, not an authored breathing animation.

The updated GLB passed the focused Godot 4.7.2 headless import and `faf_fish_001_check.gd` checks for 14 bones, paired gill parenting, meshes, materials, looping Actions and tail playback. No Godot breathing animation or native breathing playback was tested.

## Godot proof and evidence

[`game/scenes/faf_fish_001_proof.tscn`](../../../game/scenes/faf_fish_001_proof.tscn) is an isolated scene. It instantiates the GLB, rotates the instance 180° around Y to face Godot -Z, lights it, and plays `Swim`; keys 1/2/3 select the three Actions. The original native proof observed 12 bones before the breathing correction; the current GLB has 14 joints. It retains eight meshes, six materials and three looping Actions. Its head-to-tail rest vector is +Z in imported Godot coordinates; the fixture rotation resolves the scene-facing direction. [`game/dev/faf_fish_001_check.gd`](../../../game/dev/faf_fish_001_check.gd) now expects paired gill bones and checks visible tail pose change. Native Forward+ capture used Godot 4.7.2 on an AMD Radeon 880M.

Blender viewport evidence: [neutral side](evidence/01-neutral-side.png), [three-quarter](evidence/02-neutral-three-quarter.png), [front](evidence/03-neutral-front.png), [Swim deformation](evidence/04-swim-deformation-top.png), [strong bend](evidence/05-strong-tail-bend.png), [opposite bend](evidence/06-strong-tail-opposite.png). Godot evidence: [native Swim view](evidence/07-godot-native.png).

## Reuse and remaining work

Shared tooling covers bone role normalization, deterministic station weights, weighted skin reporting, and wave Action creation. `yellow_tang_001.py` owns the Yellow Tang geometry map, skin stations and motion profile; these are intentionally species-specific. The common export helper selects authored Actions and rest armature state. The next asset can reuse the pipeline without inheriting this body shape.

Remaining weaknesses: isolated proof lighting is simpler than the accepted reef; extreme tail bends are visibly angular; full reef-context style approval and gameplay-camera review are untested. The asset is ready to serve as the **first technical reference candidate**, not yet an approved replacement for the player fish.
