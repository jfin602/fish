# FAF-FISH-001 Yellow Tang candidate record

**Status, 2026-11-10:** The first original FAF fish is technically validated through Blender, GLB export, Godot import, Action playback and a native Forward+ proof. It is a reference **candidate**. Final style approval still needs a view beside the accepted reef pieces and from the gameplay camera, as required by [visual review](../visual-review.md). The current Fish player and movement are unchanged.

| Field | Result |
| --- | --- |
| `faf_id` / category / species | `FAF-FISH-001` / `FISH` / Yellow Tang; former prototype alias `FAF-001` |
| Blender source | [`assets/faf/fish/fish_001_yellow_tang.blend`](../../../assets/faf/fish/fish_001_yellow_tang.blend) |
| Runtime GLB | [`game/assets/faf/fish/fish_001_yellow_tang.glb`](../../../game/assets/faf/fish/fish_001_yellow_tang.glb) |
| Dimensions | Blender bounding box 0.0563 × 0.2370 × 0.1450 m (X/Y/Z); Godot imported bounds 0.0563 × 0.1450 × 0.2370 m (X/Y/Z) |
| Geometry / materials | 1,100 triangles; 8 mesh parts; 6 named materials |
| Rig | 1 armature, 12 deform bones; root at body pivot; mesh and armature scales 1 |
| Actions | `fish_001_yellow_tang__Glide`, `__Swim`, `__SwimFast`; 24 FPS; 2.04 s, 1.29 s, 0.79 s respectively; each imported as a linear loop |
| Tooling | `tools/asset-factory/blender/fish/rig.py`, `animation.py`, `yellow_tang_001.py`; common selected-object GLB export |
| Technical validation | Passed Blender skin and Action checks, GLB structure inspection, Godot import and automated scene check, and native Forward+ scene capture |
| Style review | Blender views reviewed against the [reef target](../references/reef-style-target.png); native isolated proof reviewed; reef-context approval pending |

## Model and rig

The prototype's recognizable oval, laterally compressed body, pointed snout, forked tail, dorsal/anal fins and restrained facets were retained. No geometry or proportion change was justified. The mesh and armature rest data were rotated from prototype +X to FAF Blender -Y forward, then canonically renamed. The source remains centered at the body pivot with applied identity object transforms. The eyes are rigidly weighted to `spine_01`.

The exported hierarchy is `root → spine_01 → spine_02 → spine_03 → spine_04 → tail_01 → tail_02 → tail_fin`. `pectoral_l` and `pectoral_r` branch from `spine_02`; `dorsal` and `anal` branch from `spine_03`. Body weights follow species-specific longitudinal stations; fin-root weights stay attached. Both strong-tail directions were inspected without visible gaps or tail-base collapse. The extreme bend does reveal a sharp low-poly angle at the peduncle, so it should remain outside ordinary swim amplitudes.

The three Actions use a traveling wave with a stable front body and progressively larger tail motion. Root translation is unanimated. First/last rotation differences were zero in Blender. The native Godot check confirmed a changing `tail_02` pose during `Swim` and all three imported clips playing as loops.

## Godot proof and evidence

[`game/scenes/faf_fish_001_proof.tscn`](../../../game/scenes/faf_fish_001_proof.tscn) is an isolated scene. It instantiates the new GLB, rotates the instance 180° around Y to face Godot -Z, lights it, and plays `Swim`; keys 1/2/3 select the three Actions. The imported asset itself has one 12-bone skeleton, eight meshes, six materials and three looping Actions. Its head-to-tail rest vector is +Z in imported Godot coordinates; the fixture rotation resolves the scene-facing direction. [`game/dev/faf_fish_001_check.gd`](../../../game/dev/faf_fish_001_check.gd) checks these properties and visible tail pose change. Native Forward+ capture used Godot 4.7.2 on an AMD Radeon 880M.

Blender viewport evidence: [neutral side](evidence/01-neutral-side.png), [three-quarter](evidence/02-neutral-three-quarter.png), [front](evidence/03-neutral-front.png), [Swim deformation](evidence/04-swim-deformation-top.png), [strong bend](evidence/05-strong-tail-bend.png), [opposite bend](evidence/06-strong-tail-opposite.png). Godot evidence: [native Swim view](evidence/07-godot-native.png).

## Reuse and remaining work

Shared tooling covers bone role normalization, deterministic station weights, weighted skin reporting, and wave Action creation. `yellow_tang_001.py` owns the Yellow Tang geometry map, skin stations and motion profile; these are intentionally species-specific. The common export helper selects authored Actions and rest armature state. The next asset can reuse the pipeline without inheriting this body shape.

Remaining weaknesses: isolated proof lighting is simpler than the accepted reef; extreme tail bends are visibly angular; full reef-context style approval and gameplay-camera review are untested. The asset is ready to serve as the **first technical reference candidate**, not yet an approved replacement for the player fish.
