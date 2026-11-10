"""Focused Blender QA for the permanent FAF breathing contract.

Run: blender -b -t 2 --python tools/asset-factory/blender/fish/check_breathing.py -- --root "$PWD"
Does not save or export any source asset.
"""

from __future__ import annotations

import argparse
from dataclasses import replace
import json
import math
from pathlib import Path
import sys
import tempfile

import bpy

TOOLING = Path(__file__).resolve().parents[1]
if str(TOOLING) not in sys.path:
    sys.path.insert(0, str(TOOLING))

from fish.breathing import (EXPOSED_SLITS, GILL_NAMES, OPERCULATE,
                            opening_rotation_z, validate_breathing_rig,
                            validate_exported_breathing)
from fish.build_reef_pack_001 import create_scenes, inspect_loops
from fish.reef_families import breathing_spec
from fish.yellow_tang_001 import BREATHING as TANG_BREATHING, add_breathing_to_existing


def positions(obj):
    depsgraph = bpy.context.evaluated_depsgraph_get()
    evaluated_obj = obj.evaluated_get(depsgraph)
    mesh = evaluated_obj.to_mesh()
    try:
        return [tuple(v.co) for v in mesh.vertices]
    finally:
        evaluated_obj.to_mesh_clear()


def distance(a, b):
    return math.dist(a, b)


def pose_check(armature, meshes, spec):
    body = next(obj for obj in meshes if obj.name.endswith("__body"))
    eyes = [obj for obj in meshes if "__eye_" in obj.name]
    for bone in armature.pose.bones:
        bone.rotation_mode = "XYZ"
        bone.rotation_euler = (0, 0, 0)
    bpy.context.view_layer.update()
    baseline = positions(body)
    eye_baseline = {eye.name: positions(eye) for eye in eyes}
    checks = {}
    for label, sides, amount in (
        ("left_subtle", ("gill_L",), .45),
        ("right_subtle", ("gill_R",), .45),
        ("both_subtle", GILL_NAMES, .45),
        ("left_strong", ("gill_L",), 1.0),
        ("right_strong", ("gill_R",), 1.0),
    ):
        for bone in armature.pose.bones:
            bone.rotation_euler = (0, 0, 0)
        for name in sides:
            armature.pose.bones[name].rotation_euler.z = opening_rotation_z(spec, name, amount)
        bpy.context.view_layer.update()
        moved = positions(body)
        displacements = [distance(a, b) for a, b in zip(baseline, moved)]
        max_m = max(displacements)
        if max_m < .00008 or max_m > .012:
            raise AssertionError(f"{armature.name} {label} implausible movement {max_m}")
        # Head/snout lies ahead of the cover envelope; all such vertices stay.
        front = [d for vertex, d in zip(body.data.vertices, displacements)
                 if vertex.co.y < spec.center_y - spec.region_y]
        if front and max(front) > 1e-6:
            raise AssertionError(f"{armature.name} {label} moved skull/snout")
        eye_m = max(distance(a, b)
                    for eye in eyes for a, b in zip(eye_baseline[eye.name], positions(eye)))
        if eye_m > 1e-6:
            raise AssertionError(f"{armature.name} {label} moved eyes: {eye_m}")
        if not all(math.isfinite(v) for point in moved for v in point):
            raise AssertionError(f"{armature.name} {label} non-finite deformation")
        checks[label] = {"max_body_displacement_m": round(max_m, 7),
                         "max_eye_displacement_m": round(eye_m, 7)}
    for bone in armature.pose.bones:
        bone.rotation_euler = (0, 0, 0)
    bpy.context.view_layer.update()
    return checks


def check_existing_actions(armature, stem):
    names = [stem + "__" + role for role in ("Glide", "Swim", "SwimFast")]
    for name in names:
        action = bpy.data.actions.get(name)
        if action is None:
            raise AssertionError(f"missing Action {name}")
        armature.animation_data.action = action
        bpy.context.scene.frame_set(int(action.frame_range[0]))
        first = {bone.name: tuple(bone.rotation_euler) for bone in armature.pose.bones}
        bpy.context.scene.frame_set(int(action.frame_range[1]))
        last = {bone.name: tuple(bone.rotation_euler) for bone in armature.pose.bones}
        if max(abs(a-b) for bone in first for a, b in zip(first[bone], last[bone])) > 1e-6:
            raise AssertionError(f"Action seam mismatch: {name}")
        if any(abs(v) > 1e-8 for v in armature.pose.bones["root"].location):
            raise AssertionError(f"root motion in {name}")
        for gill in GILL_NAMES:
            if armature.pose.bones.get(gill) and any(abs(v) > 1e-8 for v in armature.pose.bones[gill].rotation_euler):
                raise AssertionError(f"breathing accidentally animated in {name}")
    return names


