"""Deterministic Pond ground dressing. Blender 5.x: -b --python this_file -- --root REPO.

All geometry is original, in metres, with a +Z ground-contact origin. The four
families have separate construction functions; recipes select useful variants.
"""
from __future__ import annotations

import argparse
import json
import math
import random
import sys
from pathlib import Path

import bpy
import bmesh

TOOLING = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(TOOLING))
from common.export import export_selected_glb  # noqa: E402

PALETTE = {
    "silt": (0.244, 0.220, 0.151),
    "mud": (0.325, 0.263, 0.160),
    "wet_bank": (0.232, 0.211, 0.145),
    "algae_stone": (0.238, 0.310, 0.128),
}

RECIPES = [
    dict(id="FAF-TERRAIN-002A", stem="terrain_002a_eroded_bank_undercut", family="eroded_bank", seed=2501,
         length=3.4, width=1.7, height=.55, undercut=.23, scar=False, zone="shallow bank shoulder", collision="simple bank wedge"),
    dict(id="FAF-TERRAIN-002B", stem="terrain_002b_eroded_bank_scarred", family="eroded_bank", seed=2502,
         length=2.65, width=2.05, height=.72, undercut=.08, scar=True, zone="mud bank or shelf edge", collision="simple bank wedge"),
    dict(id="FAF-DETAIL-005A", stem="detail_005a_algae_stone_low", family="algae_stone", seed=5501,
         radius=.58, height=.33, stretch=1.28, zone="shallow mud, sheltered stone contact", collision="none in proof; conservative box if gameplay clearance requires"),
    dict(id="FAF-DETAIL-005B", stem="detail_005b_algae_stone_upright", family="algae_stone", seed=5502,
         radius=.43, height=.55, stretch=.84, zone="shallow bank, stone contact", collision="none in proof; conservative box if gameplay clearance requires"),
    dict(id="FAF-DETAIL-006A", stem="detail_006a_pebble_scatter", family="pebbles", seed=6601,
         count=5, spread=.78, radius=.14, zone="quiet shallow mud", collision="none"),
    dict(id="FAF-DETAIL-006B", stem="detail_006b_pebble_chain", family="pebbles", seed=6602,
         count=7, spread=1.05, radius=.11, zone="wood or stone edge", collision="none"),
    dict(id="FAF-DETAIL-007A", stem="detail_007a_leaf_litter_broad", family="leaf_litter", seed=7701,
         count=4, spread=.8, leaf_length=.32, zone="sheltered bank pocket", collision="none"),
    dict(id="FAF-DETAIL-007B", stem="detail_007b_leaf_litter_windrow", family="leaf_litter", seed=7702,
         count=6, spread=1.14, leaf_length=.25, zone="wood-adjacent mud pocket", collision="none"),
]


class Geometry:
    def __init__(self):
        self.vertices = []
        self.faces = []
        self.materials = []

    def add(self, points, polygons, material="mud"):
        offset = len(self.vertices)
        self.vertices.extend(points)
        for polygon in polygons:
            self.faces.append(tuple(offset + i for i in polygon))
            self.materials.append(material)


