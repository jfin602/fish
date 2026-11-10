"""Build the six reviewed FAF-TERRAIN-001 patches.

Background entry: blender -b --python tools/asset-factory/blender/terrain/build_001.py -- --root REPO
Existing outputs require --overwrite. A live Blender MCP session can call
create_sources/export_sources without replacing its open scene.
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
from terrain.patch import TerrainSpec, create_terrain  # noqa: E402


RECIPES = {
    "A": TerrainSpec("open_sand", (10.8, 8.6), 0.22, 0.0, 0.16, 0.95, 0.0, 6101),
    "B": TerrainSpec("rolling", (5.2, 4.4), 0.53, 0.4, 0.42, 0.75, 0.0, 6102),
    "C": TerrainSpec("reef_shelf", (5.0, 4.7), 0.89, 0.2, 0.3, 0.83, 0.83, 6103),
    "D": TerrainSpec("slope", (5.6, 4.2), 0.70, 0.65, 0.22, 0.75, 0.42, 6104),
    "E": TerrainSpec("drop_off", (5.1, 4.8), 1.02, 0.0, 0.18, 0.90, 0.78, 6105),
    "F": TerrainSpec("rocky_floor", (5.3, 4.9), 0.62, 1.1, 0.63, 0.91, 0.72, 6106),
}


def terrain_material(family, swatch, name):
    material = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    material.use_nodes = True
    srgb = hex_rgb(family, swatch)
    linear = tuple(((v + 0.055) / 1.055) ** 2.4 if v > 0.04045 else v / 12.92 for v in srgb)
    material.diffuse_color = (*linear, 1)
    shader = material.node_tree.nodes.get("Principled BSDF")
    shader.inputs["Base Color"].default_value = (*linear, 1)
    shader.inputs["Roughness"].default_value = 0.92
    shader.inputs["Metallic"].default_value = 0
    return material


def create_sources(scene):
    scene.unit_settings.system = "METRIC"
    scene.unit_settings.scale_length = 1
    source = bpy.data.collections.new("FAF-TERRAIN-001 Source")
    scene.collection.children.link(source)
    materials = (
        terrain_material("sand", "warm_light", "mat__terrain_sand_warm_light"),
        terrain_material("rock", "warm_stone", "mat__terrain_sand_rock_transition"),
        terrain_material("rock", "cool_slate", "mat__terrain_reef_cool_slate"),
    )
    return {letter: create_terrain(f"terrain_001{letter.lower()}_{spec.profile}__mesh",
                                   spec, materials, source)
            for letter, spec in RECIPES.items()}


def add_review_instances(scene, sources):
    review = bpy.data.collections.new("FAF-TERRAIN-001 Review")
    scene.collection.children.link(review)
    positions = {"A": (0, 0), "B": (-6.0, 5.4), "C": (0, 6.1),
                 "D": (6.0, 5.4), "E": (-4.5, -5.5), "F": (4.5, -5.5)}
    for letter, obj in sources.items():
        copy = obj.copy()
        copy.data = obj.data
        copy.name = "review__" + obj.name.removesuffix("__mesh")
        copy.location = (*positions[letter], 0)
        review.objects.link(copy)
        obj.hide_set(True)
    return review


def add_composition_instances(scene, sources):
    """Reproduce the A+C+D overlap proof without touching asset transforms."""
    composition = bpy.data.collections.new("FAF-TERRAIN-001 Composition")
    scene.collection.children.link(composition)
    positions = {"A": (0, 0, -0.04), "C": (-2.1, 0.6, 0),
                 "D": (2.2, -0.7, 0)}
    for letter, position in positions.items():
        obj = sources[letter]
        copy = obj.copy()
        copy.data = obj.data
        copy.name = "composition__" + obj.name.removesuffix("__mesh")
        copy.location = position
        composition.objects.link(copy)
    composition.hide_viewport = True
    return composition


def export_sources(scene, sources, root: Path, overwrite=False):
    destination_dir = root / "game/assets/faf/terrain"
    destination_dir.mkdir(parents=True, exist_ok=True)
    reports = []
    for letter, obj in sources.items():
        spec = RECIPES[letter]
        destination = destination_dir / f"terrain_001{letter.lower()}_{spec.profile}.glb"
        bpy.ops.object.select_all(action="DESELECT")
        was_hidden = obj.hide_get()
        obj.hide_set(False)
        obj.select_set(True)
        bpy.context.view_layer.objects.active = obj
        check = inspect_objects([obj])
        if check["problems"]:
            raise RuntimeError(f"{letter} source validation failed: {check}")
        scene.name = f"FAF-TERRAIN-001{letter} {spec.profile}"
        export_selected_glb(str(destination), include_animations=False, overwrite=overwrite)
        obj.hide_set(was_hidden)
        reports.append({
            "faf_id": f"FAF-TERRAIN-001{letter}", "category": "TERRAIN",
            "species_or_type": spec.profile, "generator": "terrain.patch.v1",
            "dimensions_m": [round(v, 5) for v in obj.dimensions],
            "triangles": check["triangle_count_source"],
            "materials": check["material_count"],
            "material_names": [mat.name for mat in obj.data.materials],
            "armature_bone_count": 0, "animation_actions": [],
            "origin_m": [round(v, 5) for v in obj.location],
            "rotation_rad": [round(v, 5) for v in obj.rotation_euler],
            "scale": [round(v, 5) for v in obj.scale],
            "top_edge_height_m": 0.0, "skirt_depth_m": 0.09,
            "spec": spec.__dict__, "seed": spec.seed,
            "glb": destination.relative_to(root).as_posix(),
        })
    scene.name = "FAF-TERRAIN-001 Seabed Family"
    return reports


def write_manifest(root, reports):
    source = root / "assets/faf/terrain/terrain_001_family.blend"
    source.parent.mkdir(parents=True, exist_ok=True)
    manifest = source.with_name("terrain_001_manifest.json")
    manifest.write_text(json.dumps({"source": source.relative_to(root).as_posix(),
                                    "modularity": "irregular zero-height overlap apron",
                                    "assets": reports}, indent=2) + "\n")
    return source


def build(root: Path, overwrite=False):
    source = root / "assets/faf/terrain/terrain_001_family.blend"
    if source.exists() and not overwrite:
        raise FileExistsError(source)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    scene = bpy.context.scene
    scene.name = "FAF-TERRAIN-001 Seabed Family"
    sources = create_sources(scene)
    reports = export_sources(scene, sources, root, overwrite)
    add_review_instances(scene, sources)
    add_composition_instances(scene, sources)
    source = write_manifest(root, reports)
    bpy.ops.wm.save_as_mainfile(filepath=str(source))
    return reports


if __name__ == "__main__":
    args = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, required=True)
    parser.add_argument("--overwrite", action="store_true")
    options = parser.parse_args(args)
    print("FAF_TERRAIN_001_REPORT=" + json.dumps(build(options.root, options.overwrite)))
