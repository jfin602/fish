"""Build ten large Pond wood variants and their editable Blender masters.

Run with Blender: -b --python this_file -- --root REPO [--overwrite].
"""
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

import bpy

HERE=Path(__file__).resolve().parent
TOOLING=HERE.parent
for path in (HERE,TOOLING):
    if str(path) not in sys.path: sys.path.insert(0,str(path))
from common.export import export_selected_glb
from common.validation import inspect_objects
import build_pond_wood_001 as old
import pond_cover_expansion as forms

RECIPES=[
    dict(id="FAF-WOOD-005A",stem="wood_005a_long_fallen_tree",family="long_fallen_tree",seed=5101,length=14.8,radius=1.02,bend=.24,role="elongated hard-cover landmark"),
    dict(id="FAF-WOOD-005B",stem="wood_005b_bent_fallen_tree",family="long_fallen_tree",seed=5102,length=17.2,radius=1.18,bend=1.05,role="bent bank-side hard-cover landmark"),
    dict(id="FAF-WOOD-006A",stem="wood_006a_forked_fallen_log",family="forked_fallen_log",seed=6201,radius=.88,spread=3.15,role="forked route splitter"),
    dict(id="FAF-WOOD-006B",stem="wood_006b_high_fork_log",family="forked_fallen_log",seed=6202,radius=1.02,spread=4.0,role="asymmetric high fork and vertical escape"),
    dict(id="FAF-WOOD-007A",stem="wood_007a_open_root_ball",family="root_ball_tree",seed=7301,radius=1.10,spread=2.9,role="root undercut limited refuge"),
    dict(id="FAF-WOOD-007B",stem="wood_007b_broad_root_ball",family="root_ball_tree",seed=7302,radius=1.30,spread=3.55,role="broad root-ball hide-behind"),
    dict(id="FAF-WOOD-008A",stem="wood_008a_split_decayed_log",family="split_hollow_log",seed=8401,length=7.7,outer=1.95,inner=1.53,role="end passage and side-entry hollow refuge"),
    dict(id="FAF-WOOD-008B",stem="wood_008b_broad_broken_hollow",family="split_hollow_log",seed=8402,length=9.2,outer=2.28,inner=1.78,role="broad broken hollow and lateral exit"),
    dict(id="FAF-WOOD-009A",stem="wood_009a_sunken_branch_crown",family="sunken_branch_crown",seed=9501,radius=.83,spread=4.4,role="layered branch-maze cover"),
    dict(id="FAF-WOOD-009B",stem="wood_009b_high_branch_crown",family="sunken_branch_crown",seed=9502,radius=1.02,spread=5.1,role="large vertical and lateral branch maze"),
]

OPENINGS={
    "long_fallen_tree": [dict(name="both long sides",classification="hard-cover travel lane",fish_clearance_m=1.2)],
    "forked_fallen_log": [dict(name="space between outer arms at forward half",classification="traversable lateral route",fish_clearance_m=2.1),dict(name="apex",classification="visual gap only",fish_clearance_m=0)],
    "root_ball_tree": [dict(name="central root undercut",classification="limited refuge",fish_clearance_m=1.25),dict(name="narrow upper root windows",classification="too small to enter; visual gap only",fish_clearance_m=0)],
    "split_hollow_log": [dict(name="both broken ends through center",classification="passage",fish_clearance_m=2.45),dict(name="right-side collapsed wall",classification="side-entry passage",fish_clearance_m=1.35),dict(name="missing roof",classification="peek-through visual gap",fish_clearance_m=0)],
    "sunken_branch_crown": [dict(name="raised outer branch lanes",classification="traversable lateral route",fish_clearance_m=1.7),dict(name="raised middle openings",classification="vertical escape route",fish_clearance_m=1.5),dict(name="lower outer branch crossings",classification="camera-constrained visual gap; not a declared route",fish_clearance_m=0)],
}

