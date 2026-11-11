"""Deterministic, opaque Pond foliage geometry. Coordinates are Blender metres, +Z up.

Each family owns a meaningful recipe; the shared primitives only construct ribbons,
faceted stems, and irregular horizontal leaves. UV.y is a rooted sway mask.
"""

from __future__ import annotations

from dataclasses import dataclass
import math
import random


class MeshPart:
    def __init__(self):
        self.vertices = []
        self.faces = []
        self.uv = []

    def vertex(self, xyz, uv=(0.5, 0.0)):
        self.vertices.append(xyz)
        self.uv.append(uv)
        return len(self.vertices) - 1

    def face(self, *indices):
        self.faces.append(indices)

    def ribbon(self, root, angle, height, width, lean, curve, widths, seed_offset=0.0):
        """Closed, broad faceted blade with a zero-motion base."""
        across = (-math.sin(angle), math.cos(angle))
        along = (math.cos(angle), math.sin(angle))
        rings = []
        for i, scale in enumerate(widths):
            t = i / (len(widths) - 1)
            drift = lean * t + curve * t * t
            side = seed_offset * math.sin(math.pi * t)
            cx = root[0] + along[0] * drift + across[0] * side
            cy = root[1] + along[1] * drift + across[1] * side
            hw = width * scale / 2
            thick = max(0.004, width * 0.045) * max(0.3, scale)
            ring = []
            for s, d, u in ((-1, -1, 0), (1, -1, 1), (1, 1, 1), (-1, 1, 0)):
                ring.append(self.vertex((cx + across[0] * s * hw + along[0] * d * thick,
                                         cy + across[1] * s * hw + along[1] * d * thick,
                                         root[2] + height * t), (u, t)))
            rings.append(ring)
        self.face(*reversed(rings[0]))
        for a, b in zip(rings, rings[1:]):
            for j in range(4):
                self.face(a[j], a[(j + 1) % 4], b[(j + 1) % 4], b[j])
        self.face(*rings[-1])

    def tube(self, points, radii, sides=5, v_bottom=0.0, v_top=1.0):
        rings = []
        for i, (center, radius) in enumerate(zip(points, radii)):
            t = i / (len(points) - 1)
            ring = []
            for j in range(sides):
                a = math.tau * j / sides
                ring.append(self.vertex((center[0] + math.cos(a) * radius,
                                         center[1] + math.sin(a) * radius, center[2]),
                                        (j / sides, v_bottom + (v_top - v_bottom) * t)))
            rings.append(ring)
        self.face(*reversed(rings[0]))
        for a, b in zip(rings, rings[1:]):
            for j in range(sides):
                self.face(a[j], a[(j + 1) % sides], b[(j + 1) % sides], b[j])
        self.face(*rings[-1])

    def pad(self, radius, notch, angle, seed, height=0.012, depth=0.016, sides=16):
        """Radial lily top with a visible V notch and closed underside."""
        rng = random.Random(seed)
        outline = []
        for i in range(sides + 1):
            a = angle + 0.17 + (math.tau - 0.34) * i / sides
            r = radius * (1 + rng.uniform(-0.09, 0.09))
            outline.append((math.cos(a) * r, math.sin(a) * r))
        # The notch tip reaches into the disc, leaving an open radial V.
        tip = (math.cos(angle) * radius * (1 - notch),
               math.sin(angle) * radius * (1 - notch))
        top_center = self.vertex((0, 0, height + 0.013), (0.5, 1))
        upper = [self.vertex((x, y, height + rng.uniform(-0.003, 0.006)),
                             (0.5 + x / (2 * radius), 1)) for x, y in outline]
        upper.append(self.vertex((tip[0], tip[1], height + 0.002), (0.5, 1)))
        lower = [self.vertex((x * 0.99, y * 0.99, height - depth), (0.5, 0))
                 for x, y in outline]
        lower.append(self.vertex((tip[0], tip[1], height - depth), (0.5, 0)))
        bottom_center = self.vertex((0, 0, height - depth), (0.5, 0))
        for i in range(len(upper) - 1):
            self.face(top_center, upper[i], upper[i + 1])
            self.face(bottom_center, lower[i + 1], lower[i])
            self.face(upper[i], lower[i], lower[i + 1], upper[i + 1])
        self.face(upper[-1], lower[-1], lower[0], upper[0])

    def island(self, center, radius_x, radius_y, seed, z=0.006, sides=9):
        rng = random.Random(seed)
        top = self.vertex((center[0], center[1], z + 0.009), (0.5, 1))
        bottom = self.vertex((center[0], center[1], z - 0.008), (0.5, 0))
        upper, lower = [], []
        for i in range(sides):
            a = math.tau * i / sides
            m = rng.uniform(0.78, 1.17)
            x = center[0] + math.cos(a) * radius_x * m
            y = center[1] + math.sin(a) * radius_y * m
            upper.append(self.vertex((x, y, z), (0.5, 1)))
            lower.append(self.vertex((x, y, z - 0.008), (0.5, 0)))
        for i in range(sides):
            j = (i + 1) % sides
            self.face(top, upper[i], upper[j])
            self.face(bottom, lower[j], lower[i])
            self.face(upper[i], lower[i], lower[j], upper[j])


