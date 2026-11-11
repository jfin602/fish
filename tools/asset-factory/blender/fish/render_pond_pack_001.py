"""Render repeatable Pond fish silhouette and deformation evidence.

Open one species master in background Blender, then pass --root and --species.
This script adds review-only lights/cameras in memory and never saves the master.
"""

import argparse
import json
from pathlib import Path
import sys

import bpy
from mathutils import Vector

TOOLING=Path(__file__).resolve().parents[1]
if str(TOOLING) not in sys.path:sys.path.insert(0,str(TOOLING))
from fish.pond_families import species_by_number


VIEWS={
    "side":(3.,0.,.25),
    "top":(0.,0.,3.),
    "three_quarter":(2.5,-1.3,1.4),
    "egg_front":(0.,-3.,1.),
}
SHOTS=(
    ("egg","Idle","side",.0),("egg","Hatch","egg_front",1.),
    ("larva","Idle","side",.0),
    ("juvenile","Idle","side",.0),("juvenile","Idle","top",.0),
    ("juvenile","Idle","three_quarter",.0),
    ("juvenile","SwimFast","side",.25),("juvenile","TurnLeft","three_quarter",.5),
    ("subadult","Idle","side",.0),
    ("adult","Idle","side",.0),("adult","Idle","top",.0),
    ("adult","Idle","three_quarter",.0),
    ("adult","SwimFast","side",.25),("adult","TurnRight","three_quarter",.5),
)


def _aim(obj,target):
    obj.rotation_euler=(Vector(target)-obj.location).to_track_quat("-Z","Y").to_euler()


def _prepare(scene,length):
    world=bpy.data.worlds.new(scene.name+" Review Water")
    world.use_nodes=True
    world.node_tree.nodes["Background"].inputs["Color"].default_value=(.10,.17,.11,1)
    world.node_tree.nodes["Background"].inputs["Strength"].default_value=.9
    scene.world=world
    scene.render.engine="BLENDER_EEVEE"
    scene.render.resolution_x=1200
    scene.render.resolution_y=720
    scene.render.resolution_percentage=100
    scene.view_settings.view_transform="Standard"
    scene.view_settings.look="Medium High Contrast"
    camera_data=bpy.data.cameras.new(scene.name+" Review Camera")
    camera=bpy.data.objects.new(camera_data.name,camera_data)
    scene.collection.objects.link(camera)
    camera_data.type="ORTHO"
    camera_data.clip_start=.0001
    scene.camera=camera
    for index,(position,color,energy) in enumerate(((Vector((2.,-1.,2.)),(1.,.83,.54),150),
                                                     (Vector((-1.,1.,1.)),(.45,.65,.60),80))):
        light_data=bpy.data.lights.new(scene.name+f" Pond Softbox {index}","AREA")
        light_data.energy=energy
        light_data.shape="DISK"
        light_data.size=2.5
        light_data.color=color
        light=bpy.data.objects.new(light_data.name,light_data)
        scene.collection.objects.link(light)
        light.location=position
        _aim(light,(0,0,0))
    return camera


def render(root,number):
    spec=species_by_number(number)
    evidence=root/"docs/fish-asset-factory/pond-fish-pack-001/evidence"
    evidence.mkdir(parents=True,exist_ok=True)
    manifest=json.loads((root/"assets/faf/pond/fish/pond_fish_001_manifest.json").read_text())
    reports={a["life_stage"]:a for a in manifest["assets"] if a["faf_id"]==spec.faf_id}
    cameras={}
    for stage in reports:
        scene=bpy.data.scenes[spec.faf_id+" "+stage+" Pond Source"]
        cameras[stage]=_prepare(scene,max(reports[stage]["dimensions_m"]))
    for stage,role,view,t in SHOTS:
        scene=bpy.data.scenes[spec.faf_id+" "+stage+" Pond Source"]
        bpy.context.window.scene=scene
        stem=spec.stem(stage)
        arm=bpy.data.objects[stem+"__armature"]
        action=bpy.data.actions[stem+"__"+role]
        arm.animation_data.action=action
        start,end=action.frame_range
        scene.frame_set(round(start+(end-start)*t))
        length=max(reports[stage]["dimensions_m"])
        camera=cameras[stage]
        offset=Vector(VIEWS[view]).normalized()*max(1.5,length*3)
        camera.location=offset
        _aim(camera,(0,0,0))
        camera.data.ortho_scale=length*(2.7 if stage=="egg" else 1.65)
        label=f"faf_fish_{number:03d}_{stage}_{role.lower()}_{view}.png"
        scene.render.filepath=str(evidence/label)
        bpy.ops.render.render(write_still=True,scene=scene.name)
    return len(SHOTS)


if __name__=="__main__":
    args=sys.argv[sys.argv.index("--")+1:] if "--" in sys.argv else []
    parser=argparse.ArgumentParser()
    parser.add_argument("--root",type=Path,required=True)
    parser.add_argument("--species",type=int,required=True)
    options=parser.parse_args(args)
    print("FAF_POND_RENDERS="+str(render(options.root.resolve(),options.species)))