def bank(recipe, rng):
    g = Geometry()
    length, width, height = recipe["length"], recipe["width"], recipe["height"]
    # A broad wedge tapers to soil contact around its whole irregular shoulder.
    # It remains a true low mound rather than an elevated open roof.
    stations = 6
    bands = 4
    top = []
    for i in range(stations):
        t = i / (stations - 1)
        edge_factor = .74 + .26 * math.sin(math.pi * t)
        for j in range(bands):
            u = j / (bands - 1)
            x = length * (t - .5) + .14 * width * math.sin(2.3 * u + 1.7 * t + recipe["seed"])
            asym = .08 * math.sin(2.7 * t + 2.3 * u + recipe["seed"])
            y = (u - .5) * width * edge_factor + asym
            long_fade = math.sin(math.pi * t) ** .7
            ridge = .10 * long_fade * (1 - abs(2 * u - 1))
            z = .012 + long_fade * (height * (.06 + .94 * u) + ridge)
            if j == 0:
                y += recipe["undercut"] * math.sin(math.pi * t)
                z = .012 + .025 * long_fade
            top.append((x, y, z))
    faces = []
    for i in range(stations - 1):
        for j in range(bands - 1):
            a = i * bands + j
            faces.extend([(a, a + bands, a + bands + 1), (a, a + bands + 1, a + 1)])
    g.add(top, faces, "wet_bank")
    # Close all four shoulders down to contact; no exposed floating underside.
    for edge in ([i*bands for i in range(stations)],
                 [i*bands+bands-1 for i in range(stations)],
                 list(range(bands)),
                 [(stations-1)*bands+j for j in range(bands)]):
        points = []
        for index in edge:
            x, y, z = top[index]
            points.extend([(x,y,z),(x,y,0)])
        g.add(points, [(2*i,2*i+1,2*i+3,2*i+2) for i in range(len(edge)-1)], "mud")
    if recipe["scar"]:
        # One embedded fracture scar breaks the second bank's top form.
        stone_lump(g, -.12, .32, .34, .23, height*.92, .3, rng, algae=False, sides=5)
    return g


def stone_lump(g, cx, cy, rx, ry, h, phase, rng, algae=True, sides=8):
    points = []
    for level, shrink in ((0, .83), (.72, 1.0)):
        for i in range(sides):
            a = phase + 2 * math.pi * i / sides
            jitter = rng.uniform(.87, 1.13)
            points.append((cx + rx * shrink * jitter * math.cos(a),
                           cy + ry * shrink * jitter * math.sin(a), h * level))
    points.append((cx - .09 * rx, cy + .11 * ry, h))
    offset = len(g.vertices)
    g.vertices.extend(points)
    g.faces.append(tuple(offset + i for i in reversed(range(sides))))
    g.materials.append("wet_bank")
    for i in range(sides):
        nxt = (i + 1) % sides
        g.faces.append(tuple(offset + v for v in (i, nxt, sides+nxt, sides+i)))
        g.materials.append("wet_bank")
        g.faces.append(tuple(offset + v for v in (sides+i, sides+nxt, 2*sides)))
        g.materials.append("algae_stone" if algae and i in (0, 1, 2, 6) else "silt")


def stone(recipe, rng):
    g = Geometry()
    radius = recipe["radius"]
    stone_lump(g, 0, 0, radius * recipe["stretch"], radius, recipe["height"], .17, rng)
    return g


def pebbles(recipe, rng):
    g = Geometry()
    count = recipe["count"]
    for i in range(count):
        t = i / max(1, count - 1)
        x = (t - .5) * recipe["spread"] + rng.uniform(-.07, .07)
        y = (.13 * math.sin(i * 2.35) if count == 7 else rng.uniform(-.18, .18))
        size = recipe["radius"] * rng.uniform(.57, 1.2)
        stone_lump(g, x, y, size, size * rng.uniform(.7, 1.25), size * rng.uniform(.35, .65),
                   rng.uniform(0, math.pi), rng, algae=False, sides=5)
    return g


def leaves(recipe, rng):
    g = Geometry()
    count = recipe["count"]
    for i in range(count):
        t = i / max(1, count - 1)
        x = (t - .5) * recipe["spread"] + rng.uniform(-.09, .09)
        y = rng.uniform(-.23, .23) if count == 4 else .11 * math.sin(4 * t)
        yaw = rng.uniform(-1.4, 1.4)
        length = recipe["leaf_length"] * rng.uniform(.8, 1.18)
        width = length * rng.uniform(.32, .45)
        c, s = math.cos(yaw), math.sin(yaw)
        raw = [(-.52*length,0,0),(-.26*length,-width/2,.018),
               (.24*length,-width/2,.022),(.52*length,0,.012),
               (.24*length,width/2,.026),(-.26*length,width/2,.016),(0,0,.055)]
        pts = [(x + a*c-b*s, y + a*s+b*c, z) for a,b,z in raw]
        mat = "mud" if i % 3 else "wet_bank"
        g.add(pts, [(0,1,6),(1,2,6),(2,3,6),(3,4,6),(4,5,6),(5,0,6)], mat)
    return g


