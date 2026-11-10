"""Four deterministic, metre-scale faceted coral families for FAF.

Each public Spec exposes form controls with a visible silhouette effect. Geometry
functions are deterministic within Blender; only create_coral writes a datablock.
"""

from __future__ import annotations

from dataclasses import dataclass
import math
import random


class Mesh:
    def __init__(self):
        self.vertices = []
        self.faces = []

    def add(self, vertices, faces):
        start = len(self.vertices)
        self.vertices.extend(tuple(float(v) for v in point) for point in vertices)
        self.faces.extend(tuple(start + i for i in face) for face in faces)

    def tube(self, a, b, r0, r1, sides=7, open_top=False, wall=0.28):
        """Faceted tapered stem or open thick-walled cup between two points."""
        import mathutils
        a, b = mathutils.Vector(a), mathutils.Vector(b)
        direction = (b - a).normalized()
        tangent = direction.cross(mathutils.Vector((0, 1, 0)))
        if tangent.length < 0.1:
            tangent = direction.cross(mathutils.Vector((1, 0, 0)))
        tangent.normalize()
        bitangent = direction.cross(tangent).normalized()
        base = len(self.vertices)
        for center, radius in ((a, r0), (b, r1)):
            for i in range(sides):
                theta = 2 * math.pi * i / sides
                self.vertices.append(tuple(center + radius * (math.cos(theta) * tangent + math.sin(theta) * bitangent)))
        for i in range(sides):
            j = (i + 1) % sides
            self.faces.append((base + i, base + j, base + sides + j, base + sides + i))
        self.faces.append(tuple(base + i for i in reversed(range(sides))))
        if open_top:
            inner = len(self.vertices)
            for center, radius in ((b, r1 * (1 - wall)), (a.lerp(b, 0.56), r1 * (1 - wall) * 0.72)):
                for i in range(sides):
                    theta = 2 * math.pi * i / sides
                    self.vertices.append(tuple(center + radius * (math.cos(theta) * tangent + math.sin(theta) * bitangent)))
            for i in range(sides):
                j = (i + 1) % sides
                self.faces.append((base + sides + i, base + sides + j, inner + j, inner + i))
                self.faces.append((inner + i, inner + j, inner + sides + j, inner + sides + i))
            self.faces.append(tuple(inner + sides + i for i in range(sides)))
        else:
            self.faces.append(tuple(base + sides + i for i in range(sides)))

    def lobe(self, center, radius, segments=9, rings=5):
        """Broad low-resolution ellipsoid with a flattened contact underside."""
        x, y, z = center
        rx, ry, rz = radius
        base = len(self.vertices)
        for j in range(rings + 1):
            phi = math.pi * j / rings
            for i in range(segments):
                theta = 2 * math.pi * i / segments
                zz = max(0.0, z + rz * math.cos(phi))
                self.vertices.append((x + rx * math.sin(phi) * math.cos(theta),
                                      y + ry * math.sin(phi) * math.sin(theta), zz))
        for j in range(rings):
            for i in range(segments):
                k = (i + 1) % segments
                self.faces.append((base + j * segments + i, base + j * segments + k,
                                   base + (j + 1) * segments + k, base + (j + 1) * segments + i))

    def slab(self, polygon, thickness):
        """Extrude an X/Z polygon into a solid Y-thick fan web."""
        base = len(self.vertices)
        for y_offset in (-thickness / 2, thickness / 2):
            for x, y, z in polygon:
                self.vertices.append((x, y + y_offset, z))
        n = len(polygon)
        self.faces.append(tuple(base + i for i in reversed(range(n))))
        self.faces.append(tuple(base + n + i for i in range(n)))
        for i in range(n):
            j = (i + 1) % n
            self.faces.append((base + i, base + j, base + n + j, base + n + i))


def _validate(size, count, spread, taper, asymmetry, seed):
    if not 0.25 <= size <= 4 or not 2 <= count <= 12:
        raise ValueError("size must be 0.25..4 m; count must be 2..12")
    if not 0.2 <= spread <= 1.5 or not 0.15 <= taper <= 0.9 or not 0 <= asymmetry <= 1:
        raise ValueError("spread/taper/asymmetry out of supported range")
    if not isinstance(seed, int):
        raise ValueError("seed must be an integer")


@dataclass(frozen=True)
class BranchingSpec:
    size: float = 0.9
    branch_count: int = 5
    spread: float = 0.75
    verticality: float = 1.0
    taper: float = 0.48
    asymmetry: float = 0.4
    seed: int = 1

    def validate(self):
        _validate(self.size, self.branch_count, self.spread, self.taper, self.asymmetry, self.seed)
        if not 0.55 <= self.verticality <= 1.5:
            raise ValueError("verticality must be 0.55..1.5")


