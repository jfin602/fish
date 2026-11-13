"""Five large, deterministic Pond encounter-space wood families (Blender Z up).

The old Pond wood primitive supplies faceted bark, end grain and the four shared
materials. Layouts and their simple collision descriptions are family-specific.
"""
from __future__ import annotations

import math
import random

import mathutils

import build_pond_wood_001 as old

old.mathutils = mathutils


def beam(mesh, collisions, points, radii, rng, sides=9, solid=True):
    old.branch(mesh, points, radii, rng, sides=sides)
    if solid:
        for a, b, ra, rb in zip(points, points[1:], radii, radii[1:]):
            collisions.append({"kind": "capsule", "a": list(a), "b": list(b),
                               "radius": round(min(ra, rb) * .88, 4)})


def long_fallen_tree(mesh, spec, rng):
    c = []
    length, radius, bend = spec["length"], spec["radius"], spec["bend"]
    points = []
    radii = []
    for i in range(9):
        t = i / 8
        points.append((bend * math.sin(math.pi*t) + .22*t,
                       length*(t-.5), radius*(.92+.05*math.sin(2*math.pi*t))))
        radii.append(radius*(1.08-.30*t+.045*math.sin(5*t)))
    beam(mesh,c,points,radii,rng,sides=13)
    for i in range(5):
        t = .16 + i*.155
        x,y,z = points[round(t*8)]
        sign = -1 if i%2 else 1
        reach = .9 + .35*(i%3)
        beam(mesh,c,[(x,y,z+radius*.55),(x+sign*.55,y+.18,z+radius*.78),
                     (x+sign*reach,y+.35,z+radius*(.65+.16*(i%2)))],
             [radius*.29,radius*.19,.11],rng,sides=7)
    for sign in (-1,1):
        y=-length*.43
        beam(mesh,c,[(sign*radius*.55,y,.48),(sign*(radius+.45),y-.30,.20),
                     (sign*(radius+1.15),y-.54,.02)],
             [.30,.19,.035],rng,sides=6,solid=False)
    return c


def forked_fallen_log(mesh, spec, rng):
    c=[]
    r=spec["radius"]
    high=spec["id"].endswith("B")
    beam(mesh,c,[(0,-6.3,r*.93),(.12,-4.2,r),(-.10,-2.0,r*1.12),(.10,-.45,r*1.22)],
         [r*1.05,r*.99,r*.88,r*.79],rng,sides=12)
    spread=spec["spread"]
    beam(mesh,c,[(.10,-.65,r*1.22),(.65,.75,r*1.26),
                 (spread*.72,2.55,r*(1.85 if high else 1.40)),
                 (spread,4.30,r*(2.03 if high else 1.47))],
         [r*.75,r*.69,r*.54,r*.32],rng,sides=10)
    beam(mesh,c,[(.10,-.55,r*1.20),(-.80,.52,r*1.14),
                 (-spread*(.94 if high else .84),2.13,r*.95),
                 (-spread*(1.30 if high else 1.17),3.62,r*.70)],
         [r*.68,r*.58,r*.42,r*.24],rng,sides=9)
    beam(mesh,c,[(spread*.51,1.78,r*1.38),(spread*.89,2.02,r*2.30),
                 (spread*1.05,2.26,r*2.82)],
         [r*.30,r*.20,.08],rng,sides=7)
    beam(mesh,c,[(-spread*.56,1.55,r*1.08),(-spread*.76,1.30,.14)],
         [r*.27,.09],rng,sides=6)
    return c


def root_ball_tree(mesh, spec, rng):
    c=[]
    r=spec["radius"]
    beam(mesh,c,[(0,-2.05,r*1.35),(.17,-.35,r*1.22),(.46,2.25,r*1.08),
                 (.72,5.55,r*.91)],
         [r*1.15,r*1.04,r*.83,r*.54],rng,sides=12)
    # An open central undercut faces the negative-Y approach. Roots bound it
    # from the sides, while the connected trunk stops a straight passage.
    root_angles=((-2.55,-2.20,-1.77,-1.34,-.62,.34,.88,1.42,2.03,2.62)
                 if spec["id"].endswith("B") else
                 (-2.65,-2.05,-1.40,-.48,.45,1.47,2.16,2.70))
    for i,a in enumerate(root_angles):
        side=math.sin(a)
        reach=spec["spread"]*(.78+.12*(i%3))
        start=(side*r*.52,-2.0+math.cos(a)*r*.30,r*(1.18+.07*(i%2)))
        end=(side*reach,-2.0+math.cos(a)*reach,.04)
        mid=((start[0]+end[0])*.52,(start[1]+end[1])*.52,.66+.20*(i%3))
        beam(mesh,c,[start,mid,end],[r*.27,r*.18,.055],rng,sides=7)
        if i%2==0:
            beam(mesh,c,[mid,(mid[0]+side*.65,mid[1]-.38,.21)],
                 [r*.12,.045],rng,sides=5,solid=False)
    # Higher radial struts make partial windows, not a closed convex mass.
    for sign in (-1,1):
        beam(mesh,c,[(sign*r*.36,-1.60,r*1.35),
                     (sign*(r+1.05),-2.40,r*1.9),
                     (sign*(r+1.95),-3.15,r*2.05)],
             [r*.25,r*.15,.07],rng,sides=6)
    return c


