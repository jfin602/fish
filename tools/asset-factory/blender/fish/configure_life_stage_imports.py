"""Set Godot's source-controlled loop import option for FAF life-stage GLBs.

Run after Godot's first import creates .glb.import sidecars. Only files listed
in the life-stage manifest are touched; canonical adult import settings remain.
"""

import argparse
import json
from pathlib import Path
import re


def configure(root):
    manifest=json.loads((root/"assets/faf/fish/life_stages_001_to_005_manifest.json").read_text())
    count=0
    for asset in manifest["assets"]:
        if asset["life_stage"]=="egg":
            continue
        path=root/(asset["glb"]+".import")
        text=path.read_text()
        names=[asset["stem"]+"__"+role for role in ("Glide","Swim","SwimFast")]
        value="_subresources={\n\"animations\": {\n"+",\n".join(
            json.dumps(name)+": { \"settings/loop_mode\": 1 }" for name in names)+"\n}\n}"
        updated,hits=re.subn(r"_subresources=\{.*?\}\n(?=gltf/naming_version)",
                              lambda _:value+"\n",text,flags=re.DOTALL)
        if hits!=1:
            raise ValueError(f"unexpected Godot import sidecar structure: {path}")
        if updated!=text:
            path.write_text(updated)
        count+=1
    return count


if __name__=="__main__":
    parser=argparse.ArgumentParser();parser.add_argument("--root",type=Path,required=True)
    options=parser.parse_args()
    print("FAF_LIFE_STAGE_IMPORT_OPTIONS="+str(configure(options.root.resolve())))
