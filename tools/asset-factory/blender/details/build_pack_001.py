"""Build eight FAF reef-floor details as separate GLBs and one editable master.

Run: blender -b --python tools/asset-factory/blender/details/build_pack_001.py -- --root REPO
Existing outputs require --overwrite. Review copies in the master are excluded
from every selected-object export.
"""

from __future__ import annotations

import argparse
from dataclasses import asdict
import json
from pathlib import Path
import sys

import bpy
import bmesh

TOOLING = Path(__file__).resolve().parents[1]
if str(TOOLING) not in sys.path:
    sys.path.insert(0, str(TOOLING))

from common.export import export_selected_glb  # noqa: E402
from common.palette import hex_rgb  # noqa: E402
from common.validation import inspect_objects  # noqa: E402
from details.families import (ShellSpec,RubbleSpec,PebbleSpec,SpongeSpec,
                              create_detail)  # noqa: E402


# Small, visibly distinct pairs. Each seed is local to its own generator.
RECIPES = (
    ("001A","shell","fan_shell",ShellSpec(.16,.12,.040,1.05,.18,.10,.07,1011),"shell_cream"),
    ("001B","shell","ridge_shell",ShellSpec(.10,.17,.052,.50,.12,.55,-.10,1012),"shell_sand"),
    ("002A","rubble","coral_rubble",RubbleSpec(3,.075,.14,.50,.35,2021),"rubble_coral"),
    ("002B","rubble","reef_rubble",RubbleSpec(2,.095,.18,.60,.42,2022),"rubble_stone"),
    ("003A","pebble","slate_pebbles",PebbleSpec(5,.037,.105,.27,3031),"pebble_slate"),
    ("003B","pebble","warm_pebbles",PebbleSpec(4,.050,.17,.21,3032),"pebble_warm"),
    ("004A","sponge","encrusting_sponge",SpongeSpec(1,.075,.035,0,.48,.08,4041),"sponge_ochre"),
    ("004B","sponge","cup_sponge",SpongeSpec(2,.037,.085,.11,.40,.23,4042),"sponge_rose"),
)


def _material(swatch):
    name="mat__detail_"+swatch
    material=bpy.data.materials.get(name) or bpy.data.materials.new(name)
    rgb=hex_rgb("detail",swatch)
    linear=tuple(((v+.055)/1.055)**2.4 if v>.04045 else v/12.92 for v in rgb)
    material.use_nodes=True
    material.diffuse_color=(*linear,1)
    shader=material.node_tree.nodes.get("Principled BSDF")
    shader.inputs["Base Color"].default_value=(*linear,1)
    shader.inputs["Roughness"].default_value=.9
    shader.inputs["Metallic"].default_value=0
    return material


def create_sources(scene):
    scene.unit_settings.system="METRIC"
    scene.unit_settings.scale_length=1
    sources={}
    for key,family,slug,spec,swatch in RECIPES:
        family_id=key[:3]
        collection=bpy.data.collections.get(f"FAF-DETAIL-{family_id} Source")
        if collection is None:
            collection=bpy.data.collections.new(f"FAF-DETAIL-{family_id} Source")
        if collection.name not in scene.collection.children:
            scene.collection.children.link(collection)
        stem=f"detail_{key.lower()}_{slug}"
        obj=create_detail(stem+"__mesh",family,spec,_material(swatch),collection)
        obj["faf_id"]=f"FAF-DETAIL-{key}"
        obj["generator"]=f"details.{family}.v1"
        obj["seed"]=spec.seed
        sources[key]=obj
    return sources


def add_review_instances(scene,sources):
    review=bpy.data.collections.new("FAF-DETAIL Pack Review")
    scene.collection.children.link(review)
    for row in range(4):
        for col in range(2):
            key=f"{row+1:03d}{'AB'[col]}"
            source=sources[key]
            obj=source.copy();obj.data=source.data
            obj.name="review__"+source.name.removesuffix("__mesh")
            obj.location=((col-.5)*.34,(1.5-row)*.34,0)
            review.objects.link(obj)
            source.hide_viewport=True
    return review


def _check_source(obj):
    check=inspect_objects([obj])
    bm=bmesh.new();bm.from_mesh(obj.data)
    manifold=all(edge.is_manifold for edge in bm.edges)
    bm.free()
    if (check["problems"] or not manifold or check["material_count"]!=1
            or not 20 <= check["triangle_count_source"] <= 130
            or abs(min(vertex.co.z for vertex in obj.data.vertices))>.0001):
        raise RuntimeError(f"detail source invalid: {obj.name}: {check}, manifold={manifold}")
    return check


def build(root:Path,overwrite=False):
    source=root/"assets/faf/details/detail_001_to_004_family.blend"
    manifest=source.with_name("detail_001_to_004_manifest.json")
    output=root/"game/assets/faf/details"
    destinations=[source,manifest,*[output/f"detail_{key.lower()}_{slug}.glb"
                                    for key,_,slug,_,_ in RECIPES]]
    if not overwrite and any(path.exists() for path in destinations):
        raise FileExistsError("FAF detail output exists; use --overwrite deliberately")
    source.parent.mkdir(parents=True,exist_ok=True)
    output.mkdir(parents=True,exist_ok=True)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    scene=bpy.context.scene
    scene.name="FAF-DETAIL-001–004 Reef Floor Details"
    sources=create_sources(scene)
    reports=[]
    for key,family,slug,spec,swatch in RECIPES:
        obj=sources[key]
        check=_check_source(obj)
        bpy.ops.object.select_all(action="DESELECT")
        obj.select_set(True);bpy.context.view_layer.objects.active=obj
        path=output/f"detail_{key.lower()}_{slug}.glb"
        export_selected_glb(str(path),include_animations=False,overwrite=overwrite)
        reports.append({
            "faf_id":f"FAF-DETAIL-{key}","category":"DETAIL","species_or_type":family,
            "stem":f"detail_{key.lower()}_{slug}","generator":f"details.{family}.v1",
            "dimensions_m":[round(v,5) for v in obj.dimensions],
            "triangles":check["triangle_count_source"],"materials":1,
            "material_name":obj.material_slots[0].material.name,
            "armature_bone_count":0,"animation_actions":[],
            "origin_m":[0,0,0],"rotation_rad":[0,0,0],"scale":[1,1,1],
            "palette":"detail."+swatch,"spec":asdict(spec),"seed":spec.seed,
            "glb":path.relative_to(root).as_posix(),
            "blender_source":source.relative_to(root).as_posix(),
        })
    add_review_instances(scene,sources)
    bpy.ops.object.select_all(action="DESELECT")
    bpy.ops.wm.save_as_mainfile(filepath=str(source),compress=True)
    manifest.write_text(json.dumps({"source":source.relative_to(root).as_posix(),
                                    "assets":reports},indent=2)+"\n")
    return reports


if __name__=="__main__":
    args=sys.argv[sys.argv.index("--")+1:] if "--" in sys.argv else []
    parser=argparse.ArgumentParser()
    parser.add_argument("--root",type=Path,required=True)
    parser.add_argument("--overwrite",action="store_true")
    options=parser.parse_args(args)
    print("FAF_DETAIL_PACK_001_REPORT="+json.dumps(build(options.root,options.overwrite)))
