"""Deterministic, broad-facet reef boulders for the Fish Asset Factory.

A low-resolution volume is shaped by broad directional masses and a few planar
cuts. No independent per-vertex noise is used.
"""

from __future__ import annotations

import random
import math
from dataclasses import dataclass

import bpy
import bmesh


@dataclass(frozen=True)
class BoulderSpec:
    size: float = 1.4             # widest horizontal extent in metres
    depth: float = 0.82           # Y/X footprint ratio
    verticality: float = 0.66     # height / horizontal size
    asymmetry: float = 0.42       # strength of broad footprint and shoulder bias
    facet_scale: float = 0.55     # 0 broad/coarse, 1 smaller/more numerous planes
    base_width: float = 0.82      # contact footprint relative to widest shoulder
    lean: tuple[float, float] = (0.06, -0.02)  # top offset as fraction of size
    seed: int = 101

    def validate(self) -> None:
        if not 0.5 <= self.size <= 8.0:
            raise ValueError("size must be in metres between 0.5 and 8")
        if not 0.55 <= self.depth <= 1.35:
            raise ValueError("depth must be 0.55..1.35")
        if not 0.28 <= self.verticality <= 1.5:
            raise ValueError("verticality must be 0.28..1.5")
        if not 0.0 <= self.asymmetry <= 1.0:
            raise ValueError("asymmetry must be 0..1")
        if not 0.0 <= self.facet_scale <= 1.0:
            raise ValueError("facet_scale must be 0..1")
        if not 0.62 <= self.base_width <= 1.05:
            raise ValueError("base_width must be 0.62..1.05")
        if len(self.lean) != 2 or max(abs(v) for v in self.lean) > 0.45:
            raise ValueError("lean must be an XY pair within ±0.45")
        if not isinstance(self.seed, int):
            raise ValueError("seed must be an integer")


def boulder_geometry(spec: BoulderSpec) -> tuple[list[tuple[float, float, float]], list[tuple[int, ...]]]:
    """Build a closed, plane-cut volume with a metre-scale Z=0 contact base."""
    spec.validate()
    rng = random.Random(spec.seed)
    bm = bmesh.new()
    bmesh.ops.create_icosphere(bm, subdivisions=3, radius=1.0)
    # The sphere is only a closed starting volume. These low-order fields make
    # unequal geological masses; cutting planes erase its round silhouette.
    mass_x = rng.uniform(-1.0, 1.0)
    mass_y = rng.uniform(-1.0, 1.0)
    mass_cross = rng.uniform(-1.0, 1.0)
    height_m = spec.size * spec.verticality
    for vertex in bm.verts:
        x0, y0, z0 = vertex.co
        height = (z0 + 1.0) * 0.5
        x_mass = 1.0 + spec.asymmetry * (0.22 * mass_x * x0 + 0.15 * mass_cross * y0)
        y_mass = 1.0 + spec.asymmetry * (0.19 * mass_y * y0 - 0.13 * mass_cross * x0)
        vertex.co.x = spec.size * (0.5 * x0 * x_mass + spec.lean[0] * height ** 1.5)
        vertex.co.y = spec.size * spec.depth * (0.5 * y0 * y_mass + spec.lean[1] * height ** 1.5)
        vertex.co.z = height_m * height

    def cut_and_cap(point, normal, *, remove_inside=False):
        bmesh.ops.bisect_plane(
            bm, geom=list(bm.verts) + list(bm.edges) + list(bm.faces),
            dist=0.0001, plane_co=point, plane_no=normal,
            clear_inner=remove_inside, clear_outer=not remove_inside,
        )
        boundary = [edge for edge in bm.edges if edge.is_boundary]
        if boundary:
            bmesh.ops.holes_fill(bm, edges=boundary)

    contact_height = height_m * (0.06 + 0.30 * (spec.base_width - 0.62))
    cut_and_cap((0, 0, contact_height), (0, 0, 1), remove_inside=True)
    roof_angle = rng.uniform(-math.pi, math.pi)
    cut_and_cap((0, 0, height_m * (0.86 + 0.06 * spec.facet_scale)),
                (0.16 * spec.asymmetry * math.cos(roof_angle),
                 0.16 * spec.asymmetry * math.sin(roof_angle), 1))
    first_cut = rng.uniform(-math.pi, math.pi)
    cut_count = 1 if spec.facet_scale < 0.40 else (2 if spec.facet_scale < 0.67 else 3)
    for index in range(cut_count):
        angle = first_cut + index * 2.18
        nx, ny = math.cos(angle), math.sin(angle)
        distance = 0.5 * spec.size * (0.80 + 0.10 * (1.0 - spec.asymmetry))
        cut_and_cap((distance * nx, distance * ny, height_m * 0.45),
                    (nx, ny, 0.12 * (-1 if index % 2 else 1)))
    for vertex in bm.verts:
        vertex.co.z -= contact_height
    # Merge shallow triangle fans into uneven, legible planes. The angular
    # threshold is the exposed facet-scale control; the cuts remain sharp.
    bmesh.ops.dissolve_limit(
        bm, angle_limit=math.radians(20.0 - 8.0 * spec.facet_scale),
        use_dissolve_boundaries=False, verts=list(bm.verts),
        edges=list(bm.edges), delimit=set(),
    )
    if any(edge.is_boundary or not edge.is_manifold for edge in bm.edges):
        raise RuntimeError("boulder cut produced an open or non-manifold mesh")
    bm.verts.ensure_lookup_table()
    bm.verts.index_update()
    vertices = [tuple(vertex.co) for vertex in bm.verts]
    faces = [tuple(vertex.index for vertex in face.verts) for face in bm.faces]
    bm.free()
    return vertices, faces


def create_reef_boulder(name: str, spec: BoulderSpec, material: bpy.types.Material, collection: bpy.types.Collection) -> bpy.types.Object:
    """Create one closed, flat-shaded mesh with identity object transforms."""
    vertices, faces = boulder_geometry(spec)
    mesh = bpy.data.meshes.new(name + "__geometry")
    mesh.from_pydata(vertices, [], faces)
    mesh.update(calc_edges=True)
    mesh.materials.append(material)
    for polygon in mesh.polygons:
        polygon.use_smooth = False
    rock = bpy.data.objects.new(name, mesh)
    collection.objects.link(rock)
    rock["faf_family_id"] = "FAF-ROCK-001"
    rock["generator"] = "rocks.boulder.v1"
    rock["seed"] = spec.seed
    return rock
