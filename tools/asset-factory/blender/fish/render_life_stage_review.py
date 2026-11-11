"""Render repeatable overview and species progression evidence from review.blend."""

from pathlib import Path
import argparse
import sys

import bpy

TOOLING=Path(__file__).resolve().parents[1]
if str(TOOLING) not in sys.path:sys.path.insert(0,str(TOOLING))
from fish.life_stages import base_blueprint, PROFILES


def _camera(scene,kind):
    return next(obj for obj in scene.objects if obj.type=="CAMERA" and obj.name.startswith(kind))


def _set_pose(role):
    for number,profiles in PROFILES.items():
        base=base_blueprint(number)
        for stage in [profile.stage for profile in profiles]+["adult"]:
            stem=base.stem if stage=="adult" else base.stem+"__"+stage
            arm=bpy.data.objects.get(stem+"__armature")
            if arm is None or arm.animation_data is None:
                continue
            action=bpy.data.actions.get(stem+"__"+role)
            if action is not None:
                arm.animation_data.action=action
                arm.animation_data.use_nla=False


def render(root):
    evidence=root/"docs/fish-asset-factory/life-stages-001/evidence"
    evidence.mkdir(parents=True,exist_ok=True)
    scenes=[bpy.data.scenes["FAF Life Stages | Five Families"]]+[
        bpy.data.scenes[f"FAF-FISH-{number:03d} | Progression"] for number in range(1,6)]
    for role,frame,camera_kind,suffix in (
        ("Glide",1,"Side","side"),
        ("Glide",1,"Three Quarter","three_quarter"),
        ("SwimFast",5,"Three Quarter","swimming"),
    ):
        _set_pose(role)
        for scene in scenes:
            bpy.context.window.scene=scene
            scene.frame_set(frame)
            scene.camera=_camera(scene,camera_kind)
            label="overview" if "Five Families" in scene.name else scene.name[:12].lower().replace("-","_")
            scene.render.filepath=str(evidence/(label+"_"+suffix+".png"))
            bpy.ops.render.render(write_still=True,scene=scene.name)
    return len(scenes)*3


if __name__=="__main__":
    args=sys.argv[sys.argv.index("--")+1:] if "--" in sys.argv else []
    parser=argparse.ArgumentParser();parser.add_argument("--root",type=Path,required=True)
    options=parser.parse_args(args)
    print("FAF_LIFE_STAGE_RENDER_COUNT="+str(render(options.root.resolve())))
