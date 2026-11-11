"""Pin loop/one-shot settings for every Pond GLB import sidecar."""

import argparse
import json
from pathlib import Path
import re


def configure(root):
    manifest=json.loads((root/"assets/faf/pond/fish/pond_fish_001_manifest.json").read_text())
    count=0
    for asset in manifest["assets"]:
        path=root/(asset["glb"]+".import")
        source=path.read_text()
        settings={name:{"settings/loop_mode":1 if name.removeprefix(asset["stem"]+"__")
                    in asset["loop_roles"] else 0} for name in asset["actions"]}
        value="_subresources={\n\"animations\": "+json.dumps(settings,indent=2)+"\n}"
        updated,hits=re.subn(r"_subresources=\{.*?\}\n(?=gltf/naming_version)",
                              lambda _:value+"\n",source,flags=re.DOTALL)
        if hits!=1:
            raise ValueError(f"unexpected import sidecar: {path}")
        if updated!=source:
            path.write_text(updated)
        count+=1
    return count


if __name__=="__main__":
    parser=argparse.ArgumentParser();parser.add_argument("--root",type=Path,required=True)
    options=parser.parse_args()
    print("FAF_POND_IMPORT_OPTIONS="+str(configure(options.root.resolve())))
