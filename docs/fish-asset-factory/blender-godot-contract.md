# Blender → GLB → Godot contract

FAF source or [generator](generators.md) → Blender → [visual inspection](visual-review.md) → rig/animation when applicable → technical validation → **selected GLB/glTF export** → Godot import → native Fish inspection. Generated production assets use GLB by default. A successful Blender run or headless import does not complete visual acceptance.

## Source and export

- Keep editable first-party `.blend` masters and reusable generator code separately. Generator source in `tools/asset-factory/` is tracked; local experiments, caches, screenshots and temporary exports stay in ignored `artifacts/` until a reviewed asset is deliberately placed in tracked art/runtime locations. The reviewed runtime `.glb` is an output, not the editable source.
- Use real-world meters and the [modeling axes, origins and transform rules](modeling-standards.md). Apply object transforms before export while preserving armature rest/bind correctness. Record intended dimensions and the imported Godot dimensions.
- Use named, simple materials that survive glTF. Inspect roughness, transparency, color spaces and any texture dependencies after import. Keep IDs/object/material/Action names per [asset IDs](asset-ids.md).
- Select only intended render meshes and the clean export armature. Exclude cameras, lights, reference images, hidden construction objects and Blender control widgets. Export Actions only when authored, named and inspected; verify each intended clip and loop in Godot. Keep authoritative movement in Godot.
- Reimport the GLB into the target Fish project and inspect scale, forward/up, origin, meshes, materials, skeleton, clip names, deformation and normals. Check a native Forward+ view at actual game scale and density before production acceptance. Do not change existing gameplay assets as part of this bootstrap.

## Candidate record

A small Markdown/JSON record is sufficient; no database exists yet. Capture `faf_id` (or experiment alias), `category`, `species_or_type`, `dimensions_m`, `triangle_count`, `material_count`, `armature_bone_count`, `animation_actions`, `generator_name_version`, `blender_source`, `glb_output`, `style_review_state`, and `technical_validation_state`. Fish also record `breathing_anatomy`, breathing rig/weight status, and exported breathing-joint check; a pending mechanism cannot be described as technically breathing-complete. Add reviewer/date, Blender/Godot version, screenshots and exceptions when actually observed. Use explicit `unknown`/`not tested` instead of invented numbers. Technical validation and style review are independent states. [Visual review](visual-review.md) defines acceptance evidence.

For a developmental representation, retain the fish's canonical FAF ID and add `faf_life_stage` to the record and exported GLB extras. Use a stage-suffixed file stem only for non-adults; do not overwrite adult sources, GLBs or import settings. Record stage-specific dimensions, rig/breathing classification and Actions. Configure and inspect Godot loop import settings for every exported swimming clip.
