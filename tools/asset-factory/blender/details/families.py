"""Four small, deterministic reef-floor detail construction APIs.

The families share only a tiny faceted-lump primitive. Shell and sponge forms
have their own construction, so this is not a universal debris generator.
All coordinates are metres, Z up, with a ground-contact origin and one mesh.
"""

from __future__ import annotations

from dataclasses import dataclass
import math
import random

import bpy


@dataclass(frozen=True)
class ShellSpec:
    width: float
    depth: float
    arch_height: float
    fan_angle: float
    rib_strength: float
    yaw: float
    tilt: float
    seed: int

    def validate(self):
        if not (.06 <= self.width <= .25 and .06 <= self.depth <= .25):
            raise ValueError("shell footprint must be .06–.25 m")
        if not (.012 <= self.arch_height <= .10 and .35 <= self.fan_angle <= 1.2):
            raise ValueError("shell arch or fan angle out of range")
        if not (0 <= self.rib_strength <= .35 and abs(self.tilt) <= .4):
            raise ValueError("shell rib/tilt out of range")


@dataclass(frozen=True)
class RubbleSpec:
    pieces: int
    size: float
    spread: float
    tallness: float
    angularity: float
    seed: int

    def validate(self):
        if not (2 <= self.pieces <= 5 and .035 <= self.size <= .12):
            raise ValueError("rubble count/size out of range")
        if not (.05 <= self.spread <= .22 and .45 <= self.tallness <= 1.6):
            raise ValueError("rubble spread/tallness out of range")
        if not (0 <= self.angularity <= .5):
            raise ValueError("rubble angularity out of range")


@dataclass(frozen=True)
class PebbleSpec:
    pieces: int
    size: float
    spread: float
    flatness: float
    seed: int

    def validate(self):
        if not (3 <= self.pieces <= 7 and .022 <= self.size <= .07):
            raise ValueError("pebble count/size out of range")
        if not (.045 <= self.spread <= .20 and .15 <= self.flatness <= .5):
            raise ValueError("pebble spread/flatness out of range")


@dataclass(frozen=True)
class SpongeSpec:
    cups: int
    radius: float
    height: float
    spread: float
    mouth_ratio: float
    lean: float
    seed: int

    def validate(self):
        if not (1 <= self.cups <= 3 and .025 <= self.radius <= .09):
            raise ValueError("sponge count/radius out of range")
        if not (.022 <= self.height <= .14 and 0 <= self.spread <= .13):
            raise ValueError("sponge height/spread out of range")
        if not (.3 <= self.mouth_ratio <= .65 and abs(self.lean) <= .4):
            raise ValueError("sponge opening/lean out of range")


def _append(vertices, faces, points, polygons):
    offset = len(vertices)
    vertices.extend(points)
    faces.extend(tuple(index + offset for index in face) for face in polygons)


def _turn_ground(points, yaw, tilt=0):
    cs, sn = math.cos(yaw), math.sin(yaw)
    changed = [(cs*x-sn*y, sn*x+cs*y, z+tilt*x) for x,y,z in points]
    floor = min(p[2] for p in changed)
    return [(x,y,z-floor) for x,y,z in changed]


def shell_geometry(spec: ShellSpec):
    """A shallow closed scallop plate; broad radial ribs are actual facets."""
    spec.validate()
    rng = random.Random(spec.seed)
    angle = spec.fan_angle
    divisions = 6
    radial = (.13, .55, 1.0)
    top = []
    for ring in radial:
        for index in range(divisions + 1):
            theta = -angle + 2*angle*index/divisions
            x = .5*spec.width*ring*math.sin(theta)/math.sin(angle)
            y = -.43*spec.depth + spec.depth*ring*math.cos(theta)
            rib = 1 if index % 2 == 0 else .64
            crest = spec.arch_height*math.sin(math.pi*ring)*(.65+.35*math.cos(theta))
            z = .006 + crest + spec.rib_strength*spec.arch_height*ring*(1-ring)*rib
            top.append((x,y,z))
    underside = [(x,y,0) for x,y,_ in top]
    points = _turn_ground(top+underside, spec.yaw+rng.uniform(-.025,.025), spec.tilt)
    row = divisions+1
    count = len(top)
    faces = []
    for ring in range(len(radial)-1):
        for side in range(divisions):
            a=ring*row+side; b=a+1; c=a+row+1; d=a+row
            faces.append((a,b,c,d))
            faces.append((count+d,count+c,count+b,count+a))
    boundary = list(range(row)) + [ring*row+divisions for ring in range(1,len(radial))]
    boundary += list(range((len(radial)-1)*row+divisions-1,(len(radial)-1)*row-1,-1))
    boundary += [ring*row for ring in range(len(radial)-2,0,-1)]
    for a,b in zip(boundary,boundary[1:]+boundary[:1]):
        faces.append((a,count+a,count+b,b))
    return points,faces


