"""Anatomy-aware, deterministic breathing deformation for FAF fish.

The ordinary bony-fish implementation reassigns a small part of the existing
front-body skin to two child bones. Rest geometry and existing swim weights are
otherwise untouched. New shark developmental stages use paired localized
surface controls and visible slit marks; the canonical adult remains pending.
"""

from __future__ import annotations

from dataclasses import dataclass
import math

OPERCULATE = "operculate_bony"
EXPOSED_SLITS = "exposed_gill_slits_pending"
ANATOMY_TYPES = {OPERCULATE, EXPOSED_SLITS}
GILL_NAMES = ("gill_L", "gill_R")


@dataclass(frozen=True)
class OperculumSpec:
    """Metre-scale region on the body mesh, with Blender -Y forward and +Z up."""

    center_y: float
    center_z: float
    half_width: float
    region_y: float
    region_z: float
    parent_bone: str = "spine_01"
    max_weight: float = 0.90
    safe_angle_rad: float = 0.45
    left_x_sign: int = 1

    def validate(self):
        values = (self.center_y, self.center_z, self.half_width, self.region_y,
                  self.region_z, self.max_weight, self.safe_angle_rad)
        if any(not math.isfinite(v) for v in values):
            raise ValueError("operculum parameters must be finite")
        if min(self.half_width, self.region_y, self.region_z, self.max_weight,
               self.safe_angle_rad) <= 0 or self.max_weight > 1:
            raise ValueError("invalid operculum size, weight or angle")
        if self.left_x_sign not in (-1, 1):
            raise ValueError("left_x_sign must be -1 or 1")


def classify_breathing(armature, anatomy: str):
    if anatomy not in ANATOMY_TYPES:
        raise ValueError(f"missing or unknown breathing anatomy: {anatomy!r}")
    armature["faf_breathing_anatomy"] = anatomy
    armature["faf_breathing_status"] = (
        "rigged" if anatomy == OPERCULATE else "pending_specialized_deformation")


def add_operculum_rig(armature, spec: OperculumSpec):
    """Add two deform bones behind the eye, parented to the chosen forebody."""
    import bpy

    spec.validate()
    if any(armature.data.bones.get(name) for name in GILL_NAMES):
        raise ValueError("operculum bones already exist")
    if armature.data.bones.get(spec.parent_bone) is None:
        raise ValueError(f"missing operculum parent: {spec.parent_bone}")
    bpy.ops.object.mode_set(mode="OBJECT")
    bpy.ops.object.select_all(action="DESELECT")
    armature.select_set(True)
    bpy.context.view_layer.objects.active = armature
    bpy.ops.object.mode_set(mode="EDIT")
    try:
        for name, sign in (("gill_L", spec.left_x_sign),
                           ("gill_R", -spec.left_x_sign)):
            bone = armature.data.edit_bones.new(name)
            bone.head = (sign * spec.half_width * .72, spec.center_y, spec.center_z)
            bone.tail = (sign * spec.half_width * 1.16,
                         spec.center_y + spec.region_y * .32,
                         spec.center_z + spec.region_z * .12)
            bone.parent = armature.data.edit_bones[spec.parent_bone]
            bone.use_connect = False
            bone.use_deform = True
    finally:
        bpy.ops.object.mode_set(mode="OBJECT")
    classify_breathing(armature, OPERCULATE)


def _smooth_cap(distance: float) -> float:
    t = max(0.0, min(1.0, 1.0 - distance))
    return t * t * (3.0 - 2.0 * t)


def opening_rotation_z(spec: OperculumSpec, name: str, amount: float = 1.0) -> float:
    """Local Z rotation that moves the chosen side outward in this bone layout."""
    if name not in GILL_NAMES or not 0 <= amount <= 1:
        raise ValueError("invalid gill side or opening amount")
    sign = spec.left_x_sign if name == "gill_L" else -spec.left_x_sign
    return -sign * spec.safe_angle_rad * amount


def weight_operculum_region(body, spec: OperculumSpec):
    """Transfer only parent-bone weight to each localized lateral cover region."""
    spec.validate()
    parent = body.vertex_groups.get(spec.parent_bone)
    if parent is None:
        raise ValueError(f"body missing parent weight group {spec.parent_bone}")
    if any(body.vertex_groups.get(name) for name in GILL_NAMES):
        raise ValueError("body already has operculum groups")
    groups = {name: body.vertex_groups.new(name=name) for name in GILL_NAMES}
    counts = {name: 0 for name in GILL_NAMES}
    for vert in body.data.vertices:
        x, y, z = vert.co
        side = 1 if x > 0 else -1
        name = "gill_L" if side == spec.left_x_sign else "gill_R"
        longitudinal = _smooth_cap(abs(y - spec.center_y) / spec.region_y)
        vertical = _smooth_cap(abs(z - spec.center_z) / spec.region_z)
        lateral = _smooth_cap(max(0.0, .72 - abs(x) / spec.half_width) / .32)
        influence = spec.max_weight * longitudinal * vertical * lateral
        if influence < .005:
            continue
        try:
            old = parent.weight(vert.index)
        except RuntimeError:
            continue
        moved = old * influence
        if moved < .001:
            continue
        parent.add([vert.index], old - moved, "REPLACE")
        groups[name].add([vert.index], moved, "REPLACE")
        counts[name] += 1
    if min(counts.values()) < 3:
        raise ValueError(f"operculum lacks useful bilateral mesh coverage: {counts}")
    return counts


