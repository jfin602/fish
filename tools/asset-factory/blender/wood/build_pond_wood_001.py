"""Deterministic Pond wood kit. Run Blender -b --python this_file -- --root REPO.

One source scene and one selected-mesh GLB are written for each of 12 recipes.
All geometry is in metres, Z-up, with a ground-contact origin. Trunks run along
Blender Y, which becomes Godot Z. Hollow ends are physically open mesh loops.
"""
from __future__ import annotations

import argparse
import json
import math
import random
import sys
from pathlib import Path

import bpy

TOOLING = Path(__file__).resolve().parents[1]
if str(TOOLING) not in sys.path:
    sys.path.insert(0, str(TOOLING))
from common.export import export_selected_glb
from common.validation import inspect_objects


RECIPES = [
    dict(id="FAF-WOOD-001A", stem="wood_001a_bent_fallen_log", family="fallen_log", seed=1101, length=5.8, radius=.56, bend=.34, forks=0, role="bank cover"),
    dict(id="FAF-WOOD-001B", stem="wood_001b_forked_fallen_log", family="fallen_log", seed=1102, length=5.0, radius=.64, bend=-.25, forks=2, role="forked cover"),
    dict(id="FAF-WOOD-001C", stem="wood_001c_broken_fallen_log", family="fallen_log", seed=1103, length=4.25, radius=.77, bend=.20, forks=1, role="broken end"),
    dict(id="FAF-WOOD-002A", stem="wood_002a_hero_hollow_trunk", family="hollow_trunk", seed=2201, length=10.0, radius=2.52, inner=2.12, bend=.08, role="two-way passage"),
    dict(id="FAF-WOOD-002B", stem="wood_002b_curved_hollow_trunk", family="hollow_trunk", seed=2202, length=4.75, radius=1.35, inner=.98, bend=.26, role="limited refuge"),
    dict(id="FAF-WOOD-002C", stem="wood_002c_root_flared_hollow", family="hollow_trunk", seed=2203, length=4.25, radius=1.52, inner=1.12, bend=-.16, role="small passage"),
    dict(id="FAF-WOOD-003A", stem="wood_003a_rooted_leaning_trunk", family="stump", seed=3301, height=4.35, radius=.76, lean=.42, role="bank landmark"),
    dict(id="FAF-WOOD-003B", stem="wood_003b_broken_stump", family="stump", seed=3302, height=2.15, radius=1.04, lean=-.17, role="low shoreline stump"),
    dict(id="FAF-WOOD-004A", stem="wood_004a_sparse_sticks", family="cluster", seed=4401, count=5, spread=2.4, height=1.35, role="sparse bottom cover"),
    dict(id="FAF-WOOD-004B", stem="wood_004b_forked_branches", family="cluster", seed=4402, count=4, spread=2.8, height=2.05, role="forked bank cover"),
    dict(id="FAF-WOOD-004C", stem="wood_004c_tangled_roots", family="cluster", seed=4403, count=7, spread=3.2, height=1.15, role="root tangle"),
    dict(id="FAF-WOOD-004D", stem="wood_004d_heavy_branch_cover", family="cluster", seed=4404, count=4, spread=3.5, height=1.78, role="heavy branch cover"),
]

COLORS = [("mat__wood_dark_bark", "#30291e"), ("mat__wood_bark_ochre", "#564129"),
          ("mat__wood_warm_break", "#695033"), ("mat__wood_algae_olive", "#46512b")]


class Mesh:
    def __init__(self):
        self.verts = []
        self.faces = []
        self.mats = []

    def vertex(self, p):
        self.verts.append(tuple(p))
        return len(self.verts) - 1

    def face(self, indices, mat):
        self.faces.append(tuple(indices))
        self.mats.append(mat)

    def create(self, name, materials):
        data = bpy.data.meshes.new(name)
        data.from_pydata(self.verts, [], self.faces)
        data.update()
        for mat in materials:
            data.materials.append(mat)
        for polygon, index in zip(data.polygons, self.mats):
            polygon.material_index = index
            polygon.use_smooth = False
        ob = bpy.data.objects.new(name, data)
        bpy.context.scene.collection.objects.link(ob)
        return ob