@dataclass(frozen=True)
class LilySpec:
    radius: float
    stem_depth: float
    notch: float
    stem_lean: float
    seed: int


@dataclass(frozen=True)
class GrassSpec:
    height: float
    blade_count: int
    spread: float
    blade_width: float
    lean: float
    seed: int


@dataclass(frozen=True)
class CattailSpec:
    height: float
    stalk_count: int
    spread: float
    head_length: float
    seed: int


@dataclass(frozen=True)
class ReedSpec:
    height: float
    stalk_count: int
    spread: float
    lean: float
    seed: int


@dataclass(frozen=True)
class AlgaeSpec:
    spread: float
    height: float
    paddle_count: int
    seed: int


@dataclass(frozen=True)
class MuckSpec:
    spread: float
    island_count: int
    open_fraction: float
    seed: int


def lily(spec: LilySpec):
    if not 0.28 <= spec.radius <= 0.9 or not 0.4 <= spec.stem_depth <= 2.0:
        raise ValueError("lily radius/depth outside shallow-water range")
    top, stem = MeshPart(), MeshPart()
    rng = random.Random(spec.seed)
    top.pad(spec.radius, spec.notch, rng.uniform(-2.5, 2.5), spec.seed)
    # The stem meets the pad underside and its last ring is the mud contact.
    stem.tube([(0, 0, -spec.stem_depth),
               (spec.stem_lean * 0.35, 0.015, -spec.stem_depth * 0.68),
               (spec.stem_lean * 0.82, -0.018, -spec.stem_depth * 0.3),
               (0, 0, 0)],
              [0.027, 0.022, 0.019, 0.019], sides=6)
    return {"pad": top, "stem": stem}


def grass(spec: GrassSpec):
    rng = random.Random(spec.seed)
    mesh = MeshPart()
    for i in range(spec.blade_count):
        a = math.tau * i / spec.blade_count + rng.uniform(-0.28, 0.28)
        d = rng.uniform(0.03, spec.spread * 0.34)
        mesh.ribbon((math.cos(a) * d, math.sin(a) * d, 0), a,
                    spec.height * rng.uniform(0.66, 1.05),
                    spec.blade_width * rng.uniform(0.76, 1.18),
                    spec.lean * rng.uniform(0.5, 1.15),
                    spec.lean * rng.uniform(0.15, 0.55),
                    (0.3, 0.82, 1, 0.66, 0.025), rng.uniform(-0.04, 0.04))
    return {"blades": mesh}


