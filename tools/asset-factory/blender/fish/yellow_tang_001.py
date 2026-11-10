"""FAF-FISH-001: adapt the owner's existing Yellow Tang prototype.

Run once on a copied prototype .blend. This is the species-specific recipe;
shared rig and animation rules live in sibling modules.
"""

import math

import bpy

from common.naming import action_name, asset_stem, object_name
from fish.animation import make_wave_action, reset_pose
from fish.rig import rename_deform_bones, rotate_asset_data, skin_body_at_stations, skin_report


FAF_ID = "FAF-FISH-001"
SLUG = "yellow_tang"
STEM = asset_stem(FAF_ID, SLUG)

MESH_PARTS = {
    "01 Body": "body",
    "02 Caudal fin": "tail_fin",
    "03 Dorsal fin": "dorsal_fin",
    "04 Anal fin": "anal_fin",
    "05 Left pectoral fin": "pectoral_l",
    "05 Right pectoral fin": "pectoral_r",
    "06 Left eye": "eye_l",
    "06 Right eye": "eye_r",
}

BONE_NAMES = {
    "body_01": "spine_01", "body_02": "spine_02",
    "body_03": "spine_03", "body_04": "spine_04",
    "dorsal_fin": "dorsal", "anal_fin": "anal",
    "pectoral_L": "pectoral_l", "pectoral_R": "pectoral_r",
}

# Front coordinate is -Y after the +X prototype is rotated to the FAF axis.
# Keep the head rigid, blend successive body stations, then flex the peduncle.
BODY_STATIONS = [
    (-0.087, {"tail_01": 0.69, "tail_02": 0.31}),
    (-0.077, {"spine_04": 0.11, "tail_01": 0.89}),
    (-0.064, {"spine_04": 0.58, "tail_01": 0.42}),
    (-0.048, {"spine_03": 0.12, "spine_04": 0.88}),
    (-0.030, {"spine_03": 0.60, "spine_04": 0.40}),
    (-0.010, {"spine_02": 0.125, "spine_03": 0.875}),
    (0.010, {"spine_02": 0.625, "spine_03": 0.375}),
    (0.030, {"spine_01": 0.118, "spine_02": 0.882}),
    (0.048, {"spine_01": 0.541, "spine_02": 0.459}),
    (0.064, {"spine_01": 0.918, "spine_02": 0.082}),
    (0.099, {"spine_01": 1.0}),
]

# Yellow Tangs use a restrained body wave with the largest motion in the tail.
WAVE = {
    "spine_02": (0.016, 0.34),
    "spine_03": (0.035, 0.25),
    "spine_04": (0.075, 0.17),
    "tail_01": (0.14, 0.10),
    "tail_02": (0.22, 0.03),
    "tail_fin": (0.11, -0.02),
    "pectoral_l": (0.028, 0.25),
    "pectoral_r": (-0.028, 0.25),
    "dorsal": (0.012, 0.17),
    "anal": (-0.012, 0.17),
}


def build() -> dict:
    armature = bpy.data.objects.get("FAF-001 Yellow Tang Rig")
    if armature is None or any(bpy.data.objects.get(name) is None for name in MESH_PARTS):
        raise ValueError("open a copied FAF-001 Yellow Tang prototype before building")
    if bpy.data.actions:
        raise ValueError("prototype unexpectedly has Actions; review before adapting")
    meshes = [bpy.data.objects[name] for name in MESH_PARTS]
    rotate_asset_data(meshes, armature, -math.pi / 2)
    rename_deform_bones(armature, meshes, BONE_NAMES)
    skin_body_at_stations(bpy.data.objects["01 Body"], BODY_STATIONS)
    for old, part in MESH_PARTS.items():
        bpy.data.objects[old].name = object_name(FAF_ID, SLUG, part)
    armature.name = object_name(FAF_ID, SLUG, "armature")
    materials = {
        "FAF Yellow Tang | body yellow": "mat__fish_yellow",
        "FAF Yellow Tang | sunlit facet": "mat__fish_sunlit",
        "FAF Yellow Tang | warm facet": "mat__fish_warm",
        "FAF Yellow Tang | fin yellow": "mat__fin_yellow",
        "FAF Yellow Tang | fin edge gold": "mat__fin_edge_gold",
        "FAF Yellow Tang | near-black eye": "mat__eye_black",
    }
    for old, new in materials.items():
        material = bpy.data.materials.get(old)
        if material is None:
            raise ValueError(f"missing source material: {old}")
        material.name = new
    report = skin_report(meshes, armature)
    if report["problems"]:
        raise ValueError(f"skin validation failed: {report['problems'][:10]}")
    for role, frames, scale in (("Glide", 48, 0.32), ("Swim", 30, 1.0), ("SwimFast", 18, 1.35)):
        profile = {name: (amplitude * scale, phase) for name, (amplitude, phase) in WAVE.items()}
        make_wave_action(armature, action_name(FAF_ID, SLUG, role), frames, profile)
    armature.animation_data.action = bpy.data.actions[action_name(FAF_ID, SLUG, "Glide")]
    armature.animation_data.use_nla = False
    bpy.context.scene.frame_set(1)
    reset_pose(armature)
    bpy.context.view_layer.update()
    return {"faf_id": FAF_ID, "meshes": len(meshes), "bones": len(armature.data.bones),
            "actions": sorted(action.name for action in bpy.data.actions), "skin": report}