def branching_geometry(spec: BranchingSpec) -> Mesh:
    spec.validate()
    rng = random.Random(spec.seed)
    mesh = Mesh()
    s = spec.size
    mesh.tube((0, 0, 0), (0, 0, s * 0.27), s * 0.22, s * 0.17, 8)
    angle_offset = rng.uniform(-math.pi, math.pi)
    for i in range(spec.branch_count):
        angle = angle_offset + i * math.tau / spec.branch_count + rng.uniform(-0.16, 0.16) * spec.asymmetry
        reach = s * spec.spread * rng.uniform(0.78, 1.10)
        height = s * spec.verticality * rng.uniform(0.78, 1.1)
        p0 = (0.0, 0.0, s * 0.10)
        p1 = (math.cos(angle) * reach * 0.28, math.sin(angle) * reach * 0.28, height * 0.52)
        p2 = (math.cos(angle) * reach * 0.60, math.sin(angle) * reach * 0.60, height * 0.81)
        r = s * rng.uniform(0.095, 0.132)
        mesh.tube(p0, p1, r * 1.17, r, 7)
        mesh.tube(p1, p2, r * 1.07, r * (1 - spec.taper * 0.62), 7)
        for side in (-1, 1):
            fork_angle = angle + side * rng.uniform(0.28, 0.48)
            tip = (p2[0] + math.cos(fork_angle) * reach * 0.26,
                   p2[1] + math.sin(fork_angle) * reach * 0.26,
                   p2[2] + height * rng.uniform(0.18, 0.29))
            mesh.tube(p2, tip, r * 0.83, r * (1 - spec.taper), 7)
    return mesh


@dataclass(frozen=True)
class RoundedSpec:
    size: float = 0.9
    lobe_count: int = 6
    spread: float = 0.75
    verticality: float = 0.55
    asymmetry: float = 0.35
    seed: int = 1

    def validate(self):
        _validate(self.size, self.lobe_count, self.spread, 0.5, self.asymmetry, self.seed)
        if not 0.3 <= self.verticality <= 1.0:
            raise ValueError("verticality must be 0.3..1")


def rounded_geometry(spec: RoundedSpec) -> Mesh:
    spec.validate()
    rng = random.Random(spec.seed)
    mesh = Mesh()
    s = spec.size
    mesh.lobe((0, 0, s * spec.verticality * 0.35),
              (s * 0.42, s * 0.36, s * spec.verticality * 0.35), 10, 5)
    offset = rng.uniform(-math.pi, math.pi)
    for i in range(spec.lobe_count):
        angle = offset + i * math.tau / spec.lobe_count + rng.uniform(-0.22, 0.22) * spec.asymmetry
        distance = s * spec.spread * (0.34 + rng.uniform(-0.15, 0.15) * spec.asymmetry)
        radius = s * (0.265 + rng.uniform(-0.09, 0.09) * spec.asymmetry)
        mesh.lobe((math.cos(angle) * distance, math.sin(angle) * distance,
                   s * spec.verticality * rng.uniform(0.39, 0.62)),
                  (radius, radius * rng.uniform(0.82, 1.08),
                   s * spec.verticality * rng.uniform(0.30, 0.44)), 9, 5)
    return mesh


@dataclass(frozen=True)
class TubeSpec:
    size: float = 0.85
    tube_count: int = 7
    spread: float = 0.72
    verticality: float = 0.9
    taper: float = 0.30
    asymmetry: float = 0.4
    lean: float = 0.17
    seed: int = 1

    def validate(self):
        _validate(self.size, self.tube_count, self.spread, self.taper, self.asymmetry, self.seed)
        if not 0.55 <= self.verticality <= 1.5 or not 0 <= self.lean <= 0.4:
            raise ValueError("verticality/lean out of supported range")


