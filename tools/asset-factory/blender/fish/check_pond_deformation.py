"""Inspect real Blender deformation for one five-stage Pond species master.

blender -b assets/faf/pond/fish/fish_006_bluegill.blend --python \
  tools/asset-factory/blender/fish/check_pond_deformation.py -- --root REPO --species 6
"""

import argparse
import json
from pathlib import Path
import sys

import bpy

TOOLING=Path(__file__).resolve().parents[1]
if str(TOOLING) not in sys.path:
    sys.path.insert(0,str(TOOLING))
from fish.animation import reset_pose
from fish.breathing import opening_rotation_z,validate_breathing_rig
from fish.pond_families import STAGES,PROFILES,species_by_number,_breathing_spec


def _coords(obj):
    evaluated=obj.evaluated_get(bpy.context.evaluated_depsgraph_get())
    mesh=evaluated.to_mesh()
    try:
        return [v.co.copy() for v in mesh.vertices]
    finally:
        evaluated.to_mesh_clear()


def _motion(a,b):
    if len(a)!=len(b):
        raise ValueError("deformation changed topology")
    return max((x-y).length for x,y in zip(a,b))


def check(root,number):
    spec=species_by_number(number)
    result=[]
    for stage in STAGES:
        scene=bpy.data.scenes[spec.faf_id+" "+stage+" Pond Source"]
        bpy.context.window.scene=scene
        stem=spec.stem(stage)
        arm=bpy.data.objects[stem+"__armature"]
        arm.animation_data.action=bpy.data.actions[stem+"__Idle"]
        scene.frame_set(1)
        reset_pose(arm)
        bpy.context.view_layer.update()
        if stage=="egg":
            halves=[bpy.data.objects[stem+"__shell_"+side] for side in ("l","r")]
            embryo=bpy.data.objects[stem+"__embryo"]
            neutral=[_coords(obj) for obj in [*halves,embryo]]
            arm.animation_data.action=bpy.data.actions[stem+"__Idle"]
            scene.frame_set(7)
            idle=max(_motion(before,_coords(obj)) for before,obj in zip(neutral,[*halves,embryo]))
            arm.animation_data.action=bpy.data.actions[stem+"__Hatch"]
            scene.frame_set(25)
            hatch=[_motion(before,_coords(obj)) for before,obj in zip(neutral,[*halves,embryo])]
            if idle<1e-7 or min(hatch)<1e-6:
                raise ValueError(f"{stem} egg rig fails Idle/Hatch deformation")
            result.append({"stage":stage,"idle_motion_m":round(idle,8),
                           "hatch_shell_l_m":round(hatch[0],8),
                           "hatch_shell_r_m":round(hatch[1],8),
                           "hatch_embryo_m":round(hatch[2],8)})
            continue
        profile=PROFILES[stage]
        body=bpy.data.objects[stem+"__body"]
        eyes=[bpy.data.objects[stem+"__eye_"+side] for side in ("l","r")]
        neutral=_coords(body)
        eye_neutral=[_coords(obj) for obj in eyes]
        error=_motion(neutral,[v.co for v in body.data.vertices])
        if error>1e-5:
            raise ValueError(f"{stem} rest pose changed body")
        gill=_breathing_spec(spec,profile)
        rig=validate_breathing_rig(arm,[body,*eyes],gill)
        gill_motions=[]
        for name in ("gill_L","gill_R"):
            bone=arm.pose.bones[name]
            bone.rotation_mode="XYZ"
            bone.rotation_euler.z=opening_rotation_z(gill,name)
            bpy.context.view_layer.update()
            movement=_motion(neutral,_coords(body))
            eye=max(_motion(before,_coords(obj)) for before,obj in zip(eye_neutral,eyes))
            if movement<1e-7 or eye>1e-6:
                raise ValueError(f"{stem} {name} missing localized stable-eye movement")
            gill_motions.append(movement)
            bone.rotation_euler.z=0
        swim=[]
        turn=[]
        for role in ("SwimFast","TurnLeft","TurnRight"):
            arm.animation_data.action=bpy.data.actions[stem+"__"+role]
            scene.frame_set(1+round((20 if role=="SwimFast" else 24)*spec.cadence*profile.cadence*(.25 if role=="SwimFast" else .5)))
            bpy.context.view_layer.update()
            movement=_motion(neutral,_coords(body))
            eye=max(_motion(before,_coords(obj)) for before,obj in zip(eye_neutral,eyes))
            if movement<1e-6 or eye>1e-6:
                raise ValueError(f"{stem} {role} body/head deformation failure")
            if role=="SwimFast":swim.append(movement)
            else:turn.append((role,movement,arm.pose.bones["tail_02"].rotation_euler.z))
        if turn[0][2]*turn[1][2]>=0:
            raise ValueError(f"{stem} turns do not oppose")
        result.append({"stage":stage,"neutral_error_m":round(error,9),
                       "gill_L_motion_m":round(gill_motions[0],8),
                       "gill_R_motion_m":round(gill_motions[1],8),
                       "swim_fast_motion_m":round(swim[0],8),
                       "turn_left_motion_m":round(turn[0][1],8),
                       "turn_right_motion_m":round(turn[1][1],8),
                       "eye_motion_m":0,"gill_weighted_vertices":rig["weighted_vertices"]})
    output=root/"docs/fish-asset-factory/pond-fish-pack-001/evidence"/f"deformation_{number:03d}.json"
    output.parent.mkdir(parents=True,exist_ok=True)
    output.write_text(json.dumps(result,indent=2)+"\n")
    return {"species":spec.faf_id,"stages":len(result),"report":str(output)}


if __name__=="__main__":
    args=sys.argv[sys.argv.index("--")+1:] if "--" in sys.argv else []
    parser=argparse.ArgumentParser();parser.add_argument("--root",type=Path,required=True)
    parser.add_argument("--species",type=int,required=True)
    options=parser.parse_args(args)
    print("FAF_POND_DEFORMATION="+json.dumps(check(options.root.resolve(),options.species)))