def _shell_patch(mesh, x0, x1, y0, y1, inner, outer, center, mat, rng):
    # A closed low-poly bark fragment with real inner face and warm broken
    # edges. Missing fragments therefore form real openings, not dark paint.
    corners=[]
    for radius in (outer,inner):
        for yy in (y0,y1):
            for angle in (x0,x1):
                wobble=1+.018*math.sin(4*angle+yy*.8)
                corners.append(mesh.vertex((radius*math.cos(angle)*wobble, yy,
                                            center+radius*math.sin(angle)*wobble)))
    o00,o01,o10,o11,i00,i01,i10,i11=corners
    for face,material in [((o00,o10,o11,o01),mat),((i01,i11,i10,i00),0),
                          ((o00,o01,i01,i00),2),((o11,o10,i10,i11),2),
                          ((o10,o00,i00,i10),2),((o01,o11,i11,i01),2)]:
        mesh.face(face,material)


def split_hollow_log(mesh, spec, rng):
    c=[]
    outer,inner,length=spec["outer"],spec["inner"],spec["length"]
    sides=14; sections=7; step=length/sections; da=math.tau/sides
    solid=set()
    for j in range(sections):
        y0=-length*.5+j*step; y1=y0+step
        for k in range(sides):
            # A broad broken side doorway and missing crown fragments. Roof
            # remains in either end bay, creating real partial shelter.
            broad=spec["id"].endswith("B")
            side_gap=j in ((1,2,3,4) if broad else (2,3,4)) and k in (0,1,12,13)
            roof_gap=j in ((2,3,4,5) if broad else (3,4)) and k in ((3,4,5) if broad else (3,4))
            end_damage=j==0 and k in (2,3,12)
            if side_gap or roof_gap or end_damage: continue
            solid.add((j,k))
            a0=k*da; a1=(k+1)*da
            _shell_patch(mesh,a0,a1,y0,y1,inner,outer,outer,
                         3 if (j*7+k)%23==0 else (1 if (j+k)%6==0 else 0),rng)
    # Merge contiguous panels into one collision slab per angular run. Missing
    # wall and roof sections remain open and the shape count stays affordable.
    for k in range(sides):
        run=[]
        for j in range(sections+1):
            if j<sections and (j,k) in solid:
                run.append(j)
            elif run:
                first,last=run[0],run[-1]
                c.append({"kind":"shell_slab","angle":round((k+.5)*da,6),
                          "y":round(-length*.5+(first+last+1)*step*.5,5),
                          "length":round((last-first+1)*step*.97,5),
                          "inner":inner,"outer":outer,"sides":sides})
                run=[]
    # Disconnected collapsed roof pieces are structural but stay outside the
    # side doorway and central swim corridor.
    beam(mesh,c,[(-outer*.75,-length*.37,outer*1.50),
                 (-outer*1.25,-length*.1,outer*1.57)],
         [.27,.13],rng,sides=7)
    return c


def sunken_branch_crown(mesh, spec, rng):
    c=[]
    r=spec["radius"]
    beam(mesh,c,[(0,-4.7,.55),(0,-2.8,.75),(.30,-.65,1.05),(.48,1.10,1.18)],
         [r*1.05,r*.94,r*.77,r*.56],rng,sides=11)
    spread=spec["spread"]
    arms=[(-1,-.25,3.10,1.70), (1,.25,4.15,2.85),
          (-1,1.2,2.50,3.95),(1,1.6,3.25,1.48),
          (-1,2.45,4.40,2.55),(1,2.8,2.85,4.45)]
    if spec["id"].endswith("B"):
        arms.extend([(-1,4.10,3.65,4.70),(1,4.70,4.20,3.75)])
    for i,(sign,base_y,reach,h) in enumerate(arms):
        start=(.25,base_y,1.10)
        mid=(sign*reach*.52,base_y+1.25,h*.63)
        end=(sign*reach,base_y+2.8,h)
        beam(mesh,c,[start,mid,end],[r*.49,r*.30,r*.12],rng,sides=8)
        if i%2==0:
            beam(mesh,c,[mid,(mid[0]+sign*.55,mid[1]+1.15,mid[2]+.82)],
                 [r*.17,.06],rng,sides=6)
    return c


FAMILIES={
    "long_fallen_tree":long_fallen_tree,
    "forked_fallen_log":forked_fallen_log,
    "root_ball_tree":root_ball_tree,
    "split_hollow_log":split_hollow_log,
    "sunken_branch_crown":sunken_branch_crown,
}


def make(spec, materials):
    mesh=old.Mesh()
    rng=random.Random(spec["seed"])
    collisions=FAMILIES[spec["family"]](mesh,spec,rng)
    obj=mesh.create(spec["stem"]+"__mesh",materials)
    obj["faf_id"]=spec["id"]
    obj["faf_habitat"]="pond"
    obj["faf_family"]=spec["family"]
    obj["faf_seed"]=spec["seed"]
    return obj,collisions
