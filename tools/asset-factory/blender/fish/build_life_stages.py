"""Build the twenty missing developmental variants without touching adult files.

blender -b --python tools/asset-factory/blender/fish/build_life_stages.py -- --root REPO
Outputs are guarded; --overwrite only replaces files owned by this builder.
"""

from __future__ import annotations

import argparse
import json
from pathlib import Path
import sys

import bpy
import bmesh
from mathutils import Vector

TOOLING = Path(__file__).resolve().parents[1]
if str(TOOLING) not in sys.path:
    sys.path.insert(0, str(TOOLING))

from common.export import export_selected_glb
from common.validation import inspect_objects
from fish.breathing import validate_breathing_rig, validate_exported_breathing
from fish.life_stages import PROFILES, all_swimming_blueprints, base_blueprint
from fish.reef_families import _material, breathing_spec, create_fish
from fish.build_reef_pack_001 import inspect_loops


EGG_COLORS = {
    1: ("#D9C989", "#A48B5B"), 2: ("#D8B6A2", "#9C765D"),
    3: ("#A5D0C8", "#648A80"), 4: ("#DCD2AE", "#93856C"),
}
EGG_DIAMETERS = {1: .0032, 2: .0026, 3: .0022, 4: .0028}


def _ellipsoid(name, radius, stretch, material, collection, center=(0, 0, 0), subdivisions=1):
    mesh = bpy.data.meshes.new(name + "__geometry")
    bm = bmesh.new()
    bmesh.ops.create_icosphere(bm, subdivisions=subdivisions, radius=radius)
    for vertex in bm.verts:
        vertex.co = Vector((vertex.co.x*stretch[0]+center[0],
                            vertex.co.y*stretch[1]+center[1],
                            vertex.co.z*stretch[2]+center[2]))
    bm.to_mesh(mesh)
    bm.free()
    mesh.update()
    mesh.materials.append(material)
    obj = bpy.data.objects.new(name, mesh)
    collection.objects.link(obj)
    return obj


def create_egg(number, scene):
    base = base_blueprint(number)
    stem = base.stem + "__egg"
    diameter = EGG_DIAMETERS[number]
    collection = bpy.data.collections.new(base.faf_id + " Egg Source")
    scene.collection.children.link(collection)
    shell_hex, embryo_hex = EGG_COLORS[number]
    shell = _material("mat__" + stem + "_shell", shell_hex)
    embryo = _material("mat__" + stem + "_embryo", embryo_hex)
    # Opaque broad-facet shell and a surface-visible embryo mark. No miniature
    # fish skeleton or expensive transparency layer is required.
    mesh_a = _ellipsoid(stem+"__shell", diameter*.5, (1,.82,1.05), shell, collection)
    mesh_b = _ellipsoid(stem+"__embryo_mark", diameter*.155,
                        (.68,.50,.68), embryo, collection,
                        center=(diameter*.39,-diameter*.10,diameter*.08), subdivisions=0)
    for obj in (mesh_a, mesh_b):
        obj["faf_id"] = base.faf_id
        obj["faf_life_stage"] = "egg"
    coords=[tuple(vertex.co) for obj in (mesh_a,mesh_b) for vertex in obj.data.vertices]
    dimensions=[round(max(v[i] for v in coords)-min(v[i] for v in coords),6) for i in range(3)]
    return [mesh_a, mesh_b], {
        "faf_id": base.faf_id, "species": base.slug, "life_stage": "egg",
        "stem": stem, "dimensions_m": dimensions,
        "triangles": sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in (mesh_a,mesh_b)),
        "materials": 2, "bones": 0, "actions": [], "breathing_anatomy": "not_applicable_egg",
        "generator_config": {"diameter_m": diameter, "shell": shell_hex, "embryo": embryo_hex,
                             "deterministic": True},
    }


def _select(objects, active):
    bpy.ops.object.select_all(action="DESELECT")
    for obj in objects:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = active


