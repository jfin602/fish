"""Measure actual skin movement for all newly authored swimming life stages.

Run with life_stages_001_to_005.blend open in Blender background mode.
"""

import argparse
import json
from pathlib import Path
import sys

import bpy

TOOLING=Path(__file__).resolve().parents[1]
if str(TOOLING) not in sys.path:sys.path.insert(0,str(TOOLING))
from fish.animation import reset_pose
from fish.breathing import opening_rotation_z
from fish.life_stages import all_swimming_blueprints
from fish.reef_families import breathing_spec


def _coords(obj):
    depsgraph=bpy.context.evaluated_depsgraph_get()
    evaluated=obj.evaluated_get(depsgraph)
    mesh=evaluated.to_mesh()
    try:
        return [vertex.co.copy() for vertex in mesh.vertices]
    finally:
        evaluated.to_mesh_clear()


def _maximum(a,b):
    if len(a)!=len(b):raise ValueError("topology changed during pose")
    return max((x-y).length for x,y in zip(a,b))


def check(root):
    report=[]
    for bp in all_swimming_blueprints():
        scene=bpy.data.scenes[bp.faf_id+" "+bp.life_stage]
        bpy.context.window.scene=scene
        arm=bpy.data.objects[bp.stem+"__armature"]
        body=bpy.data.objects[bp.stem+"__body"]
        eyes=[bpy.data.objects[bp.stem+"__eye_"+side] for side in ("l","r")]
        arm.animation_data.action=bpy.data.actions[bp.stem+"__Glide"]
        scene.frame_set(1)
        reset_pose(arm)
        bpy.context.view_layer.update()
        neutral=_coords(body)
        eye_neutral=[_coords(obj) for obj in eyes]
        neutral_error=_maximum(neutral,[v.co for v in body.data.vertices])
        if neutral_error>1e-5:
            raise ValueError(f"{bp.stem} neutral mesh differs from source")
        spec=breathing_spec(bp)
        gill_motion={}
        for side in ("gill_L","gill_R"):
            bone=arm.pose.bones[side]
            bone.rotation_mode="XYZ"
            bone.rotation_euler.z=opening_rotation_z(spec,side,1.0)
            bpy.context.view_layer.update()
            movement=_maximum(neutral,_coords(body))
            eye_movement=max(_maximum(before,_coords(obj)) for before,obj in zip(eye_neutral,eyes))
            if movement < 1e-6 or eye_movement > 1e-6:
                raise ValueError(f"{bp.stem} {side} lacks localized stable-eye deformation")
            gill_motion[side]=round(movement,8)
            bone.rotation_euler.z=0
        arm.animation_data.action=bpy.data.actions[bp.stem+"__SwimFast"]
        scene.frame_set(1+bp.frames[2]//4)
        bpy.context.view_layer.update()
        swim_motion=_maximum(neutral,_coords(body))
        eye_swim=max(_maximum(before,_coords(obj)) for before,obj in zip(eye_neutral,eyes))
        if swim_motion < 1e-6 or eye_swim>1e-6:
            raise ValueError(f"{bp.stem} swim deformation/head stability failure")
        report.append({"faf_id":bp.faf_id,"life_stage":bp.life_stage,
                       "neutral_error_m":round(neutral_error,9),
                       "strong_gill_motion_m":gill_motion,
                       "swim_body_motion_m":round(swim_motion,8),
                       "eye_motion_m":round(max(eye_swim,eye_movement),9)})
    output=root/"docs/fish-asset-factory/life-stages-001/deformation-report.json"
    output.parent.mkdir(parents=True,exist_ok=True)
    output.write_text(json.dumps(report,indent=2)+"\n")
    return {"stages":len(report),"report":str(output)}


if __name__=="__main__":
    args=sys.argv[sys.argv.index("--")+1:] if "--" in sys.argv else []
    parser=argparse.ArgumentParser();parser.add_argument("--root",type=Path,required=True)
    options=parser.parse_args(args)
    print("FAF_LIFE_STAGE_DEFORMATION="+json.dumps(check(options.root.resolve())))
