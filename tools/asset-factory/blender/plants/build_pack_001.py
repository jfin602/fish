"""Build FAF-PLANT-001–003 Blender master, manifest and eight separate GLBs.

Background Blender entry: blender -b --python tools/asset-factory/blender/plants/build_pack_001.py -- --root REPO
Use --overwrite to replace an existing reviewed pack deliberately.
"""

from __future__ import annotations

import argparse
import json
from pathlib import Path
import sys

import bpy

TOOLING = Path(__file__).resolve().parents[1]
if str(TOOLING) not in sys.path:
    sys.path.insert(0, str(TOOLING))

from common.export import export_selected_glb  # noqa: E402
from common.palette import hex_rgb  # noqa: E402
from common.validation import inspect_objects  # noqa: E402
from plants.families import (SeaGrassSpec, BroadSeaweedSpec, ReefAlgaeSpec,
                             create_plant)  # noqa: E402


RECIPES = {
    "sea_grass": {
        "A": (SeaGrassSpec(0.58, 7, 0.32, 0.085, 0.12, 0.12, 5101), "green"),
        "B": (SeaGrassSpec(0.76, 8, 0.40, 0.074, 0.17, 0.18, 5102), "deep_green"),
        "C": (SeaGrassSpec(0.47, 6, 0.27, 0.103, 0.09, 0.10, 5103), "yellow_green"),
    },
    "broad_seaweed": {
        "A": (BroadSeaweedSpec(1.16, 4, 0.42, 0.25, 0.23, 0.21, 5201), "deep_green"),
        "B": (BroadSeaweedSpec(1.35, 5, 0.52, 0.22, 0.31, 0.27, 5202), "green"),
        "C": (BroadSeaweedSpec(0.97, 3, 0.38, 0.29, 0.16, 0.17, 5203), "muted_teal"),
    },
    "reef_algae": {
        "A": (ReefAlgaeSpec(0.36, 5, 0.34, 0.14, 0.13, 0.16, 5301), "yellow_green"),
        "B": (ReefAlgaeSpec(0.44, 6, 0.43, 0.12, 0.16, 0.21, 5302), "deep_green"),
    },
}

FAMILY_INDEX = {"sea_grass": "001", "broad_seaweed": "002", "reef_algae": "003"}
FAMILY_STEM = {"sea_grass": "sea_grass", "broad_seaweed": "broad_seaweed", "reef_algae": "reef_algae"}


def plant_material(swatch):
    name = f"mat__plant_{swatch}"
    material = bpy.data.materials.get(name)
    if material is None:
        material = bpy.data.materials.new(name)
    material.use_nodes = True
    srgb = hex_rgb("vegetation", swatch)
    linear = tuple(((v + 0.055) / 1.055) ** 2.4 if v > 0.04045 else v / 12.92 for v in srgb)
    material.diffuse_color = (*linear, 1)
    shader = material.node_tree.nodes.get("Principled BSDF")
    shader.inputs["Base Color"].default_value = (*linear, 1)
    shader.inputs["Roughness"].default_value = 0.86
    shader.inputs["Metallic"].default_value = 0
    return material


def create_sources(scene):
    scene.unit_settings.system = "METRIC"
    scene.unit_settings.scale_length = 1
    sources = {}
    for family, variants in RECIPES.items():
        collection = bpy.data.collections.new(f"FAF-PLANT-{FAMILY_INDEX[family]} Source")
        scene.collection.children.link(collection)
        for letter, (spec, swatch) in variants.items():
            stem = f"plant_{FAMILY_INDEX[family]}{letter.lower()}_{FAMILY_STEM[family]}"
            sources[(family, letter)] = create_plant(stem + "__mesh", family, spec,
                                                     plant_material(swatch), collection)
    return sources


def add_review_instances(scene, sources):
    review = bpy.data.collections.new("FAF-PLANT Pack Review")
    scene.collection.children.link(review)
    rows = {"sea_grass": 0, "broad_seaweed": 2.3, "reef_algae": 4.5}
    for (family, letter), obj in sources.items():
        copy = obj.copy()
        copy.data = obj.data
        copy.name = "review__" + obj.name.removesuffix("__mesh")
        copy.location = ((ord(letter) - ord("B")) * 1.7, rows[family], 0)
        review.objects.link(copy)
        obj.hide_set(True)
    return review


def export_sources(scene, sources, root, overwrite=False):
    output = root / "game/assets/faf/plants"
    output.mkdir(parents=True, exist_ok=True)
    reports = []
    for (family, letter), obj in sources.items():
        spec, swatch = RECIPES[family][letter]
        stem = f"plant_{FAMILY_INDEX[family]}{letter.lower()}_{FAMILY_STEM[family]}"
        destination = output / (stem + ".glb")
        bpy.ops.object.select_all(action="DESELECT")
        was_hidden = obj.hide_get()
        obj.hide_set(False)
        obj.select_set(True)
        bpy.context.view_layer.objects.active = obj
        check = inspect_objects([obj])
        if check["problems"] or not 20 <= check["triangle_count_source"] <= 300:
            raise RuntimeError(f"{stem} failed FAF source checks: {check}")
        if len(obj.data.uv_layers) != 1 or not obj.data.uv_layers[0].data:
            raise RuntimeError(f"{stem} is missing the sway-ready UV coordinate")
        scene.name = f"FAF-PLANT-{FAMILY_INDEX[family]}{letter} {family}"
        export_selected_glb(str(destination), include_animations=False, overwrite=overwrite)
        obj.hide_set(was_hidden)
        reports.append({
            "faf_id": f"FAF-PLANT-{FAMILY_INDEX[family]}{letter}",
            "category": "PLANT", "species_or_type": family,
            "stem": stem, "family": family,
            "generator": f"plants.{family}.v1",
            "dimensions_m": [round(v, 5) for v in obj.dimensions],
            "triangles": check["triangle_count_source"],
            "materials": check["material_count"],
            "armature_bone_count": 0, "animation_actions": [],
            "origin_m": [round(v, 5) for v in obj.location],
            "rotation_rad": [round(v, 5) for v in obj.rotation_euler],
            "scale": [round(v, 5) for v in obj.scale],
            "palette": f"vegetation.{swatch}",
            "sway_coordinate": "UV.y, base=0, tip=1",
            "spec": spec.__dict__,
            "glb": destination.relative_to(root).as_posix(),
        })
    scene.name = "FAF-PLANT-001-to-003 Vegetation Pack"
    return reports


def build(root: Path, overwrite=False):
    source = root / "assets/faf/plants/plant_001_to_003_family.blend"
    if not overwrite and source.exists():
        raise FileExistsError(source)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    scene = bpy.context.scene
    scene.name = "FAF-PLANT-001-to-003 Vegetation Pack"
    sources = create_sources(scene)
    reports = export_sources(scene, sources, root, overwrite)
    add_review_instances(scene, sources)
    source.parent.mkdir(parents=True, exist_ok=True)
    manifest = source.with_name("plant_001_to_003_manifest.json")
    manifest.write_text(json.dumps({"source": source.relative_to(root).as_posix(),
                                    "assets": reports}, indent=2) + "\n")
    bpy.ops.wm.save_as_mainfile(filepath=str(source))
    return reports


if __name__ == "__main__":
    args = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, required=True)
    parser.add_argument("--overwrite", action="store_true")
    options = parser.parse_args(args)
    print("FAF_PLANT_PACK_REPORT=" + json.dumps(build(options.root, options.overwrite)))
