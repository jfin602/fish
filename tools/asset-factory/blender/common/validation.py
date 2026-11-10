"""Basic Blender scene checks; review and Godot import remain separate."""

import math
from collections import Counter


def triangle_count(mesh) -> int:
    """Count triangles from the evaluated Blender mesh polygon loops."""
    return sum(max(0, len(poly.vertices) - 2) for poly in mesh.polygons)


def inspect_objects(objects) -> dict:
    """Summarize selected Blender objects without changing the scene."""
    items = list(objects)
    meshes = [obj for obj in items if obj.type == "MESH"]
    armatures = [obj for obj in items if obj.type == "ARMATURE"]
    material_names = {
        slot.material.name for obj in meshes for slot in obj.material_slots
        if slot.material is not None
    }
    problems = []
    counts = Counter(obj.name for obj in items)
    for name, count in counts.items():
        if count > 1:
            problems.append(f"duplicate selected object name: {name}")
    for obj in items:
        if obj.type not in {"MESH", "ARMATURE"}:
            problems.append(f"non-export object selected: {obj.name} ({obj.type})")
        if any(not math.isfinite(value) for value in (*obj.location, *obj.rotation_euler, *obj.scale)):
            problems.append(f"non-finite transform: {obj.name}")
        if any(abs(value - 1.0) > 1e-4 for value in obj.scale):
            problems.append(f"unapplied scale: {obj.name}")
        if any(abs(value) > 1e-4 for value in obj.rotation_euler):
            problems.append(f"unapplied Euler rotation: {obj.name}")
        if obj.name.endswith(tuple(f".{number:03d}" for number in range(1, 1000))):
            problems.append(f"Blender auto-suffixed name: {obj.name}")
    if not meshes:
        problems.append("no mesh selected")
    return {
        "object_count": len(items),
        "mesh_count": len(meshes),
        "triangle_count_source": sum(triangle_count(obj.data) for obj in meshes),
        "material_count": len(material_names),
        "armature_count": len(armatures),
        "bone_count": sum(len(obj.data.bones) for obj in armatures),
        "problems": problems,
    }