def main(root):
    results = []
    negative_cases = []
    built = create_scenes()
    for bp, scene, armature, meshes, report in built:
        bpy.context.window.scene = scene
        if bp.breathing_anatomy == OPERCULATE:
            spec = breathing_spec(bp)
            validation = validate_breathing_rig(armature, meshes, spec)
            poses = pose_check(armature, meshes, spec)
        else:
            validation = validate_breathing_rig(armature, meshes, None, require_complete=False)
            poses = None
            try:
                validate_breathing_rig(armature, meshes, None, require_complete=True)
            except ValueError:
                pass
            else:
                raise AssertionError("pending shark incorrectly passed final validation")
        actions = check_existing_actions(armature, bp.stem)
        glb = root / "game/assets/faf/fish" / (bp.stem + ".glb")
        export = validate_exported_breathing(glb, bp.breathing_anatomy,
                     require_complete=bp.breathing_anatomy == OPERCULATE)
        results.append({"faf_id": bp.faf_id, "validation": validation,
                        "poses": poses, "actions": actions, "export": export})
        if bp.number == 4:
            def must_reject(label, fn):
                try:
                    fn()
                except ValueError:
                    negative_cases.append(label)
                else:
                    raise AssertionError(f"validator accepted {label}")

            del armature["faf_breathing_anatomy"]
            must_reject("missing_classification", lambda: validate_breathing_rig(armature, meshes, spec))
            armature["faf_breathing_anatomy"] = OPERCULATE
            must_reject("wrong_parent", lambda: validate_breathing_rig(
                armature, meshes, replace(spec, parent_bone="spine_02")))
            body = next(obj for obj in meshes if obj.name.endswith("__body"))
            body.vertex_groups.remove(body.vertex_groups["gill_L"])
            must_reject("missing_weights", lambda: validate_breathing_rig(armature, meshes, spec))
            bpy.ops.object.select_all(action="DESELECT")
            armature.select_set(True)
            bpy.context.view_layer.objects.active = armature
            bpy.ops.object.mode_set(mode="EDIT")
            armature.data.edit_bones.remove(armature.data.edit_bones["gill_L"])
            bpy.ops.object.mode_set(mode="OBJECT")
            must_reject("missing_bone", lambda: validate_breathing_rig(armature, meshes, spec))
            # Rename one exported joint in a disposable GLB copy. The JSON
            # chunk length stays valid because the replacement is equal size.
            damaged = glb.read_bytes().replace(b"gill_L", b"gill_X", 1)
            with tempfile.NamedTemporaryFile(suffix=".glb") as temporary:
                temporary.write(damaged)
                temporary.flush()
                must_reject("missing_exported_bone", lambda: validate_exported_breathing(
                    temporary.name, OPERCULATE))
    bpy.ops.wm.open_mainfile(filepath=str(root / "assets/faf/fish/fish_001_yellow_tang.blend"))
    armature = bpy.data.objects["fish_001_yellow_tang__armature"]
    if not armature.get("faf_breathing_anatomy"):
        add_breathing_to_existing()
    meshes = [obj for obj in bpy.data.objects if obj.type == "MESH" and obj.parent == armature]
    validation = validate_breathing_rig(armature, meshes, TANG_BREATHING)
    poses = pose_check(armature, meshes, TANG_BREATHING)
    actions = check_existing_actions(armature, "fish_001_yellow_tang")
    export = validate_exported_breathing(root / "game/assets/faf/fish/fish_001_yellow_tang.glb", OPERCULATE)
    results.insert(0, {"faf_id": "FAF-FISH-001", "validation": validation,
                       "poses": poses, "actions": actions, "export": export})
    print("FAF_BREATHING_CHECK=" + json.dumps({"assets": results,
                                                "negative_cases": negative_cases}))


if __name__ == "__main__":
    args = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", required=True, type=Path)
    main(parser.parse_args(args).root.resolve())
