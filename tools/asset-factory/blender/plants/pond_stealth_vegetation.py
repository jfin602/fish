"""Deterministic, metre-scale Pond soft-cover geometry (+Z up).

Five separate construction functions share only the small mesh primitives below.
The origin is the mud contact and UV.y is the fixed-root to flexible-tip mask.
"""

from dataclasses import dataclass
import math
import random

from plants.pond_foliage import MeshPart


def add(a, b):
    return tuple(x + y for x, y in zip(a, b))


def mul(a, s):
    return tuple(x * s for x in a)


def sub(a, b):
    return tuple(x - y for x, y in zip(a, b))


def norm(a):
    size = math.sqrt(sum(x * x for x in a))
    return tuple(x / max(size, 1e-8) for x in a)


def cross(a, b):
    return (a[1] * b[2] - a[2] * b[1],
            a[2] * b[0] - a[0] * b[2],
            a[0] * b[1] - a[1] * b[0])


class PlantPart(MeshPart):
    def stalk(self, points, radius, height, sides=5):
        """Faceted taper around a curved, sometimes leaning centre line."""
        rings = []
        for i, center in enumerate(points):
            tangent = norm(sub(points[min(i + 1, len(points) - 1)],
                               points[max(i - 1, 0)]))
            across = norm(cross(tangent, (0, 1, 0)))
            other = norm(cross(tangent, across))
            taper = (1 - 0.78 * i / (len(points) - 1)) * radius
            ring = []
            for j in range(sides):
                a = math.tau * j / sides
                p = add(center, add(mul(across, taper * math.cos(a)),
                                    mul(other, taper * math.sin(a))))
                ring.append(self.vertex(p, (j / sides, max(0, min(1, center[2] / height)))))
            rings.append(ring)
        self.face(*reversed(rings[0]))
        for a, b in zip(rings, rings[1:]):
            for j in range(sides):
                k = (j + 1) % sides
                self.face(a[j], a[k], b[k], b[j])
        self.face(*rings[-1])

    def leaf(self, root, tip, width, height, feather=False):
        """Closed faceted lance, with an independent midrib and visible edge."""
        direction = norm(sub(tip, root))
        across_raw = cross(direction, (0, 0, 1))
        across = norm(across_raw) if sum(x*x for x in across_raw) > .01 else (1, 0, 0)
        lift = norm(cross(across, direction))
        if feather:
            stations = ((0, 0), (.42, .75), (.74, .9), (1, 0))
        else:
            stations = ((0, 0), (.23, .68), (.48, 1), (.76, .72), (1, 0))
        upper_left, upper_right, lower_left, lower_right = [], [], [], []
        for t, w in stations:
            center = add(add(root, mul(sub(tip, root), t)), mul(lift, .025 * math.sin(math.pi*t)))
            uv_y = max(0, min(1, center[2]/height))
            thickness = .004 if feather else .006
            w = max(.045, w)
            for side, dest in ((-1, upper_left),(1, upper_right)):
                dest.append(self.vertex(add(add(center, mul(across, side*width*w/2)),
                                            mul(lift, thickness)), ((side+1)/2, uv_y)))
            if not feather:
                for side, dest in ((-1, lower_left),(1, lower_right)):
                    dest.append(self.vertex(add(add(center, mul(across, side*width*w/2)),
                                                mul(lift, -thickness)), ((side+1)/2, uv_y)))
        for i in range(len(upper_left)-1):
            self.face(upper_left[i], upper_right[i], upper_right[i+1], upper_left[i+1])
            if feather:
                continue
            self.face(lower_right[i+1], lower_right[i], lower_left[i], lower_left[i+1])
            self.face(upper_left[i+1], lower_left[i+1], lower_left[i], upper_left[i])
            self.face(lower_right[i], lower_right[i+1], upper_right[i+1], upper_right[i])
        if not feather:
            self.face(upper_left[0], lower_left[0], lower_right[0], upper_right[0])
            self.face(upper_right[-1], lower_right[-1], lower_left[-1], upper_left[-1])


@dataclass(frozen=True)
class RibbonGrassSpec:
    height: float
    blade_count: int
    spread: float
    blade_width: float
    lean: float
    seed: int


@dataclass(frozen=True)
class BranchingWeedSpec:
    height: float
    width: float
    stalk_count: int
    forks_per_stalk: int
    leaf_pairs: int
    seed: int


