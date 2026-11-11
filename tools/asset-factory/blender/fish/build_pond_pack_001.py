"""Build five original Pond species and all 25 rigged stage exports.

Run from the repo root:
blender -b -t 2 --python tools/asset-factory/blender/fish/build_pond_pack_001.py -- --root "$PWD"
The guarded --overwrite option affects only this builder's Pond outputs.
"""

from pathlib import Path
import argparse
import json
import sys

import bpy

TOOLING=Path(__file__).resolve().parents[1]
if str(TOOLING) not in sys.path:
    sys.path.insert(0,str(TOOLING))
from common.export import export_selected_glb
from common.validation import inspect_objects
from fish.pond_families import SPECIES,STAGES,PROFILES,create_egg,create_swimmer


def _select(objects,arm):
    bpy.ops.object.select_all(action="DESELECT")
    for obj in objects:
        obj.select_set(True)
    bpy.context.view_layer.objects.active=arm


def build(root,overwrite=False,species_number=None):
    source_dir=root/"assets/faf/pond/fish"
    output_dir=root/"game/assets/faf/pond/fish"
    manifest_path=source_dir/"pond_fish_001_manifest.json"
    source_dir.mkdir(parents=True,exist_ok=True)
    output_dir.mkdir(parents=True,exist_ok=True)
    if manifest_path.exists() and not overwrite and species_number is None:
        raise FileExistsError("Pond manifest exists; use --overwrite for owned outputs")
    reports=[]
    chosen=[s for s in SPECIES if species_number is None or s.number==species_number]
    if not chosen:
        raise ValueError("unknown Pond FAF species")
    for spec in chosen:
        source=source_dir/(f"fish_{spec.number:03d}_{spec.slug}.blend")
        if source.exists() and not overwrite:
            raise FileExistsError(f"Pond Blender master exists: {source}")
        bpy.ops.wm.read_factory_settings(use_empty=True)
        empties=list(bpy.data.scenes)
        built=[]
        for stage in STAGES:
            scene=bpy.data.scenes.new(spec.faf_id+" "+stage+" Pond Source")
            bpy.context.window.scene=scene
            scene.unit_settings.system="METRIC"
            scene.unit_settings.scale_length=1
            relative_source=source.relative_to(root).as_posix()
            arm,meshes,report=(create_egg(spec,scene,relative_source) if stage=="egg" else
                               create_swimmer(spec,PROFILES[stage],scene,relative_source))
            built.append((scene,arm,meshes,report))
        for scene in empties:
            bpy.data.scenes.remove(scene)
        for scene,arm,meshes,report in built:
            bpy.context.window.scene=scene
            _select([arm,*meshes],arm)
            inspect=inspect_objects([arm,*meshes])
            if inspect["problems"] or inspect["triangle_count_source"]!=report["triangles"]:
                raise ValueError(f"{report['stem']} failed source validation: {inspect}")
            for obj in [arm,*meshes]:
                if tuple(round(v,6) for v in obj.location)!=(0,0,0) or any(abs(v)>1e-6 for v in obj.rotation_euler) or any(abs(v-1)>1e-6 for v in obj.scale):
                    raise ValueError(f"{obj.name} has nonidentity export transform")
            output=output_dir/(report["stem"]+".glb")
            export_selected_glb(str(output),include_animations=True,overwrite=overwrite,
                                animation_mode="NLA_TRACKS",export_metadata=True)
            report["glb"]=output.relative_to(root).as_posix()
            report["technical_review"]="source_and_export_generated"
            reports.append(report)
        bpy.context.window.scene=built[0][0]
        bpy.context.preferences.filepaths.save_version=0
        bpy.ops.wm.save_as_mainfile(filepath=str(source),compress=True)
        print("FAF_POND_SPECIES_BUILT "+spec.faf_id+" "+spec.common_name)
    if species_number is None:
        manifest={"pack":"Pond FAF fish 001","generator":"fish.pond_families.v1",
                  "habitat":"pond","blender_version":bpy.app.version_string,
                  "source_reference":"owner-supplied best-pond-faf.png; local reference, not redistributed",
                  "assets":reports}
        manifest_path.write_text(json.dumps(manifest,indent=2)+"\n")
    return {"assets":len(reports),"species":len(chosen),
            "manifest":str(manifest_path) if species_number is None else "not written for partial build"}


if __name__=="__main__":
    args=sys.argv[sys.argv.index("--")+1:] if "--" in sys.argv else []
    parser=argparse.ArgumentParser()
    parser.add_argument("--root",type=Path,required=True)
    parser.add_argument("--overwrite",action="store_true")
    parser.add_argument("--species",type=int)
    options=parser.parse_args(args)
    print("FAF_POND_PACK="+json.dumps(build(options.root.resolve(),options.overwrite,options.species)))
