"""Reusable fish rig adaptation and deterministic longitudinal skinning.

Species choose their own bones and weight stations. This module does not impose
a body shape or bone count on other fish.
"""

import math

from mathutils import Matrix


def rotate_asset_data(mesh_objects, armature, radians: float) -> None:
    """Bake a Z-axis orientation change into rest bones and mesh data.

    Call on identity-transform objects before animation is authored. Meshes must
    be parented to the supplied armature and have no existing Actions.
    """
    if armature.animation_data and armature.animation_data.action:
        raise ValueError("rotate rest data before creating Actions")
    transform = Matrix.Rotation(radians, 4, "Z")
    for obj in mesh_objects:
        if obj.parent != armature:
            raise ValueError(f"mesh is not parented to export armature: {obj.name}")
        if any(abs(value - 1) > 1e-5 for value in obj.scale):
            raise ValueError(f"mesh has unapplied scale: {obj.name}")
        obj.data.transform(transform)
        obj.data.update()
    armature.data.transform(transform)


def rename_deform_bones(armature, mesh_objects, names: dict[str, str]) -> None:
    """Rename bones and matching vertex groups without relying on UI side effects."""
    for old, new in names.items():
        bone = armature.data.bones.get(old)
        if bone is None:
            raise ValueError(f"missing bone: {old}")
        if armature.data.bones.get(new) is not None and old != new:
            raise ValueError(f"target bone name already exists: {new}")
        bone.name = new
        for obj in mesh_objects:
            group = obj.vertex_groups.get(old)
            if group is not None:
                group.name = new


def _blend_stations(position: float, stations: list[tuple[float, dict[str, float]]]) -> dict[str, float]:
    """Interpolate sparse per-species weight stations along a body axis."""
    ordered = sorted(stations, key=lambda station: station[0])
    if position <= ordered[0][0]:
        return dict(ordered[0][1])
    if position >= ordered[-1][0]:
        return dict(ordered[-1][1])
    for (start, weights_a), (end, weights_b) in zip(ordered, ordered[1:]):
        if start <= position <= end:
            factor = (position - start) / (end - start)
            names = weights_a.keys() | weights_b.keys()
            weights = {
                name: (1 - factor) * weights_a.get(name, 0.0) + factor * weights_b.get(name, 0.0)
                for name in names
            }
            total = sum(weights.values())
            return {name: weight / total for name, weight in weights.items() if weight > 1e-6}
    raise AssertionError("position was not bracketed")


def skin_body_at_stations(mesh_object, stations: list[tuple[float, dict[str, float]]]) -> None:
    """Replace body groups using the fish's -Y forward coordinate.

    Stations store front coordinate (-local Y), so a species can choose its own
    spacing and influence profile. Every vertex at a given ring gets the same
    deterministic normalized weights.
    """
    if len(stations) < 2 or len({position for position, _ in stations}) != len(stations):
        raise ValueError("weight stations need at least two unique positions")
    for _, weights in stations:
        if not weights or any(weight < 0 or not math.isfinite(weight) for weight in weights.values()):
            raise ValueError("invalid station weights")
    for group in list(mesh_object.vertex_groups):
        mesh_object.vertex_groups.remove(group)
    names = sorted({name for _, weights in stations for name in weights})
    groups = {name: mesh_object.vertex_groups.new(name=name) for name in names}
    for vertex in mesh_object.data.vertices:
        weights = _blend_stations(-vertex.co.y, stations)
        for name, weight in weights.items():
            groups[name].add([vertex.index], weight, "REPLACE")


def skin_report(mesh_objects, armature) -> dict:
    """Report unweighted/non-normalized vertices and unknown deform groups."""
    bones = {bone.name for bone in armature.data.bones}
    problems = []
    for obj in mesh_objects:
        for vertex in obj.data.vertices:
            total = sum(group.weight for group in vertex.groups)
            if abs(total - 1.0) > 1e-3:
                problems.append(f"{obj.name}: vertex {vertex.index} weight sum {total:.4f}")
            for group in vertex.groups:
                name = obj.vertex_groups[group.group].name
                if name not in bones:
                    problems.append(f"{obj.name}: unknown deform group {name}")
    return {"mesh_count": len(mesh_objects), "bone_count": len(bones), "problems": problems}
