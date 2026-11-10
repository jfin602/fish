"""Render representative close gill/fin inspection evidence from the stage master.

blender -b assets/faf/fish/life_stages_001_to_005.blend --python \
  tools/asset-factory/blender/fish/render_life_stage_gills.py -- --root REPO
"""

from pathlib import Path
import argparse
import sys

import bpy
from mathutils import Vector

TOOLING = Path(__file__).resolve().parents[1]
if str(TOOLING) not in sys.path:
    sys.path.insert(0, str(TOOLING))
from fish.animation import reset_pose
from fish.breathing import opening_rotation_z
from fish.life_stages import PROFILES, stage_blueprint
from fish.reef_families import breathing_spec


SAMPLES = {1: "juvenile", 2: "juvenile", 3: "juvenile",
           4: "subadult", 5: "newborn_pup"}


def _aim(obj, point):
    obj.rotation_euler = (Vector(point) - obj.location).to_track_quat("-Z", "Y").to_euler()


def render(root):
    evidence = root / "docs/fish-asset-factory/life-stages-001/evidence"
    evidence.mkdir(parents=True, exist_ok=True)
    for number, stage in SAMPLES.items():
        profile = next(item for item in PROFILES[number] if item.stage == stage)
        bp = stage_blueprint(number, profile)
        scene = bpy.data.scenes[bp.faf_id + " " + stage]
        bpy.context.window.scene = scene
        arm = bpy.data.objects[bp.stem + "__armature"]
        arm.animation_data.action = bpy.data.actions[bp.stem + "__Glide"]
        scene.frame_set(1)
        reset_pose(arm)
        gill = breathing_spec(bp)
        arm.pose.bones["gill_L"].rotation_mode = "XYZ"
        arm.pose.bones["gill_L"].rotation_euler.z = opening_rotation_z(gill, "gill_L")
        bpy.context.view_layer.update()

        length = bp.shape[-1][0] - bp.shape[0][0]
        target = (0, (bp.eye_y + gill.center_y) * .5, 0)
        camera_data = bpy.data.cameras.new("Inspection Side Camera")
        camera = bpy.data.objects.new(camera_data.name, camera_data)
        scene.collection.objects.link(camera)
        camera.location = (max(.2, length * 4), target[1], length * .38)
        _aim(camera, target)
        camera_data.type = "ORTHO"
        camera_data.ortho_scale = length * .78
        scene.camera = camera

        light_data = bpy.data.lights.new("Inspection Softbox", "AREA")
        light_data.energy = 220
        light_data.size = max(.3, length * 3)
        light = bpy.data.objects.new(light_data.name, light_data)
        scene.collection.objects.link(light)
        light.location = (max(.3, length * 5), target[1] - length * .4,
                          max(.3, length * 3))
        _aim(light, target)
        scene.world = bpy.data.worlds.new("Inspection Water")
        scene.world.color = (.04, .22, .30)
        scene.render.engine = "BLENDER_EEVEE"
        scene.render.resolution_x = 1100
        scene.render.resolution_y = 700
        scene.render.resolution_percentage = 100
        scene.render.filepath = str(evidence / f"faf_fish_{number:03d}_{stage}_gill_open.png")
        bpy.ops.render.render(write_still=True, scene=scene.name)
    return len(SAMPLES)


if __name__ == "__main__":
    args = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, required=True)
    options = parser.parse_args(args)
    print("FAF_LIFE_STAGE_GILL_RENDERS=" + str(render(options.root.resolve())))