def _verify_source(objects, report, swimming):
    check = inspect_objects(objects)
    if check["problems"] or check["triangle_count_source"] != report["triangles"]:
        raise ValueError(f"{report['stem']} invalid source: {check}")
    if check["armature_count"] != (1 if swimming else 0):
        raise ValueError(f"{report['stem']} wrong armature count")
    for obj in objects:
        if tuple(round(v,6) for v in obj.location) != (0,0,0) or any(abs(v)>1e-6 for v in obj.rotation_euler):
            raise ValueError(f"{obj.name} nonidentity export transform")


def build(root: Path, overwrite=False):
    output_dir = root / "game/assets/faf/fish"
    source = root / "assets/faf/fish/life_stages_001_to_005.blend"
    manifest = root / "assets/faf/fish/life_stages_001_to_005_manifest.json"
    if not overwrite and (source.exists() or manifest.exists()):
        raise FileExistsError("life-stage source already exists; use --overwrite for owned outputs")
    output_dir.mkdir(parents=True,exist_ok=True)
    source.parent.mkdir(parents=True,exist_ok=True)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    empty_scenes=list(bpy.data.scenes)
    built=[]
    for number in range(1,5):
        bp=base_blueprint(number)
        scene=bpy.data.scenes.new(bp.faf_id + " Egg")
        bpy.context.window.scene=scene
        objects,report=create_egg(number,scene)
        built.append((scene,objects,report,None))
    for bp in all_swimming_blueprints():
        scene=bpy.data.scenes.new(bp.faf_id + " " + bp.life_stage)
        bpy.context.window.scene=scene
        armature,meshes,report=create_fish(bp,scene)
        report["stem"]=bp.stem
        report["loop_checks"]=inspect_loops(bp,scene,armature)
        armature["faf_id"]=bp.faf_id
        armature["faf_life_stage"]=bp.life_stage
        for obj in meshes:
            obj["faf_id"]=bp.faf_id
            obj["faf_life_stage"]=bp.life_stage
        built.append((scene,[armature,*meshes],report,bp))
    for scene in empty_scenes:
        bpy.data.scenes.remove(scene)
    for scene,objects,report,bp in built:
        bpy.context.window.scene=scene
        armature=objects[0] if bp else None
        _select(objects,armature or objects[0])
        _verify_source(objects,report,bp is not None)
        if bp:
            report["breathing_validation"]=validate_breathing_rig(
                armature,objects[1:],breathing_spec(bp))
        path=output_dir/(report["stem"]+".glb")
        export_selected_glb(str(path),include_animations=bp is not None,
                            overwrite=overwrite,animation_mode="NLA_TRACKS" if bp else "ACTIONS",
                            export_metadata=True)
        if bp:
            report["breathing_export"]=validate_exported_breathing(path,bp.breathing_anatomy,
                                                       parent_bone=breathing_spec(bp).parent_bone)
        report["glb"]=path.relative_to(root).as_posix()
        report["blender_source"]=source.relative_to(root).as_posix()
        report["technical_validation_state"]="source_and_export_passed"
        report["style_review_state"]="pending_native_review"
    bpy.context.window.scene=built[0][0]
    bpy.context.preferences.filepaths.save_version=0
    bpy.ops.wm.save_as_mainfile(filepath=str(source),compress=True)
    manifest.write_text(json.dumps({
        "family":"FAF-FISH-001–005 developmental stages",
        "generator":"fish.life_stages.v1", "blender_version":bpy.app.version_string,
        "source":source.relative_to(root).as_posix(),
        "adult_policy":"canonical existing adult Blender/GLB files are unchanged",
        "assets":[report for _,_,report,_ in built],
        "adult_assets":[{"faf_id":base_blueprint(n).faf_id,"life_stage":"adult",
                         "stem":base_blueprint(n).stem,
                         "glb":f"game/assets/faf/fish/{base_blueprint(n).stem}.glb"}
                        for n in range(1,6)],
    },indent=2)+"\n")
    return {"new_assets":len(built),"source":str(source),"manifest":str(manifest)}


if __name__ == "__main__":
    args=sys.argv[sys.argv.index("--")+1:] if "--" in sys.argv else []
    parser=argparse.ArgumentParser()
    parser.add_argument("--root",type=Path,required=True)
    parser.add_argument("--overwrite",action="store_true")
    options=parser.parse_args(args)
    print("FAF_LIFE_STAGES="+json.dumps(build(options.root.resolve(),options.overwrite)))