def tube_geometry(spec: TubeSpec) -> Mesh:
    spec.validate()
    rng = random.Random(spec.seed)
    mesh = Mesh()
    s = spec.size
    mesh.lobe((0, 0, s * 0.07), (s * 0.43 * spec.spread, s * 0.40 * spec.spread, s * 0.09), 10, 3)
    offset = rng.uniform(-math.pi, math.pi)
    for i in range(spec.tube_count):
        angle = offset + i * math.tau / spec.tube_count + rng.uniform(-0.15, 0.15) * spec.asymmetry
        dist = s * spec.spread * rng.uniform(0.15, 0.38)
        x, y = math.cos(angle) * dist, math.sin(angle) * dist
        h = s * spec.verticality * rng.uniform(0.58, 1.0)
        r = s * rng.uniform(0.085, 0.135)
        tip = (x + math.cos(angle + 0.5) * s * spec.lean * rng.uniform(0.4, 1),
               y + math.sin(angle + 0.5) * s * spec.lean * rng.uniform(0.4, 1), h)
        mesh.tube((x, y, s * 0.015), tip, r * (1 + spec.taper), r, 9, open_top=True, wall=0.33)
    # A leaning stem's perpendicular contact ring can cross Z=0. Flatten only
    # the contact underside so every variant retains an exact ground pivot.
    mesh.vertices = [(x, y, max(0.0, z)) for x, y, z in mesh.vertices]
    return mesh


@dataclass(frozen=True)
class FanSpec:
    size: float = 1.0
    rib_count: int = 6
    spread: float = 1.0
    verticality: float = 0.82
    curvature: float = 0.18
    asymmetry: float = 0.35
    seed: int = 1

    def validate(self):
        _validate(self.size, self.rib_count, self.spread, 0.5, self.asymmetry, self.seed)
        if not 0.55 <= self.verticality <= 1.4 or not 0 <= self.curvature <= 0.45:
            raise ValueError("verticality/curvature out of supported range")


def fan_geometry(spec: FanSpec) -> Mesh:
    spec.validate()
    rng = random.Random(spec.seed)
    mesh = Mesh()
    s = spec.size
    mesh.tube((0, 0, 0), (0, 0, s * 0.23), s * 0.13, s * 0.095, 7)
    ribs = []
    for i in range(spec.rib_count):
        t = (i / (spec.rib_count - 1)) * 2 - 1
        width = s * spec.spread * 0.52
        top_x = t * width + rng.uniform(-0.07, 0.07) * s * spec.asymmetry
        top_z = s * spec.verticality * (0.82 + 0.17 * (1 - abs(t)) + rng.uniform(-0.06, 0.06))
        mid = (top_x * 0.49 + spec.curvature * s * (1 - t * t),
               s * (0.05 * t + rng.uniform(-0.025, 0.025)), top_z * 0.58)
        tip = (top_x, s * (0.12 * t + rng.uniform(-0.035, 0.035)), top_z)
        mesh.tube((0, 0, s * 0.16), mid, s * 0.075, s * 0.048, 6)
        mesh.tube(mid, tip, s * 0.055, s * 0.035, 6)
        ribs.append((mid, tip))
    for i in range(spec.rib_count - 1):
        # Solid, low-density webs bridge each pair; the low valley leaves an
        # irregular scalloped crown while the broad sheet reads from distance.
        a, b = ribs[i], ribs[i + 1]
        bottom_z = min(a[0][2], b[0][2]) * (0.72 if i % 2 else 0.82)
        poly = [(a[0][0], a[0][1], bottom_z),
                (a[1][0], a[1][1], a[1][2] * 0.97),
                ((a[1][0] + b[1][0]) * 0.5, (a[1][1] + b[1][1]) * 0.5,
                 (a[1][2] + b[1][2]) * (0.39 if i % 2 else 0.42)),
                (b[1][0], b[1][1], b[1][2] * 0.97),
                (b[0][0], b[0][1], bottom_z)]
        mesh.slab(poly, s * 0.09)
    return mesh


GENERATORS = {
    "branching": branching_geometry,
    "rounded": rounded_geometry,
    "tube": tube_geometry,
    "fan": fan_geometry,
}


def create_coral(name, family, spec, material, collection):
    """Create a single flat-shaded mesh with identity transforms and Z=0 pivot."""
    import bpy
    geometry = GENERATORS[family](spec)
    mesh = bpy.data.meshes.new(name)
    mesh.from_pydata(geometry.vertices, [], geometry.faces)
    mesh.update(calc_edges=True)
    mesh.materials.append(material)
    for polygon in mesh.polygons:
        polygon.use_smooth = False
    obj = bpy.data.objects.new(name, mesh)
    collection.objects.link(obj)
    obj["faf_family_id"] = {"branching": "FAF-CORAL-001", "rounded": "FAF-CORAL-002",
                            "tube": "FAF-CORAL-003", "fan": "FAF-CORAL-004"}[family]
    obj["generator"] = f"coral.{family}.v1"
    obj["seed"] = spec.seed
    return obj
