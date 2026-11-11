"""Set each Pond creature GLB Action's Godot loop mode from the manifest."""

from pathlib import Path
import argparse
import json
import re


def configure(root):
    manifest=json.loads((root/"assets/faf/pond/creatures/pond_creature_001_manifest.json").read_text())
    for asset in manifest["assets"]:
        path=root/(asset["glb"]+".import")
        source=path.read_text()
        modes={name:{"settings/loop_mode":1 if name.removeprefix(asset["stem"]+"__")
               in asset["loop_roles"] else 0} for name in asset["actions"]}
        replacement="_subresources={\n\"animations\": "+json.dumps(modes,indent=2)+"\n}"
        updated,hits=re.subn(r"_subresources=\{.*?\}\n(?=gltf/naming_version)",
                              replacement+"\n",source,flags=re.DOTALL)
        if hits!=1:raise ValueError(f"unexpected import sidecar: {path}")
        if updated!=source:path.write_text(updated)
    return len(manifest["assets"])


if __name__=="__main__":
    p=argparse.ArgumentParser();p.add_argument("--root",type=Path,required=True)
    a=p.parse_args();print("FAF_POND_CREATURE_IMPORT_OPTIONS",configure(a.root.resolve()))
