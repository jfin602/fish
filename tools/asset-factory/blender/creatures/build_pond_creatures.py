"""Build the three original Pond creatures as separate Blender masters and GLBs.

blender -b -t 2 --python tools/asset-factory/blender/creatures/build_pond_creatures.py -- --root "$PWD"
"""

from pathlib import Path
import argparse
import json
import sys

import bpy

TOOLING=Path(__file__).resolve().parents[1]
if str(TOOLING) not in sys.path:sys.path.insert(0,str(TOOLING))
from common.export import export_selected_glb
from common.validation import inspect_objects
from creatures.pond_creatures import CREATURES,build


def main(root,overwrite):
    src=root/"assets/faf/pond/creatures"
    runtime=root/"game/assets/faf/pond/creatures"
    src.mkdir(parents=True,exist_ok=True);runtime.mkdir(parents=True,exist_ok=True)
    manifest=src/"pond_creature_001_manifest.json"
    owned=[src/(c.stem+".blend") for c in CREATURES]+[runtime/(c.stem+".glb") for c in CREATURES]+[manifest]
    if not overwrite:
        existing=[str(p) for p in owned if p.exists()]
        if existing:raise FileExistsError("owned outputs already exist: "+", ".join(existing))
    records=[]
    for c in CREATURES:
        blend=src/(c.stem+".blend")
        arm,parts,record=build(c,blend.relative_to(root).as_posix())
        bpy.ops.object.select_all(action="DESELECT")
        for obj in [arm,*parts]:obj.select_set(True)
        bpy.context.view_layer.objects.active=arm
        report=inspect_objects([arm,*parts])
        if report["problems"] or report["triangle_count_source"]!=record["triangles"]:
            raise ValueError(f"{c.stem} invalid: {report}")
        for obj in [arm,*parts]:
            if any(abs(v)>1e-6 for v in obj.location) or any(abs(v)>1e-6 for v in obj.rotation_euler) or any(abs(v-1)>1e-6 for v in obj.scale):
                raise ValueError(f"{obj.name} nonidentity transform")
        glb=runtime/(c.stem+".glb")
        export_selected_glb(str(glb),include_animations=True,overwrite=overwrite,
                            animation_mode="NLA_TRACKS",export_metadata=True)
        record["glb"]=glb.relative_to(root).as_posix()
        bpy.context.preferences.filepaths.save_version=0
        bpy.ops.wm.save_as_mainfile(filepath=str(blend),compress=True)
        records.append(record)
        print("FAF_POND_CREATURE_BUILT",c.faf_id,c.common,record["triangles"],record["bones"])
    manifest.write_text(json.dumps({"pack":"Pond FAF creatures 001","habitat":"pond",
        "generator":"creatures.pond_creatures.v1","blender_version":bpy.app.version_string,
        "source_reference":"owner-supplied /home/jfin/best-pond-faf.png; style only",
        "assets":records},indent=2)+"\n")
    print("FAF_POND_CREATURE_PACK_OK",len(records))


if __name__=="__main__":
    args=sys.argv[sys.argv.index("--")+1:] if "--" in sys.argv else []
    parser=argparse.ArgumentParser();parser.add_argument("--root",type=Path,required=True)
    parser.add_argument("--overwrite",action="store_true")
    options=parser.parse_args(args)
    main(options.root.resolve(),options.overwrite)
