"""Deterministic graphic low-poly vegetation families for FAF-PLANT-001–003.

All blades are solid, opaque, low-segment ribbons. Every vertex carries UV.y
equal to its normalized height, reserving a base-anchored mask for later sway.
"""

from __future__ import annotations

from dataclasses import dataclass
import math
import random


class PlantMesh:
    def __init__(self):
        self.vertices: list[tuple[float, float, float]] = []
        self.faces: list[tuple[int, ...]] = []
        self.uv: list[tuple[float, float]] = []

    def base_disk(self, radius, height, sides=7):
        """Low faceted contact pad that visibly binds a compact rosette."""
        start = len(self.vertices)
        for z, scale in ((0.0, 1.0), (height, 0.82)):
            for index in range(sides):
                angle = math.tau * index / sides
                self.vertices.append((math.cos(angle) * radius * scale,
                                      math.sin(angle) * radius * scale, z))
                self.uv.append((0.5, 0.0))
        for index in range(sides):
            following = (index + 1) % sides
            self.faces.append((start + index, start + following,
                               start + sides + following, start + sides + index))
        self.faces.append(tuple(start + index for index in reversed(range(sides))))
        self.faces.append(tuple(start + sides + index for index in range(sides)))

    def blade(self, base_xy, height, width, angle, lean, curvature,
              widths, thickness, bend_side=0.0):
        """Add one tapered solid ribbon with longitudinal sway-ready stations."""
        x0, y0 = base_xy
        across = (-math.sin(angle), math.cos(angle))
        forward = (math.cos(angle), math.sin(angle))
        start = len(self.vertices)
        stations = len(widths)
        for index, fraction in enumerate(widths):
            t = index / (stations - 1)
            radius = lean * t + curvature * t * t
            side = bend_side * math.sin(math.pi * t) * t
            cx = x0 + forward[0] * radius + across[0] * side
            cy = y0 + forward[1] * radius + across[1] * side
            half_width = width * fraction * 0.5
            for u, sign, face_sign in ((0, -1, -1), (1, 1, -1),
                                       (1, 1, 1), (0, -1, 1)):
                self.vertices.append((cx + across[0] * half_width + forward[0] * face_sign * thickness * 0.5
                                      if sign == 1 else cx - across[0] * half_width + forward[0] * face_sign * thickness * 0.5,
                                      cy + across[1] * half_width + forward[1] * face_sign * thickness * 0.5
                                      if sign == 1 else cy - across[1] * half_width + forward[1] * face_sign * thickness * 0.5,
                                      height * t))
                self.uv.append((float(u), float(t)))
        for index in range(stations - 1):
            a = start + index * 4
            b = a + 4
            for side in range(4):
                next_side = (side + 1) % 4
                self.faces.append((a + side, a + next_side, b + next_side, b + side))
        self.faces.append(tuple(start + i for i in (3, 2, 1, 0)))
        top = start + (stations - 1) * 4
        self.faces.append(tuple(top + i for i in range(4)))


def _check_common(size, count, spread, curvature, lean, seed):
    if not 0.15 <= size <= 3.5 or not 2 <= count <= 14:
        raise ValueError("size must be 0.15..3.5 m and count 2..14")
    if not 0.1 <= spread <= 1.5 or not 0 <= curvature <= 0.75 or not 0 <= lean <= 0.65:
        raise ValueError("spread/curvature/lean outside supported range")
    if not isinstance(seed, int):
        raise ValueError("seed must be an integer")


@dataclass(frozen=True)
class SeaGrassSpec:
    height: float = 0.58
    blade_count: int = 7
    spread: float = 0.32
    width: float = 0.085
    curvature: float = 0.12
    lean: float = 0.12
    seed: int = 1

    def validate(self):
        _check_common(self.height, self.blade_count, self.spread, self.curvature, self.lean, self.seed)
        if not 0.035 <= self.width <= 0.18:
            raise ValueError("grass width must be 0.035..0.18 m")


def sea_grass_geometry(spec: SeaGrassSpec) -> PlantMesh:
    spec.validate()
    rng = random.Random(spec.seed)
    mesh = PlantMesh()
    rotation = rng.uniform(-math.pi, math.pi)
    for index in range(spec.blade_count):
        angle = rotation + math.tau * index / spec.blade_count + rng.uniform(-0.26, 0.26)
        dist = spec.spread * rng.uniform(0.08, 0.42)
        base = (math.cos(angle) * dist, math.sin(angle) * dist)
        height = spec.height * rng.uniform(0.66, 1.0)
        width = spec.width * rng.uniform(0.78, 1.16)
        mesh.blade(base, height, width, angle,
                   spec.lean * rng.uniform(0.5, 1.3),
                   spec.curvature * rng.uniform(0.55, 1.2),
                   (0.42, 0.88, 0.76, 0.055),
                   0.009, bend_side=rng.uniform(-0.035, 0.035))
    return mesh