NATIVE_ROUTES={
    "long_fallen_tree":"both long-side Fish-controller paths passed",
    "forked_fallen_log":"lateral fork Fish-controller path passed",
    "root_ball_tree":"central undercut Fish-controller refuge entry passed",
    "split_hollow_log":"end-to-end and side-entry Fish-controller paths passed; fish-sphere probes passed",
    "sunken_branch_crown":"lateral and ascending Fish-controller paths passed",
}


def build(root:Path,overwrite:bool=False):
    source_dir=root/"assets/faf/pond/wood"
    export_dir=root/"game/assets/faf/pond/wood"
    source_dir.mkdir(parents=True,exist_ok=True)
    export_dir.mkdir(parents=True,exist_ok=True)
    manifest=source_dir/"pond_wood_002_manifest.json"
    paths=[manifest]
    for spec in RECIPES:
        paths.extend((source_dir/(spec["stem"]+".blend"),export_dir/(spec["stem"]+".glb")))
    if not overwrite and any(path.exists() for path in paths):
        raise FileExistsError("wood-002 output exists; pass --overwrite deliberately")
    records=[]
    for spec in RECIPES:
        bpy.ops.wm.read_factory_settings(use_empty=True)
        bpy.context.preferences.filepaths.save_version=0
        bpy.context.scene.unit_settings.system="METRIC"
        mats=[old.material(name,color) for name,color in old.COLORS]
        obj,collisions=forms.make(spec,mats)
        bpy.ops.object.select_all(action="DESELECT")
        obj.select_set(True);bpy.context.view_layer.objects.active=obj
        check=inspect_objects([obj])
        if check["problems"]: raise RuntimeError(f"{spec['id']}: {check['problems']}")
        source=source_dir/(spec["stem"]+".blend")
        output=export_dir/(spec["stem"]+".glb")
        bpy.ops.wm.save_as_mainfile(filepath=str(source))
        export_selected_glb(str(output),include_animations=False,overwrite=True,export_metadata=True)
        records.append(dict(faf_id=spec["id"],family=spec["family"],stem=spec["stem"],
                            habitat="pond",role=spec["role"],seed=spec["seed"],
                            generator_config=spec,dimensions_m=[round(x,5) for x in obj.dimensions],
                            triangle_count=check["triangle_count_source"],material_count=check["material_count"],
                            blender_source=str(source.relative_to(root)),glb=str(output.relative_to(root)),
                            godot_wrapper=f"game/scenes/faf_pond_wood_002/{spec['stem']}.tscn",
                            collision_shapes=collisions,collision_type=("open shell panels plus outer capsule" if spec["family"]=="split_hollow_log" else "segmented capsules"),
                            collision_count=len(collisions),openings=(OPENINGS[spec["family"]]+([dict(name="lower B-fork cross-limb opening",classification="camera-constrained visual gap; not a declared route",fish_clearance_m=0)] if spec["id"]=="FAF-WOOD-006B" else [])),
                            pivot="ground-contact origin; Blender +Z up, length along Y",
                            technical_state="candidate; import and native checks recorded separately",
                            visual_review_state="reviewed in Blender MCP and isolated native Forward+; owner style approval pending",
                            native_traversal_state=NATIVE_ROUTES[spec["family"]]+"; see wood-002 record"))
    manifest.write_text(json.dumps(dict(pack="pond-wood-002",generator="wood.pond_cover_expansion v1",
                                        asset_count=len(records),assets=records),indent=2)+"\n")
    print(f"POND_WOOD_002_BUILT assets={len(records)} triangles={sum(x['triangle_count'] for x in records)} manifest={manifest}")


if __name__=="__main__":
    argv=sys.argv[sys.argv.index("--")+1:] if "--" in sys.argv else []
    parser=argparse.ArgumentParser()
    parser.add_argument("--root",required=True,type=Path)
    parser.add_argument("--overwrite",action="store_true")
    args=parser.parse_args(argv)
    build(args.root.resolve(),args.overwrite)