def validate_breathing_rig(armature, meshes, spec: OperculumSpec | None = None,
                           *, require_complete: bool = True, check_neutral: bool = True):
    """Fail incomplete breathing support; explicit shark pending remains visible."""
    anatomy = armature.get("faf_breathing_anatomy")
    if anatomy not in ANATOMY_TYPES:
        raise ValueError("FAF fish has no valid breathing anatomy classification")
    if anatomy == EXPOSED_SLITS:
        if any(armature.data.bones.get(name) for name in GILL_NAMES):
            raise ValueError("exposed-gill species must not receive false opercula")
        if armature.get("faf_breathing_status") != "pending_specialized_deformation":
            raise ValueError("exposed-gill species requires explicit pending status")
        if require_complete:
            raise ValueError("exposed-gill breathing deformation is still pending")
        return {"anatomy": anatomy, "complete": False, "status": "pending"}
    if spec is None:
        raise ValueError("operculate validation requires anatomical parameters")
    spec.validate()
    if armature.get("faf_breathing_status") != "rigged":
        raise ValueError("operculum rig is not marked rigged")
    for name in GILL_NAMES:
        bone = armature.data.bones.get(name)
        if bone is None or not bone.use_deform or bone.parent is None or bone.parent.name != spec.parent_bone:
            raise ValueError(f"missing, non-deforming or incorrectly parented {name}")
    body = next((obj for obj in meshes if obj.name.endswith("__body")), None)
    if body is None:
        raise ValueError("missing body mesh for operculum validation")
    counts = {}
    for name, sign in (("gill_L", spec.left_x_sign),
                       ("gill_R", -spec.left_x_sign)):
        group = body.vertex_groups.get(name)
        if group is None:
            raise ValueError(f"missing skin group {name}")
        affected = []
        for vert in body.data.vertices:
            try:
                weight = group.weight(vert.index)
            except RuntimeError:
                continue
            if weight > .001:
                affected.append((vert, weight))
        if len(affected) < 3:
            raise ValueError(f"insufficient localized weight on {name}")
        for vert, weight in affected:
            x, y, z = vert.co
            if x * sign <= 0 or abs(y - spec.center_y) >= spec.region_y or abs(z - spec.center_z) >= spec.region_z:
                raise ValueError(f"{name} weight escaped the gill region")
        counts[name] = len(affected)
    for obj in meshes:
        if "__eye_" in obj.name and any(obj.vertex_groups.get(name) for name in GILL_NAMES):
            raise ValueError("eye received operculum weight")
    neutral_error = 0.0
    if check_neutral:
        import bpy
        # Existing Actions may be assigned. Explicitly clear the pose, then
        # compare evaluated local mesh coordinates to unchanged source geometry.
        original = {b.name: tuple(b.rotation_euler) for b in armature.pose.bones}
        for bone in armature.pose.bones:
            bone.rotation_mode = "XYZ"
            bone.rotation_euler = (0, 0, 0)
        bpy.context.view_layer.update()
        depsgraph = bpy.context.evaluated_depsgraph_get()
        evaluated_obj = body.evaluated_get(depsgraph)
        evaluated = evaluated_obj.to_mesh()
        try:
            neutral_error = max((evaluated.vertices[i].co - vert.co).length
                                for i, vert in enumerate(body.data.vertices))
        finally:
            evaluated_obj.to_mesh_clear()
            for bone in armature.pose.bones:
                bone.rotation_euler = original[bone.name]
            bpy.context.view_layer.update()
        if neutral_error > 1e-5:
            raise ValueError(f"rest pose changed: {neutral_error:.8f} m")
    return {"anatomy": anatomy, "complete": True, "parent": spec.parent_bone,
            "weighted_vertices": counts, "neutral_error_m": neutral_error}


def validate_exported_breathing(glb_path, anatomy: str, *, parent_bone="spine_01",
                                require_complete=True):
    """Check the GLB skin itself, independent of Blender's editable armature."""
    import json
    from pathlib import Path
    import struct

    if anatomy not in ANATOMY_TYPES:
        raise ValueError("GLB breathing anatomy is missing or unknown")
    blob = Path(glb_path).read_bytes()
    if len(blob) < 20 or blob[:4] != b"glTF":
        raise ValueError("invalid GLB header")
    chunk_length, chunk_type = struct.unpack_from("<II", blob, 12)
    if chunk_type != 0x4E4F534A:
        raise ValueError("GLB has no JSON chunk")
    data = json.loads(blob[20:20 + chunk_length])
    nodes = data.get("nodes", [])
    skins = data.get("skins", [])
    if len(skins) != 1:
        raise ValueError("fish GLB requires exactly one export skin")
    names = {nodes[index].get("name") for index in skins[0]["joints"]}
    if anatomy == OPERCULATE:
        if not set(GILL_NAMES) <= names:
            raise ValueError("GLB skin omits required gill bones")
        parent_indices = {i for i, node in enumerate(nodes)
                          if node.get("name") == parent_bone}
        if len(parent_indices) != 1:
            raise ValueError("GLB omits the operculum parent bone")
        parent_index = next(iter(parent_indices))
        for name in GILL_NAMES:
            index = next(i for i, node in enumerate(nodes) if node.get("name") == name)
            if index not in nodes[parent_index].get("children", []):
                raise ValueError(f"GLB {name} has wrong exported parent")
    else:
        if set(GILL_NAMES) & names:
            raise ValueError("exposed-gill GLB contains false operculum bones")
        if require_complete:
            raise ValueError("exposed-gill breathing deformation is still pending")
    return {"anatomy": anatomy, "skin_joints": len(names),
            "gill_bones_exported": sorted(set(GILL_NAMES) & names),
            "complete": anatomy == OPERCULATE}