def branch(mesh, points, widths, rng, bark=0, sides=7):
    """Piecewise faceted tapered limb. Each ring follows the branch's local tangent."""
    rings = []
    for j, point in enumerate(points):
        p = tuple(point)
        prev = points[max(0, j - 1)]
        nxt = points[min(len(points) - 1, j + 1)]
        tangent = mathutils.Vector((nxt[0]-prev[0], nxt[1]-prev[1], nxt[2]-prev[2])).normalized()
        helper = mathutils.Vector((0, 0, 1)) if abs(tangent.z) < .85 else mathutils.Vector((1, 0, 0))
        u = tangent.cross(helper).normalized()
        v = tangent.cross(u).normalized()
        ring = []
        for k in range(sides):
            angle = math.tau * k / sides
            wobble = 1 + .065 * math.sin(k * 3.1 + j * 1.7 + rng.random())
            offset = (u * math.cos(angle) + v * math.sin(angle)) * widths[j] * wobble
            ring.append(mesh.vertex((p[0]+offset.x, p[1]+offset.y, p[2]+offset.z)))
        rings.append(ring)
    for j in range(len(rings)-1):
        for k in range(sides):
            mesh.face((rings[j][k], rings[j][(k+1)%sides], rings[j+1][(k+1)%sides], rings[j+1][k]),
                      3 if (j+k) % 17 == 0 else (bark if (j+k) % 5 else 1))
    mesh.face(tuple(reversed(rings[0])), 2)
    mesh.face(tuple(rings[-1]), 2)


def long_log(mesh, spec, rng):
    length = spec["length"]
    radius = spec["radius"]
    sections = 9
    points = []
    widths = []
    for i in range(sections):
        t = i/(sections-1)
        points.append((spec["bend"] * math.sin(math.pi*t) + .12 * t,
                       length * (t-.5),
                       radius * (.93 + .11*math.sin(math.pi*t)) + .06*math.sin(t*math.tau)))
        widths.append(radius * (1.05 - .18*t + .06*math.sin(3.5*t)))
    branch(mesh, points, widths, rng, sides=11)
    for i in range(spec["forks"]):
        sign = 1 if i % 2 == 0 else -1
        t = .28 + .43*i/max(1, spec["forks"])
        y = length*(t-.5)
        x = spec["bend"]*math.sin(math.pi*t)
        z = radius*1.65
        branch(mesh, [(x,y,z), (x+sign*.48,y+.45,z+.3),
                      (x+sign*(1.4+i*.25), y+.9, z+.22+i*.2)],
               [radius*.45, radius*.28, .035], rng, sides=7)
    # Low root wedges anchor both ends in mud; they do not close any opening.
    for side in (-1, 1):
        for sign in (-1, 1):
            y = side*length*.40
            branch(mesh, [(0,y,radius*.65), (sign*.45,y+side*.17,.24),
                          (sign*(radius+.55), y+side*.38,.025)],
                   [radius*.20, radius*.12, .015], rng, sides=5)