BUILDERS = {"eroded_bank":bank, "algae_stone":stone, "pebbles":pebbles, "leaf_litter":leaves}


def material(name):
    m = bpy.data.materials.new("mat__pond_" + name)
    m.use_nodes = True
    color = tuple((c / 12.92 if c <= .04045 else ((c + .055) / 1.055) ** 2.4)
                  for c in PALETTE[name])
    m.diffuse_color = (*color, 1)
    bsdf = m.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = (*color, 1)
    bsdf.inputs["Roughness"].default_value = .94
    bsdf.inputs["Metallic"].default_value = 0
    return m


def build(root):
    output = root / "game/assets/faf/pond/ground"
    source = root / "assets/faf/pond/ground"
    output.mkdir(parents=True, exist_ok=True)
    source.mkdir(parents=True, exist_ok=True)
    records = []
    for recipe in RECIPES:
        bpy.ops.wm.read_factory_settings(use_empty=True)
        scene = bpy.context.scene
        scene.unit_settings.system = "METRIC"
        scene.unit_settings.scale_length = 1
        rng = random.Random(recipe["seed"])
        geometry = BUILDERS[recipe["family"]](recipe, rng)
        mesh = bpy.data.meshes.new(recipe["stem"] + "__mesh")
        mesh.from_pydata(geometry.vertices, [], geometry.faces)
        mesh.update()
        obj = bpy.data.objects.new(recipe["stem"] + "__mesh", mesh)
        scene.collection.objects.link(obj)
        used = sorted(set(geometry.materials))
        for name in used: mesh.materials.append(material(name))
        for polygon, name in zip(mesh.polygons, geometry.materials):
            polygon.material_index = used.index(name)
            polygon.use_smooth = False
        bm = bmesh.new(); bm.from_mesh(mesh)
        bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
        bm.to_mesh(mesh); bm.free()
        mesh.update(); mesh.calc_loop_triangles()
        obj["faf_id"] = recipe["id"]
        obj["faf_habitat"] = "pond"
        obj["faf_variant"] = recipe["id"][-1]
        obj["generator"] = "ground_dressing.build_pack_001 v1"
        obj["seed"] = recipe["seed"]
        assert abs(min(v.co.z for v in mesh.vertices)) < 1e-5
        assert tuple(obj.scale) == (1,1,1) and obj.location.length == 0
        bpy.context.view_layer.objects.active = obj
        obj.select_set(True)
        glb = output / (recipe["stem"] + ".glb")
        export_selected_glb(str(glb), include_animations=False, overwrite=True, export_metadata=True)
        blend = source / (recipe["stem"] + ".blend")
        bpy.ops.wm.save_as_mainfile(filepath=str(blend), compress=True)
        records.append({"faf_id":recipe["id"],"family":recipe["family"],"stem":recipe["stem"],
                        "habitat":"pond","seed":recipe["seed"],"generator":"ground_dressing.build_pack_001 v1",
                        "generator_config":recipe,"dimensions_m":[round(v,4) for v in obj.dimensions],
                        "triangle_count":len(mesh.loop_triangles),"material_count":len(used),
                        "materials":["mat__pond_"+n for n in used],
                        "pivot":"ground contact, local +Z up", "support_footprint_m":[round(obj.dimensions.x,3),round(obj.dimensions.y,3)],
                        "placement_zone":recipe["zone"],"collision":recipe["collision"],
                        "blender_source":blend.relative_to(root).as_posix(),"glb":glb.relative_to(root).as_posix(),
                        "technical_state":"generated; import check pending","style_state":"native review pending"})
    manifest = source / "pond_ground_001_manifest.json"
    manifest.write_text(json.dumps({"pack":"pond-ground-001","asset_count":8,
                                    "shared_materials":["silt","mud","wet_bank","algae_stone"],
                                    "assets":records}, indent=2)+"\n")
    print("FAF_POND_GROUND_REPORT="+json.dumps({"assets":len(records),"triangles":[x["triangle_count"] for x in records]}))


if __name__ == "__main__":
    args = sys.argv[sys.argv.index("--")+1:] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, required=True)
    build(parser.parse_args(args).root.resolve())