@dataclass(frozen=True)
class CoontailSpec:
    height: float
    width: float
    stalk_count: int
    whorls: int
    horizontal_bias: float
    seed: int


@dataclass(frozen=True)
class PondweedSpec:
    height: float
    width: float
    stalk_count: int
    leaves_per_stalk: int
    leaf_width: float
    seed: int


@dataclass(frozen=True)
class ThicketSpec:
    height: float
    width: float
    stalk_count: int
    root_count: int
    crown_density: int
    seed: int


def ribbon_grass(spec):
    rng = random.Random(spec.seed)
    blades = PlantPart()
    for i in range(spec.blade_count):
        angle = math.tau * (i + .38*rng.random()) / spec.blade_count
        radius = spec.spread * (.18 + .82 * math.sqrt(rng.random()))
        root = (radius*math.cos(angle), radius*math.sin(angle), 0)
        # Long centre blades and shorter edge blades; not one globally scaled clump.
        band = .69 + .31*rng.random() if i % 5 else .45 + .22*rng.random()
        h = spec.height * band
        bend = spec.lean*(.55+rng.random()) * (h/spec.height)**1.3
        blades.ribbon(root, angle + rng.uniform(-.36,.36), h,
                      spec.blade_width*rng.uniform(.7,1.25), bend,
                      rng.uniform(-.17,.23)*h,
                      (1,.96,.82,.60,.14), rng.uniform(-.09,.09))
    return {"blades": blades}


def branching_weed(spec):
    rng = random.Random(spec.seed)
    stems, leaves = PlantPart(), PlantPart()
    for i in range(spec.stalk_count):
        a = math.tau*(i+rng.uniform(-.25,.25))/spec.stalk_count
        r = spec.width*rng.uniform(.08,.3)
        base = (r*math.cos(a), r*math.sin(a), 0)
        h = spec.height*rng.uniform(.74,1)
        tip = (base[0]+math.cos(a)*spec.width*.30,
               base[1]+math.sin(a)*spec.width*.30,h)
        mid = (base[0]+(tip[0]-base[0])*.42,
               base[1]+(tip[1]-base[1])*.42,h*.52)
        stems.stalk((base,mid,tip), .032+spec.height*.008, spec.height)
        for j in range(spec.forks_per_stalk):
            t = .26 + .58*(j+.3*rng.random())/spec.forks_per_stalk
            origin = add(base, mul(sub(tip,base), t))
            direction = a + (1 if j%2 else -1)*rng.uniform(.75,1.65)
            length = spec.width*rng.uniform(.43,.78)
            end = (origin[0]+length*math.cos(direction),
                   origin[1]+length*math.sin(direction),
                   min(spec.height, origin[2]+h*rng.uniform(.13,.31)))
            stems.stalk((origin,add(origin,mul(sub(end,origin),.56)),end), .017, spec.height,4)
            for k in range(spec.leaf_pairs):
                u = .25+.62*(k+.25*rng.random())/spec.leaf_pairs
                anchor = add(origin,mul(sub(end,origin),u))
                la = direction+(math.pi/2 if k%2 else -math.pi/2)
                reach = spec.width*rng.uniform(.17,.29)
                leaf_tip=(anchor[0]+reach*math.cos(la), anchor[1]+reach*math.sin(la),
                          anchor[2]+reach*rng.uniform(.25,.8))
                leaves.leaf(anchor,leaf_tip,.07+spec.width*.04,spec.height,feather=True)
    return {"stems":stems,"leaf_clusters":leaves}


def coontail(spec):
    rng = random.Random(spec.seed)
    stems, fronds = PlantPart(), PlantPart()
    for i in range(spec.stalk_count):
        a = math.tau*(i+rng.uniform(-.25,.25))/spec.stalk_count
        base=(spec.width*.12*math.cos(a),spec.width*.12*math.sin(a),0)
        h=spec.height*rng.uniform(.72,1)
        end=(base[0]+spec.width*spec.horizontal_bias*math.cos(a),
             base[1]+spec.width*spec.horizontal_bias*math.sin(a),h)
        stems.stalk((base,add(base,mul(sub(end,base),.48)),end),.023,spec.height,4)
        for j in range(spec.whorls):
            t=.22+.73*j/max(1,spec.whorls-1)
            center=add(base,mul(sub(end,base),t))
            count=5+(j%2)
            for k in range(count):
                theta=math.tau*(k+rng.uniform(-.13,.13))/count+a
                reach=spec.width*rng.uniform(.26,.43)*(1-.28*t)
                tip=(center[0]+reach*math.cos(theta),
                     center[1]+reach*math.sin(theta),
                     center[2]+reach*rng.uniform(.12,.43))
                fronds.leaf(center,tip,reach*.30,spec.height,feather=True)
    return {"stems":stems,"chunky_whorls":fronds}