def hollow(mesh, spec, rng):
    length = spec["length"]
    outer = spec["radius"]
    inner = spec["inner"]
    center_z = outer
    sides = 16 if outer > 2 else 12
    sections = 9 if outer > 2 else 6
    outer_rings, inner_rings = [], []
    for j in range(sections):
        t = j/(sections-1)
        y = (t-.5)*length
        cx = spec["bend"] * math.sin(math.pi*t)
        flare = 1.0 + (.12 if spec["id"].endswith("C") else .055) * abs(2*t-1)**2
        rings = [[], []]
        for k in range(sides):
            a = math.tau*k/sides
            # Broad asymmetric bark facets and uneven broken rims. The inner
            # opening stays regular so authored collision remains honest.
            irregular = 1 + .043*math.sin(3*a + t*2.3 + spec["seed"]*.01) + .028*math.sin(5*a-t*4)
            edge_jag = (.11*math.sin(3*a+spec["seed"]*.01) if j in (0, sections-1) else 0)
            rings[0].append(mesh.vertex((cx+outer*flare*math.cos(a)*irregular*(1+.08*math.sin(math.pi*t)),
                                         y+edge_jag, center_z+outer*math.sin(a)*irregular)))
            opening = 1 + .035*math.sin(3*a + .25) + .022*math.cos(5*a)
            rings[1].append(mesh.vertex((cx+inner*opening*math.cos(a), y+edge_jag*.48,
                                         center_z+inner*opening*math.sin(a))))
        outer_rings.append(rings[0]); inner_rings.append(rings[1])
    for j in range(sections-1):
        for k in range(sides):
            n = (k+1)%sides
            mesh.face((outer_rings[j][k], outer_rings[j][n], outer_rings[j+1][n], outer_rings[j+1][k]),
                      3 if (k+j*3)%29 == 0 else (1 if (k+j)%6 == 0 else 0))
            mesh.face((inner_rings[j+1][k], inner_rings[j+1][n], inner_rings[j][n], inner_rings[j][k]), 0)
    for k in range(sides):
        n = (k+1)%sides
        mesh.face((outer_rings[0][n], outer_rings[0][k], inner_rings[0][k], inner_rings[0][n]), 2 if k%4 else 1)
        mesh.face((outer_rings[-1][k], outer_rings[-1][n], inner_rings[-1][n], inner_rings[-1][k]), 2 if k%4 else 0)
    # Radial roots at the entrance/exit remain outside the opening.
    for end in (-1, 1):
        for side in (-1, 1):
            base_y = end*length*.43
            branch(mesh, [(side*outer*.74,base_y,.82),
                          (side*(outer+.38),base_y+end*.36,.28),
                          (side*(outer+1.05),base_y+end*.65,.02)],
                   [.27,.15,.018], rng, sides=6)
    # Unequal broken limbs and buttresses interrupt the repeated tube stamp.
    for i in range(3 if outer > 2 else 2):
        sign = -1 if i % 2 else 1
        at_y = length * (-.33 + .29*i)
        start_x = sign * outer * (.87 if i % 2 else .93)
        branch(mesh, [(start_x,at_y,center_z+outer*.36),
                      (start_x+sign*.65,at_y+.35,center_z+outer*.56),
                      (start_x+sign*(1.3+.18*i),at_y+.8,center_z+outer*.48)],
               [outer*.17,outer*.105,.035],rng,sides=7)


def stump(mesh, spec, rng):
    h, r, lean = spec["height"], spec["radius"], spec["lean"]
    branch(mesh, [(0,0,r*.95), (lean*.3,0,h*.42), (lean,0,h)],
           [r*1.08,r*.85,r*.62], rng, sides=11)
    for i in range(6):
        a = math.tau*i/6 + .13
        branch(mesh, [(math.cos(a)*r*.68, math.sin(a)*r*.68,.65),
                      (math.cos(a)*(r+.36), math.sin(a)*(r+.36),.21),
                      (math.cos(a)*(r+.90), math.sin(a)*(r+.90),.025)],
               [.23,.13,.015], rng, sides=5)