def cattail(spec: CattailSpec):
    rng = random.Random(spec.seed)
    stems, heads, leaves = MeshPart(), MeshPart(), MeshPart()
    for i in range(spec.stalk_count):
        a = math.tau * i / spec.stalk_count + rng.uniform(-0.35, 0.35)
        d = spec.spread * rng.uniform(0.12, 0.54)
        x, y = math.cos(a) * d, math.sin(a) * d
        h = spec.height * rng.uniform(0.76, 1.0)
        dx, dy = math.cos(a) * rng.uniform(0.02, 0.12), math.sin(a) * rng.uniform(0.02, 0.12)
        stems.tube([(x, y, 0), (x + dx * 0.3, y + dy * 0.3, h * 0.5),
                    (x + dx, y + dy, h)], [0.013, 0.011, 0.008], sides=5)
        head_base = h - spec.head_length * rng.uniform(0.85, 1.1)
        heads.tube([(x + dx, y + dy, head_base), (x + dx, y + dy, head_base + 0.03),
                    (x + dx, y + dy, h - 0.025), (x + dx, y + dy, h)],
                   [0.018, 0.034, 0.031, 0.014], sides=6,
                   v_bottom=head_base / h, v_top=1)
        # Long narrow leaves emerge from mud, separate from the cylindrical seed head.
        for offset in (-0.5,):
            la = a + offset
            leaves.ribbon((x * 0.7, y * 0.7, 0), la, h * rng.uniform(0.48, 0.72),
                          0.045, 0.16, 0.08, (0.24, 0.75, 0.64, 0.03))
    return {"stalks": stems, "seed_heads": heads, "leaves": leaves}


def reed(spec: ReedSpec):
    rng = random.Random(spec.seed)
    stems, leaves = MeshPart(), MeshPart()
    for i in range(spec.stalk_count):
        a = math.tau * i / spec.stalk_count + rng.uniform(-0.21, 0.21)
        d = spec.spread * rng.uniform(0.18, 0.56)
        x, y = math.cos(a) * d, math.sin(a) * d
        h = spec.height * rng.uniform(0.68, 1.0)
        lean = spec.lean * rng.uniform(0.45, 1)
        stems.tube([(x, y, 0), (x + math.cos(a) * lean * 0.35,
                                y + math.sin(a) * lean * 0.35, h * 0.55),
                    (x + math.cos(a) * lean, y + math.sin(a) * lean, h)],
                   [0.012, 0.009, 0.003], sides=4)
        if i % 2 == 0:
            leaves.ribbon((x, y, 0), a + 0.4, h * rng.uniform(0.37, 0.59),
                          0.035, 0.11, 0.1, (0.15, 0.68, 0.8, 0.02))
    return {"stems": stems, "leaves": leaves}


def algae(spec: AlgaeSpec):
    rng = random.Random(spec.seed)
    base, paddles = MeshPart(), MeshPart()
    base.island((0, 0), spec.spread * 0.37, spec.spread * 0.25, spec.seed, z=0.011, sides=7)
    for i in range(spec.paddle_count):
        a = math.tau * i / spec.paddle_count + rng.uniform(-0.32, 0.32)
        d = spec.spread * rng.uniform(0.05, 0.23)
        paddles.ribbon((math.cos(a) * d, math.sin(a) * d, 0.015), a,
                       spec.height * rng.uniform(0.62, 1),
                       spec.spread * rng.uniform(0.12, 0.21),
                       spec.spread * 0.23, spec.spread * 0.09,
                       (0.35, 0.9, 0.71, 0.04))
    return {"contact": base, "paddles": paddles}


def muck(spec: MuckSpec):
    rng = random.Random(spec.seed)
    patch = MeshPart()
    # Separated asymmetric islands preserve visible water gaps instead of one solid lid.
    for i in range(spec.island_count):
        a = math.tau * i / spec.island_count + rng.uniform(-0.27, 0.27)
        d = spec.spread * rng.uniform(0.22, 0.58)
        r = spec.spread * (1 - spec.open_fraction) * rng.uniform(0.17, 0.28)
        patch.island((math.cos(a) * d, math.sin(a) * d), r * rng.uniform(0.8, 1.2),
                     r * rng.uniform(0.5, 1.0), spec.seed + i * 17)
    return {"islands": patch}


GENERATORS = {
    "lily_pad": lily, "tall_grass": grass, "cattail": cattail,
    "reed": reed, "submerged_algae": algae, "floating_muck": muck,
}
