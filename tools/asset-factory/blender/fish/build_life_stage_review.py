"""Create a separate presentation .blend; source/export transforms stay identity.

blender -b life_stages_001_to_005.blend --python build_life_stage_review.py -- --root REPO
"""

from pathlib import Path
import argparse
import math
import sys

import bpy
from mathutils import Vector

TOOLING=Path(__file__).resolve().parents[1]
if str(TOOLING) not in sys.path:sys.path.insert(0,str(TOOLING))
from fish.life_stages import base_blueprint

STAGES={
    1:("egg","larva","juvenile","subadult","adult"),
    2:("egg","larva","juvenile","subadult","adult"),
    3:("egg","larva","juvenile","subadult","adult"),
    4:("egg","larva","juvenile","subadult","adult"),
    5:("newborn_pup","early_juvenile","juvenile","subadult","adult"),
}
DISPLAY_SCALE={1:3.2,2:4.5,3:4.5,4:3.5,5:.65}


def _append_adults(root):
    for path in (root/"assets/faf/fish/fish_001_yellow_tang.blend",
                 root/"assets/faf/fish/reef_fish_002_to_005.blend"):
        with bpy.data.libraries.load(str(path),link=False) as (source,target):
            target.collections=[name for name in source.collections if
                                name=="FAF-001 Yellow Tang" or
                                name.startswith("FAF-FISH-") and name.endswith(" Source")]


def _collection(number,stage):
    if stage=="adult":
        return bpy.data.collections["FAF-001 Yellow Tang" if number==1 else
                                    base_blueprint(number).faf_id+" Source"]
    return bpy.data.collections[base_blueprint(number).faf_id+
                                (" Egg Source" if stage=="egg" else " "+stage+" Source")]


def _camera(scene,name,position,target,ortho):
    data=bpy.data.cameras.new(name)
    obj=bpy.data.objects.new(name,data)
    scene.collection.objects.link(obj)
    obj.location=position
    direction=Vector(target)-obj.location
    obj.rotation_euler=direction.to_track_quat("-Z","Y").to_euler()
    data.type="ORTHO";data.ortho_scale=ortho
    scene.camera=obj
    return obj


def _light(scene):
    data=bpy.data.lights.new("FAF Review Softbox","AREA")
    data.energy=1300;data.shape="DISK";data.size=7
    obj=bpy.data.objects.new(data.name,data)
    scene.collection.objects.link(obj)
    obj.location=(3,-1,5)
    obj.rotation_euler=(Vector((0,0,0))-obj.location).to_track_quat("-Z","Y").to_euler()


def _review_scene(name,rows):
    scene=bpy.data.scenes.new(name)
    scene.render.engine="BLENDER_EEVEE"
    scene.render.resolution_x=2000
    scene.render.resolution_y=1500 if len(rows)>1 else 550
    scene.render.resolution_percentage=100
    scene.world=bpy.data.worlds.new(name+" World")
    scene.world.use_nodes=True
    scene.world.node_tree.nodes["Background"].inputs["Color"].default_value=(.025,.18,.26,1)
    scene.world.node_tree.nodes["Background"].inputs["Strength"].default_value=.65
    _light(scene)
    for row,number in enumerate(rows):
        base=base_blueprint(number)
        z=(len(rows)-1-row)*1.1 if len(rows)>1 else 0
        for column,stage in enumerate(STAGES[number]):
            instance=bpy.data.objects.new(base.faf_id+" "+stage,None)
            scene.collection.objects.link(instance)
            instance.instance_type="COLLECTION"
            instance.instance_collection=_collection(number,stage)
            instance.location=(0,(column-2)*1.02,z)
            scale=DISPLAY_SCALE[number]*(9 if stage=="egg" else 1)
            instance.scale=(scale,scale,scale)
            instance["review_only_scale"]="egg enlarged 9x for visibility" if stage=="egg" else "species row normalized"
            instance["faf_id"]=base.faf_id
            instance["faf_life_stage"]=stage
    center_z=(len(rows)-1)*.55 if len(rows)>1 else 0
    side=_camera(scene,"Side Camera",(8,0,center_z),(0,0,center_z),
            8.0 if len(rows)>1 else 7.4)
    _camera(scene,"Three Quarter Camera",(7,-4,center_z+2.0),(0,0,center_z),
            8.8 if len(rows)>1 else 8.0)
    scene.camera=side
    return scene


def build(root):
    _append_adults(root)
    overview=_review_scene("FAF Life Stages | Five Families",list(STAGES))
    for number in STAGES:
        _review_scene(base_blueprint(number).faf_id+" | Progression",[number])
    bpy.context.window.scene=overview
    bpy.context.preferences.filepaths.save_version=0
    output=root/"assets/faf/fish/life_stages_review.blend"
    bpy.ops.wm.save_as_mainfile(filepath=str(output),compress=True)
    return output


if __name__=="__main__":
    args=sys.argv[sys.argv.index("--")+1:] if "--" in sys.argv else []
    parser=argparse.ArgumentParser();parser.add_argument("--root",type=Path,required=True)
    options=parser.parse_args(args)
    print("FAF_LIFE_STAGE_REVIEW="+str(build(options.root.resolve())))
