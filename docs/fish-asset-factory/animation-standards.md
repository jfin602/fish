# Animation standards

Reusable Action roles for a rigged fish are `Idle` or `Glide`, `SwimSlow`, `Swim`, and `SwimFast`; add `TurnLeft` / `TurnRight` when useful, plus explicitly named species actions only when the model actually needs them. The [asset naming contract](asset-ids.md) defines exported Action names. A family generator may produce a subset for an experiment, but the candidate record must say what exists and what remains unverified.

Motion should remain smooth and organic despite low-poly surfaces. Where anatomically appropriate, use a traveling body wave rather than rotating the entire fish as a rigid prop. Tuna and sharks rely more on tail drive; eel-like species bend more of the body. Restrained pectoral and other fin motion should follow species anatomy. Preserve a comparatively stable head/eyes. Check loops at the first/last pose and during playback for visible seams, clipping, collapse and fin snapping. Verify actual imported Actions in Godot before saying they work.

Godot owns authoritative position, heading, speed, turns and gameplay state. Blender Actions provide presentation/deformation; exported clips should not translate the game root through the world. Map action playback to motor presentation state during integration, without giving animation control of the simulation.

For [developmental fish assets](life-stages-001/README.md), keep the `Glide`, `Swim`, `SwimFast` role names and add the stage suffix to each non-adult Action stem. Tune cadence and wave amplitude by species and stage instead of scaling an adult Action. Eggs have no swimming Actions. Imported clips must loop with a clean seam; neither Blender nor Godot animation may supply game translation.

The first [Pond fish pack](pond-fish-pack-001/README.md) adds `Idle`, `Glide`, `SwimSlow`, `Swim`, `SwimFast`, `TurnLeft`, and `TurnRight` for each swimming stage. Its rigged Eggs instead have a looping deforming `Idle` and a one-shot `Hatch`; neither role is a swim cycle. Turns are one-shot; the other five swimming roles loop.
