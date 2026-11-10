"""Reusable presentation-only fish body-wave Action authoring."""

import math


def make_wave_action(armature, name: str, frames: int, profile: dict[str, tuple[float, float]], samples: int = 16):
    """Create one loop, keying local Z bend while leaving root translation alone.

    Each profile entry is bone: (amplitude radians, cycle phase). Species choose
    their own profile. The first and last samples have identical pose.
    """
    import bpy

    if frames < 8 or samples < 8:
        raise ValueError("wave needs enough frames and samples")
    if bpy.data.actions.get(name):
        raise ValueError(f"Action already exists: {name}")
    for bone_name in profile:
        if armature.pose.bones.get(bone_name) is None:
            raise ValueError(f"missing animated bone: {bone_name}")
    armature.animation_data_create()
    action = bpy.data.actions.new(name=name)
    action.use_fake_user = True
    armature.animation_data.action = action
    for bone in armature.pose.bones:
        bone.rotation_mode = "XYZ"
        bone.rotation_euler = (0, 0, 0)
    for step in range(samples + 1):
        cycle = step / samples
        frame = 1 + frames * cycle
        for bone_name, (amplitude, phase) in profile.items():
            bone = armature.pose.bones[bone_name]
            bone.rotation_euler.z = amplitude * math.sin(2 * math.pi * (cycle - phase))
            bone.keyframe_insert(data_path="rotation_euler", frame=frame, group=bone_name)
    track = armature.animation_data.nla_tracks.new()
    track.name = name
    track.strips.new(name, 1, action)
    armature.animation_data.use_nla = False
    return action


def reset_pose(armature) -> None:
    for bone in armature.pose.bones:
        bone.rotation_mode = "XYZ"
        bone.rotation_euler = (0, 0, 0)