def broad_pondweed(spec):
    rng=random.Random(spec.seed)
    stems, leaves=PlantPart(),PlantPart()
    for i in range(spec.stalk_count):
        a=math.tau*(i+rng.uniform(-.23,.23))/spec.stalk_count
        base=(spec.width*.13*math.cos(a),spec.width*.13*math.sin(a),0)
        h=spec.height*rng.uniform(.72,1)
        end=(base[0]+spec.width*.40*math.cos(a),base[1]+spec.width*.40*math.sin(a),h)
        stems.stalk((base,add(base,mul(sub(end,base),.45)),end),.018,spec.height,4)
        for j in range(spec.leaves_per_stalk):
            t=.25+.65*(j+.2*rng.random())/spec.leaves_per_stalk
            anchor=add(base,mul(sub(end,base),t))
            la=a+(math.pi/2 if j%2 else -math.pi/2)+rng.uniform(-.22,.22)
            reach=spec.width*rng.uniform(.32,.52)
            tip=(anchor[0]+reach*math.cos(la),anchor[1]+reach*math.sin(la),
                 min(spec.height*1.01,anchor[2]+reach*rng.uniform(.40,.94)))
            leaves.leaf(anchor,tip,spec.leaf_width*rng.uniform(.75,1.2),spec.height)
    return {"stems":stems,"broad_leaves":leaves}


def rooted_thicket(spec):
    rng=random.Random(spec.seed)
    wood,foliage=PlantPart(),PlantPart()
    for i in range(spec.root_count):
        a=math.tau*(i+.1*rng.random())/spec.root_count
        start=(0,0,.12)
        end=(spec.width*.45*math.cos(a),spec.width*.45*math.sin(a),-.025)
        wood.stalk((start,add(start,mul(sub(end,start),.48)),end),.04,spec.height,5)
    for i in range(spec.stalk_count):
        a=math.tau*(i+rng.uniform(-.22,.22))/spec.stalk_count
        base=(spec.width*.07*math.cos(a),spec.width*.07*math.sin(a),0)
        h=spec.height*rng.uniform(.66,1)
        tip=(spec.width*rng.uniform(.26,.50)*math.cos(a),
             spec.width*rng.uniform(.26,.50)*math.sin(a),h)
        wood.stalk((base,(base[0]*.5+tip[0]*.5,base[1]*.5+tip[1]*.5,h*.52),tip),
                   .050,spec.height,5)
        for j in range(spec.crown_density):
            t=.28+.60*(j+.2*rng.random())/spec.crown_density
            origin=add(base,mul(sub(tip,base),t))
            direction=a+(-1 if j%2 else 1)*rng.uniform(.55,1.3)
            reach=spec.width*rng.uniform(.22,.43)
            end=(origin[0]+reach*math.cos(direction),
                 origin[1]+reach*math.sin(direction),
                 min(spec.height,origin[2]+reach*rng.uniform(.4,.9)))
            wood.stalk((origin,end),.022,spec.height,4)
            # Larger foliage masses occupy branch tips; the centre remains porous.
            for k in range(3):
                theta=direction+(k-1)*.8
                leaf_tip=(end[0]+reach*.55*math.cos(theta),
                          end[1]+reach*.55*math.sin(theta),
                          end[2]+reach*.24*(k-1))
                foliage.leaf(end,leaf_tip,.16+spec.width*.07,spec.height)
    return {"roots_and_shoots":wood,"crown_foliage":foliage}


GENERATORS={
    "ribbon_grass":ribbon_grass,
    "branching_waterweed":branching_weed,
    "coontail":coontail,
    "broad_pondweed":broad_pondweed,
    "rooted_thicket":rooted_thicket,
}