@dataclass(frozen=True)
class BroadSeaweedSpec:
    height: float = 1.18
    frond_count: int = 4
    spread: float = 0.45
    width: float = 0.22
    curvature: float = 0.23
    lean: float = 0.21
    seed: int = 1

    def validate(self):
        _check_common(self.height, self.frond_count, self.spread, self.curvature, self.lean, self.seed)
        if not 0.12 <= self.width <= 0.40:
            raise ValueError("seaweed width must be 0.12..0.40 m")


def broad_seaweed_geometry(spec: BroadSeaweedSpec) -> PlantMesh:
    spec.validate()
    rng = random.Random(spec.seed)
    mesh = PlantMesh()
    rotation = rng.uniform(-math.pi, math.pi)
    for index in range(spec.frond_count):
        angle = rotation + math.tau * index / spec.frond_count + rng.uniform(-0.17, 0.17)
        dist = spec.spread * rng.uniform(0.04, 0.22)
        base = (math.cos(angle) * dist, math.sin(angle) * dist)
        height = spec.height * rng.uniform(0.73, 1.0)
        width = spec.width * rng.uniform(0.82, 1.15)
        mesh.blade(base, height, width, angle,
                   spec.lean * rng.uniform(0.55, 1.15),
                   spec.curvature * rng.uniform(0.7, 1.25),
                   (0.28, 0.74, 1.0, 0.84, 0.96, 0.48, 0.035),
                   0.014, bend_side=rng.uniform(-0.10, 0.10))
    return mesh


@dataclass(frozen=True)
class ReefAlgaeSpec:
    height: float = 0.34
    paddle_count: int = 5
    spread: float = 0.35
    width: float = 0.13
    curvature: float = 0.13
    lean: float = 0.16
    seed: int = 1

    def validate(self):
        _check_common(self.height, self.paddle_count, self.spread, self.curvature, self.lean, self.seed)
        if not 0.07 <= self.width <= 0.25:
            raise ValueError("algae width must be 0.07..0.25 m")


def reef_algae_geometry(spec: ReefAlgaeSpec) -> PlantMesh:
    spec.validate()
    rng = random.Random(spec.seed)
    mesh = PlantMesh()
    mesh.base_disk(spec.spread * 0.43, spec.height * 0.075)
    rotation = rng.uniform(-math.pi, math.pi)
    for index in range(spec.paddle_count):
        angle = rotation + math.tau * index / spec.paddle_count + rng.uniform(-0.34, 0.34)
        dist = spec.spread * rng.uniform(0.08, 0.34)
        base = (math.cos(angle) * dist, math.sin(angle) * dist)
        mesh.blade(base, spec.height * rng.uniform(0.55, 1),
                   spec.width * rng.uniform(0.80, 1.16), angle,
                   spec.lean * rng.uniform(0.5, 1.25),
                   spec.curvature * rng.uniform(0.5, 1.2),
                   (0.22, 0.95, 1.1, 0.06), 0.013,
                   bend_side=rng.uniform(-0.025, 0.025))
    return mesh


GENERATORS = {
    "sea_grass": sea_grass_geometry,
    "broad_seaweed": broad_seaweed_geometry,
    "reef_algae": reef_algae_geometry,
}


def create_plant(name, family, spec, material, collection):
    """Create one flat-shaded mesh with a UV.y sway mask and identity transform."""
    import bpy
    geometry = GENERATORS[family](spec)
    mesh = bpy.data.meshes.new(name)
    mesh.from_pydata(geometry.vertices, [], geometry.faces)
    mesh.update(calc_edges=True)
    mesh.materials.append(material)
    layer = mesh.uv_layers.new(name="sway_uv")
    for polygon in mesh.polygons:
        polygon.use_smooth = False
        for loop_index in polygon.loop_indices:
            vertex_index = mesh.loops[loop_index].vertex_index
            layer.data[loop_index].uv = geometry.uv[vertex_index]
    obj = bpy.data.objects.new(name, mesh)
    collection.objects.link(obj)
    obj["faf_family_id"] = {"sea_grass": "FAF-PLANT-001",
                            "broad_seaweed": "FAF-PLANT-002",
                            "reef_algae": "FAF-PLANT-003"}[family]
    obj["generator"] = f"plants.{family}.v1"
    obj["seed"] = spec.seed
    obj["sway_mask"] = "UV.y: base 0, tip 1; Blender +Z / Godot +Y"
    return obj