def cluster(mesh, spec, rng):
    count, spread, height = spec["count"], spec["spread"], spec["height"]
    for i in range(count):
        a = math.tau*(i+.24*rng.random())/count
        r = spread*(.16+.22*rng.random())
        x, y = math.cos(a)*r, math.sin(a)*r
        reach = spread*(.38+.25*rng.random())
        tip = height*(.55+.5*rng.random())
        branch(mesh, [(x,y,.06), (x+math.cos(a)*reach*.55,y+math.sin(a)*reach*.55,tip*.60),
                      (x+math.cos(a)*reach,y+math.sin(a)*reach,tip)],
               [(.15 if spec["id"].endswith("A") else .24), .11, .018], rng, sides=6)
        if spec["id"].endswith(("B", "D")) and i%2 == 0:
            branch(mesh, [(x+math.cos(a)*reach*.5,y+math.sin(a)*reach*.5,tip*.6),
                          (x+math.cos(a+.7)*reach*.8,y+math.sin(a+.7)*reach*.8,tip*.93)],
                   [.11,.015], rng, sides=5)


def material(name, color):
    mat = bpy.data.materials.new(name)
    mat.use_nodes = True
    def linear(c):
        v = int(c,16)/255
        return ((v+.055)/1.055)**2.4 if v>.04045 else v/12.92
    rgb = tuple(linear(color[i:i+2]) for i in (1,3,5))
    mat.diffuse_color = (*rgb,1)
    shader = mat.node_tree.nodes.get("Principled BSDF")
    shader.inputs["Base Color"].default_value = (*rgb,1)
    shader.inputs["Roughness"].default_value = .94
    return mat


def build(root):
    source_dir = root/"assets/faf/pond/wood"
    export_dir = root/"game/assets/faf/pond/wood"
    source_dir.mkdir(parents=True,exist_ok=True)
    export_dir.mkdir(parents=True,exist_ok=True)
    records=[]
    for spec in RECIPES:
        bpy.ops.wm.read_factory_settings(use_empty=True)
        bpy.context.preferences.filepaths.save_version = 0
        scene=bpy.context.scene
        scene.unit_settings.system="METRIC"
        mats=[material(n,c) for n,c in COLORS]
        rng=random.Random(spec["seed"])
        mesh=Mesh()
        {"fallen_log":long_log,"hollow_trunk":hollow,"stump":stump,"cluster":cluster}[spec["family"]](mesh,spec,rng)
        obj=mesh.create(spec["stem"]+"__mesh",mats)
        obj["faf_id"]=spec["id"]
        obj["faf_habitat"]="pond"
        obj["faf_role"]=spec["role"]
        bpy.ops.object.select_all(action="DESELECT")
        obj.select_set(True)
        bpy.context.view_layer.objects.active=obj
        check=inspect_objects([obj])
        if check["problems"]:
            raise RuntimeError(f"{spec['id']}: {check['problems']}")
        source=source_dir/(spec["stem"]+".blend")
        output=export_dir/(spec["stem"]+".glb")
        bpy.ops.wm.save_as_mainfile(filepath=str(source))
        export_selected_glb(str(output),include_animations=False,overwrite=True,export_metadata=True)
        records.append(dict(faf_id=spec["id"],family=spec["family"],stem=spec["stem"],
                            role=spec["role"],seed=spec["seed"],generator_config=spec,
                            dimensions_m=[round(x,3) for x in obj.dimensions],
                            triangle_count=check["triangle_count_source"],
                            material_count=check["material_count"],
                            blender_source=str(source.relative_to(root)),glb=str(output.relative_to(root)),
                            pivot="ground-contact origin; Blender +Z up, length along Y",
                            habitat="pond",transform="identity",technical_state="generated; Godot import pending",
                            style_state="native review pending"))
    out=source_dir/"pond_wood_001_manifest.json"
    out.write_text(json.dumps({"pack":"pond-wood-001","generator":"wood.build_pond_wood_001 v1",
                               "asset_count":len(records),"assets":records},indent=2)+"\n")
    print("FAF_POND_WOOD_BUILT="+str(out))


if __name__=="__main__":
    import mathutils
    argv=sys.argv[sys.argv.index("--")+1:] if "--" in sys.argv else []
    parser=argparse.ArgumentParser();parser.add_argument("--root",type=Path,required=True)
    build(parser.parse_args(argv).root.resolve())
