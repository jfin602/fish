"""Deterministic broad-polygon reef-floor patches (metres, Blender Z up).

The outer irregular apron is always at Z=0. Chunks are authored by overlapping
aprons over a base seabed; they are not square tiles or an infinite terrain grid.
"""

from __future__ import annotations

from dataclasses import dataclass
import math
import random


@dataclass(frozen=True)
class TerrainSpec:
    profile: str
    footprint: tuple[float, float]
    elevation: float
    slope_direction: float
    roughness: float
    edge_profile: float
    reef_coverage: float
    seed: int


PROFILES = {"open_sand", "rolling", "reef_shelf", "slope", "drop_off", "rocky_floor"}
SPOKES = 13
RINGS = (0.24, 0.51, 0.76, 1.0)


def _smoothstep(low: float, high: float, value: float) -> float:
    t = max(0.0, min(1.0, (value - low) / (high - low)))
    return t * t * (3.0 - 2.0 * t)


def terrain_geometry(spec: TerrainSpec):
    """Return vertices, faces and three-region material indices.

    Parameters change macro form only: no per-vertex noise or grid sampling.
    Material indices are sand, warm transition stone, cool reef stone.
    """
    if spec.profile not in PROFILES:
        raise ValueError(f"unsupported terrain profile: {spec.profile}")
    if min(spec.footprint) <= 0 or spec.elevation < 0:
        raise ValueError("footprint must be positive and elevation nonnegative")
    if not 0 <= spec.roughness <= 1 or not 0 <= spec.edge_profile <= 1 or not 0 <= spec.reef_coverage <= 1:
        raise ValueError("roughness, edge_profile and reef_coverage must be in 0..1")
    rng = random.Random(spec.seed)
    phases = (rng.uniform(0, math.tau), rng.uniform(0, math.tau))
    radial = [1 + spec.edge_profile * rng.uniform(-0.25, 0.25) for _ in range(SPOKES)]
    angle_jitter = [spec.edge_profile * rng.uniform(-0.055, 0.055) for _ in range(SPOKES)]
    vertices = [(0.0, 0.0, _height(spec, 0.0, 0.0, 0.0, phases))]
    for ring in RINGS:
        for spoke in range(SPOKES):
            angle = math.tau * spoke / SPOKES + angle_jitter[spoke]
            shoulder = 1 + spec.roughness * 0.16 * math.sin(2 * angle + phases[0])
            radius = ring * (radial[spoke] if ring == 1.0 else radial[spoke] * shoulder)
            x = math.cos(angle) * radius * spec.footprint[0] * 0.5
            y = math.sin(angle) * radius * spec.footprint[1] * 0.5
            z = 0.0 if ring == 1.0 else _height(spec, x, y, ring, phases)
            vertices.append((x, y, z))
    # A short downward rim conceals open edges without supplying gameplay collision.
    for spoke in range(SPOKES):
        x, y, _ = vertices[1 + 3 * SPOKES + spoke]
        vertices.append((x, y, -0.09))
    faces = []
    materials = []
    def add(face, material):
        faces.append(face)
        materials.append(material)
    for spoke in range(SPOKES):
        nxt = (spoke + 1) % SPOKES
        add((0, 1 + spoke, 1 + nxt), _region(spec, 0, spoke))
    for band in range(3):
        inner = 1 + band * SPOKES
        outer = inner + SPOKES
        for spoke in range(SPOKES):
            nxt = (spoke + 1) % SPOKES
            add((inner + spoke, outer + spoke, outer + nxt, inner + nxt),
                _region(spec, band + 1, spoke))
    top = 1 + 3 * SPOKES
    bottom = 1 + 4 * SPOKES
    for spoke in range(SPOKES):
        nxt = (spoke + 1) % SPOKES
        add((top + spoke, bottom + spoke, bottom + nxt, top + nxt), 0)
    add(tuple(bottom + spoke for spoke in reversed(range(SPOKES))), 0)
    return vertices, faces, materials


def _height(spec, x, y, ring, phases):
    if ring >= 1:
        return 0.0
    u = x / (spec.footprint[0] * 0.5)
    v = y / (spec.footprint[1] * 0.5)
    direction = u * math.cos(spec.slope_direction) + v * math.sin(spec.slope_direction)
    broad = 0.5 + 0.25 * math.sin(2.6 * u + phases[0]) + 0.25 * math.cos(2.2 * v + phases[1])
    # Spread the shoulder over two broad rings so elevated profiles join sand
    # as terrain contours instead of reading as perched, bevelled pucks.
    fade = 1 - _smoothstep(0.47, 1, ring)
    if spec.profile == "open_sand":
        form = 0.045 + 0.045 * broad
    elif spec.profile == "rolling":
        form = 0.12 + 0.42 * broad
    elif spec.profile == "reef_shelf":
        form = 0.96 - 0.12 * ring + 0.06 * broad
    elif spec.profile == "slope":
        form = 0.17 + 0.78 * (direction + 1) * 0.5
    elif spec.profile == "drop_off":
        form = 0.14 + 0.86 * (1 - _smoothstep(-0.18, 0.37, direction))
    else:
        form = 0.24 + 0.63 * broad
    macro = spec.roughness * (broad - 0.5) * 0.32
    return max(0.0, spec.elevation * (form + macro) * fade)


def _region(spec, band, spoke):
    if spec.profile in {"open_sand", "rolling"}:
        return 0
    if band == 3:
        return 0
    angle = math.tau * (spoke + 0.5) / SPOKES
    patch = 0.16 * math.cos(angle - spec.slope_direction) + 0.08 * math.sin(2 * angle + spec.seed)
    score = spec.reef_coverage - (0.12, 0.22, 0.36)[band] + patch
    if score > 0.49:
        return 2
    return 1 if score > 0.20 else 0


def create_terrain(name, spec, palette_materials, collection):
    import bpy

    vertices, faces, regions = terrain_geometry(spec)
    mesh = bpy.data.meshes.new(name + "__geometry")
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    used = sorted(set(regions))
    for region in used:
        mesh.materials.append(palette_materials[region])
    for polygon, region in zip(mesh.polygons, regions):
        polygon.material_index = used.index(region)
        polygon.use_smooth = False
    obj = bpy.data.objects.new(name, mesh)
    collection.objects.link(obj)
    return obj
