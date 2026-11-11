"""Measure source skin deformation and snake bend safety across named Actions."""

from pathlib import Path
import argparse
import json
import math
import sys

import bpy
from mathutils import Vector


def vertex_positions(obj):
    deps=bpy.context.evaluated_depsgraph_get()
    evaluated=obj.evaluated_get(deps)
    mesh=evaluated.to_mesh()
    try:
        return [evaluated.matrix_world @ v.co for v in mesh.vertices]
    finally:
        evaluated.to_mesh_clear()


def distance(a,b):
    return max((x-y).length for x,y in zip(a,b))


def check(root):
    manifest=json.loads((root/"assets/faf/pond/creatures/pond_creature_001_manifest.json").read_text())
    results=[]
    for record in manifest["assets"]:
        bpy.ops.wm.open_mainfile(filepath=str(root/record["source"]))
        arm=bpy.data.objects[record["stem"]+"__armature"]
        parts=[obj for obj in bpy.data.objects if obj.type=="MESH"]
        if not parts or any(not any(mod.type=="ARMATURE" and mod.object==arm for mod in obj.modifiers)
                            for obj in parts):
            raise ValueError(record["stem"]+" has unskinned mesh")
        baseline={obj.name:vertex_positions(obj) for obj in parts}
        moving_name="__hind_L" if record["faf_id"].endswith("001") else "__body" if record["faf_id"].endswith("002") else "__fore_L"
        moving=next(obj for obj in parts if moving_name in obj.name)
        action_result={}
        for action_name in record["actions"]:
            action=bpy.data.actions[action_name]
            arm.animation_data.action=action
            lo,hi=action.frame_range
            displacements=[]
            for sample in (0,.25,.5,.75,1):
                bpy.context.scene.frame_set(int(round(lo+(hi-lo)*sample)))
                bpy.context.view_layer.update()
                displacements.append(distance(vertex_positions(moving),baseline[moving.name]))
            peak=max(displacements)
            if peak<.00005:
                raise ValueError(action_name+" fails to deform visible skin")
            if action_name.split("__")[-1] in record["loop_roles"]:
                if abs(displacements[0]-displacements[-1])>.00002:
                    raise ValueError(action_name+" has source displacement seam")
            action_result[action_name.split("__")[-1]]=round(peak,6)
        snake_gap=None
        snake_segment_ratio=None
        if record["faf_id"].endswith("002"):
            body=bpy.data.objects[record["stem"]+"__body"]
            snake_gap=100.
            snake_segment_ratio=[100.,0.]
            for role in ("SurfaceSwim","UnderwaterSwim","TurnLeft","TurnRight"):
                arm.animation_data.action=bpy.data.actions[record["stem"]+"__"+role]
                for frame in (7,13,19,25,31,37,43,49,55):
                    bpy.context.scene.frame_set(frame)
                    p=vertex_positions(body)
                    centers=[sum((p[i*10+j] for j in range(10)),Vector())/10 for i in range(45)]
                    for i in range(44):
                        ratio=(centers[i+1]-centers[i]).length/(.91/44)
                        snake_segment_ratio[0]=min(snake_segment_ratio[0],ratio)
                        snake_segment_ratio[1]=max(snake_segment_ratio[1],ratio)
                        if ratio < .65 or ratio > 1.35:
                            raise ValueError(f"snake segment collapse/stretch {role} frame {frame}: {i}, {ratio}")
                    for i in range(45):
                        for j in range(i+7,45):
                            d=(centers[i]-centers[j]).length
                            snake_gap=min(snake_gap,d)
                            if d<.020:
                                raise ValueError(f"snake near self intersection {role} frame {frame}: {i},{j}, {d}")
        turtle_foot_min_lateral=None
        if record["faf_id"].endswith("003"):
            feet=[obj for obj in parts if "__fore_foot_" in obj.name or "__hind_foot_" in obj.name]
            turtle_foot_min_lateral=100.
            for role in ("BottomWalk","SurfaceFloat","Swim","TurnLeft","TurnRight","Dive"):
                arm.animation_data.action=bpy.data.actions[record["stem"]+"__"+role]
                for frame in (1,11,21,31,41,51,61):
                    bpy.context.scene.frame_set(frame)
                    for foot in feet:
                        vertices=vertex_positions(foot)
                        center=sum(vertices,Vector())/len(vertices)
                        turtle_foot_min_lateral=min(turtle_foot_min_lateral,abs(center.x))
                        if abs(center.x)<.135 and center.z>-.025:
                            raise ValueError(f"turtle foot entered shell {role} frame {frame}: {foot.name}")
        results.append({"faf_id":record["faf_id"],"skinned_meshes":len(parts),
                        "peak_deformation_m":action_result,"snake_nonadjacent_min_center_gap_m":snake_gap,
                        "snake_segment_length_ratio_min_max":snake_segment_ratio,
                        "turtle_foot_min_lateral_m":turtle_foot_min_lateral})
    return results


if __name__=="__main__":
    args=sys.argv[sys.argv.index("--")+1:] if "--" in sys.argv else []
    p=argparse.ArgumentParser();p.add_argument("--root",type=Path,required=True)
    opts=p.parse_args(args)
    print("FAF_POND_CREATURE_DEFORMATION="+json.dumps(check(opts.root.resolve()),indent=2))