def _faceted_lump(center, radius_x, radius_y, height, sides, angularity, phase, rng):
    """One closed low-poly stone/fracture form, with no vertex noise field."""
    points=[]
    radii=[1+angularity*rng.uniform(-1,1) for _ in range(sides)]
    for level in (0,.62):
        for i in range(sides):
            a=phase+math.tau*i/sides
            scale=(.83 if level==0 else 1)*radii[i]
            points.append((center[0]+radius_x*scale*math.cos(a),
                           center[1]+radius_y*scale*math.sin(a),height*level))
    points.append((center[0]+rng.uniform(-.18,.18)*radius_x,
                   center[1]+rng.uniform(-.18,.18)*radius_y,height))
    faces=[tuple(reversed(range(sides)))]
    for i in range(sides):
        j=(i+1)%sides
        faces.append((i,j,sides+j,sides+i))
        faces.append((sides+i,sides+j,2*sides))
    return points,faces


def _fracture_chip(center,radius_x,radius_y,height,phase,angularity,rng):
    """Five-sided rubble with an uneven planar break instead of a peak."""
    sides=5
    radii=[1+angularity*rng.uniform(-1,1) for _ in range(sides)]
    points=[]
    for upper in (False,True):
        for side in range(sides):
            angle=phase+math.tau*side/sides
            factor=(.83 if not upper else rng.uniform(.63,.92))*radii[side]
            points.append((center[0]+radius_x*factor*math.cos(angle)
                           +(radius_x*.12 if upper else 0),
                           center[1]+radius_y*factor*math.sin(angle),
                           height*rng.uniform(.70,1.0) if upper else 0))
    faces=[tuple(reversed(range(sides))),tuple(range(sides,2*sides))]
    for side in range(sides):
        nextside=(side+1)%sides
        faces.append((side,nextside,sides+nextside,sides+side))
    return points,faces


def rubble_geometry(spec: RubbleSpec):
    """Broken coral/reef chunks with uneven broad fracture facets."""
    spec.validate()
    rng=random.Random(spec.seed)
    vertices=[];faces=[]
    for index in range(spec.pieces):
        angle=math.tau*(index+.13*rng.uniform(-1,1))/spec.pieces
        distance=spec.spread*(.12+.36*rng.random())
        center=(distance*math.cos(angle),distance*math.sin(angle))
        scale=rng.uniform(.75,1.23)
        points,polygons=_fracture_chip(center,spec.size*scale*.52,
            spec.size*scale*rng.uniform(.35,.62),spec.size*spec.tallness*scale,
            angle+rng.uniform(-.4,.4),spec.angularity,rng)
        _append(vertices,faces,points,polygons)
    return vertices,faces


def pebble_geometry(spec: PebbleSpec):
    """A low, scattered cluster with deliberately unequal stone footprints."""
    spec.validate()
    rng=random.Random(spec.seed)
    vertices=[];faces=[]
    for index in range(spec.pieces):
        angle=math.tau*(index+.22*rng.uniform(-1,1))/spec.pieces
        distance=spec.spread*(.15+.42*rng.random())
        center=(distance*math.cos(angle),distance*math.sin(angle))
        scale=rng.uniform(.68,1.27)
        points,polygons=_faceted_lump(center,spec.size*scale*.58,
            spec.size*scale*rng.uniform(.38,.68),spec.size*spec.flatness*scale,
            5,.12,angle+rng.uniform(-.6,.6),rng)
        _append(vertices,faces,points,polygons)
    return vertices,faces


def _cup(center,radius,height,mouth_ratio,lean,phase):
    sides=5
    rings=((.0,.78,0),(.62,1,.25),(1,.87,1),(.94,mouth_ratio,1),(.43,mouth_ratio*.75,.72))
    points=[]
    for zfactor,radiusfactor,leanfactor in rings:
        for side in range(sides):
            angle=phase+math.tau*side/sides
            points.append((center[0]+radius*radiusfactor*math.cos(angle)+lean*height*leanfactor,
                           center[1]+radius*radiusfactor*math.sin(angle),height*zfactor))
    faces=[tuple(reversed(range(sides)))]
    for ring in range(len(rings)-1):
        for side in range(sides):
            nextside=(side+1)%sides
            a=ring*sides+side;b=ring*sides+nextside
            faces.append((a,b,b+sides,a+sides))
    faces.append(tuple(range((len(rings)-1)*sides,len(rings)*sides)))
    return points,faces


def sponge_geometry(spec: SpongeSpec):
    """One to three stout open cups; a geometric recess supplies the shadow."""
    spec.validate()
    rng=random.Random(spec.seed)
    vertices=[];faces=[]
    for index in range(spec.cups):
        angle=math.tau*(index+.1*rng.random())/spec.cups
        distance=spec.spread*(.2+.45*rng.random()) if spec.cups>1 else 0
        center=(distance*math.cos(angle),distance*math.sin(angle))
        scale=rng.uniform(.82,1.18)
        points,polygons=_cup(center,spec.radius*scale,
            spec.height*scale*rng.uniform(.85,1.12),spec.mouth_ratio,
            spec.lean,angle)
        _append(vertices,faces,points,polygons)
    return vertices,faces


GENERATORS={"shell":shell_geometry,"rubble":rubble_geometry,
            "pebble":pebble_geometry,"sponge":sponge_geometry}


def create_detail(name, family, spec, material, collection):
    vertices,faces=GENERATORS[family](spec)
    mesh=bpy.data.meshes.new(name+'__geometry')
    mesh.from_pydata(vertices,[],faces)
    mesh.update(calc_edges=True)
    mesh.materials.append(material)
    for polygon in mesh.polygons:polygon.use_smooth=False
    obj=bpy.data.objects.new(name,mesh)
    collection.objects.link(obj)
    return obj
